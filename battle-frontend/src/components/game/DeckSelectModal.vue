<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { InformationDto, WeaponDto, SkillDto } from '../../types/battle';
import { PROFESSION_MAP, DAMAGE_TYPE_MAP } from '../../utils/constants';

const props = defineProps<{
  visible: boolean;
  myProfile: InformationDto;
  target: { id: string | number; name: string };
}>();

const emit = defineEmits<{
  (e: 'confirm', deckWeaponIds: number[], deckSkillIds: number[]): void;
  (e: 'cancel'): void;
}>();

// ──── 卡组容量 ────────────────────────────────────────────────
const capacity = computed(() => Math.floor(props.myProfile.level / 5) + 2);

// ──── 已选卡牌（weaponId/skillId 列表，可重复）──────────────────
const selectedWeaponIds = ref<number[]>([]);
const selectedSkillIds  = ref<number[]>([]);

const totalSelected = computed(
  () => selectedWeaponIds.value.length + selectedSkillIds.value.length
);
const isOver = computed(() => totalSelected.value > capacity.value);

// 当 modal 打开时重置
watch(() => props.visible, (v) => {
  if (v) {
    selectedWeaponIds.value = [];
    selectedSkillIds.value  = [];
  }
});

// ──── 互斥检测 ────────────────────────────────────────────────
// 获取当前已选的所有 ExclusiveGroup 集合
const selectedGroups = computed(() => {
  const groups = new Set<string>();
  for (const id of selectedWeaponIds.value) {
    const w = props.myProfile.weapons.find(x => x.id === id);
    if (w?.exclusiveGroup) groups.add(w.exclusiveGroup);
  }
  for (const id of selectedSkillIds.value) {
    const s = props.myProfile.skills.find(x => x.id === id);
    if (s?.exclusiveGroup) groups.add(s.exclusiveGroup);
  }
  return groups;
});

// 判断玩家是否持有无视互斥的 R4 被动（名称约定："无法无天"）
const hasWuFaWuTian = computed(() =>
  props.myProfile.skills.some(s => s.name === '无法无天')
);

function isConflict(item: WeaponDto | SkillDto): boolean {
  if (hasWuFaWuTian.value) return false;
  if (!item.exclusiveGroup) return false;
  // 只有在该组已有其他物品时才冲突（自身不算冲突）
  const id = (item as any).id as number;
  const type = 'damageType' in item ? 'weapon' : 'skill';
  const alreadySelected = type === 'weapon'
    ? selectedWeaponIds.value.includes(id)
    : selectedSkillIds.value.includes(id);
  // 如果组里已有成员（且不是自身），则有冲突
  return selectedGroups.value.has(item.exclusiveGroup) && !alreadySelected;
}

// ──── 计数工具 ────────────────────────────────────────────────
function countInDeck(id: number, type: 'weapon' | 'skill') {
  const arr = type === 'weapon' ? selectedWeaponIds.value : selectedSkillIds.value;
  return arr.filter(x => x === id).length;
}

function maxCount(id: number, type: 'weapon' | 'skill') {
  if (type === 'weapon') {
    return props.myProfile.weapons.filter(x => x.id === id).length;
  }
  return props.myProfile.skills.filter(x => x.id === id).length;
}

// ──── 添加 / 移除 ────────────────────────────────────────────
function addItem(id: number, type: 'weapon' | 'skill') {
  if (totalSelected.value >= capacity.value) return;
  const arr = type === 'weapon' ? selectedWeaponIds : selectedSkillIds;
  const max = maxCount(id, type);
  if (arr.value.filter(x => x === id).length >= max) return;
  arr.value = [...arr.value, id];
}

function removeItem(id: number, type: 'weapon' | 'skill') {
  const arr = type === 'weapon' ? selectedWeaponIds : selectedSkillIds;
  const idx = arr.value.lastIndexOf(id);
  if (idx !== -1) {
    arr.value = [...arr.value.slice(0, idx), ...arr.value.slice(idx + 1)];
  }
}

// ──── 稀有度颜色 ─────────────────────────────────────────────
const rarityColors: Record<number, string> = {
  1: 'text-slate-400 border-slate-600',
  2: 'text-blue-400 border-blue-600',
  3: 'text-violet-400 border-violet-600',
  4: 'text-amber-400 border-amber-500',
};
const rarityLabel: Record<number, string> = { 1: 'R1', 2: 'R2', 3: 'R3', 4: 'R4' };

function confirm() {
  emit('confirm', [...selectedWeaponIds.value], [...selectedSkillIds.value]);
}
</script>

<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="visible"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm p-4"
        @click.self="$emit('cancel')">

        <div class="bg-slate-900 border border-white/10 rounded-2xl w-full max-w-2xl shadow-2xl
                    flex flex-col max-h-[90vh] overflow-hidden">

          <!-- Header -->
          <div class="px-6 py-4 border-b border-white/10 flex items-center justify-between shrink-0">
            <div>
              <h2 class="text-lg font-bold text-white">选择出战卡组</h2>
              <p class="text-xs text-slate-400 mt-0.5">
                对战 <span class="text-red-400 font-semibold">{{ target.name }}</span>
                &nbsp;·&nbsp;容量
                <span :class="isOver ? 'text-red-400' : 'text-indigo-400'" class="font-bold">
                  {{ totalSelected }} / {{ capacity }}
                </span>
              </p>
            </div>
            <button @click="$emit('cancel')"
              class="w-8 h-8 rounded-full bg-white/5 hover:bg-white/15 transition flex items-center justify-center text-slate-400 hover:text-white">
              ✕
            </button>
          </div>

          <!-- Body (scrollable) -->
          <div class="flex-1 overflow-y-auto custom-scrollbar px-6 py-4 space-y-6">

            <!-- Weapons -->
            <section>
              <h3 class="text-xs font-black text-slate-400 tracking-widest uppercase mb-3">武器</h3>
              <div class="space-y-1.5">
                <div v-for="weapon in myProfile.weapons" :key="weapon.id"
                  class="flex items-center gap-3 p-2.5 rounded-lg border transition-all"
                  :class="[
                    isConflict(weapon)
                      ? 'border-orange-500/40 bg-orange-500/5'
                      : 'border-white/5 bg-white/[0.03] hover:bg-white/[0.06]'
                  ]">

                  <!-- 稀有度徽章 -->
                  <span class="text-[10px] font-black border rounded px-1 shrink-0"
                    :class="rarityColors[weapon.rareLevel] ?? 'text-slate-400 border-slate-600'">
                    {{ rarityLabel[weapon.rareLevel] ?? '??' }}
                  </span>

                  <!-- 伤害类型图标 -->
                  <span class="text-sm shrink-0" :title="DAMAGE_TYPE_MAP[weapon.damageType]?.label">
                    {{ DAMAGE_TYPE_MAP[weapon.damageType]?.icon ?? '❓' }}
                  </span>

                  <!-- 名称 & 职业 -->
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-1.5">
                      <span class="text-sm text-white font-medium truncate">{{ weapon.name }}</span>
                      <span v-if="isConflict(weapon)"
                        class="text-[9px] text-orange-400 border border-orange-500/50 rounded px-1 shrink-0">
                        互斥
                      </span>
                      <span v-if="weapon.exclusiveGroup && !isConflict(weapon)"
                        class="text-[9px] text-slate-500 border border-slate-700 rounded px-1 shrink-0">
                        组:{{ weapon.exclusiveGroup }}
                      </span>
                    </div>
                    <div class="text-[10px] text-slate-500 mt-0.5">
                      {{ PROFESSION_MAP[weapon.profession]?.label ?? weapon.profession }}
                      <span v-if="weapon.damageType" class="ml-1"
                        :class="DAMAGE_TYPE_MAP[weapon.damageType]?.color">
                        · {{ DAMAGE_TYPE_MAP[weapon.damageType]?.label }}
                      </span>
                    </div>
                  </div>

                  <!-- 数量控制 -->
                  <div class="flex items-center gap-1.5 shrink-0">
                    <button @click="removeItem(weapon.id, 'weapon')"
                      :disabled="countInDeck(weapon.id, 'weapon') === 0"
                      class="w-6 h-6 rounded-full text-sm font-bold flex items-center justify-center
                             transition disabled:opacity-30 disabled:cursor-not-allowed
                             bg-white/10 hover:bg-white/20 text-white">−</button>
                    <span class="w-5 text-center text-sm font-bold text-white">
                      {{ countInDeck(weapon.id, 'weapon') }}
                    </span>
                    <button @click="addItem(weapon.id, 'weapon')"
                      :disabled="totalSelected >= capacity || countInDeck(weapon.id, 'weapon') >= maxCount(weapon.id, 'weapon')"
                      class="w-6 h-6 rounded-full text-sm font-bold flex items-center justify-center
                             transition disabled:opacity-30 disabled:cursor-not-allowed
                             bg-indigo-600 hover:bg-indigo-500 text-white">+</button>
                  </div>
                </div>
              </div>
            </section>

            <!-- Skills -->
            <section>
              <h3 class="text-xs font-black text-slate-400 tracking-widest uppercase mb-3">技能</h3>
              <div class="space-y-1.5">
                <div v-for="skill in myProfile.skills" :key="skill.id"
                  class="flex items-center gap-3 p-2.5 rounded-lg border transition-all"
                  :class="[
                    isConflict(skill)
                      ? 'border-orange-500/40 bg-orange-500/5'
                      : 'border-white/5 bg-white/[0.03] hover:bg-white/[0.06]'
                  ]">

                  <!-- 稀有度徽章 -->
                  <span class="text-[10px] font-black border rounded px-1 shrink-0"
                    :class="rarityColors[skill.rareLevel] ?? 'text-slate-400 border-slate-600'">
                    {{ rarityLabel[skill.rareLevel] ?? '??' }}
                  </span>

                  <!-- 被动标识 -->
                  <span class="text-[9px] shrink-0 border rounded px-1"
                    :class="skill.isPassive
                      ? 'text-amber-400 border-amber-500/50 bg-amber-500/10'
                      : 'text-slate-500 border-slate-700'">
                    {{ skill.isPassive ? '被动' : '主动' }}
                  </span>

                  <!-- 名称 & 职业 -->
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-1.5">
                      <span class="text-sm text-white font-medium truncate">{{ skill.name }}</span>
                      <span v-if="isConflict(skill)"
                        class="text-[9px] text-orange-400 border border-orange-500/50 rounded px-1 shrink-0">
                        互斥
                      </span>
                      <span v-if="skill.exclusiveGroup && !isConflict(skill)"
                        class="text-[9px] text-slate-500 border border-slate-700 rounded px-1 shrink-0">
                        组:{{ skill.exclusiveGroup }}
                      </span>
                    </div>
                    <div class="text-[10px] text-slate-500 mt-0.5">
                      {{ PROFESSION_MAP[skill.profession]?.label ?? skill.profession }}
                    </div>
                  </div>

                  <!-- 数量控制 -->
                  <div class="flex items-center gap-1.5 shrink-0">
                    <button @click="removeItem(skill.id, 'skill')"
                      :disabled="countInDeck(skill.id, 'skill') === 0"
                      class="w-6 h-6 rounded-full text-sm font-bold flex items-center justify-center
                             transition disabled:opacity-30 disabled:cursor-not-allowed
                             bg-white/10 hover:bg-white/20 text-white">−</button>
                    <span class="w-5 text-center text-sm font-bold text-white">
                      {{ countInDeck(skill.id, 'skill') }}
                    </span>
                    <button @click="addItem(skill.id, 'skill')"
                      :disabled="totalSelected >= capacity || countInDeck(skill.id, 'skill') >= maxCount(skill.id, 'skill')"
                      class="w-6 h-6 rounded-full text-sm font-bold flex items-center justify-center
                             transition disabled:opacity-30 disabled:cursor-not-allowed
                             bg-indigo-600 hover:bg-indigo-500 text-white">+</button>
                  </div>
                </div>
              </div>
            </section>
          </div>

          <!-- Footer -->
          <div class="px-6 py-4 border-t border-white/10 flex justify-between items-center shrink-0">
            <div class="text-xs text-slate-500">
              <span v-if="totalSelected === 0" class="text-yellow-500/80">空卡组 — 将只用拳头攻击</span>
              <span v-else-if="isOver" class="text-red-400">已超出容量限制</span>
              <span v-else>已选 {{ totalSelected }} 张 · 剩余 {{ capacity - totalSelected }} 位</span>
            </div>
            <div class="flex gap-2">
              <button @click="$emit('cancel')"
                class="px-4 py-2 text-sm rounded-lg bg-white/5 hover:bg-white/10 text-slate-300 transition">
                取消
              </button>
              <button @click="confirm"
                :disabled="isOver"
                class="px-5 py-2 text-sm font-bold rounded-lg transition
                       disabled:opacity-40 disabled:cursor-not-allowed
                       bg-red-600 hover:bg-red-500 text-white">
                出战！
              </button>
            </div>
          </div>

        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
