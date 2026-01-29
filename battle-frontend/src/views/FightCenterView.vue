<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import FightReviewer from '../components/combat/FightReviewer.vue';
// 假设这是你封装好的 API 路径
import { postFight } from '../api/battle'; 

const props = defineProps<{
  battleId?: string;
  battleInitData?: {
    attackerId: string;
    defenderId: string;
  };
}>();

const router = useRouter();

// --- 状态变量 ---
const battleJson = ref<any>(null);    // 存储后端返回的战斗长 JSON
const isLoading = ref(false);        // 加载状态
const isDataError = ref(false);      // 数据错误标记

// --- 核心逻辑：获取战斗数据 ---
const loadBattleData = async () => {
  // 重置状态
  isLoading.value = true;
  isDataError.value = false;
  battleJson.value = null;

  try {
    // 调用封装好的 API
    // 参数 1&2: 直接对战 ID；参数 3: 历史回放 ID
    const res = await postFight(
      props.battleInitData?.attackerId,
      props.battleInitData?.defenderId,
      props.battleId
    );
    if (res) {
      battleJson.value = res;
    } else {
      isDataError.value = true;
    }
  } catch (error) {
    console.error('获取战斗数据失败:', error);
    isDataError.value = true;
  } finally {
    isLoading.value = false;
  }
};

// --- 生命周期与监听 ---

// 1. 初始化检查
onMounted(() => {
  if (props.battleInitData || props.battleId) {
    loadBattleData();
  }
});

// 2. 监听路由参数变化（用于点击右侧历史列表时触发更新）
watch(() => props.battleId, (newId) => {
  if (newId) {
    loadBattleData();
  }
});

// 模拟历史数据，保持你原有的结构
const historyList = [
  { id: '1', title: '森林遭遇战', time: '10:20' },
  { id: '2', title: '竞技场决斗', time: 'Yesterday' },
];
</script>

<template>
  <div class="flex flex-col h-screen bg-slate-950 text-slate-200 overflow-hidden">
    <header class="h-20 border-b border-white/10 flex items-center justify-center bg-slate-900/50 backdrop-blur-md relative">
      <h1 class="text-3xl font-black italic tracking-widest text-transparent bg-clip-text bg-linear-to-r from-blue-400 to-indigo-500">
        战斗中心
      </h1>
      <div class="absolute bottom-0 w-full h-px bg-linear-to-r from-transparent via-blue-500 to-transparent opacity-50"></div>
    </header>

    <main class="flex flex-1 overflow-hidden">
      <section class="flex-1 relative flex items-center justify-center p-6 bg-[radial-gradient(circle_at_center,var(--tw-gradient-stops))] from-slate-900 via-transparent to-transparent">
        
        <div v-if="isLoading" class="flex flex-col items-center gap-4">
          <div class="w-10 h-10 border-2 border-blue-500/20 border-t-blue-500 rounded-full animate-spin"></div>
          <p class="text-blue-400 font-mono text-sm tracking-widest animate-pulse">LOADING COMBAT DATA...</p>
        </div>

        <FightReviewer 
          v-else-if="battleJson" 
          :battleData="battleJson" 
          @close="battleJson = null"
        />

        <div v-else-if="isDataError" class="flex flex-col items-center gap-4 text-rose-500/60">
          <span class="text-4xl">🚫</span>
          <p class="font-bold tracking-widest">战斗信息错误或不存在</p>
        </div>

        <div v-else class="flex flex-col items-center gap-4 opacity-40">
          <span class="text-6xl">⚔️</span>
          <p class="text-xl font-bold tracking-widest">请从右侧选择对局开始</p>
        </div>
      </section>

      <aside class="w-80 border-l border-white/10 bg-black/30 flex flex-col">
        <div class="p-4 border-b border-white/5">
          <button 
            @click="router.push('/lobby')"
            class="w-full py-3 bg-white/5 hover:bg-white/10 border border-white/10 rounded-xl transition-all flex items-center justify-center gap-2 group"
          >
            <span class="group-hover:-translate-x-1 transition-transform">↩</span>
            返回大厅
          </button>
        </div>

        <div class="flex-1 overflow-y-auto p-4 space-y-3 custom-scrollbar">
          <h3 class="text-xs font-bold text-slate-500 uppercase tracking-widest mb-4">历史对局</h3>
          <div 
            v-for="item in historyList" 
            :key="item.id"
            @click="router.push({ name: 'FightCenter', params: { battleId: item.id } })"
            class="p-4 rounded-xl bg-white/5 border border-white/5 hover:border-blue-500/50 cursor-pointer transition-all hover:bg-blue-500/5 group"
          >
            <div class="flex justify-between items-start mb-1">
              <span class="font-bold group-hover:text-blue-400 transition-colors">{{ item.title }}</span>
              <span class="text-[10px] text-slate-500">{{ item.time }}</span>
            </div>
            <p class="text-[10px] text-slate-500 italic">ID: {{ item.id }}</p>
          </div>
        </div>
      </aside>
    </main>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
}

/* 简单的进入淡入效果 */
.v-enter-active, .v-leave-active {
  transition: opacity 0.5s ease;
}
.v-enter-from, .v-leave-to {
  opacity: 0;
}
</style>