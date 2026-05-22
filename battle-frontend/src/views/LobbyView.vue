<script setup lang="ts">
import { ref, onMounted,computed } from 'vue';
import { useRouter } from 'vue-router';
// 保持你提供的 import 路径完全一致
import { getFighters, getProfile } from '../api/battle';
import type { InformationDto, SkillDto } from '../types/battle';
import type { FighterDto } from '../api/battle';
import CharacterCard from '../components/game/CharacterCard.vue';
import ItemCard from '../components/game/ItemCard.vue';
import BackpackView from '../components/game/BackpackView.vue';
import { getAwardCount } from '../api/award';

const router = useRouter();

// 获取当前用户ID (不做响应式，因为登录后不会变)
const currentUserId = localStorage.getItem('userId') || '';

// --- 状态数据 ---
const fighters = ref<FighterDto[]>([]);
const myProfile = ref<InformationDto | null>(null);
const targetProfile = ref<InformationDto | null>(null);
const isTargetLoading = ref(false);

// --- 背包 ---
const showBackpack = ref(false);
const pendingAwardCount = ref(0);

async function refreshAwardCount() {
  try {
    pendingAwardCount.value = await getAwardCount();
  } catch {
    pendingAwardCount.value = 0;
  }
}

function handleBackpackClose(refreshed: boolean) {
  showBackpack.value = false;
  if (refreshed) refreshAwardCount();
}

// --- 初始化 ---
onMounted(async () => {
  try {
    const [list, me] = await Promise.all([getFighters(), getProfile()]);
    fighters.value = list;
    myProfile.value = me;
  } catch (err) {
    console.error("初始化大厅失败", err);
  }
  await refreshAwardCount();
});

// --- 核心修复：影子判断与开关逻辑 ---
const handleSelectFighter = async (fighter: FighterDto) => {
  // 1. 再次点击取消逻辑 (Toggle Logic)

  console.log("1：", targetProfile.value ? targetProfile.value.id : "null");
  console.log("self" + String(fighter.id))

  // 使用 String() 确保 数字类型id 和 字符串类型id 能够正确比对
  if (targetProfile.value && String(targetProfile.value.id) === String(fighter.id)) {
    console.log("收起/取消选中:", fighter.name);

    targetProfile.value = null;
    return;
  }

  // 2. 选中新目标逻辑
  console.log("选中目标:", fighter.name);
  isTargetLoading.value = true;
  // 先置空，确保动画状态重置
  targetProfile.value = null;

  try {
    const data = await getProfile(fighter.id.toString());
    targetProfile.value = data;
  } catch (err) {
    console.error("加载对手失败", err);
  } finally {
    isTargetLoading.value = false;
  }
};

const handlePK = () => {
  if (!myProfile.value || !targetProfile.value) return;
  
  router.push({
    name: 'FightCenter',
    // 使用 state 传递关键参数，刷新页面后 state 会变空
    state: { 
      battleInitData: {
        attackerId: myProfile.value.id,
        defenderId: targetProfile.value.id,
        timestamp: Date.now()
      } 
    }
  });
};

// 辅助：技能排序
const getSortedSkills = (skills: SkillDto[] | undefined) => {
  if (!skills) return [];
  // 假设 isPassive 是 boolean 或 0/1，此处做通用处理
  return [...skills].sort((a, b) => Number(!!b.isPassive) - Number(!!a.isPassive));
};

// 辅助：影子判断 (封装成函数更稳定)
const isShadow = (id: string | number) => {
  return String(id) === String(currentUserId);
};
const sortedFighters = computed(() => {
  return [...fighters.value].sort((a, b) => {
    const isANpc = a.name.startsWith('NPC__') && a.name.endsWith('__NPC');
    const isBNpc = b.name.startsWith('NPC__') && b.name.endsWith('__NPC');

    // NPC 优先级更高，排在前面
    if (isANpc && !isBNpc) return -1;
    if (!isANpc && isBNpc) return 1;
    return 0;
  });
});

// 辅助方法：格式化名称
const formatName = (name: string) => {
  if (name.startsWith('NPC__') && name.endsWith('__NPC')) {
    return name.replace('NPC__', '').replace('__NPC', '');
  }
  return name;
};

// 辅助方法：判断是否是 NPC
const isNpc = (name: string) => name.startsWith('NPC__') && name.endsWith('__NPC');
</script>

<template>
  <div class="h-screen w-full flex bg-[#020617] text-slate-200 overflow-hidden font-sans">

    <aside class="w-80 border-r border-white/5 bg-slate-900/60 flex flex-col shrink-0 backdrop-blur-sm">
      <div class="p-6 border-b border-white/5">
        <h2 class="text-xs font-black text-indigo-400 tracking-[0.2em] uppercase">Battle Hall</h2>
      </div>

      <div class="flex-1 overflow-y-auto custom-scrollbar p-3 space-y-2">
        <div v-for="f in sortedFighters" :key="f.id" @click="handleSelectFighter(f)"
          class="relative p-4 rounded-lg border transition-all cursor-pointer group flex items-center gap-3 overflow-hidden select-none"
          :class="[
            isShadow(f.id)
              ? 'bg-slate-950 border-slate-800 text-slate-600 grayscale opacity-75'
              : isNpc(f.name)
                ? 'bg-amber-500/5 border-amber-500/20 hover:bg-amber-500/10 hover:border-amber-500/40' // NPC 特殊色（琥珀色）
                : 'bg-white/5 border-transparent hover:bg-white/10 hover:border-indigo-500/30',

            targetProfile && String(targetProfile.id) === String(f.id) ? 'border-red-500 bg-red-900/10' : ''
          ]">
          <div v-if="targetProfile && String(targetProfile.id) === String(f.id)"
            class="absolute left-0 top-0 bottom-0 w-1 bg-red-500"></div>

          <div class="flex-1">
            <div class="font-bold text-sm flex items-center gap-2">
              <span :class="isNpc(f.name) ? 'text-amber-400' : ''">{{ formatName(f.name) }}</span>

              <span v-if="isShadow(f.id)"
                class="text-[10px] bg-slate-800 px-1 rounded text-slate-500 border border-slate-700">(影子)</span>
              <span v-if="isNpc(f.name)"
                class="text-[9px] bg-amber-500/20 px-1 rounded text-amber-500 border border-amber-500/30 uppercase tracking-tighter">Boss</span>
            </div>
            <div class="text-[10px] opacity-60 font-mono mt-1">LV.{{ f.level }} • {{ f.profession }}</div>
          </div>

          <div v-if="targetProfile && String(targetProfile.id) === String(f.id)"
            class="text-red-500 font-black text-xs animate-pulse">VS</div>
          <div v-else-if="isTargetLoading && !targetProfile && !isShadow(f.id)"
            class="w-3 h-3 border-2 border-indigo-500 border-t-transparent rounded-full animate-spin"></div>
        </div>
      </div>
    </aside>
    <main class="flex-1 overflow-y-auto custom-scrollbar relative bg-main-pattern">
      <div v-if="myProfile" class="w-full py-12 px-12 flex flex-col gap-16 pb-32">

        <section>
          <div class="relative z-20">
            <div class="flex items-center gap-4 mb-4">
              <div class="w-2 h-8 bg-indigo-500"></div>
              <h3 class="font-black text-2xl text-white tracking-tighter">PROFILE</h3>
            </div>
            <CharacterCard :info="myProfile" class="w-full shadow-2xl" />
          </div>

          <Transition name="drawer">
            <div v-if="targetProfile" class="relative z-10 overflow-hidden">
              <div class="flex items-center justify-center py-6 relative">
                <div class="h-px bg-linear-to-r from-transparent via-red-500/50 to-transparent w-full absolute"></div>
                <span
                  class="bg-[#0f172a] px-4 text-red-500 font-black italic text-xl relative z-10 border border-red-900/50 rounded-full">VS</span>
              </div>
              <CharacterCard :info="targetProfile" class="w-full shadow-red-900/20 border-red-500/30"
                :class="{ 'grayscale opacity-80': isShadow(targetProfile.id) }" />
            </div>
          </Transition>
        </section>

        <section>
          <div class="relative z-20 bg-[#020617]/80 backdrop-blur-sm py-4">
            <div class="flex items-center gap-4 mb-4">
              <div class="w-2 h-8 bg-indigo-400"></div>
              <h3 class="font-black text-2xl text-white tracking-tighter">SKILLS</h3>
            </div>
            <div class="flex flex-wrap gap-4">
              <ItemCard v-for="s in getSortedSkills(myProfile.skills)" :key="s.name" :item="s" type="技能" />
            </div>
          </div>

          <Transition name="drawer">
            <div v-if="targetProfile" class="relative z-10 overflow-hidden">
              <div class="flex items-center justify-center py-8">
                <div class="h-px bg-slate-800 w-full"></div>
                <span class="absolute text-red-800/40 font-black text-4xl italic select-none">VERSUS</span>
              </div>
              <div class="flex flex-wrap gap-4 p-6 rounded-xl border border-red-900/20 bg-red-950/10">
                <ItemCard v-for="s in getSortedSkills(targetProfile.skills)" :key="s.name" :item="s" type="技能" />
              </div>
            </div>
          </Transition>
        </section>

        <section>
          <div class="relative z-20 bg-[#020617]/80 backdrop-blur-sm py-4">
            <div class="flex items-center gap-4 mb-4">
              <div class="w-2 h-8 bg-indigo-300"></div>
              <h3 class="font-black text-2xl text-white tracking-tighter">ARSENAL</h3>
            </div>
            <div class="flex flex-wrap gap-4">
              <ItemCard v-for="w in myProfile.weapons" :key="w.name" :item="w" type="武器" />
            </div>
          </div>

          <Transition name="drawer">
            <div v-if="targetProfile" class="relative z-10 overflow-hidden">
              <div class="flex items-center justify-center py-8">
                <div class="h-px bg-slate-800 w-full"></div>
                <span class="absolute text-red-800/40 font-black text-4xl italic select-none">VERSUS</span>
              </div>
              <div class="flex flex-wrap gap-4 p-6 rounded-xl border border-red-900/20 bg-red-950/10">
                <ItemCard v-for="w in targetProfile.weapons" :key="w.name" :item="w" type="武器" />
              </div>
            </div>
          </Transition>
        </section>

      </div>
    </main>

    <aside class="w-24 border-l border-white/5 bg-slate-900 flex flex-col items-center py-8 gap-6 shrink-0 z-30">

      <!-- 背包按钮（带奖励角标） -->
      <button @click="showBackpack = true" title="背包"
              class="relative w-12 h-12 rounded-full bg-white/5 hover:bg-indigo-600 hover:text-white
                     transition-all text-xl flex items-center justify-center border border-white/10">
        🎒
        <span v-if="pendingAwardCount > 0"
              class="absolute -top-1 -right-1 min-w-[18px] h-[18px] px-1
                     bg-red-500 rounded-full text-white text-[10px] font-black
                     flex items-center justify-center leading-none shadow-lg">
          {{ pendingAwardCount > 9 ? '9+' : pendingAwardCount }}
        </span>
      </button>

      <!-- 历史对局按钮 -->
      <button @click="router.push({ name: 'FightCenter' })" title="历史对局"
              class="w-12 h-12 rounded-full bg-white/5 hover:bg-indigo-600 hover:text-white
                     transition-all text-xl flex items-center justify-center border border-white/10">
        🏆
      </button>

      <!-- 设置（占位） -->
      <button title="设置"
              class="w-12 h-12 rounded-full bg-white/5 hover:bg-indigo-600 hover:text-white
                     transition-all text-xl flex items-center justify-center border border-white/10">
        ⚙️
      </button>

      <div class="mt-auto"></div>

      <Transition name="pop">
        <button v-if="targetProfile" @click="handlePK"
          class="w-16 h-16 rounded-full bg-red-600 text-white shadow-[0_0_30px_rgba(220,38,38,0.5)] flex items-center justify-center hover:scale-110 active:scale-90 transition-all group relative">
          <span class="font-black italic text-xl z-10">PK</span>
          <div class="absolute inset-0 rounded-full bg-red-600 animate-ping opacity-75"></div>
        </button>
      </Transition>
    </aside>

  </div>

  <!-- 背包弹层 -->
  <BackpackView v-if="showBackpack" @close="handleBackpackClose" />
</template>
<style scoped>
/* 背景纹理 */
.bg-main-pattern {
  background-image:
    linear-gradient(to bottom, rgba(2, 6, 23, 0.9), rgba(2, 6, 23, 0.95)),
    url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%231e293b' fill-opacity='0.2'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
}

/* 核心动画：抽屉式展开 (Drawer Slide) */
/* 使用 max-height 实现高度动画，需要设一个足够大的值 */
.drawer-enter-active,
.drawer-leave-active {
  transition: all 0.6s cubic-bezier(0.25, 0.8, 0.25, 1);
  max-height: 1000px;
  /* 足够容纳内容 */
  opacity: 1;
  transform: translateY(0);
}

.drawer-enter-from,
.drawer-leave-to {
  max-height: 0;
  opacity: 0;
  transform: translateY(-20px);
  /* 稍微向上收起，营造钻入效果 */
  margin-top: 0;
  padding-top: 0;
  padding-bottom: 0;
}

/* PK 按钮弹出动画 */
.pop-enter-active {
  animation: pop-in 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.pop-leave-active {
  transition: all 0.3s ease;
  opacity: 0;
  transform: scale(0);
}

@keyframes pop-in {
  0% {
    transform: scale(0);
    opacity: 0;
  }

  100% {
    transform: scale(1);
    opacity: 1;
  }
}

/* 滚动条美化 */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.2);
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #334155;
  border-radius: 10px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #475569;
}
</style>