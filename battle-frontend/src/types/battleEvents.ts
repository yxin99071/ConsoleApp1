export interface RawBuffDef {
  id: number
  name: string
  isBuff: boolean
  isDeBuff: boolean
  isDamage: boolean
  description: string
}

export interface RawItemData {
  name: string
  profession: string
  secondProfession?: string
  description: string
  rareLevel: number
  buffIds: number[]
}

export interface RawSkillData extends RawItemData {
  isPassive: boolean
}

export interface RawPlayerData {
  id: number
  name: string
  profession: string
  stats: { maxHealth: number }
  weapons: RawItemData[]
  skills: RawSkillData[]
}

export interface BattleStartData {
  Players: RawPlayerData[]
  BuffLibrary: RawBuffDef[]
}

// Discriminated union — every event type has a known data shape.
// Add new event types here when the backend adds them.
export type BattleEvent =
  | { type: 'BattleStart';   depth: 0;      data: BattleStartData }
  | { type: 'BattleEnd'; depth: 0; data: {
      Winner: string
      UserId: number
      UserName: string
      LevelChange: { From: number; To: number; IsLeveledUp: boolean }
      ExperienceChange: { Before: number; After: number; Gained: number }
      StatsChange: {
        Health:       { From: number; To: number }
        Strength:     { From: number; To: number }
        Agility:      { From: number; To: number }
        Intelligence: { From: number; To: number }
      }
    }}
  | { type: 'RoundBegin';    depth: number; data: { Unit: string } }
  | { type: 'Action';        depth: number; data: { Actor: string; Type: string; Name: string } }
  | { type: 'Damage';        depth: number; data: { Target: string; Value: number; HP: number; Critical?: boolean } }
  | { type: 'Healing';       depth: number; data: { Target: string; Value: number; HP: number } }
  | { type: 'BuffApply';     depth: number; data: { Target: string; BuffName: string; BuffLevel: number; LastRound: number } }
  | { type: 'BuffUpdate';    depth: number; data: { Unit: string; BuffName: string; Remain: number } }
  | { type: 'BuffTick';      depth: number; data: { Unit: string; BuffName: string; Damage: number; HP: number } }
  | { type: 'BuffExpire';    depth: number; data: { Target: string; BuffName: string } }
  | { type: 'Dodge';         depth: number; data: { Target: string } }
  | { type: 'Passive';       depth: number; data: { Unit: string; SkillName: string } }
  | { type: 'ReactionBegin'; depth: number; data: { Actor: string; Type: string } }

export function castBattleEvents(raw: unknown[]): BattleEvent[] {
  return raw as BattleEvent[]
}
