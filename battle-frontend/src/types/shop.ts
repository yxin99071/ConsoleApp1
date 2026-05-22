import type { AwardItemDto } from './award'
import type { BuffSummaryDto } from './battle'

// ── 背包物品（带数量，用于熔炼 Tab）────────────────────────────────────────

export interface OwnedItemDto {
  id: number
  name: string
  description: string
  profession: string
  secondProfession?: string
  rareLevel: number
  isPassive: boolean
  isUnique: boolean
  count: number
  itemType: 'WEAPON' | 'SKILL'
  exclusiveGroup?: string | null
  /** 仅武器有效：SHARP / BLUNT / MAGIC */
  damageType?: string
  buffs: BuffSummaryDto[]
}

export interface InventoryDto {
  weapons: OwnedItemDto[]
  skills:  OwnedItemDto[]
  lotteryPoint: number
}

// ── 每日商店 ─────────────────────────────────────────────────────────────────

export interface ShopSlotDto {
  id:          number
  slotIndex:   number
  isLocked:    boolean
  isPurchased: boolean
  price:       number
  itemType:    'WEAPON' | 'SKILL'
  item:        AwardItemDto
}

export interface DailyShopDto {
  slots:             ShopSlotDto[]
  nextRefreshTime:   string   // ISO
  lotteryPoint:      number
  manualRefreshCost: number
}

// ── 抽卡 / 熔炼结果 ──────────────────────────────────────────────────────────

export interface DrawResultDto {
  itemType:   'WEAPON' | 'SKILL'
  item:       AwardItemDto
  newBalance: number
}

export interface SmeltResultDto {
  earned:     number
  newBalance: number
}
