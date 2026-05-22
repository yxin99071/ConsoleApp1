import api from './auth'
import type { AwardListDto } from '../types/award'

export const getAwardList = async (): Promise<AwardListDto[]> => {
  const res = await api.get<AwardListDto[]>('/battle/awards')
  return res.data
}

export const getAwardCount = async (): Promise<number> => {
  const res = await api.get<number>('/battle/awards/count')
  return res.data
}

export const claimAward = async (awardListId: number, itemId: number): Promise<void> => {
  await api.post('/battle/awards/claim', { awardListId, itemId })
}
