import api from './auth';

export interface DefaultDeckDto {
  weaponIds: number[];
  skillIds:  number[];
  capacity:  number;
}

/** 获取当前用户的默认出战卡组 */
export const getDefaultDeck = async (): Promise<DefaultDeckDto> => {
  const res = await api.get<DefaultDeckDto>('/settings/default-deck');
  return res.data;
};

/** 保存当前用户的默认出战卡组 */
export const setDefaultDeck = async (dto: Omit<DefaultDeckDto, 'capacity'>): Promise<void> => {
  await api.post('/settings/default-deck', dto);
};
