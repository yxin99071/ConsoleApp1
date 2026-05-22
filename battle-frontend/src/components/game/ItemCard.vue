<script setup lang="ts">
import { computed, ref, onMounted } from 'vue';
import { PROFESSION_MAP } from '../../utils/constants';
import gsap from 'gsap';
import type { ItemDto } from '../../types/battle';
import BuffIcon from './BuffIcon.vue';

// ── 稀有度视觉配置 ──────────────────────────────────────────
const RARE_CONFIG = {
  1: {
    name:   'Common',
    color:  'border-slate-500',
    text:   'text-slate-400',
    bg:     'from-slate-900 to-slate-950',
    glow:   'shadow-none',
    badge:  'bg-slate-800 border-slate-600'
  },
  2: {
    name:   'Rare',
    color:  'border-purple-500',
    text:   'text-purple-300',
    bg:     'from-slate-900 via-purple-900/20 to-slate-950',
    glow:   'shadow-[0_0_15px_rgba(168,85,247,0.3)]',
    badge:  'bg-purple-900/40 border-purple-500/50'
  },
  3: {
    name:   'Epic',
    color:  'border-red-600',
    text:   'text-red-400',
    bg:     'from-slate-900 via-red-900/30 to-slate-950',
    glow:   'shadow-[0_0_25px_rgba(220,38,38,0.4)]',
    badge:  'bg-red-900/40 border-red-600/50'
  },
  4: {
    name:   'Legend',
    color:  'border-transparent',
    text:   'text-amber-300',
    bg:     'from-amber-900/30 via-slate-900 to-amber-900/30',
    glow:   'shadow-[0_0_35px_rgba(245,158,11,0.5)]',
    badge:  'bg-amber-900/60 border-amber-400/50'
  }
} as const;

const props = defineProps<{
  item:      ItemDto;
  type:      '技能' | '武器';
  isActive?: boolean;
}>();

const cardRef = ref<HTMLElement | null>(null);

// ── 稀有度 & 职业 computed ───────────────────────────────────
const rare = computed(() => {
  const lvl = props.item.rareLevel as keyof typeof RARE_CONFIG;
  return RARE_CONFIG[lvl] ?? RARE_CONFIG[1];
});

const FALLBACK_PROFESSION = {
  icon: '👤', label: '未知', color: 'text-slate-400',
  headerBg: 'bg-slate-700/50', badgeBg: 'bg-slate-700/90',
  badgeBorder: 'border-slate-400/50', badgeText: 'text-slate-200',
  stampOpacity: 'opacity-[0.05]',
};

const profession = computed(
  () => PROFESSION_MAP[props.item.profession] ?? PROFESSION_MAP['MORTAL'] ?? FALLBACK_PROFESSION
);

const isPassiveSkill = computed(() => props.type === '技能' && props.item.isPassive);

// ── GSAP 动画 ────────────────────────────────────────────────
const handleMouseMove = (e: MouseEvent) => {
  if (!cardRef.value) return;
  const rect = cardRef.value.getBoundingClientRect();
  const xPct = (e.clientX - rect.left) / rect.width  - 0.5;
  const yPct = (e.clientY - rect.top)  / rect.height - 0.5;
  gsap.to(cardRef.value, {
    rotateX: -yPct * 10,
    rotateY:  xPct * 10,
    duration: 0.4,
    ease: 'power2.out',
    transformPerspective: 1000,
  });
};

const handleMouseLeave = () => {
  if (!cardRef.value) return;
  gsap.to(cardRef.value, {
    rotateX: 0, rotateY: 0, scale: 1,
    duration: 0.6,
    ease: 'elastic.out(1, 0.6)'
  });
};

const handleMouseEnter = () => {
  gsap.to(cardRef.value, { scale: 1.05, duration: 0.3 });
};

onMounted(() => {
  if (cardRef.value) {
    gsap.from(cardRef.value, {
      opacity: 0, y: 30, rotateY: 15,
      duration: 0.8,
      delay: Math.random() * 0.4,
      ease: 'back.out(1.7)'
    });
  }
});

// ── Buff tooltip ─────────────────────────────────────────────
const activeBuff = ref<any>(null);
const floatPos   = ref({ x: 0, y: 0 });

const showBuff = (e: MouseEvent, buff: any) => {
  activeBuff.value = buff;
  floatPos.value   = { x: e.clientX, y: e.clientY - 10 };
};
const hideBuff = () => { activeBuff.value = null; };
</script>

<template>
  <div ref="cardRef"
       class="card-root relative w-52 h-72 rounded-xl border-2 flex flex-col
              overflow-hidden will-change-transform bg-linear-to-br select-none cursor-default"
       :class="[
         rare.color,
         rare.bg,
         rare.glow,
         isActive ? 'ring-4 ring-white z-50' : '',
         item.rareLevel === 4 ? 'legendary-border' : ''
       ]"
       @mousemove="handleMouseMove"
       @mouseleave="handleMouseLeave"
       @mouseenter="handleMouseEnter">

    <!-- 传说扫光 -->
    <div v-if="item.rareLevel === 4"
         class="absolute inset-0 bg-shimmer pointer-events-none z-0" />

    <!-- ── 职业印花（占位层）─────────────────────────────────
         待美术资源就绪后，将 <span> 替换为：
         <img
           :src="`/assets/profession-stamps/${item.profession.toLowerCase()}.webp`"
           class="w-full h-full object-cover mix-blend-luminosity pointer-events-none select-none"
         />
         ──────────────────────────────────────────────────── -->
    <div class="absolute inset-x-0 bottom-0 top-10 flex items-center justify-center
                pointer-events-none overflow-hidden z-0">
      <span class="leading-none select-none text-[8.5rem]"
            :class="profession.stampOpacity">
        {{ profession.icon }}
      </span>
    </div>

    <!-- ── Header（职业色背景）──────────────────────────────── -->
    <div class="h-10 shrink-0 border-b border-white/10 flex items-center justify-center
                relative z-10 backdrop-blur-sm"
         :class="profession.headerBg">

      <!-- 卡名（左侧留给角章 ~52px 宽度） -->
      <span class="font-black text-[13px] tracking-widest uppercase italic truncate
                   pl-14 pr-3 w-full text-center"
            :class="rare.text">
        {{ item.name }}
      </span>

      <!-- 职业角章：icon + 中文标签 -->
      <div class="absolute -left-px -top-px z-20">
        <div class="flex items-center gap-1 px-2 py-1.5 rounded-br-lg border shadow-md"
             :class="[profession.badgeBg, profession.badgeBorder]">
          <span class="text-[11px] leading-none">{{ profession.icon }}</span>
          <span class="text-[9px] font-black uppercase tracking-wider leading-none"
                :class="profession.badgeText">
            {{ profession.label }}
          </span>
        </div>
      </div>
    </div>

    <!-- ── Card Body ──────────────────────────────────────────── -->
    <div class="p-3 flex-1 flex flex-col gap-3 relative z-10">

      <!-- 预览框（印花透过此处半透明背景透出） -->
      <div class="w-full h-20 bg-black/50 rounded border border-white/5
                  flex items-center justify-center overflow-hidden">
        <span class="text-3xl filter drop-shadow-[0_0_8px_rgba(255,255,255,0.3)]">
          {{ type === '武器' ? '⚔️' : (isPassiveSkill ? '💎' : '🪄') }}
        </span>
      </div>

      <!-- 类型标签 -->
      <div class="flex justify-center">
        <span class="px-3 py-0.5 rounded-full text-[10px] border-2 font-black
                     uppercase tracking-tighter bg-black/60"
              :class="[rare.text, rare.color]">
          {{ isPassiveSkill ? 'PASSIVE SKILL' : type }}
        </span>
      </div>

      <!-- 描述 -->
      <p class="text-[12px] leading-snug text-slate-300 line-clamp-3 italic
                opacity-80 text-center px-1">
        "{{ item.description }}"
      </p>

      <!-- 底部：Buff + LVL ──────────────────────────────────── -->
      <div class="mt-auto flex justify-between items-end pt-2 border-t border-white/10">

        <!-- Buff 图标 -->
        <div class="flex gap-1.5 pb-1 flex-wrap">
          <div v-for="buff in item.buffs" :key="buff.name"
               class="relative"
               @mouseenter="showBuff($event, buff)"
               @mouseleave="hideBuff">
            <div class="w-6 h-6 rounded bg-black/40 border border-white/10
                        flex items-center justify-center
                        hover:border-blue-400 transition-all cursor-help backdrop-blur-sm">
              <span class="text-[12px]">
                {{ buff.isDeBuff ? '💢' : (buff.isDamage ? '💥' : '✨') }}
              </span>
            </div>
          </div>
        </div>

        <!-- Buff tooltip -->
        <Teleport to="body">
          <div v-if="activeBuff"
               class="fixed z-[9999] pointer-events-none transform -translate-x-1/2 -translate-y-full"
               :style="{ left: `${floatPos.x}px`, top: `${floatPos.y}px` }">
            <BuffIcon :buff="activeBuff" class="shadow-2xl ring-1 ring-white/20" />
          </div>
        </Teleport>

        <!-- 稀有度等级徽章 -->
        <div class="relative group">
          <div v-if="item.rareLevel >= 3"
               class="absolute inset-0 bg-current opacity-30 blur-md animate-pulse"
               :class="rare.text" />
          <div class="relative px-2 py-1 rounded-tl-xl border-l-2 border-t-2
                      border-white/20 backdrop-blur-md flex flex-col items-center"
               :class="rare.badge">
            <span class="text-[8px] font-bold opacity-70 text-white">LVL</span>
            <span class="text-2xl font-black italic leading-none tracking-tighter"
                  :class="rare.text">
              {{ item.rareLevel }}
            </span>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped>
/* 1. 基础 3D 设定 */
.card-root {
  transform-style: preserve-3d;
  backface-visibility: hidden;
  transition: box-shadow 0.3s ease;
}

/* 2. 传说级旋转边框 */
@property --angle {
  syntax: '<angle>';
  initial-value: 0deg;
  inherits: false;
}

.legendary-border {
  border: 2px solid transparent !important;
  background-image:
    linear-gradient(#0f172a, #0f172a),
    linear-gradient(var(--angle), #f59e0b, #fff7ed, #78350f, #f59e0b);
  background-origin: border-box;
  background-clip: content-box, border-box;
  animation: rotate-gradient 4s linear infinite;
}

@keyframes rotate-gradient {
  to { --angle: 360deg; }
}

/* 3. 传说扫光 */
.bg-shimmer {
  background: linear-gradient(
    45deg,
    transparent 30%,
    rgba(255, 255, 255, 0.08) 50%,
    transparent 70%
  );
  background-size: 200% 200%;
  animation: shimmer-effect 6s infinite linear;
}

@keyframes shimmer-effect {
  0%   { background-position: -200% 0; }
  100% { background-position:  200% 0; }
}

/* 4. 辅助 */
.group:hover .animate-pulse {
  animation-duration: 0.5s;
}
</style>
