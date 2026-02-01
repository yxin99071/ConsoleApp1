<template>
  <div class="fight-reviewer min-h-screen bg-slate-950 p-8 text-slate-200">
    <button @click="$emit('close')" class="absolute top-4 right-4 text-slate-500 hover:text-white transition-colors">
      ✕ CLOSE
    </button>

    <div v-if="players.length >= 2" class="max-w-6xl mx-auto">
      <div class="flex justify-between items-start gap-12 mb-10">
        <FighterInfo 
          v-if="players[0]"
          :player="players[0]" 
          :is-acting="currentActor === players[0].name"
          class="flex-1"
        />

        <div class="self-center text-5xl font-black italic text-amber-500">VS</div>

        <FighterInfo 
          v-if="players[1]"
          :player="players[1]" 
          :is-acting="currentActor === players[1].name"
          class="flex-1"
        />
      </div>

      <div class="grid grid-cols-3 gap-6">
        <div class="col-span-1 bg-slate-900 border border-slate-800 rounded-xl p-4 h-80 flex flex-col">
          <h4 class="text-[10px] font-bold text-amber-500 uppercase mb-3">Debug: Buff Pool</h4>
          <div class="flex-1 overflow-y-auto font-mono text-[10px] space-y-2 custom-scrollbar">
            <div v-for="[id, buff] in buffPool" :key="id" class="p-2 bg-black/40 rounded">
              <div class="flex justify-between">
                <span class="text-amber-400">#{{ id }}</span>
                <span>{{ buff.name }}</span>
              </div>
            </div>
          </div>
        </div>

        <div class="col-span-2 bg-slate-900 border border-slate-800 rounded-xl p-4 h-80 flex flex-col">
          <h4 class="text-[10px] font-bold text-slate-500 uppercase mb-3">Combat Logs</h4>
          <div class="flex-1 overflow-y-auto space-y-2 text-sm custom-scrollbar">
            <div v-for="(log, i) in logs" :key="i" class="text-slate-400">
              <span class="text-slate-700 mr-2">[{{ i + 1 }}]</span> {{ log }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import FighterInfo from './FighterInfo.vue';
import type { PlayerBattleInstance } from '../../types/battle';
import { buildStaticResourcePool , initPlayerState} from '../../engine/fightDataInitApi';
const props = defineProps<{
  battleData: any; // 接收父组件传入的完整 JSON
}>();

const emit = defineEmits(['close']);

const players = ref<PlayerBattleInstance[]>([]);
const buffPool = ref(new Map());
const logs = ref<string[]>([]);
const currentActor = ref<string | null>(null);

// 核心初始化逻辑
const syncData = (data: any) => {
  console.log(data);
  if (!data) return;
  
  try {
    const { buffPool: pool } = buildStaticResourcePool(data);
    buffPool.value = pool;
    players.value = initPlayerState(data);
    
    logs.value = ["Data received from parent.", `Loaded ${pool.size} buffs.`];
    console.log(buffPool,players)
  } catch (err) {
    console.error("Parse Error:", err);
  }
};

// 监听数据变化（支持多次打开不同的战斗记录）
watch(() => props.battleData, (newData) => {
  syncData(newData);
}, { immediate: true });
</script>