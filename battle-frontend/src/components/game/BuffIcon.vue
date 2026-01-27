<script setup lang="ts">
import { computed } from 'vue';
import type { BuffSummaryDto } from '../../types/battle';

const props = defineProps<{
  buff: BuffSummaryDto & { isDamage: boolean } // 包含你新加的 isDamage 字段
}>();

// 状态对应的临时文字或占位符（等图标资源到位后替换为 <img>）
const statusIcons = computed(() => [
  { show: props.buff.isBuff, text: '增益', color: 'text-green-500', icon: '⬆️' },
  { show: props.buff.isDeBuff, text: '减益', color: 'text-red-500', icon: '⬇️' },
  { show: props.buff.isDamage, text: '伤害', color: 'text-orange-600', icon: '💥' }
]);
</script>

<template>
  <div class="group relative flex items-center p-1 bg-slate-800/50 rounded border border-slate-700 hover:bg-slate-700 transition-colors">
    <span class="text-xs font-bold text-slate-200 mr-2">{{ buff.name }}</span>

    <div class="flex gap-1">
      <template v-for="item in statusIcons" :key="item.text">
        <span v-if="item.show" :title="item.text" :class="['text-[10px]', item.color]">
          {{ item.icon }}
        </span>
      </template>
    </div>

    <div class="ml-2 px-1 bg-slate-900 rounded text-[10px] text-blue-400">
      {{ buff.lastRound }}R
    </div>

    <div class="invisible group-hover:visible absolute bottom-full left-0 mb-2 w-32 p-2 bg-black text-white text-[10px] rounded shadow-lg z-10">
      {{ buff.description }}
    </div>
  </div>
</template>