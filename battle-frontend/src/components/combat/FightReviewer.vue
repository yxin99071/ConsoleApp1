<script setup lang="ts">
import { ref, onMounted } from 'vue';
import gsap from 'gsap';
import ItemCard from '../game/ItemCard.vue';
import FighterInfo from './FighterInfo.vue';

const props = defineProps<{
  battleData: any[]; // 完整的扁平 JSON 数组
}>();

const emit = defineEmits(['close']);

// --- 状态变量 ---
const leftFighter = ref<any>(null);
const rightFighter = ref<any>(null);
const activeItem = ref<any>(null);
const currentLog = ref("准备对局...");
const battleLogs = ref<string[]>([]);
const showResult = ref(false);
const resultData = ref<any>(null);

// --- 索引表：用于根据 Name 查找详细信息 ---
const weaponMap = new Map();
const skillMap = new Map();
const buffMap = new Map();

// --- 特效与结算状态 ---
const damageEffect = ref({ show: false, value: 0, isCrit: false });
const expStats = ref({ current: 0, percent: 0 });

const delay = (ms: number) => new Promise(res => setTimeout(res, ms));

// 获取 Fighter Ref
const getFighterRef = (name: string) => {
  return leftFighter.value.name === name ? leftFighter : rightFighter;
};

/**
 * 核心逻辑：数据预处理
 */
const prepareDataIndices = (startNode: any) => {
  // 1. 索引 Buff 库
  startNode.data.BuffLibrary.forEach((b: any) => buffMap.set(b.name, b));
  
  // 2. 索引各玩家的技能和武器（用于 ItemCard 展示）
  startNode.data.Players.forEach((p: any) => {
    p.weapons.forEach((w: any) => weaponMap.set(w.name, w));
    p.skills.forEach((s: any) => skillMap.set(s.name, s));
  });
};

/**
 * 动画特效：光束投影
 */
const playBeamEffect = (isLeft: boolean) => {
  return new Promise((resolve) => {
    const beam = document.createElement('div');
    beam.className = `fixed h-1 z-50 rounded-full blur-[2px] pointer-events-none ${
      isLeft ? 'bg-linear-to-r from-blue-400 to-white' : 'bg-linear-to-l from-red-400 to-white'
    }`;
    document.body.appendChild(beam);

    const startX = window.innerWidth / 2;
    const startY = window.innerHeight / 2;
    const targetX = isLeft ? window.innerWidth * 0.75 : window.innerWidth * 0.25;

    gsap.set(beam, { x: startX, y: startY, width: 0 });
    const tl = gsap.timeline({ onComplete: () => { beam.remove(); resolve(true); } });
    tl.to(beam, { width: 150, duration: 0.2 })
      .to(beam, { x: targetX, width: 50, duration: 0.2, ease: "power2.in" });
  });
};

/**
 * 播放流程控制
 */
const startPlayback = async () => {
  const startNode = props.battleData.find(n => n.type === 'BattleStart');
  const endNode = props.battleData.find(n => n.type === 'BattleEnd');
  const steps = props.battleData.filter(n => n.type !== 'BattleStart' && n.type !== 'BattleEnd');

  if (!startNode) return;

  // 1. 初始化索引
  prepareDataIndices(startNode);

  // 2. 初始化角色状态
  const initP = startNode.data.Players;
  leftFighter.value = {
    ...initP[0],
    currentHp: initP[0].stats.maxHealth,
    weaponsCount: initP[0].weapons.length,
    skillsCount: initP[0].skills.length,
    buffs: [],
    status: 'idle'
  };
  rightFighter.value = {
    ...initP[1],
    currentHp: initP[1].stats.maxHealth,
    weaponsCount: initP[1].weapons.length,
    skillsCount: initP[1].skills.length,
    buffs: [],
    status: 'idle'
  };

  await delay(1000);

  // 3. 执行指令
  for (const step of steps) {
    const { type, data } = step;

    switch (type) {
      case 'Action':
        const actor = getFighterRef(data.Actor);
        const isLeft = actor.value === leftFighter.value;

        // A. 资源递减
        if (data.Category === 'weapon') actor.value.weaponsCount--;
        else actor.value.skillsCount--;

        // B. Buff 回合结算：行动方 Buff 减 1
        actor.value.buffs = actor.value.buffs.map((b: any) => ({
          ...b,
          lastRound: b.lastRound - 1
        })).filter((b: any) => b.lastRound > 0);

        // C. 数据装配 & 展示
        const itemInfo = data.Category === 'weapon' ? weaponMap.get(data.Name) : skillMap.get(data.Name);
        activeItem.value = itemInfo;
        
        currentLog.value = `[${data.Actor}] 使用了 ${data.Name}`;
        battleLogs.value.unshift(currentLog.value);

        actor.value.status = 'active';
        await delay(2000); // 重点停留

        // D. 动作衔接
        activeItem.value = null;
        actor.value.status = 'attack';
        await playBeamEffect(isLeft);
        break;

      case 'Damage':
        const target = getFighterRef(data.Target);
        damageEffect.value = { show: true, value: data.Value, isCrit: data.Critical };
        target.value.status = 'hit';
        target.value.currentHp = data.HP;
        
        currentLog.value = `${data.Target} 受到 ${data.Value} 点伤害 ${data.Critical ? ' (暴击!)' : ''}`;
        battleLogs.value.unshift(currentLog.value);

        await delay(1200);
        damageEffect.value.show = false;
        target.value.status = 'idle';
        break;

      case 'BuffApply':
        const bTarget = getFighterRef(data.Target);
        const staticBuff = buffMap.get(data.BuffName);
        bTarget.value.buffs.push({
          ...staticBuff,
          level: data.BuffLevel,
          lastRound: data.LastRound
        });
        currentLog.value = `${data.Target} 获得效果: ${data.BuffName}`;
        battleLogs.value.unshift(currentLog.value);
        await delay(500);
        break;
    }
    
    // 重置非活动状态
    if (leftFighter.value.status !== 'active') leftFighter.value.status = 'idle';
    if (rightFighter.value.status !== 'active') rightFighter.value.status = 'idle';
  }

  // 4. 结算
  if (endNode) {
    resultData.value = endNode.data;
    await delay(500);
    showResult.value = true;
    animateExp();
  }
};

const animateExp = () => {
  const exp = resultData.value.ExperienceChange;
  expStats.value.current = exp.before;
  gsap.to(expStats.value, {
    current: exp.after,
    duration: 2.5,
    ease: "power2.out",
    onUpdate: () => {
      expStats.value.percent = (expStats.value.current % 1000) / 10;
    }
  });
};

onMounted(startPlayback);
</script>

<template>
  <div class="w-full h-full relative flex flex-col items-center justify-between p-10 bg-slate-950/40 select-none overflow-hidden">
    
    <div class="w-full flex justify-between items-center flex-1 relative z-10">
      
      <FighterInfo 
        v-if="leftFighter"
        v-bind="leftFighter"
        :maxHp="leftFighter.stats.maxHealth"
        :weaponCount="leftFighter.weaponsCount"
        :skillCount="leftFighter.skillsCount"
        side="left"
      />

      <div class="relative flex-1 h-[520px] flex items-center justify-center pointer-events-none">
        
        <Transition name="item-focus">
          <div v-if="activeItem" class="z-50 scale-125 origin-center">
            <ItemCard 
              :item="activeItem" 
              :type="activeItem.category === 'weapon' ? '武器' : '技能'" 
              :isActive="true"
            />
          </div>
        </Transition>

        <Transition name="damage-pop">
          <div v-if="damageEffect.show" 
               class="absolute z-[60] flex flex-col items-center pointer-events-none"
               :class="damageEffect.isCrit ? 'top-1/4' : 'top-1/3'">
            
            <span v-if="damageEffect.isCrit" 
                  class="text-red-500 font-black italic text-5xl tracking-tighter uppercase drop-shadow-[0_0_20px_rgba(239,68,68,0.9)] animate-bounce mb-2">
              CRITICAL!
            </span>
            
            <span class="font-black italic drop-shadow-2xl leading-none"
                  :class="[
                    damageEffect.isCrit ? 'text-8xl text-white' : 'text-7xl text-amber-400'
                  ]">
              -{{ damageEffect.value }}
            </span>
          </div>
        </Transition>
      </div>

      <FighterInfo 
        v-if="rightFighter"
        v-bind="rightFighter"
        :maxHp="rightFighter.stats.maxHealth"
        :weaponCount="rightFighter.weaponsCount"
        :skillCount="rightFighter.skillsCount"
        side="right"
      />
    </div>

    <div class="w-full max-w-4xl h-36 bg-slate-900/90 border border-white/10 rounded-2xl backdrop-blur-xl p-4 flex flex-col shadow-2xl relative z-20 overflow-hidden">
      <div class="flex items-center gap-2 mb-2 border-b border-white/5 pb-2">
        <div class="w-1.5 h-1.5 bg-blue-500 rounded-full animate-pulse"></div>
        <h4 class="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Combat Protocol Log</h4>
      </div>
      
      <div class="flex-1 overflow-y-auto custom-scrollbar space-y-2 pr-2">
        <p v-for="(log, idx) in battleLogs" :key="idx" 
           class="text-sm font-medium tracking-wide transition-all duration-300"
           :class="idx === 0 ? 'text-blue-300 translate-x-1' : 'text-slate-500 opacity-50'">
          <span class="text-[10px] opacity-20 mr-3 font-mono">STEP_{{ battleLogs.length - idx }}</span>
          {{ log }}
        </p>
      </div>
    </div>

    <Transition name="result-screen">
      <div v-if="showResult" 
           class="absolute inset-0 z-[100] bg-slate-950/98 backdrop-blur-3xl flex flex-col items-center justify-center p-12">
        
        <div class="absolute inset-0 bg-[radial-gradient(circle_at_center,rgba(59,130,246,0.1),transparent)] pointer-events-none"></div>

        <div class="relative text-center mb-10">
          <h3 class="text-xs font-black tracking-[0.6em] text-blue-500 uppercase mb-4 opacity-70">Battle Terminated</h3>
          <h2 class="text-9xl font-black italic tracking-tighter text-white mb-2 filter drop-shadow-sm">
            VICTORY
          </h2>
          <div class="h-1 w-full bg-linear-to-r from-transparent via-blue-500/50 to-transparent"></div>
          <p class="mt-8 text-4xl font-black italic text-transparent bg-clip-text bg-linear-to-b from-blue-400 to-indigo-600 uppercase">
            {{ resultData?.UserName }}
          </p>
        </div>

        <div class="w-[500px] space-y-6 bg-white/5 border border-white/10 p-10 rounded-[2rem] backdrop-blur-sm relative overflow-hidden group shadow-2xl">
          <div class="flex justify-between items-end">
            <div>
              <p class="text-[10px] font-bold text-slate-500 uppercase mb-2 tracking-widest">Experience Gained</p>
              <div class="flex items-baseline gap-2">
                <span class="text-5xl font-black italic text-white">{{ Math.floor(expStats.current) }}</span>
                <span class="text-slate-600 font-mono text-lg">/ 1000</span>
              </div>
            </div>
            <div class="text-right pb-1">
              <span class="text-blue-400 font-black italic text-2xl animate-pulse">+{{ resultData?.ExperienceChange.gained }}</span>
            </div>
          </div>

          <div class="relative h-4 bg-black/60 rounded-full border border-white/5 overflow-hidden">
            <div class="absolute inset-y-0 left-0 bg-linear-to-r from-blue-600 via-indigo-400 to-blue-600 transition-all duration-300 shadow-[0_0_15px_rgba(37,99,235,0.5)]"
                 :style="{ width: `${expStats.percent}%` }">
              <div class="absolute inset-0 bg-shimmer"></div>
            </div>
          </div>
        </div>

        <div class="mt-16 flex gap-8">
          <button @click="showResult = false" 
                  class="px-14 py-4 bg-white/5 hover:bg-white/10 border border-white/10 text-slate-300 font-black italic tracking-widest rounded-2xl transition-all active:scale-95">
            REVIEW LOGS
          </button>
          <button @click="emit('close')" 
                  class="px-14 py-4 bg-blue-600 hover:bg-blue-500 text-white font-black italic tracking-widest rounded-2xl shadow-[0_0_40px_rgba(37,99,235,0.3)] transition-all active:scale-95 hover:-translate-y-1">
            CONFIRM
          </button>
        </div>
      </div>
    </Transition>

  </div>
</template>
<style scoped>
/* 1. ItemCard 入场特写：模拟从下方快速飞入并带有一点回弹 */
.item-focus-enter-active {
  transition: all 0.6s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.item-focus-leave-active {
  transition: all 0.4s cubic-bezier(0.4, 0, 1, 1);
}
.item-focus-enter-from {
  opacity: 0;
  transform: scale(0.4) translateY(200px) rotateX(-30deg);
}
.item-focus-leave-to {
  opacity: 0;
  transform: scale(1.6) blur(20px);
}

/* 2. 伤害数字弹出与暴击抖动 */
.damage-pop-enter-active {
  animation: damage-dynamic 1.3s cubic-bezier(0.22, 1, 0.36, 1) forwards;
}

@keyframes damage-dynamic {
  0% {
    opacity: 0;
    transform: translateY(60px) scale(0.4);
  }
  15% {
    opacity: 1;
    transform: translateY(-30px) scale(1.3);
  }
  20% {
    /* 模拟暴击时的瞬间震动 */
    transform: translateY(-30px) scale(1.3) rotate(3deg);
  }
  25% {
    transform: translateY(-30px) scale(1.3) rotate(-3deg);
  }
  30% {
    transform: translateY(-30px) scale(1.2) rotate(0);
  }
  85% {
    opacity: 1;
    transform: translateY(-100px) scale(1);
  }
  100% {
    opacity: 0;
    transform: translateY(-150px) scale(0.8);
  }
}

/* 3. 结算蒙层切换 */
.result-screen-enter-active,
.result-screen-leave-active {
  transition: all 0.8s cubic-bezier(0.4, 0, 0.2, 1);
}
.result-screen-enter-from,
.result-screen-leave-to {
  opacity: 0;
  backdrop-filter: blur(0px);
  transform: scale(1.05);
}

/* 4. 经验条 Shimmer 特效：增强填充时的能量感 */
.bg-shimmer {
  background: linear-gradient(
    90deg,
    transparent 0%,
    rgba(255, 255, 255, 0.3) 50%,
    transparent 100%
  );
  background-size: 200% 100%;
  animation: shimmer-swipe 1.5s infinite linear;
}

@keyframes shimmer-swipe {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}

/* 5. 战斗日志滚动条 */
.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.02);
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(59, 130, 246, 0.3);
  border-radius: 10px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(59, 130, 246, 0.6);
}

/* 6. 经验数值跳动增强 */
.font-black.italic {
  font-variant-numeric: tabular-nums; /* 防止数字跳动时宽度闪烁 */
}

/* 按钮悬停光晕 */
button {
  position: relative;
  overflow: hidden;
}
button::after {
  content: '';
  position: absolute;
  top: -50%; left: -50%;
  width: 200%; height: 200%;
  background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
  transform: scale(0);
  transition: transform 0.6s ease-out;
}
button:hover::after {
  transform: scale(1);
}
</style>