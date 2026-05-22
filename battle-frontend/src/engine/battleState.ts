import type { BattleEvent, BattleStartData, RawBuffDef } from '../types/battleEvents'

export interface ActiveBuff {
  name: string
  level: number
  remainRound: number
  isBuff: boolean
  isDeBuff: boolean
  isDamage: boolean
  description: string
}

export interface PlayerState {
  id: number
  name: string
  profession: string
  maxHealth: number
  currentHealth: number
  weapons: string[]
  skills: string[]
  activeBuffs: ActiveBuff[]
}

export interface BattleState {
  players: PlayerState[]
  buffPool: Map<number, RawBuffDef>
  round: number
  phase: 'idle' | 'playing' | 'finished'
  winner: string | null
  currentActor: string | null
  currentActionName: string | null
}

export function emptyState(): BattleState {
  return {
    players: [],
    buffPool: new Map(),
    round: 0,
    phase: 'idle',
    winner: null,
    currentActor: null,
    currentActionName: null,
  }
}

function fromBattleStart(data: BattleStartData): BattleState {
  const buffPool = new Map<number, RawBuffDef>()
  for (const b of (data.BuffLibrary ?? [])) buffPool.set(b.id, b)

  const players: PlayerState[] = (data.Players ?? []).map(p => ({
    id: p.id,
    name: p.name,
    profession: p.profession ?? '',
    maxHealth: p.stats?.maxHealth ?? 0,
    currentHealth: p.stats?.maxHealth ?? 0,
    weapons: (p.weapons ?? []).map(w => w.name),
    skills: (p.skills ?? []).map(s => s.name),
    activeBuffs: [],
  }))

  return { ...emptyState(), players, buffPool, phase: 'playing' }
}

function patch(
  players: PlayerState[],
  name: string,
  fn: (p: PlayerState) => PlayerState,
): PlayerState[] {
  return players.map(p => (p.name === name ? fn(p) : p))
}

// Pure reducer — no side effects, no async, no Vue imports.
// Returns a new BattleState; the old one is never mutated.
export function applyEvent(state: BattleState, event: BattleEvent): BattleState {
  switch (event.type) {
    case 'BattleStart':
      return fromBattleStart(event.data)

    case 'BattleEnd':
      return { ...state, phase: 'finished', winner: event.data.Winner, currentActor: null, currentActionName: null }

    case 'RoundBegin':
      return { ...state, round: state.round + 1, currentActor: event.data.Unit, currentActionName: null }

    case 'Action':
      return { ...state, currentActor: event.data.Actor, currentActionName: event.data.Name }

    case 'Damage':
      return { ...state, players: patch(state.players, event.data.Target, p => ({ ...p, currentHealth: event.data.HP })) }

    case 'Healing':
      return { ...state, players: patch(state.players, event.data.Target, p => ({ ...p, currentHealth: event.data.HP })) }

    case 'BuffTick':
      return { ...state, players: patch(state.players, event.data.Unit, p => ({ ...p, currentHealth: event.data.HP })) }

    case 'BuffApply': {
      const def = Array.from(state.buffPool.values()).find(b => b.name === event.data.BuffName)
      if (!def) return state
      const incoming: ActiveBuff = {
        name: event.data.BuffName,
        level: event.data.BuffLevel,
        remainRound: event.data.LastRound,
        isBuff: def.isBuff,
        isDeBuff: def.isDeBuff,
        isDamage: def.isDamage,
        description: def.description,
      }
      return {
        ...state,
        players: patch(state.players, event.data.Target, p => ({
          ...p,
          activeBuffs: [...p.activeBuffs.filter(b => b.name !== event.data.BuffName), incoming],
        })),
      }
    }

    case 'BuffUpdate':
      return {
        ...state,
        players: patch(state.players, event.data.Unit, p => ({
          ...p,
          activeBuffs: p.activeBuffs.map(b =>
            b.name === event.data.BuffName
              ? { ...b, remainRound: event.data.Remain }
              : b,
          ),
        })),
      }

    case 'BuffExpire':
      return {
        ...state,
        players: patch(state.players, event.data.Target, p => ({
          ...p,
          activeBuffs: p.activeBuffs.filter(b => b.name !== event.data.BuffName),
        })),
      }

    default:
      return state
  }
}
