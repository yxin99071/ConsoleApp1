<script setup lang="ts">
import CardBase from './CardBase.vue';
// 假设你的 WeaponDto 包含 name, type, atk, description 等
const props = defineProps<{
  weapon: any; 
  isActive?: boolean;
}>();
</script>

<template>
  <CardBase :is-active="isActive" class="weapon-card-theme">
    <div class="w-full h-24 bg-amber-900/20 rounded-lg flex items-center justify-center border border-amber-600/30">
      <span class="text-4xl">⚔️</span>
    </div>

    <div class="mt-2 flex items-center justify-between border-b border-amber-600/20 pb-1">
      <span class="text-xs text-amber-500 font-bold uppercase tracking-widest">{{ weapon.type || '武器' }}</span>
      <span class="text-sm font-mono text-white">ATK: {{ weapon.atk || 0 }}</span>
    </div>

    <div class="mt-2 flex-1">
      <h3 class="text-base font-bold text-white truncate">{{ weapon.name }}</h3>
      <p class="mt-1 text-[11px] text-slate-400 leading-snug line-clamp-3">
        {{ weapon.description }}
      </p>
    </div>
  </CardBase>
</template>

<style scoped>
.weapon-card-theme {
  background: linear-gradient(135deg, #1e293b 0%, #451a03 100%);
}
</style>