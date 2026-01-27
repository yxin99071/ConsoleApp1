<script setup lang="ts">
/**
 * CardBase 是所有游戏卡片的通用外壳
 * isActive: 控制卡片是否向上浮动（轮到该单位回合）
 * isDead: 控制卡片是否变灰（单位阵亡）
 */
defineProps<{
  isActive?: boolean;
  isDead?: boolean;
}>();
</script>

<template>
  <div 
    class="relative w-52 h-72 rounded-lg border-2 bg-slate-900 flex flex-col transition-all duration-300 shadow-2xl overflow-hidden"
    :class="isActive ? 'translate-y-6 border-yellow-400' : 'border-slate-700'"
  >
    <div class="h-10 bg-slate-800 border-b border-slate-700 flex items-center justify-center relative">
      <slot name="header"></slot>
      <div class="absolute -left-1 -top-1 flex items-center gap-0.5">
        <slot name="profession"></slot>
      </div>
    </div>

    <div class="p-3 flex-1 flex flex-col gap-2">
      <slot></slot>
    </div>
  </div>
</template>

<style scoped>
/* 这里可以添加一些 CSS 原生无法轻易实现的滤镜效果 */
.grayscale {
  filter: grayscale(100%) brightness(0.7);
}
</style>