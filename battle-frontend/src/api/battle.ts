import api from './auth'; // 使用之前封装好的带拦截器的 axios 实例
import type { InformationDto,InitProfileDto,FightRequestDto } from '../types/battle'; // 假设之前的 InformationDto 在这里

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

export const postFight = async (attackerId:string|undefined,defenderId:string|undefined,historyId:string|undefined) => {
  try {
    
    const data:FightRequestDto = 
    {
        attacker:attackerId?.toString(),
        defender:defenderId?.toString(),
        history:historyId?.toString()      
    }
    // 这里的路径根据你的 Vite/Webpack 配置的 proxy 或 baseURL 决定
    const response = await api.post('/battle/fight', data);
    
    // 按照你的要求，如果返回为空或异常，由组件层处理
    return response.data;
  } catch (error) {
    console.error('API [postFight] Error:', error);
    throw error; // 抛出错误供组件内的 try-catch 捕获
  }
};

export const initProfile =async (data: InitProfileDto) => {
  const res = await api.post('user/init', data);
  return res.data;
};