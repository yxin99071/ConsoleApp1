<script setup lang="ts">
import { ref } from 'vue';
import ItemCard from '../components/game/ItemCard.vue';
import type { ItemDto } from '../types/battle';
import type { InformationDto } from '../types/battle';
import CharacterCard from '../components/game/CharacterCard.vue';

// 1. 模拟数据：双职业法师技能
const magicSkill = ref<ItemDto>({
  name: "奥术洪流",
  profession: "MAGICIAN",
  secondProfession: "RANGER", // 演示主法副敏
  description: "引导纯净的奥术能量对随机目标进行 5 次打击。由于结合了游侠的直觉，每次打击都有更高概率命中弱点。",
  buffs: [
    { name: "魔力流失", isBuff: false, isDeBuff: true, isDamage: false, lastRound: 2, description: "每回合减少蓝量" }
  ]
});

// 2. 模拟数据：单职业战士武器
const warriorWeapon = ref<ItemDto>({
  name: "巨人之傲",
  profession: "WARRIOR",
  description: "重达百斤的双手巨剑。虽然挥舞缓慢，但每一次重击都能让大地颤抖。凡人无法窥视其全貌。",
  buffs: [
    { name: "威压", isBuff: true, isDeBuff: false, isDamage: false, lastRound: 3, description: "提升自身护甲" }
  ]
});

const heroData = ref<InformationDto>({
  id:'1',
  name: "艾德温",
  level: 28,
  exp: 450,
  health: 480,
  strength: 24,
  agility: 35,
  intelligence: 18,
  profession: "RANGER",
  secondProfession: "WARRIOR",
  // 即使这里有列表，CharacterCard 也可以只读基础信息部分
  skills: [
    { name: "奥术洪流", profession: "MAGICIAN", description: "打击随机目标...",isPassive:true, buffs: [] }
  ],
  weapons: [
    { name: "新手长弓", profession: "RANGER", description: "普通的木弓", buffs: [] }
  ]
});


// 状态控制
const activeCard = ref<string | null>(null);
</script>

<template>
    <div class="p-10 bg-slate-950 min-h-screen space-y-12">
    <CharacterCard :info="heroData" />

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <section class="space-y-4">
        <h3 class="text-indigo-400 font-bold border-b border-indigo-900 pb-2">技能列表</h3>
        <div class="flex flex-wrap gap-6">
          <ItemCard 
            v-for="s in heroData.skills" 
            :key="s.name" 
            :item="s" 
            type="技能" 
            />
        </div>
      </section>

      <section class="space-y-4">
        <h3 class="text-amber-500 font-bold border-b border-amber-900 pb-2">武器库存</h3>
        <div class="flex flex-wrap gap-6">
          <ItemCard 
            v-for="w in heroData.weapons" 
            :key="w.name" 
            :item="w" 
            type="武器" 
          />
        </div>
      </section>
    </div>
  </div>
  <div class="min-h-screen bg-slate-950 p-12 text-white">
    <header class="mb-12 border-l-4 border-indigo-500 pl-4">
      <h1 class="text-2xl font-bold">ItemCard 职业规范预览</h1>
      <p class="text-slate-500 text-sm italic">测试主副职业图标展示与布局</p>
    </header>

    <div class="flex flex-wrap gap-12 items-end justify-center">
      
      <div class="flex flex-col items-center gap-4">
        <span class="text-[10px] font-mono text-purple-400">MAGICIAN + RANGER</span>
        <ItemCard 
          :item="magicSkill" 
          type="技能" 
          :isActive="activeCard === 'skill'"
          @click="activeCard = activeCard === 'skill' ? null : 'skill'"
          class="cursor-pointer"
        />
      </div>

      <div class="flex flex-col items-center gap-4">
        <span class="text-[10px] font-mono text-red-400">WARRIOR ONLY</span>
        <ItemCard 
          :item="warriorWeapon" 
          type="武器" 
          :isActive="activeCard === 'weapon'"
          @click="activeCard = activeCard === 'weapon' ? null : 'weapon'"
          class="cursor-pointer"
        />
      </div>

    </div>

    <div class="mt-20 p-4 bg-slate-900/50 rounded-lg border border-slate-800 max-w-2xl mx-auto">
      <h3 class="text-xs font-bold text-slate-500 mb-2 uppercase">布局检查清单：</h3>
      <ul class="text-[11px] text-slate-400 space-y-1">
        <li>✅ 左上角主职业图标是否带有明显的背景块？</li>
        <li>✅ 副职业图标是否略小且错位叠加在下方？</li>
        <li>✅ 卡片名称是否在独立的顶部横幅区域？</li>
        <li>✅ 点击卡片时，黄色边框与向上位移是否同步？</li>
      </ul>
    </div>
  </div>
</template>