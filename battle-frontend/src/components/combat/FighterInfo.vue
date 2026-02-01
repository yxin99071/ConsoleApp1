<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import gsap from 'gsap';
import type { PlayerBattleInstance } from '../../types/battle';

const props = defineProps<{
  player: PlayerBattleInstance;
  isActing?: boolean; 
}>();

const cardRef = ref<HTMLElement | null>(null);
const hpBarRef = ref<HTMLElement | null>(null);
const hpTextRef = ref<HTMLElement | null>(null);

// 监听血量变化，执行 GSAP 动效
watch(() => props.player.currentHealth, (newVal, oldVal) => {
  if (!hpBarRef.value || !hpTextRef.value) return;

  // 1. 血条平滑滚动
  gsap.to(hpBarRef.value, {
    width: `${(newVal / props.player.maxHealth) * 100}%`,
    duration: 0.6,
    ease: "back.out(1.2)"
  });

  // 2. 数字滚动
  gsap.to(hpTextRef.value, {
    innerText: newVal,
    duration: 0.6,
    snap: { innerText: 1 },
    ease: "power2.out"
  });

  // 3. 受伤抖动反馈
  if (newVal < oldVal && cardRef.value) {
    gsap.fromTo(cardRef.value, { x: -4 }, { x: 0, duration: 0.05, repeat: 5, yoyo: true });
  }
});

onMounted(() => {
  if (cardRef.value) {
    gsap.from(cardRef.value, { y: 20, opacity: 0, duration: 0.8, ease: "expo.out" });
  }
});
</script>

<template>
  <div 
    ref="cardRef"
    class="p-5 rounded-xl border-2 bg-slate-900 shadow-2xl transition-all duration-300"
    :class="[isActing ? 'border-amber-400' : 'border-slate-700']"
  >
    <div class="flex justify-between items-center mb-4">
      <h3 class="text-2xl font-black italic text-white tracking-wider uppercase">{{ player.name }}</h3>
      <span class="text-xs font-mono text-slate-500">HP {{ Math.round((player.currentHealth / player.maxHealth) * 100) }}%</span>
    </div>

    <div class="relative h-8 bg-black/50 rounded-lg p-1 border border-slate-800">
      <div ref="hpBarRef" class="h-full bg-gradient-to-r from-red-600 to-rose-400 rounded-sm shadow-[0_0_10px_rgba(239,68,68,0.3)]"
        :style="{ width: `${(player.currentHealth / player.maxHealth) * 100}%` }"></div>
      <div class="absolute inset-0 flex justify-center items-center text-sm font-black text-white">
        <span ref="hpTextRef">{{ player.currentHealth }}</span> / {{ player.maxHealth }}
      </div>
    </div>

    <div class="mt-6 grid grid-cols-2 gap-4">
      <div>
        <p class="text-[9px] font-bold text-slate-500 uppercase tracking-widest mb-2">Arsenal</p>
        <div class="flex flex-wrap gap-1">
          <div v-for="(_, i) in player.weapons" :key="'w'+i" class="w-2.5 h-3.5 bg-orange-600 rounded-sm transform -skew-x-12"></div>
        </div>
      </div>
      <div>
        <p class="text-[9px] font-bold text-slate-500 uppercase tracking-widest mb-2">Arcane</p>
        <div class="flex flex-wrap gap-1">
          <div v-for="(_, i) in player.skills" :key="'s'+i" class="w-2.5 h-2.5 bg-cyan-500 rounded-full"></div>
        </div>
      </div>
    </div>

    <div class="mt-6 pt-4 border-t border-slate-800 flex flex-wrap gap-2 min-h-[40px]">
      <div v-for="(buff, index) in player.activeBuffs" :key="index" class="relative group">
        <div class="w-9 h-9 rounded-md border flex items-center justify-center text-[10px] font-bold"
          :class="buff.isBuff ? 'bg-emerald-950/40 border-emerald-500 text-emerald-400' : 'bg-rose-950/40 border-rose-500 text-rose-400'">
          {{ buff.name[0] }}
          <span class="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] flex items-center justify-center bg-slate-950 text-white rounded-full text-[9px] border border-slate-700">
            {{ buff.lastRound }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>