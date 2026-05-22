<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import ItemCard from './ItemCard.vue'
import { getAwardList, claimAward } from '../../api/award'
import type { AwardListDto, AwardItemDto } from '../../types/award'

const emit = defineEmits<{ (e: 'close', refreshCount: boolean): void }>()

// ── Tab ──────────────────────────────────────────────────
type Tab = 'awards' | 'smelt' | 'exchange'
const activeTab = ref<Tab>('awards')
const tabs: { key: Tab; label: string; icon: string }[] = [
  { key: 'awards',   label: '奖励领取', icon: '🎁' },
  { key: 'smelt',    label: '熔炼',     icon: '🔥' },
  { key: 'exchange', label: '换取',     icon: '🏪' },
]

// ── Awards state ─────────────────────────────────────────
const awards      = ref<AwardListDto[]>([])
const cursor      = ref(0)                  // 当前正在领的奖励索引
const selectedId  = ref<number | null>(null)
const isLoading   = ref(false)
const isClaiming  = ref(false)
const claimDone   = ref(false)              // 本次打开是否全部领完

const currentAward = computed<AwardListDto | null>(() => awards.value[cursor.value] ?? null)
const total        = computed(() => awards.value.length)
const remaining    = computed(() => total.value - cursor.value)

async function loadAwards() {
  isLoading.value = true
  try {
    awards.value = await getAwardList()
    cursor.value = 0
    selectedId.value = null
    claimDone.value = awards.value.length === 0
  } finally {
    isLoading.value = false
  }
}

async function handleClaim() {
  if (!currentAward.value || selectedId.value === null || isClaiming.value) return
  isClaiming.value = true
  try {
    await claimAward(currentAward.value.id, selectedId.value)
    // 移除已领取的条目，重置选择
    awards.value.splice(cursor.value, 1)
    selectedId.value = null
    // 如果已越界则退到最后一个
    if (cursor.value >= awards.value.length)
      cursor.value = Math.max(0, awards.value.length - 1)
    if (awards.value.length === 0)
      claimDone.value = true
  } finally {
    isClaiming.value = false
  }
}

function handleClose() {
  emit('close', claimDone.value)
}

// ItemCard 期望的 ItemDto 形状 ── AwardItemDto 是其超集，直接传入
function toItemDto(item: AwardItemDto) {
  return item  // 字段完全兼容
}

onMounted(loadAwards)
</script>

<template>
  <!-- Backdrop -->
  <Teleport to="body">
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm"
         @click.self="handleClose">

      <div class="relative w-[900px] max-h-[88vh] flex flex-col bg-slate-950 border border-white/10
                  rounded-2xl shadow-[0_0_80px_rgba(0,0,0,0.8)] overflow-hidden">

        <!-- ── Header ── -->
        <header class="h-14 shrink-0 flex items-center justify-between px-6
                        border-b border-white/5 bg-slate-900/60">
          <div class="flex items-center gap-2">
            <span class="text-xl">🎒</span>
            <span class="font-black text-white tracking-widest uppercase text-sm">Backpack</span>
          </div>
          <button @click="handleClose"
                  class="w-8 h-8 rounded-full flex items-center justify-center
                         text-slate-500 hover:text-white hover:bg-white/10 transition-all text-sm">
            ✕
          </button>
        </header>

        <!-- ── Tabs ── -->
        <nav class="shrink-0 flex border-b border-white/5 bg-slate-900/40">
          <button v-for="tab in tabs" :key="tab.key"
                  @click="activeTab = tab.key"
                  class="flex items-center gap-2 px-6 py-3 text-xs font-bold uppercase tracking-widest
                         transition-all relative"
                  :class="activeTab === tab.key
                    ? 'text-white'
                    : 'text-slate-600 hover:text-slate-400'">
            <span>{{ tab.icon }}</span>
            <span>{{ tab.label }}</span>
            <div v-if="activeTab === tab.key"
                 class="absolute bottom-0 left-0 right-0 h-0.5 bg-indigo-500 rounded-full"/>
          </button>
        </nav>

        <!-- ── Tab Content ── -->
        <div class="flex-1 overflow-y-auto custom-scrollbar">

          <!-- ══ 奖励领取 ══ -->
          <div v-if="activeTab === 'awards'" class="p-6 min-h-[460px] flex flex-col">

            <!-- Loading -->
            <div v-if="isLoading" class="flex-1 flex items-center justify-center">
              <div class="w-10 h-10 border-2 border-indigo-500 border-t-transparent
                           rounded-full animate-spin"/>
            </div>

            <!-- 全部领完 -->
            <div v-else-if="claimDone || awards.length === 0"
                 class="flex-1 flex flex-col items-center justify-center gap-4 text-center">
              <span class="text-5xl">✅</span>
              <p class="text-slate-300 font-bold text-lg">奖励已全部领取</p>
              <p class="text-slate-600 text-sm">继续战斗以解锁更多奖励</p>
            </div>

            <!-- 领取界面 -->
            <template v-else-if="currentAward">
              <!-- 进度 -->
              <div class="flex items-center justify-between mb-5 shrink-0">
                <div>
                  <p class="text-[10px] text-slate-500 uppercase tracking-widest mb-1">
                    待领取奖励
                  </p>
                  <h3 class="font-black text-white text-lg tracking-tight">
                    {{ currentAward.type === 'WEAPON' ? '⚔️ 武器奖励' : '🪄 技能奖励' }}
                    <span class="text-slate-500 font-normal text-sm ml-2">
                      Lv.{{ currentAward.awardLevel }} 解锁
                    </span>
                  </h3>
                </div>
                <div class="text-right">
                  <p class="text-[10px] text-slate-600 mb-1">进度</p>
                  <div class="flex items-center gap-2">
                    <div class="flex gap-1">
                      <div v-for="(_, i) in awards" :key="i"
                           class="w-2 h-2 rounded-full transition-colors"
                           :class="i === cursor ? 'bg-indigo-500' : 'bg-slate-700'"/>
                    </div>
                    <span class="text-xs font-mono text-slate-500">
                      {{ cursor + 1 }} / {{ total }}
                    </span>
                  </div>
                </div>
              </div>

              <!-- 提示 -->
              <p class="text-[11px] text-slate-500 mb-5 shrink-0">
                从以下 {{ currentAward.items.length }} 件中选择 1 件加入背包
              </p>

              <!-- 卡片选择区 -->
              <div class="flex gap-6 justify-center flex-wrap flex-1">
                <div v-for="item in currentAward.items" :key="item.id"
                     class="cursor-pointer transition-all duration-200"
                     :class="selectedId === item.id
                       ? 'scale-105 drop-shadow-[0_0_20px_rgba(99,102,241,0.6)]'
                       : 'opacity-70 hover:opacity-90'"
                     @click="selectedId = item.id">
                  <ItemCard
                    :item="toItemDto(item)"
                    :type="currentAward.type === 'WEAPON' ? '武器' : '技能'"
                    :is-active="selectedId === item.id"
                  />
                </div>
              </div>

              <!-- 确认按钮 -->
              <div class="flex justify-end mt-6 shrink-0">
                <button
                  @click="handleClaim"
                  :disabled="selectedId === null || isClaiming"
                  class="px-8 py-2.5 rounded-lg font-black text-sm uppercase tracking-widest
                         transition-all disabled:opacity-30 disabled:cursor-not-allowed"
                  :class="selectedId !== null
                    ? 'bg-indigo-600 hover:bg-indigo-500 text-white shadow-[0_0_20px_rgba(99,102,241,0.4)]'
                    : 'bg-slate-800 text-slate-500'">
                  <span v-if="isClaiming">领取中…</span>
                  <span v-else>确认领取 →</span>
                </button>
              </div>
            </template>
          </div>

          <!-- ══ 熔炼 (占位) ══ -->
          <div v-else-if="activeTab === 'smelt'"
               class="p-6 min-h-[460px] flex flex-col items-center justify-center gap-6">
            <div class="w-20 h-20 rounded-full bg-orange-900/20 border border-orange-500/20
                         flex items-center justify-center text-4xl">
              🔥
            </div>
            <div class="text-center">
              <p class="font-black text-white text-xl mb-2">熔炼工坊</p>
              <p class="text-slate-500 text-sm max-w-xs leading-relaxed">
                将背包中多余的武器或技能熔炼成货币，用于换取商店中的物品。
              </p>
            </div>
            <div class="grid grid-cols-2 gap-4 w-full max-w-sm opacity-40 pointer-events-none select-none">
              <div class="h-24 rounded-xl border-2 border-dashed border-slate-700
                           flex items-center justify-center text-slate-600 text-sm">
                选择卡牌
              </div>
              <div class="h-24 rounded-xl border border-slate-800 bg-slate-900/60
                           flex flex-col items-center justify-center gap-1">
                <span class="text-2xl">🪙</span>
                <span class="text-xs text-slate-600">熔炼价值</span>
              </div>
            </div>
            <span class="px-3 py-1 rounded-full bg-orange-900/20 border border-orange-500/20
                          text-orange-400 text-[10px] font-bold uppercase tracking-widest">
              Coming Soon
            </span>
          </div>

          <!-- ══ 换取 (占位) ══ -->
          <div v-else-if="activeTab === 'exchange'"
               class="p-6 min-h-[460px] flex flex-col gap-5">
            <div class="flex items-center gap-3 mb-2">
              <div class="w-1 h-6 bg-indigo-500 rounded"/>
              <h3 class="font-black text-white tracking-tight">换取商店</h3>
              <span class="px-2 py-0.5 rounded-full bg-indigo-900/30 border border-indigo-500/20
                            text-indigo-400 text-[9px] font-bold uppercase tracking-widest ml-auto">
                Coming Soon
              </span>
            </div>

            <!-- 三个子商店占位 -->
            <div class="grid grid-cols-1 gap-4 opacity-50 pointer-events-none select-none">
              <div v-for="shop in [
                { icon: '🗓️', name: '每日商店', desc: '每日轮换，直接兑换指定卡牌' },
                { icon: '⚔️', name: '职业商店', desc: '按职业方向抽选，消耗对应货币' },
                { icon: '💎', name: '稀有商店', desc: '按稀有等级抽选，获取精良装备' },
              ]" :key="shop.name"
                class="flex items-center gap-4 p-4 rounded-xl border border-slate-800
                        bg-slate-900/40">
                <div class="w-12 h-12 rounded-xl bg-slate-800 flex items-center justify-center
                             text-2xl shrink-0">
                  {{ shop.icon }}
                </div>
                <div>
                  <p class="font-bold text-white text-sm">{{ shop.name }}</p>
                  <p class="text-slate-500 text-xs mt-0.5">{{ shop.desc }}</p>
                </div>
                <div class="ml-auto">
                  <div class="flex items-center gap-1.5 text-slate-600 text-xs">
                    <span>🪙</span>
                    <span class="font-mono">—</span>
                  </div>
                </div>
              </div>
            </div>

            <p class="text-center text-slate-700 text-xs mt-auto pt-4">
              货币体系设计中，敬请期待
            </p>
          </div>

        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #334155; border-radius: 10px; }
</style>
