<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import ItemCard from './ItemCard.vue'
import { getAwardList, claimAward } from '../../api/award'
import {
  getDailyShop, manualRefreshShop, lockSlot, purchaseSlot,
  drawByProfession, drawByRarity, smeltItem, getInventory, getR4Status,
} from '../../api/shop'
import type { AwardListDto, AwardItemDto } from '../../types/award'
import type {
  DailyShopDto, ShopSlotDto, InventoryDto, OwnedItemDto,
  DrawResultDto, SmeltResultDto,
} from '../../types/shop'
import { PROFESSION_MAP } from '../../utils/constants'

const emit = defineEmits<{ (e: 'close', refreshCount: boolean): void }>()

// ── Tab ──────────────────────────────────────────────────────────────────────
type Tab = 'awards' | 'smelt' | 'exchange'
const activeTab = ref<Tab>('awards')
const tabs: { key: Tab; label: string; icon: string }[] = [
  { key: 'awards',   label: '奖励领取', icon: '🎁' },
  { key: 'smelt',    label: '熔炼',     icon: '🔥' },
  { key: 'exchange', label: '换取',     icon: '🏪' },
]

// ═══════════════════════════════════════════════════════════════════════════════
// ── 奖励领取 Tab ──────────────────────────────────────────────────────────────
// ═══════════════════════════════════════════════════════════════════════════════

const awards      = ref<AwardListDto[]>([])
const cursor      = ref(0)
const selectedId  = ref<number | null>(null)
const isLoading   = ref(false)
const isClaiming  = ref(false)
const claimDone   = ref(false)

const currentAward = computed<AwardListDto | null>(() => awards.value[cursor.value] ?? null)
const total        = computed(() => awards.value.length)

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
    awards.value.splice(cursor.value, 1)
    selectedId.value = null
    if (cursor.value >= awards.value.length)
      cursor.value = Math.max(0, awards.value.length - 1)
    if (awards.value.length === 0) claimDone.value = true
  } finally {
    isClaiming.value = false
  }
}

function handleClose() { emit('close', claimDone.value) }
function toItemDto(item: AwardItemDto) { return item }

// ═══════════════════════════════════════════════════════════════════════════════
// ── 熔炼 Tab ─────────────────────────────────────────────────────────────────
// ═══════════════════════════════════════════════════════════════════════════════

const inventory       = ref<InventoryDto | null>(null)
const smeltSelected   = ref<OwnedItemDto | null>(null)
const isSmelting      = ref(false)
const smeltResult     = ref<SmeltResultDto | null>(null)
const smeltError      = ref<string | null>(null)

const allOwnedItems = computed<OwnedItemDto[]>(() => {
  if (!inventory.value) return []
  return [...inventory.value.weapons, ...inventory.value.skills]
    .sort((a, b) => b.rareLevel - a.rareLevel || a.name.localeCompare(b.name))
})

const smeltReward = computed(() => {
  const tbl = [0, 12, 30, 80, 200]
  return tbl[smeltSelected.value?.rareLevel ?? 0] ?? 0
})

async function loadInventory() {
  isLoading.value = true
  try { inventory.value = await getInventory() }
  finally { isLoading.value = false }
}

async function handleSmelt() {
  if (!smeltSelected.value || isSmelting.value) return
  isSmelting.value = true
  smeltResult.value = null
  smeltError.value  = null
  try {
    smeltResult.value = await smeltItem(smeltSelected.value.itemType, smeltSelected.value.id)
    if (inventory.value) inventory.value.lotteryPoint = smeltResult.value.newBalance
    // Reload inventory
    await loadInventory()
    smeltSelected.value = null
  } catch (e: any) {
    smeltError.value = e?.response?.data?.message ?? '熔炼失败'
  } finally {
    isSmelting.value = false
  }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ── 换取 Tab ─────────────────────────────────────────────────────────────────
// ═══════════════════════════════════════════════════════════════════════════════

type ExchangeSection = 'daily' | 'profession' | 'rarity'
const exchangeSection = ref<ExchangeSection>('daily')

// ── 每日商店 ─────────────────────────────────────────────────────────────────
const dailyShop       = ref<DailyShopDto | null>(null)
const isShopLoading   = ref(false)
const isRefreshing    = ref(false)
const isPurchasing    = ref(false)
const shopError       = ref<string | null>(null)
const shopSuccess     = ref<string | null>(null)
const countdown       = ref('')
let   countdownTimer: ReturnType<typeof setInterval> | null = null

function updateCountdown() {
  if (!dailyShop.value) return
  const diff = new Date(dailyShop.value.nextRefreshTime).getTime() - Date.now()
  if (diff <= 0) { countdown.value = '刷新中…'; return }
  const h = Math.floor(diff / 3_600_000)
  const m = Math.floor((diff % 3_600_000) / 60_000)
  const s = Math.floor((diff % 60_000) / 1000)
  countdown.value = `${h}:${String(m).padStart(2,'0')}:${String(s).padStart(2,'0')}`
}

async function loadDailyShop() {
  isShopLoading.value = true
  shopError.value = null
  try {
    dailyShop.value = await getDailyShop()
    updateCountdown()
    if (countdownTimer) clearInterval(countdownTimer)
    countdownTimer = setInterval(updateCountdown, 1000)
  } catch { shopError.value = '加载商店失败' }
  finally { isShopLoading.value = false }
}

async function handleManualRefresh() {
  if (isRefreshing.value) return
  isRefreshing.value = true
  shopError.value = null
  try {
    dailyShop.value = await manualRefreshShop()
    updateCountdown()
    showShopMessage('✅ 商店已刷新')
  } catch (e: any) {
    shopError.value = e?.response?.data?.message ?? '刷新失败'
  } finally {
    isRefreshing.value = false
  }
}

async function handleLock(slot: ShopSlotDto) {
  // Toggle: if already locked, pass slotId=0 to unlock all; else lock this slot
  const targetId = slot.isLocked ? 0 : slot.id
  try {
    dailyShop.value = await lockSlot(targetId)
  } catch { /* silent */ }
}

async function handlePurchase(slot: ShopSlotDto) {
  if (isPurchasing.value) return
  isPurchasing.value = true
  shopError.value    = null
  try {
    await purchaseSlot(slot.id)
    await loadDailyShop()   // Reload to reflect changes
    showShopMessage(`✅ 已获得「${slot.item.name}」`)
  } catch (e: any) {
    shopError.value = e?.response?.data?.message ?? '购买失败'
  } finally {
    isPurchasing.value = false
  }
}

function showShopMessage(msg: string) {
  shopSuccess.value = msg
  setTimeout(() => { shopSuccess.value = null }, 3000)
}

// ── 职业抽 ───────────────────────────────────────────────────────────────────
const isDrawing       = ref(false)
const drawResult      = ref<DrawResultDto | null>(null)
const drawError       = ref<string | null>(null)

const professions = [
  { key: 'WARRIOR',  label: '战士', icon: '⚔️',  cost: 40 },
  { key: 'RANGER',   label: '游侠', icon: '🏹',  cost: 40 },
  { key: 'MAGICIAN', label: '法师', icon: '🔮',  cost: 40 },
  { key: 'MORTAL',   label: '凡人', icon: '👤',  cost: 40 },
]

async function handleProfDraw(profession: string) {
  if (isDrawing.value) return
  isDrawing.value = true
  drawResult.value = null
  drawError.value  = null
  try {
    drawResult.value = await drawByProfession(profession)
    if (inventory.value) inventory.value.lotteryPoint = drawResult.value.newBalance
    if (dailyShop.value) dailyShop.value.lotteryPoint = drawResult.value.newBalance
  } catch (e: any) {
    drawError.value = e?.response?.data?.message ?? '抽卡失败'
  } finally {
    isDrawing.value = false
  }
}

// ── 稀有度抽 ─────────────────────────────────────────────────────────────────
const allR4Owned      = ref(false)
const rarities = [
  { level: 1, label: 'R1 普通', cost: 20,  color: 'text-slate-300',   border: 'border-slate-600',   bg: 'bg-slate-700/40'  },
  { level: 2, label: 'R2 稀有', cost: 50,  color: 'text-blue-300',    border: 'border-blue-600',    bg: 'bg-blue-900/20'   },
  { level: 3, label: 'R3 史诗', cost: 120, color: 'text-violet-300',  border: 'border-violet-600',  bg: 'bg-violet-900/20' },
  { level: 4, label: 'R4 传奇', cost: 400, color: 'text-amber-300',   border: 'border-amber-500',   bg: 'bg-amber-900/20'  },
]

async function handleRarityDraw(rarity: number) {
  if (isDrawing.value) return
  if (rarity === 4 && allR4Owned.value) return
  isDrawing.value = true
  drawResult.value = null
  drawError.value  = null
  try {
    drawResult.value = await drawByRarity(rarity)
    if (inventory.value) inventory.value.lotteryPoint = drawResult.value.newBalance
    if (dailyShop.value) dailyShop.value.lotteryPoint = drawResult.value.newBalance
    if (rarity === 4) {
      const status = await getR4Status()
      allR4Owned.value = status.allR4Owned
    }
  } catch (e: any) {
    drawError.value = e?.response?.data?.message ?? '抽卡失败'
  } finally {
    isDrawing.value = false
  }
}

// ── 通用货币显示 ─────────────────────────────────────────────────────────────
const lotteryPoint = computed(() => {
  if (activeTab.value === 'smelt') return inventory.value?.lotteryPoint ?? 0
  return dailyShop.value?.lotteryPoint ?? inventory.value?.lotteryPoint ?? 0
})

// ── 稀有度颜色 ───────────────────────────────────────────────────────────────
const rareLevelColors = ['', 'text-slate-300', 'text-blue-400', 'text-violet-400', 'text-amber-400']
const rareLevelLabels = ['', 'R1', 'R2', 'R3', 'R4 ✦']

// ── 生命周期 ─────────────────────────────────────────────────────────────────
onMounted(async () => {
  await loadAwards()
  loadInventory()
  const r4 = await getR4Status().catch(() => ({ allR4Owned: false }))
  allR4Owned.value = r4.allR4Owned
})

onUnmounted(() => { if (countdownTimer) clearInterval(countdownTimer) })

async function handleTabChange(key: Tab) {
  activeTab.value = key
  if (key === 'exchange' && !dailyShop.value) await loadDailyShop()
  if (key === 'smelt' && !inventory.value) await loadInventory()
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm"
         @click.self="handleClose">

      <div class="relative w-[960px] max-h-[90vh] flex flex-col bg-slate-950 border border-white/10
                  rounded-2xl shadow-[0_0_80px_rgba(0,0,0,0.8)] overflow-hidden">

        <!-- ── Header ── -->
        <header class="h-14 shrink-0 flex items-center justify-between px-6
                        border-b border-white/5 bg-slate-900/60">
          <div class="flex items-center gap-3">
            <span class="text-xl">🎒</span>
            <span class="font-black text-white tracking-widest uppercase text-sm">Backpack</span>
          </div>
          <!-- 碎片余额 -->
          <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-amber-900/30
                       border border-amber-500/30 text-amber-300 text-xs font-bold">
            <span>🪙</span>
            <span class="font-mono">{{ lotteryPoint }}</span>
            <span class="text-amber-500/70 font-normal">碎片</span>
          </div>
          <button @click="handleClose"
                  class="w-8 h-8 rounded-full flex items-center justify-center
                         text-slate-500 hover:text-white hover:bg-white/10 transition-all text-sm">✕</button>
        </header>

        <!-- ── Tabs ── -->
        <nav class="shrink-0 flex border-b border-white/5 bg-slate-900/40">
          <button v-for="tab in tabs" :key="tab.key"
                  @click="handleTabChange(tab.key)"
                  class="flex items-center gap-2 px-6 py-3 text-xs font-bold uppercase tracking-widest
                         transition-all relative"
                  :class="activeTab === tab.key ? 'text-white' : 'text-slate-600 hover:text-slate-400'">
            <span>{{ tab.icon }}</span>
            <span>{{ tab.label }}</span>
            <div v-if="activeTab === tab.key"
                 class="absolute bottom-0 left-0 right-0 h-0.5 bg-indigo-500 rounded-full"/>
          </button>
        </nav>

        <!-- ── Tab Content ── -->
        <div class="flex-1 overflow-y-auto custom-scrollbar min-h-0">

          <!-- ══════════════════════════════════════════════════════════════════
               奖励领取
          ══════════════════════════════════════════════════════════════════ -->
          <div v-if="activeTab === 'awards'" class="p-6 min-h-[480px] flex flex-col">
            <div v-if="isLoading" class="flex-1 flex items-center justify-center">
              <div class="w-10 h-10 border-2 border-indigo-500 border-t-transparent rounded-full animate-spin"/>
            </div>
            <div v-else-if="claimDone || awards.length === 0"
                 class="flex-1 flex flex-col items-center justify-center gap-4 text-center">
              <span class="text-5xl">✅</span>
              <p class="text-slate-300 font-bold text-lg">奖励已全部领取</p>
              <p class="text-slate-600 text-sm">继续战斗以解锁更多奖励</p>
            </div>
            <template v-else-if="currentAward">
              <div class="flex items-center justify-between mb-5 shrink-0">
                <div>
                  <p class="text-[10px] text-slate-500 uppercase tracking-widest mb-1">待领取奖励</p>
                  <h3 class="font-black text-white text-lg tracking-tight">
                    {{ currentAward.type === 'WEAPON' ? '⚔️ 武器奖励' : '🪄 技能奖励' }}
                    <span class="text-slate-500 font-normal text-sm ml-2">Lv.{{ currentAward.awardLevel }} 解锁</span>
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
                    <span class="text-xs font-mono text-slate-500">{{ cursor + 1 }} / {{ total }}</span>
                  </div>
                </div>
              </div>
              <p class="text-[11px] text-slate-500 mb-5 shrink-0">
                从以下 {{ currentAward.items.length }} 件中选择 1 件加入背包
              </p>
              <div class="flex gap-6 justify-center flex-wrap flex-1">
                <div v-for="item in currentAward.items" :key="item.id"
                     class="cursor-pointer transition-all duration-200"
                     :class="selectedId === item.id
                       ? 'scale-105 drop-shadow-[0_0_20px_rgba(99,102,241,0.6)]'
                       : 'opacity-70 hover:opacity-90'"
                     @click="selectedId = item.id">
                  <ItemCard :item="toItemDto(item)"
                            :type="currentAward.type === 'WEAPON' ? '武器' : '技能'"
                            :is-active="selectedId === item.id"/>
                </div>
              </div>
              <div class="flex justify-end mt-6 shrink-0">
                <button @click="handleClaim"
                        :disabled="selectedId === null || isClaiming"
                        class="px-8 py-2.5 rounded-lg font-black text-sm uppercase tracking-widest transition-all
                               disabled:opacity-30 disabled:cursor-not-allowed"
                        :class="selectedId !== null
                          ? 'bg-indigo-600 hover:bg-indigo-500 text-white shadow-[0_0_20px_rgba(99,102,241,0.4)]'
                          : 'bg-slate-800 text-slate-500'">
                  <span v-if="isClaiming">领取中…</span>
                  <span v-else>确认领取 →</span>
                </button>
              </div>
            </template>
          </div>

          <!-- ══════════════════════════════════════════════════════════════════
               熔炼
          ══════════════════════════════════════════════════════════════════ -->
          <div v-else-if="activeTab === 'smelt'" class="p-6 min-h-[480px] flex gap-6">

            <!-- 左：卡牌列表 -->
            <div class="flex-1 min-w-0">
              <p class="text-[10px] text-slate-500 uppercase tracking-widest mb-3">选择卡牌熔炼</p>
              <div v-if="isLoading" class="flex items-center justify-center h-40">
                <div class="w-8 h-8 border-2 border-orange-500 border-t-transparent rounded-full animate-spin"/>
              </div>
              <div v-else-if="allOwnedItems.length === 0"
                   class="flex flex-col items-center justify-center h-40 gap-3 text-slate-600">
                <span class="text-3xl">📭</span>
                <p class="text-sm">背包空空如也</p>
              </div>
              <div v-else class="grid grid-cols-3 gap-2 max-h-[420px] overflow-y-auto custom-scrollbar pr-1">
                <button v-for="item in allOwnedItems" :key="`${item.itemType}-${item.id}`"
                        @click="smeltSelected = smeltSelected?.id === item.id && smeltSelected?.itemType === item.itemType ? null : item; smeltResult = null"
                        class="p-3 rounded-xl border transition-all text-left group"
                        :class="smeltSelected?.id === item.id && smeltSelected?.itemType === item.itemType
                          ? 'border-orange-500 bg-orange-900/20'
                          : 'border-slate-800 bg-slate-900/60 hover:border-slate-700'">
                  <div class="flex items-start justify-between gap-1 mb-1">
                    <span class="font-bold text-xs text-white leading-tight">{{ item.name }}</span>
                    <span class="shrink-0 text-[10px] font-bold" :class="rareLevelColors[item.rareLevel]">
                      {{ rareLevelLabels[item.rareLevel] }}
                    </span>
                  </div>
                  <div class="flex items-center justify-between">
                    <span class="text-[10px] text-slate-500">
                      {{ item.itemType === 'WEAPON' ? '⚔️ 武器' : '🪄 技能' }} ·
                      {{ PROFESSION_MAP[item.profession]?.label ?? item.profession }}
                    </span>
                    <span class="text-[10px] font-mono text-slate-400">×{{ item.count }}</span>
                  </div>
                </button>
              </div>
            </div>

            <!-- 右：熔炼确认 -->
            <div class="w-52 shrink-0 flex flex-col gap-4">
              <p class="text-[10px] text-slate-500 uppercase tracking-widest">熔炼详情</p>

              <div v-if="smeltSelected" class="flex-1 flex flex-col gap-3">
                <div class="p-3 rounded-xl border border-orange-500/30 bg-orange-900/10">
                  <p class="font-bold text-white text-sm mb-1">{{ smeltSelected.name }}</p>
                  <p class="text-[10px] text-slate-400 leading-relaxed">{{ smeltSelected.description }}</p>
                  <div class="mt-2 flex items-center gap-2">
                    <span class="text-[10px] font-bold" :class="rareLevelColors[smeltSelected.rareLevel]">
                      {{ rareLevelLabels[smeltSelected.rareLevel] }}
                    </span>
                    <span class="text-slate-600 text-[10px]">·</span>
                    <span class="text-[10px] text-slate-400">×{{ smeltSelected.count }}</span>
                  </div>
                </div>

                <div class="p-3 rounded-xl border border-amber-500/20 bg-amber-900/10 flex items-center gap-2">
                  <span class="text-lg">🪙</span>
                  <div>
                    <p class="text-[10px] text-slate-500">熔炼可得</p>
                    <p class="font-black text-amber-300 text-lg">+{{ smeltReward }}</p>
                  </div>
                </div>

                <!-- 熔炼结果 flash -->
                <Transition name="fade">
                  <div v-if="smeltResult"
                       class="p-2 rounded-lg bg-emerald-900/20 border border-emerald-500/30
                              text-emerald-400 text-xs text-center font-bold">
                    ✅ +{{ smeltResult.earned }} 碎片
                  </div>
                </Transition>
                <Transition name="fade">
                  <div v-if="smeltError"
                       class="p-2 rounded-lg bg-red-900/20 border border-red-500/30
                              text-red-400 text-xs text-center">
                    ❌ {{ smeltError }}
                  </div>
                </Transition>

                <button @click="handleSmelt" :disabled="isSmelting"
                        class="mt-auto py-2.5 rounded-lg font-black text-sm uppercase tracking-wider
                               transition-all disabled:opacity-40 disabled:cursor-not-allowed
                               bg-orange-600 hover:bg-orange-500 text-white
                               shadow-[0_0_20px_rgba(234,88,12,0.3)]">
                  <span v-if="isSmelting">熔炼中…</span>
                  <span v-else>🔥 确认熔炼</span>
                </button>
              </div>

              <div v-else class="flex-1 flex flex-col items-center justify-center gap-3
                                  text-slate-700 text-center">
                <span class="text-3xl opacity-30">🔥</span>
                <p class="text-xs">点击左侧卡牌<br>选择要熔炼的物品</p>
              </div>
            </div>
          </div>

          <!-- ══════════════════════════════════════════════════════════════════
               换取
          ══════════════════════════════════════════════════════════════════ -->
          <div v-else-if="activeTab === 'exchange'" class="flex min-h-[480px]">

            <!-- 左侧子菜单 -->
            <aside class="w-36 shrink-0 border-r border-white/5 flex flex-col py-4 gap-1 px-2">
              <button v-for="sec in [
                { key: 'daily',      icon: '🗓️', label: '每日商店' },
                { key: 'profession', icon: '⚔️', label: '职业抽卡' },
                { key: 'rarity',     icon: '💎', label: '稀有度抽' },
              ]" :key="sec.key"
                @click="exchangeSection = sec.key as ExchangeSection"
                class="flex items-center gap-2 px-3 py-2.5 rounded-lg text-xs font-bold transition-all"
                :class="exchangeSection === sec.key
                  ? 'bg-indigo-600 text-white'
                  : 'text-slate-500 hover:text-white hover:bg-white/5'">
                <span>{{ sec.icon }}</span>
                <span>{{ sec.label }}</span>
              </button>
            </aside>

            <!-- 右侧内容 -->
            <div class="flex-1 min-w-0 p-5 overflow-y-auto custom-scrollbar">

              <!-- ──────────── 每日商店 ──────────── -->
              <div v-if="exchangeSection === 'daily'">
                <div class="flex items-center justify-between mb-4">
                  <div>
                    <h3 class="font-black text-white text-base">每日轮换商店</h3>
                    <p class="text-[10px] text-slate-500 mt-0.5">
                      系统刷新倒计时：<span class="font-mono text-indigo-400">{{ countdown || '—' }}</span>
                    </p>
                  </div>
                  <button @click="handleManualRefresh" :disabled="isRefreshing"
                          class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border transition-all text-xs
                                 font-bold disabled:opacity-40 disabled:cursor-not-allowed
                                 border-slate-600 text-slate-300 hover:border-indigo-500 hover:text-white">
                    <span :class="{ 'animate-spin': isRefreshing }">🔄</span>
                    <span>手动刷新</span>
                    <span class="text-amber-400 font-mono">(-{{ dailyShop?.manualRefreshCost ?? 20 }})</span>
                  </button>
                </div>

                <!-- 消息 -->
                <Transition name="fade">
                  <div v-if="shopSuccess"
                       class="mb-3 p-2 rounded-lg bg-emerald-900/20 border border-emerald-500/30
                              text-emerald-400 text-xs text-center font-bold">
                    {{ shopSuccess }}
                  </div>
                </Transition>
                <Transition name="fade">
                  <div v-if="shopError"
                       class="mb-3 p-2 rounded-lg bg-red-900/20 border border-red-500/30
                              text-red-400 text-xs text-center">
                    ❌ {{ shopError }}
                  </div>
                </Transition>

                <!-- 加载中 -->
                <div v-if="isShopLoading" class="flex items-center justify-center h-40">
                  <div class="w-8 h-8 border-2 border-indigo-500 border-t-transparent rounded-full animate-spin"/>
                </div>

                <!-- 4 槽位 -->
                <div v-else class="grid grid-cols-4 gap-3">
                  <div v-for="index in [0,1,2,3]" :key="index">
                    <!-- 有效槽位 -->
                    <template v-if="dailyShop?.slots.find(s => s.slotIndex === index && !s.isPurchased)">
                      <div v-for="slot in [dailyShop!.slots.find(s => s.slotIndex === index && !s.isPurchased)!]"
                           :key="slot.id"
                           class="rounded-xl border p-3 flex flex-col gap-2 transition-all"
                           :class="slot.isLocked
                             ? 'border-amber-500/50 bg-amber-900/10'
                             : 'border-slate-700 bg-slate-900/60'">

                        <!-- 物品名 + 稀有度 -->
                        <div class="flex items-start justify-between gap-1">
                          <span class="font-bold text-white text-xs leading-tight">{{ slot.item.name }}</span>
                          <span class="text-[10px] font-bold shrink-0"
                                :class="rareLevelColors[slot.item.rareLevel]">
                            {{ rareLevelLabels[slot.item.rareLevel] }}
                          </span>
                        </div>

                        <!-- 职业 + 类型 -->
                        <p class="text-[10px] text-slate-500">
                          {{ PROFESSION_MAP[slot.item.profession]?.label ?? slot.item.profession }} ·
                          {{ slot.itemType === 'WEAPON' ? '武器' : '技能' }}
                        </p>

                        <!-- 锁定 & 购买 -->
                        <div class="flex gap-1.5 mt-auto pt-1">
                          <button @click="handleLock(slot)"
                                  class="flex-1 py-1 rounded text-[10px] font-bold transition-all border"
                                  :class="slot.isLocked
                                    ? 'bg-amber-600 border-amber-500 text-white'
                                    : 'bg-slate-800 border-slate-700 text-slate-400 hover:text-white'">
                            {{ slot.isLocked ? '🔒 已锁' : '🔓 锁定' }}
                          </button>
                          <button @click="handlePurchase(slot)" :disabled="isPurchasing"
                                  class="flex-1 py-1 rounded text-[10px] font-bold transition-all
                                         bg-indigo-600 hover:bg-indigo-500 text-white
                                         disabled:opacity-40 disabled:cursor-not-allowed">
                            🪙{{ slot.price }}
                          </button>
                        </div>
                      </div>
                    </template>

                    <!-- 空槽位（已购买或无） -->
                    <div v-else
                         class="rounded-xl border border-dashed border-slate-800
                                h-[108px] flex items-center justify-center text-slate-700 text-xs">
                      —
                    </div>
                  </div>
                </div>
              </div>

              <!-- ──────────── 职业抽卡 ──────────── -->
              <div v-else-if="exchangeSection === 'profession'">
                <div class="mb-4">
                  <h3 class="font-black text-white text-base">职业抽卡</h3>
                  <p class="text-[10px] text-slate-500 mt-0.5">每次消耗 40 碎片，稀有度随机，职业固定</p>
                </div>

                <!-- 消息 -->
                <Transition name="fade">
                  <div v-if="drawError"
                       class="mb-3 p-2 rounded-lg bg-red-900/20 border border-red-500/30
                              text-red-400 text-xs text-center">
                    ❌ {{ drawError }}
                  </div>
                </Transition>

                <div class="grid grid-cols-2 gap-3 mb-4">
                  <button v-for="prof in professions" :key="prof.key"
                          @click="handleProfDraw(prof.key)" :disabled="isDrawing"
                          class="flex items-center gap-3 p-4 rounded-xl border transition-all
                                 disabled:opacity-40 disabled:cursor-not-allowed group"
                          :style="{ borderColor: `${PROFESSION_MAP[prof.key]?.badgeBorder?.replace('border-', '') ?? 'rgba(255,255,255,0.1)'}` }"
                          :class="[
                            PROFESSION_MAP[prof.key]?.headerBg ?? 'bg-slate-900/60',
                            'hover:brightness-125 active:scale-95'
                          ]">
                    <span class="text-2xl">{{ PROFESSION_MAP[prof.key]?.icon ?? prof.icon }}</span>
                    <div class="flex-1 text-left">
                      <p class="font-black text-white text-sm">{{ prof.label }}</p>
                      <p class="text-[10px] mt-0.5" :class="PROFESSION_MAP[prof.key]?.badgeText ?? 'text-slate-400'">
                        🪙 {{ prof.cost }} 碎片
                      </p>
                    </div>
                    <span class="text-slate-500 group-hover:text-white transition-colors text-sm">▶</span>
                  </button>
                </div>

                <!-- 抽卡结果 -->
                <Transition name="draw-result">
                  <div v-if="drawResult"
                       class="p-4 rounded-xl border border-indigo-500/40 bg-indigo-900/20">
                    <p class="text-[10px] text-indigo-400 uppercase tracking-widest mb-2">✨ 抽卡结果</p>
                    <div class="flex items-center gap-3">
                      <div class="w-10 h-10 rounded-lg bg-indigo-900/40 flex items-center justify-center text-xl">
                        {{ drawResult.itemType === 'WEAPON' ? '⚔️' : '🪄' }}
                      </div>
                      <div class="flex-1">
                        <div class="flex items-center gap-2">
                          <p class="font-black text-white text-sm">{{ drawResult.item.name }}</p>
                          <span class="text-[10px] font-bold" :class="rareLevelColors[drawResult.item.rareLevel]">
                            {{ rareLevelLabels[drawResult.item.rareLevel] }}
                          </span>
                          <span v-if="drawResult.item.isUnique"
                                class="px-1 rounded text-[9px] font-bold bg-amber-900/40 text-amber-400 border border-amber-500/30">
                            传奇
                          </span>
                        </div>
                        <p class="text-[10px] text-slate-400 mt-0.5">{{ drawResult.item.description }}</p>
                      </div>
                      <div class="text-right">
                        <p class="text-[10px] text-slate-500">余额</p>
                        <p class="font-mono font-bold text-amber-300 text-sm">🪙 {{ drawResult.newBalance }}</p>
                      </div>
                    </div>
                  </div>
                </Transition>
              </div>

              <!-- ──────────── 稀有度抽 ──────────── -->
              <div v-else-if="exchangeSection === 'rarity'">
                <div class="mb-4">
                  <h3 class="font-black text-white text-base">稀有度抽卡</h3>
                  <p class="text-[10px] text-slate-500 mt-0.5">职业与类型随机，稀有度固定</p>
                </div>

                <!-- 消息 -->
                <Transition name="fade">
                  <div v-if="drawError"
                       class="mb-3 p-2 rounded-lg bg-red-900/20 border border-red-500/30
                              text-red-400 text-xs text-center">
                    ❌ {{ drawError }}
                  </div>
                </Transition>

                <div class="grid grid-cols-2 gap-3 mb-4">
                  <button v-for="r in rarities" :key="r.level"
                          @click="handleRarityDraw(r.level)"
                          :disabled="isDrawing || (r.level === 4 && allR4Owned)"
                          class="flex items-center justify-between p-4 rounded-xl border transition-all
                                 disabled:opacity-40 disabled:cursor-not-allowed
                                 hover:brightness-125 active:scale-95"
                          :class="[r.border, r.bg]">
                    <div>
                      <p class="font-black text-sm" :class="r.color">{{ r.label }}</p>
                      <p class="text-[10px] text-slate-500 mt-0.5">
                        <span v-if="r.level === 4 && allR4Owned" class="text-amber-500">✦ 已集齐</span>
                        <span v-else>🪙 {{ r.cost }} 碎片</span>
                      </p>
                    </div>
                    <span class="text-xl" :class="r.color">{{ ['','◇','◆','◈','✦'][r.level] }}</span>
                  </button>
                </div>

                <!-- 抽卡结果 -->
                <Transition name="draw-result">
                  <div v-if="drawResult"
                       class="p-4 rounded-xl border border-indigo-500/40 bg-indigo-900/20">
                    <p class="text-[10px] text-indigo-400 uppercase tracking-widest mb-2">✨ 抽卡结果</p>
                    <div class="flex items-center gap-3">
                      <div class="w-10 h-10 rounded-lg bg-indigo-900/40 flex items-center justify-center text-xl">
                        {{ drawResult.itemType === 'WEAPON' ? '⚔️' : '🪄' }}
                      </div>
                      <div class="flex-1">
                        <div class="flex items-center gap-2">
                          <p class="font-black text-white text-sm">{{ drawResult.item.name }}</p>
                          <span class="text-[10px] font-bold" :class="rareLevelColors[drawResult.item.rareLevel]">
                            {{ rareLevelLabels[drawResult.item.rareLevel] }}
                          </span>
                          <span v-if="drawResult.item.isUnique"
                                class="px-1 rounded text-[9px] font-bold bg-amber-900/40 text-amber-400 border border-amber-500/30">
                            传奇
                          </span>
                        </div>
                        <p class="text-[10px] text-slate-400 mt-0.5">{{ drawResult.item.description }}</p>
                      </div>
                      <div class="text-right">
                        <p class="text-[10px] text-slate-500">余额</p>
                        <p class="font-mono font-bold text-amber-300 text-sm">🪙 {{ drawResult.newBalance }}</p>
                      </div>
                    </div>
                  </div>
                </Transition>
              </div>

            </div>
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

.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

.draw-result-enter-active { animation: slide-in 0.35s cubic-bezier(0.25, 0.8, 0.25, 1); }
.draw-result-leave-active { transition: opacity 0.2s; }
.draw-result-leave-to { opacity: 0; }

@keyframes slide-in {
  from { transform: translateY(12px); opacity: 0; }
  to   { transform: translateY(0);    opacity: 1; }
}
</style>
