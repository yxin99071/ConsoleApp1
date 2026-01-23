export interface BattleEvent {
  Type: string;
  Depth: number;
  Data: any;
}

// 这里的 Player 接口可以根据 JSON 结构进一步细化
export interface Player {
  Id: number;
  Name: string;
  Stats: { MaxHealth: number; Agility: number; Strength: number; Intelligence: number };
  Weapons: any[];
  Skills: any[];
  // 运行时状态
  CurrentHP: number;
  ActiveBuffs: { name: string; level: number; rounds: number }[]; 
}

export interface Buff {
  name: string;
  description: string;
}

// 2. 定义武器和技能的通用结构
export interface GameItem {
  name: string;
  profession: string;
  description: string;
  buffs: Buff[];
}

// 3. 定义主用户数据的结构
export interface UserProfile {
  name: string;
  exp: number;
  level: number;
  profession: string;
  secondProfession: string | null;
  skillDTO: GameItem[];
  weaponDTO: GameItem[];
}