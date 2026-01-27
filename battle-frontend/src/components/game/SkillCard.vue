<script setup lang="ts">
import type { SkillDto } from '../../types/battle';
import CardBase from './CardBase.vue';
import BuffIcon from './BuffIcon.vue';

// 接收技能数据和激活状态
defineProps<{
  skill: SkillDto;
  isActive?: boolean;
}>();
</script>

<template>
  <CardBase :is-active="isActive" class="skill-card-theme">
    
    <div class="relative w-full h-24 bg-indigo-950/50 rounded-lg flex items-center justify-center border border-indigo-500/30 overflow-hidden">
      <span class="text-4xl">✨</span> 
      <div class="absolute top-1 left-1 px-1.5 py-0.5 bg-black/60 rounded text-[10px] text-indigo-300 border border-indigo-500/20">
        {{ skill.profession }}
      </div>
    </div>

    <div class="mt-2 flex-1">
      <h3 class="text-base font-bold text-white truncate">{{ skill.name }}</h3>
      <p class="mt-1 text-[11px] text-slate-400 leading-snug line-clamp-4">
        {{ skill.description }}
      </p>
    </div>

    <div class="mt-auto pt-2 border-t border-slate-700/50">
      <div class="flex flex-wrap gap-1">
        <BuffIcon 
          v-for="(b, i) in skill.buffs" 
          :key="i" 
          :buff="b" 
        />
        <span v-if="!skill.buffs.length" class="text-[10px] text-slate-600 italic">瞬发伤害</span>
      </div>
    </div>

  </CardBase>
</template>

<style>
/* 技能卡特有的渐变背景 */
.skill-card-theme {
  background: linear-gradient(135deg, #1e293b 0%, #1e1b4b 100%);
}
</style>