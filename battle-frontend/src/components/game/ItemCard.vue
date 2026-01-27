<script setup lang="ts">
import { computed } from 'vue';
import { PROFESSION_MAP } from '../../utils/constants';

interface BuffSummaryDto {
  name: string;
  isBuff: boolean;
  isDeBuff: boolean;
  isDamage: boolean;
  lastRound: number;
  description: string;
}

interface ItemDto {
  name: string;
  profession: string;
  secondProfession?: string;
  description: string;
  buffs: BuffSummaryDto[];
  isPassive?: boolean; // 已修正为 boolean
}

const props = defineProps<{
  item: ItemDto;
  type: '技能' | '武器';
  isActive?: boolean;
}>();

// 核心逻辑判断
const isPassiveSkill = computed(() => props.type === '技能' && props.item.isPassive === true);
const isWeapon = computed(() => props.type === '武器');

// 1. 整体皮肤计算
const themeClasses = computed(() => {
  if (isPassiveSkill.value) {
    // 银色：被动技能
    return 'border-slate-400 bg-gradient-to-br from-slate-800 via-slate-900 to-slate-950 shadow-slate-900/50';
  }
  if (isWeapon.value) {
    // 红色：武器
    return 'border-red-600/50 bg-gradient-to-br from-slate-900 via-slate-950 to-[#450a0a]/40 shadow-red-900/30';
  }
  // 金色：普通主动技能
  return 'border-amber-600/50 bg-gradient-to-br from-slate-900 via-slate-950 to-[#451a03]/30 shadow-amber-900/40';
});

// 2. 文字与标签颜色计算
const accentColor = computed(() => {
  if (isPassiveSkill.value) return 'text-slate-200';
  if (isWeapon.value) return 'text-red-400';
  return 'text-amber-400';
});

const tagClass = computed(() => {
  if (isPassiveSkill.value) return 'bg-slate-700 border-slate-500 text-slate-300';
  if (isWeapon.value) return 'bg-red-900/50 border-red-700 text-red-500';
  return 'bg-amber-900/50 border-amber-600 text-amber-500';
});
</script>

<template>
  <div 
    class="relative w-52 h-72 rounded-xl border-2 transition-all duration-300 shadow-2xl flex flex-col overflow-hidden"
    :class="[themeClasses, isActive ? '-translate-y-4 ring-2 ring-white/30' : '']"
  >
    <div class="h-10 border-b border-white/10 flex items-center justify-center relative bg-black/20">
      <span class="font-bold text-sm tracking-wide truncate px-6" :class="accentColor">
        {{ item.name }}
      </span>

      <div class="absolute -left-1 -top-1 flex items-start">
        <div class="bg-slate-800 border border-slate-600 rounded-br-lg p-1.5 shadow-lg z-10">
          <span class="text-xs">{{ PROFESSION_MAP[item.profession]?.icon || '❓' }}</span>
        </div>
        <div v-if="item.secondProfession" 
             class="bg-slate-900/90 border border-slate-700 rounded-br-md p-1 mt-1 -ml-1 scale-90 shadow-md">
          <span class="text-[13px]">{{ PROFESSION_MAP[item.secondProfession]?.icon }}</span>
        </div>
      </div>
    </div>

    <div class="p-3 flex-1 flex flex-col gap-3">
      <div class="w-full h-20 bg-black/40 rounded border border-white/5 flex items-center justify-center">
        <span class="text-2xl opacity-40">
          {{ isWeapon ? '⚔️' : (isPassiveSkill ? '💎' : '🪄') }}
        </span>
      </div>

      <div class="flex justify-center">
        <span class="px-3 py-0.5 rounded-full text-[13px] border font-bold uppercase tracking-tighter" :class="tagClass">
          {{ isPassiveSkill ? 'PASSIVE' : type }}
        </span>
      </div>

      <p class="text-[14px] leading-relaxed text-slate-400 line-clamp-5 italic">
        {{ item.description }}
      </p>

      <div v-if="item.buffs?.length" class="mt-auto flex flex-wrap gap-1 pt-2 border-t border-white/5">
        <div v-for="buff in item.buffs" :key="buff.name" 
             class="w-4 h-4 rounded-sm bg-slate-800 border border-white/10 flex items-center justify-center"
             :title="buff.name">
          <span class="text-[11px]">{{ buff.isDeBuff ? '💢' : '✨' }}</span>
        </div>
      </div>
    </div>
  </div>
</template>