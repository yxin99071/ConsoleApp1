<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getProfile } from '../api/battle';
import { getDefaultDeck, setDefaultDeck } from '../api/settings';
import type { InformationDto, WeaponDto, SkillDto } from '../types/battle';
import { PROFESSION_MAP, DAMAGE_TYPE_MAP } from '../utils/constants';

const router = useRouter();
const profile    = ref<InformationDto | null>(null);
const loading    = ref(true);
const saving     = ref(false);
const saveMsg    = ref('');

const deckWeaponIds = ref<number[]>([]);
const deckSkillIds  = ref<number[]>([]);
const capacity      = ref(2);

onMounted(async () => {
  try {
    const [p, d] = await Promise.all([getProfile(), getDefaultDeck()]);
    profile.value      = p;
    deckWeaponIds.value = d.weaponIds;
    deckSkillIds.value  = d.skillIds;
    capacity.value      = d.capacity;
  } finally {
    loading.value = false;
  }
});

const totalSelected = computed(
  () => deckWeaponIds.value.length + deckSkillIds.value.length
);
const isOver = computed(() => totalSelected.value > capacity.value);

// ──── 互斥 ───────────────────────────────────────────────────
const selectedGroups = computed(() => {
  const groups = new Set<string>();
  if (!profile.value) return groups;
  for (const id of deckWeaponIds.value) {
    const w = profile.value.weapons.find(x => x.id === id);
    if (w?.exclusiveGroup) groups.add(w.exclusiveGroup);
  }
  for (const id of deckSkillIds.value) {
    const s = profile.value.skills.find(x => x.id === id);
    if (s?.exclusiveGroup) groups.add(s.exclusiveGroup);
  }
  return groups;
});

const hasWuFaWuTian = computed(() =>
  profile.value?.skills.some(s => s.name === '无法无天') ?? false
);

function isConflict(item: WeaponDto | SkillDto): boolean {
  if (hasWuFaWuTian.value || !item.exclusiveGroup) return false;
  const id   = item.id;
  const type = 'damageType' in item ? 'weapon' : 'skill';
  const selected = type === 'weapon'
    ? deckWeaponIds.value.includes(id)
    : deckSkillIds.value.includes(id);
  return selectedGroups.value.has(item.exclusiveGroup) && !selected;
}

// ──── 数量 ────────────────────────────────────────────────────
function countInDeck(id: number, type: 'weapon' | 'skill') {
  return (type === 'weapon' ? deckWeaponIds.value : deckSkillIds.value).filter(x => x === id).length;
}
function maxCount(id: number, type: 'weapon' | 'skill') {
  if (!profile.value) return 0;
  return type === 'weapon'
    ? profile.value.weapons.filter(x => x.id === id).length
    : profile.value.skills.filter(x => x.id === id).length;
}
function addItem(id: number, type: 'weapon' | 'skill') {
  if (totalSelected.value >= capacity.value) return;
  const arr = type === 'weapon' ? deckWeaponIds : deckSkillIds;
  if (arr.value.filter(x => x === id).length >= maxCount(id, type)) return;
  arr.value = [...arr.value, id];
}
function removeItem(id: number, type: 'weapon' | 'skill') {
  const arr = type === 'weapon' ? deckWeaponIds : deckSkillIds;
  const idx = arr.value.lastIndexOf(id);
  if (idx !== -1)
    arr.value = [...arr.value.slice(0, idx), ...arr.value.slice(idx + 1)];
}

// ──── 保存 ────────────────────────────────────────────────────
async function saveDeck() {
  saving.value = true;
  saveMsg.value = '';
  try {
    await setDefaultDeck({ weaponIds: deckWeaponIds.value, skillIds: deckSkillIds.value });
    saveMsg.value = '✓ 已保存';
    setTimeout(() => { saveMsg.value = '' }, 3000);
  } catch {
    saveMsg.value = '保存失败，请重试';
  } finally {
    saving.value = false;
  }
}

const rarityColors: Record<number, string> = {
  1: 'text-slate-400 border-slate-600',
  2: 'text-blue-400 border-blue-600',
  3: 'text-violet-400 border-violet-600',
  4: 'text-amber-400 border-amber-500',
};
const rarityLabel: Record<number, string> = { 1: 'R1', 2: 'R2', 3: 'R3', 4: 'R4' };
</script>

<template>
  <div class="min-h-screen bg-[#020617] text-slate-200 font-sans">

    <!-- Top bar -->
    <div class="border-b border-white/5 bg-slate-900/60 backdrop-blur-sm px-6 py-4 flex items-center gap-4">
      <button @click="router.back()"
        class="w-8 h-8 rounded-full bg-white/5 hover:bg-white/15 transition flex items-center justify-center text-slate-400 hover:text-white text-sm">
        ←
      </button>
      <h1 class="text-sm font-black text-indigo-400 tracking-[0.2em] uppercase">Settings</h1>
    </div>

    <div v-if="loading" class="flex justify-center items-center h-64">
      <div class="w-8 h-8 border-2 border-indigo-500 border-t-transparent rounded-full animate-spin"></div>
    </div>

    <div v-else-if="profile" class="max-w-2xl mx-auto px-6 py-10 space-y-10">

      <!-- Section: Default Deck -->
      <section>
        <div class="flex items-center justify-between mb-4">
          <div>
            <h2 class="text-base font-bold text-white">默认出战 Build</h2>
            <p class="text-xs text-slate-500 mt-0.5">
              被挑战时自动使用此卡组 · 容量
              <span :class="isOver ? 'text-red-400' : 'text-indigo-400'" class="font-bold">
                {{ totalSelected }} / {{ capacity }}
              </span>
            </p>
          </div>
          <div class="flex items-center gap-3">
            <span v-if="saveMsg" class="text-sm text-emerald-400">{{ saveMsg }}</span>
            <button @click="saveDeck"
              :disabled="saving || isOver"
              class="px-4 py-2 text-sm font-bold rounded-lg transition
                     disabled:opacity-40 disabled:cursor-not-allowed
                     bg-indigo-600 hover:bg-indigo-500 text-white">
              {{ saving ? '保存中…' : '保存卡组' }}
            </button>
          </div>
        </div>

        <!-- 武器列表 -->
        <div class="space-y-2 mb-6">
          <p class="text-[10px] font-bold text-slate-500 tracking-widest uppercase">武器</p>
          <div v-for="weapon in profile.weapons" :key="weapon.id"
            class="flex items-center gap-3 p-2.5 rounded-lg border transition"
            :class="isConflict(weapon)
              ? 'border-orange-500/40 bg-orange-500/5'
              : 'border-white/5 bg-white/[0.03] hover:bg-white/[0.06]'">

            <span class="text-[10px] font-black border rounded px-1 shrink-0"
              :class="rarityColors[weapon.rareLevel]">
              {{ rarityLabel[weapon.rareLevel] }}
            </span>

            <span class="text-sm shrink-0" :title="DAMAGE_TYPE_MAP[weapon.damageType]?.label">
              {{ DAMAGE_TYPE_MAP[weapon.damageType]?.icon ?? '❓' }}
            </span>

            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-1.5">
                <span class="text-sm text-white font-medium truncate">{{ weapon.name }}</span>
                <span v-if="isConflict(weapon)"
                  class="text-[9px] text-orange-400 border border-orange-500/50 rounded px-1 shrink-0">互斥</span>
              </div>
              <div class="text-[10px] text-slate-500">
                {{ PROFESSION_MAP[weapon.profession]?.label ?? weapon.profession }}
                <span v-if="weapon.damageType" :class="DAMAGE_TYPE_MAP[weapon.damageType]?.color" class="ml-1">
                  · {{ DAMAGE_TYPE_MAP[weapon.damageType]?.label }}
                </span>
              </div>
            </div>

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

        <!-- 技能列表 -->
        <div class="space-y-2">
          <p class="text-[10px] font-bold text-slate-500 tracking-widest uppercase">技能</p>
          <div v-for="skill in profile.skills" :key="skill.id"
            class="flex items-center gap-3 p-2.5 rounded-lg border transition"
            :class="isConflict(skill)
              ? 'border-orange-500/40 bg-orange-500/5'
              : 'border-white/5 bg-white/[0.03] hover:bg-white/[0.06]'">

            <span class="text-[10px] font-black border rounded px-1 shrink-0"
              :class="rarityColors[skill.rareLevel]">
              {{ rarityLabel[skill.rareLevel] }}
            </span>

            <span class="text-[9px] shrink-0 border rounded px-1"
              :class="skill.isPassive
                ? 'text-amber-400 border-amber-500/50 bg-amber-500/10'
                : 'text-slate-500 border-slate-700'">
              {{ skill.isPassive ? '被动' : '主动' }}
            </span>

            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-1.5">
                <span class="text-sm text-white font-medium truncate">{{ skill.name }}</span>
                <span v-if="isConflict(skill)"
                  class="text-[9px] text-orange-400 border border-orange-500/50 rounded px-1 shrink-0">互斥</span>
              </div>
              <div class="text-[10px] text-slate-500">
                {{ PROFESSION_MAP[skill.profession]?.label ?? skill.profession }}
              </div>
            </div>

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

      <!-- Placeholder for other settings -->
      <section class="border border-white/5 rounded-xl p-6 opacity-40">
        <h2 class="text-sm font-bold text-slate-400 mb-1">其他设置</h2>
        <p class="text-xs text-slate-600">音量、显示等设置（占位，尚未实现）</p>
      </section>

    </div>
  </div>
</template>
