import api from './auth'
import type {
  DailyShopDto, InventoryDto,
  DrawResultDto, SmeltResultDto,
} from '../types/shop'
import type { ShopSlotDto } from '../types/shop'

// ── 每日商店 ──────────────────────────────────────────────────────────────────

export const getDailyShop = async (): Promise<DailyShopDto> => {
  const res = await api.get<DailyShopDto>('/shop/daily')
  return res.data
}

export const manualRefreshShop = async (): Promise<DailyShopDto> => {
  const res = await api.post<DailyShopDto>('/shop/daily/refresh')
  return res.data
}

export const lockSlot = async (slotId: number): Promise<DailyShopDto> => {
  const res = await api.post<DailyShopDto>('/shop/daily/lock', { slotId })
  return res.data
}

export const purchaseSlot = async (slotId: number): Promise<ShopSlotDto> => {
  const res = await api.post<ShopSlotDto>('/shop/daily/purchase', { slotId })
  return res.data
}

// ── 抽卡 ──────────────────────────────────────────────────────────────────────

export const drawByProfession = async (profession: string): Promise<DrawResultDto> => {
  const res = await api.post<DrawResultDto>('/shop/draw/profession', { profession })
  return res.data
}

export const drawByRarity = async (rarity: number): Promise<DrawResultDto> => {
  const res = await api.post<DrawResultDto>('/shop/draw/rarity', { rarity })
  return res.data
}

// ── 熔炼 ──────────────────────────────────────────────────────────────────────

export const smeltItem = async (itemType: string, itemId: number): Promise<SmeltResultDto> => {
  const res = await api.post<SmeltResultDto>('/shop/smelt', { itemType, itemId })
  return res.data
}

// ── 背包 ──────────────────────────────────────────────────────────────────────

export const getInventory = async (): Promise<InventoryDto> => {
  const res = await api.get<InventoryDto>('/shop/inventory')
  return res.data
}

export const getR4Status = async (): Promise<{ allR4Owned: boolean }> => {
  const res = await api.get<{ allR4Owned: boolean }>('/shop/r4-status')
  return res.data
}
