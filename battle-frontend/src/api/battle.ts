import api from './auth'; // 使用之前封装好的带拦截器的 axios 实例
import type { InformationDto,InitProfileDto } from '../types/battle'; // 假设之前的 InformationDto 在这里

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
  return res.data;
};

export const initProfile =async (data: InitProfileDto) => {
  const res = await api.post('/init', data);
  return res.data;
};