<script setup lang="ts">
import { computed } from 'vue';
import { PROFESSION_MAP } from '../../utils/constants';
import type { InformationDto } from '../../types/battle';

const props = defineProps<{ info: InformationDto }>();

// 获取当前登录用户 ID 用于权限判断
const currentUserId = localStorage.getItem('userId');
const isOwner = computed(() => String(props.info.id) === String(currentUserId));

// 经验逻辑
const nextLevel = computed(() => props.info.level + 1);
const expThreshold = computed(() => 50.0 * nextLevel.value * nextLevel.value + 50.0 * nextLevel.value);
const expPercentage = computed(() => Math.min((props.info.exp / expThreshold.value) * 100, 100));

// 三维总和及高度计算
const statsTotal = computed(() => props.info.strength + props.info.agility + props.info.intelligence);
const getHeight = (val: number) => statsTotal.value > 0 ? (val / statsTotal.value * 100) : 0;

// 定义头像点击事件
const handleAvatarClick = () => {
  if (!isOwner.value) return;
  console.log("Open Change Avatar Modal");
  // 此处可触发修改头像的逻辑或 Emit
};
</script>

<template>
  <div class="w-full bg-[#0b1221] border border-slate-800 rounded-3xl p-8 shadow-2xl flex items-center gap-10 text-white relative overflow-hidden">
    
    <div v-if="isOwner" class="absolute top-0 right-0 p-1 px-3 bg-indigo-600 text-[10px] font-bold rounded-bl-xl uppercase tracking-widest">
      Authorized
    </div>

    <div class="flex items-center gap-8 flex-1">
      <div 
        class="relative shrink-0 group"
        :class="isOwner ? 'cursor-pointer' : ''"
        @click="handleAvatarClick"
      >
        <div class="w-32 h-32 rounded-2xl bg-slate-800 border-2 border-slate-700 flex items-center justify-center text-5xl transition-all"
             :class="isOwner ? 'group-hover:border-indigo-500 group-hover:bg-slate-700 shadow-indigo-500/20 shadow-2xl' : ''">
          👤
          <div v-if="isOwner" class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 rounded-2xl flex items-center justify-center transition-opacity">
            <span class="text-[10px] font-bold tracking-tighter">EDIT</span>
          </div>
        </div>
        
        <div class="absolute -bottom-2 -right-2 flex gap-1">
          <div v-if="info.profession" class="w-10 h-10 bg-slate-900 border border-slate-600 rounded-lg flex items-center justify-center shadow-xl">
            {{ PROFESSION_MAP[info.profession]?.icon }}
          </div>
          <div v-if="info.secondProfession" class="w-8 h-8 bg-slate-800 border border-slate-700 rounded-lg flex items-center justify-center shadow-lg scale-90">
            {{ PROFESSION_MAP[info.secondProfession]?.icon }}
          </div>
        </div>
      </div>

      <div class="space-y-4 flex-1">
        <div class="space-y-1">
          <div class="flex items-baseline gap-3">
            <h1 class="text-5xl font-black tracking-tighter">{{ info.name }}</h1>
            <span class="text-xl font-mono text-indigo-400 font-bold">LV.{{ info.level }}</span>
          </div>
          <p class="text-[12px] text-slate-500 font-mono tracking-[0.2em] uppercase">Hero Information</p>
        </div>

        <div v-if="isOwner" class="w-full max-w-xs space-y-2">
          <div class="flex justify-between text-[14px] font-mono text-slate-400">
            <span>EXP: {{ info.exp }}</span>
            <span>NEXT: {{ expThreshold }}</span>
          </div>
          <div class="h-1.5 w-full bg-black/40 rounded-full overflow-hidden border border-white/5">
            <div 
              class="h-full bg-indigo-500 shadow-[0_0_10px_rgba(99,102,241,0.5)] transition-all duration-1000"
              :style="{ width: expPercentage + '%' }"
            ></div>
          </div>
        </div>
      </div>
    </div>

    <div class="flex gap-4 h-48 shrink-0 items-end">
      <div class="w-20 h-full bg-slate-800/20 border border-slate-700/50 rounded-xl p-3 flex flex-col relative">
        <span class="text-[12px] text-slate-500 font-bold uppercase italic text-center">HP</span>
        <div class="flex-1 flex items-center justify-center">
          <span class="text-2xl font-mono text-green-400 font-bold leading-none">{{ info.health }}</span>
        </div>
      </div>

      <div class="flex flex-col items-center gap-3 w-14 h-full">
        <div class="flex-1 w-full bg-black/40 rounded-lg relative overflow-hidden border border-white/5">
          <div class="absolute bottom-0 w-full bg-red-500 transition-all duration-1000 ease-out shadow-[0_0_15px_rgba(239,68,68,0.3)]"
               :style="{ height: getHeight(info.strength) + '%' }"></div>
        </div>
        <div class="text-center">
          <div class="text-[10px] text-red-500 font-bold mb-1">STR</div>
          <div class="font-mono text-lg leading-none">{{ info.strength }}</div>
        </div>
      </div>

      <div class="flex flex-col items-center gap-3 w-14 h-full">
        <div class="flex-1 w-full bg-black/40 rounded-lg relative overflow-hidden border border-white/5">
          <div class="absolute bottom-0 w-full bg-green-500 transition-all duration-1000 ease-out shadow-[0_0_15px_rgba(34,197,94,0.3)]"
               :style="{ height: getHeight(info.agility) + '%' }"></div>
        </div>
        <div class="text-center">
          <div class="text-[10px] text-green-500 font-bold mb-1">AGI</div>
          <div class="font-mono text-lg leading-none">{{ info.agility }}</div>
        </div>
      </div>

      <div class="flex flex-col items-center gap-3 w-14 h-full">
        <div class="flex-1 w-full bg-black/40 rounded-lg relative overflow-hidden border border-white/5">
          <div class="absolute bottom-0 w-full bg-purple-500 transition-all duration-1000 ease-out shadow-[0_0_15px_rgba(168,85,247,0.3)]"
               :style="{ height: getHeight(info.intelligence) + '%' }"></div>
        </div>
        <div class="text-center">
          <div class="text-[10px] text-purple-500 font-bold mb-1">INT</div>
          <div class="font-mono text-lg leading-none">{{ info.intelligence }}</div>
        </div>
      </div>
    </div>
  </div>
</template>