import api from './auth'; // 使用之前封装好的带拦截器的 axios 实例
import type { InformationDto, InitProfileDto, FightRequestDto } from '../types/battle';
import type { BattleEvent } from '../types/battleEvents';

// 简略玩家信息 DTO
export interface FighterDto {
  id: string;
  name: string;
  level: number;
  profession: string;
  secondProfession?:string;
}

// 1. 获取大厅玩家列表
export const getFighters = async (): Promise<FighterDto[]> => {
  const res = await api.get<FighterDto[]>('/user/fighters');
  return res.data;
};

// 2. 获取档案 (不传 id 查自己，传 id 查别人)
export const getProfile = async (id?: string): Promise<InformationDto> => {
  // 如果有 id 拼接到 query 参数，没有则不传
  const url = id ? `/user/profile?id=${id}` : '/user/profile';
  const res = await api.get<InformationDto>(url);
  if (!id && res.data) {
    localStorage.setItem('userProfession', res.data.profession || '');
    // 如果有其他需要同步的信息也可以写在这里
    // localStorage.setItem('userName', res.data.name);
  }

  return res.data;
};

export const postFight = async (
  attackerId: string | undefined,
  defenderId: string | undefined,
  historyId: string | undefined,
  deckWeaponIds?: number[],
  deckSkillIds?: number[]
) => {
  try {
    const data: FightRequestDto = {
      attacker: attackerId?.toString(),
      defender: defenderId?.toString(),
      history:  historyId?.toString(),
      deckWeaponIds,
      deckSkillIds,
    };
    const response = await api.post('/battle/fight', data);
    return response.data;
  } catch (error) {
    console.error('API [postFight] Error:', error);
    throw error;
  }
};

export const initProfile = async (data: InitProfileDto) => {
  const res = await api.post('user/init', data);
  return res.data;
};

// ── 历史对局 ──────────────────────────────────────────────

export interface BattleRecordDto {
  id:           number;
  isWin:        boolean;
  opponentName: string;
  createdTime:  string; // ISO 字符串
}

/** 获取当前用户的历史对局列表 */
export const getBattleList = async (): Promise<BattleRecordDto[]> => {
  const res = await api.get<BattleRecordDto[]>('/battle/battlelist');
  return res.data;
};

/** 获取某场对局的完整事件流，直接喂给 FightReviewer */
export const getBattleReplay = async (id: number): Promise<BattleEvent[]> => {
  const res = await api.post<BattleEvent[]>('/battle/replay', id, {
    headers: { 'Content-Type': 'application/json' },
  });
  return res.data;
};