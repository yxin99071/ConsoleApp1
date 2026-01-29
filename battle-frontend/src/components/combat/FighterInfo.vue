<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { gsap } from 'gsap';
import type { BuffSummaryDto } from '../../types/battle';
import BuffIcon from '../game/BuffIcon.vue';

/**
 * 状态定义：
 * idle: 静止
 * active: 当前行动回合（卡片浮起）
 * attack: 发起攻击（后坐力动画）
 * hit: 受到伤害（抖动动画）
 * dodge: 闪避（侧移动画）
 */
export type FighterStatus = 'idle' | 'active' | 'attack' | 'hit' | 'dodge';

interface Props {
  name: string;
  side: 'left' | 'right';
  currentHp: number;
  maxHp: number;
  weaponCount: number;
  skillCount: number;
  buffs: BuffSummaryDto[];
  status: FighterStatus;
}

const props = withDefaults(defineProps<Props>(), {
  status: 'idle',
});

// 核心 DOM 引用，用于 GSAP 动画执行
const cardRef = ref<HTMLElement | null>(null);

// 1. 血条百分比计算
const hpPercentage = computed(() => {
  const p = (props.currentHp / props.maxHp) * 100;
  return Math.max(0, Math.min(100, p));
});

// 2. 根据血量反馈视觉状态（Cloudwind 风格色彩）
const hpStatusColor = computed(() => {
  if (hpPercentage.value > 60) return 'bg-emerald-500 shadow-[0_0_10px_rgba(16,185,129,0.4)]';
  if (hpPercentage.value > 25) return 'bg-amber-500 shadow-[0_0_10px_rgba(245,158,11,0.4)]';
  return 'bg-rose-500 shadow-[0_0_10px_rgba(244,63,94,0.6)]';
});

/**
 * 3. GSAP 动画状态调度
 * 监听 status 的变化，触发不同的 GSAP 补间动画
 */
watch(() => props.status, (newStatus) => {
  if (!cardRef.value) return;

  const isLeft = props.side === 'left';
  const xDirection = isLeft ? -1 : 1; // 决定物理反馈的方向

  switch (newStatus) {
    case 'active':
      // 浮起动画
      gsap.to(cardRef.value, { y: -25, duration: 0.4, ease: "back.out(1.7)" });
      break;

    case 'attack':
      // 后坐力动画：快速向后缩，然后弹回
      gsap.timeline()
        .to(cardRef.value, { 
          x: 20 * xDirection, 
          rotation: 5 * xDirection, 
          duration: 0.1, 
          ease: "power2.out" 
        })
        .to(cardRef.value, { x: 0, rotation: 0, duration: 0.4, ease: "elastic.out(1, 0.3)" });
      break;

    case 'hit':
      // 受击动画：剧烈左右抖动
      gsap.fromTo(cardRef.value, 
        { x: -8 }, 
        { x: 8, duration: 0.05, repeat: 5, yoyo: true, onComplete: () => {
            gsap.to(cardRef.value, { x: 0, duration: 0.2 });
        }});
      break;

    case 'dodge':
      // 闪避动画：向外侧快速平移并淡出，再回来
      gsap.timeline()
        .to(cardRef.value, { x: -60 * xDirection, opacity: 0.4, duration: 0.2, ease: "power2.in" })
        .to(cardRef.value, { x: 0, opacity: 1, duration: 0.3, ease: "power2.out", delay: 0.1 });
      break;

    case 'idle':
      // 回到初始位置
      gsap.to(cardRef.value, { y: 0, x: 0, rotation: 0, opacity: 1, duration: 0.3 });
      break;
  }
});
</script>

<template>
  <div 
    ref="cardRef"
    class="relative flex flex-col w-72 p-5 bg-slate-900/80 border border-white/10 rounded-3xl backdrop-blur-xl shadow-2xl select-none"
    :class="side === 'right' ? 'items-end' : 'items-start'"
  >
    <div 
      class="w-full flex items-end justify-between mb-3 px-1"
      :class="side === 'right' ? 'flex-row-reverse' : 'flex-row'"
    >
      <h2 class="text-xl font-black italic tracking-tighter text-white uppercase leading-none">
        {{ name }}
      </h2>
      <div class="font-mono text-sm font-bold tabular-nums">
        <span :class="hpPercentage < 25 ? 'text-rose-500 animate-pulse' : 'text-slate-300'">
          {{ currentHp }}
        </span>
        <span class="text-slate-600 text-xs ml-0.5">/ {{ maxHp }}</span>
      </div>
    </div>

    <div class="w-full h-2.5 bg-slate-800/50 rounded-full overflow-hidden border border-black/20 mb-5">
      <div 
        class="h-full transition-all duration-700 ease-out"
        :class="[hpStatusColor, side === 'right' ? 'float-right' : '']"
        :style="{ width: `${hpPercentage}%` }"
      ></div>
    </div>

    <div class="relative w-full aspect-[4/3] bg-gradient-to-br from-slate-800 to-slate-900 rounded-2xl overflow-hidden border border-white/5 mb-5 shadow-inner group">
      <div 
        class="w-full h-full flex items-center justify-center text-7xl transition-transform duration-700 group-hover:scale-110"
        :class="side === 'right' ? '-scale-x-100' : ''"
      >
        {{ side === 'left' ? '⚔️' : '🧙' }}
      </div>

      <div 
        class="absolute bottom-3 flex gap-2 px-3 w-full"
        :class="side === 'right' ? 'flex-row-reverse' : 'flex-row'"
      >
        <div class="flex items-center gap-1.5 px-2.5 py-1 bg-black/40 backdrop-blur-md rounded-lg border border-white/10">
          <span class="text-xs">武器</span>
          <span class="text-sm font-mono font-bold text-amber-400">{{ weaponCount }}</span>
        </div>
        <div class="flex items-center gap-1.5 px-2.5 py-1 bg-black/40 backdrop-blur-md rounded-lg border border-white/10">
          <span class="text-xs">技能</span>
          <span class="text-sm font-mono font-bold text-cyan-400">{{ skillCount }}</span>
        </div>
      </div>
    </div>

    <div 
      class="w-full flex flex-wrap gap-2 min-h-[36px]"
      :class="side === 'right' ? 'flex-row-reverse' : 'flex-row'"
    >
      <TransitionGroup 
        name="buff-anim"
        @before-leave="(el) => (el as HTMLElement).style.position = 'absolute'"
      >
        <BuffIcon 
          v-for="b in buffs" 
          :key="b.name" 
          :buff="b" 
        />
      </TransitionGroup>
      
      <div v-if="buffs.length === 0" class="flex items-center gap-2 opacity-20 grayscale">
        <div class="w-6 h-6 rounded border border-dashed border-white"></div>
        <span class="text-[10px] tracking-widest uppercase">No Active Buffs</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Buff 进出场动画逻辑 */
.buff-anim-enter-active,
.buff-anim-leave-active {
  transition: all 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.buff-anim-enter-from {
  opacity: 0;
  transform: scale(0) translateY(10px);
}
.buff-anim-leave-to {
  opacity: 0;
  transform: scale(1.5) blur(4px);
}
/* 平滑移动其他 Buff 占位 */
.buff-anim-move {
  transition: transform 0.4s ease;
}
</style>