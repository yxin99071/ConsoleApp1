<script setup lang="ts">
import { computed } from 'vue';
import type { BuffSummaryDto } from '../../types/battle';

const props = defineProps<{
  buff: BuffSummaryDto // 包含你新加的 isDamage 字段
}>();

// 状态对应的临时文字或占位符（等图标资源到位后替换为 <img>）
const statusIcons = computed(() => [
  { show: props.buff.isBuff, text: '增益', color: 'text-green-500', icon: '⬆️' },
  { show: props.buff.isDeBuff, text: '减益', color: 'text-red-500', icon: '⬇️' },
  { show: props.buff.isDamage, text: '伤害', color: 'text-orange-600', icon: '💥' }
]);
</script>

<template>
  <div class="flex flex-col min-w-max max-w-50 p-2 bg-slate-900/95 backdrop-blur-md rounded-lg border border-slate-700 shadow-2xl ring-1 ring-white/10">
    <div class="flex items-center justify-between gap-4 mb-1">
      <span class="text-[12px] font-black text-white whitespace-nowrap">{{ buff.name }}</span>
      <span class="px-1.5 py-0.5 bg-blue-500/20 rounded text-[10px] text-blue-400 border border-blue-500/30">
        {{ buff.lastRound }}R
      </span>
    </div>
    
    <div class="flex gap-2 mb-1.5">
      <template v-for="item in statusIcons" :key="item.text">
        <div v-if="item.show" :class="['flex items-center gap-1 text-[10px]', item.color]">
          <span>{{ item.icon }}</span>
          <span class="font-bold">{{ item.text }}</span>
        </div>
      </template>
    </div>

    <p class="text-[11px] text-slate-300 leading-relaxed border-t border-white/10 pt-1.5 italic">
      {{ buff.description }}
    </p>
  </div>
</template>