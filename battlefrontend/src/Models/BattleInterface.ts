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