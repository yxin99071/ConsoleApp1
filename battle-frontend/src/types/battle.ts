// 1. 基础 Buff 信息
export interface BuffSummaryDto {
  name: string;
  isBuff: boolean;
  isDeBuff: boolean;
  isDamage: boolean;
  lastRound: number;
  description: string;
}
export interface FightRequestDto {
  attacker: string|undefined; // 对应 attackerId
  defender: string|undefined; // 对应 defenderId
  history: string|undefined;//对应历史对战
}

export interface PlayerBattleInstance {
  id: number;
  name: string;
  maxHealth: number;
  currentHealth: number;
  weapons: string[];
  skills: string[];
  activeBuffs: BuffSummaryDto[]; // 确保这里引用的是接口名，而不是手动写的对象结构
}

// 2. 技能与武器的基类 (对应 C# 的 ItemDto)
export interface ItemDto {
  name: string;
  profession: string;
  secondProfession?: string;
  description: string;
  buffs: BuffSummaryDto[];
  isPassive?: boolean;
  rareLevel: number;
}

// 3. 具体实现 (目前字段与基类一致)
export interface SkillDto extends ItemDto {
    isPassive:boolean
}
export interface WeaponDto extends ItemDto {}

// 4. 玩家/大厅个人信息 (对应 InformationDto)
export interface InformationDto {
  id:string;
  name: string;
  exp: number;
  level: number;
  health: number;
  agility: number;
  strength: number;
  intelligence: number;
  profession?: string;
  secondProfession?: string;
  //以上为基本信息
  skills: SkillDto[];
  weapons: WeaponDto[];
}

export interface InitProfileDto {
  name: string;
  account: string;
  profession: string;
  secondProfession: string | null;
  initialSkills: string[];
}