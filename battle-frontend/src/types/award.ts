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
  buffs: BuffSummaryDto[]
}

export interface AwardListDto {
  id: number
  type: 'WEAPON' | 'SKILL'
  awardLevel: number
  items: AwardItemDto[]
}
