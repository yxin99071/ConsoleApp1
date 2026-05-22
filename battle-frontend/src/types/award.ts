import type { BuffSummaryDto } from './battle'

export interface AwardItemDto {
  id: number
  name: string
  description: string
  profession: string
  secondProfession?: string
  rareLevel: number
  isPassive: boolean
  isUnique: boolean
  exclusiveGroup?: string | null
  /** 仅武器有效：SHARP / BLUNT / MAGIC */
  damageType?: string
  buffs: BuffSummaryDto[]
}

export interface AwardListDto {
  id: number
  type: 'WEAPON' | 'SKILL'
  awardLevel: number
  items: AwardItemDto[]
}
