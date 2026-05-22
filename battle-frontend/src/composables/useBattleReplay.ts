import { ref, watch, type Ref } from 'vue'
import { applyEvent, emptyState, type BattleState } from '../engine/battleState'
import type { BattleEvent } from '../types/battleEvents'

export interface LogEntry {
  id: number
  text: string
  category: 'action' | 'damage' | 'heal' | 'buff' | 'system' | 'dodge'
  depth: number
}

// Milliseconds each event type "holds the screen" at 1× speed.
const DELAYS: Partial<Record<BattleEvent['type'], number>> & { default: number } = {
  Action:     700,
  Damage:     400,
  Healing:    300,
  Dodge:      400,
  Passive:    500,
  BuffApply:  200,
  BuffTick:   350,
  RoundBegin: 450,
  BattleEnd:  400,
  default:    200,
}

type LogBuilder = (e: any) => { text: string; category: LogEntry['category'] } | null

const LOG_BUILDERS: Partial<Record<BattleEvent['type'], LogBuilder>> = {
  RoundBegin:    e => ({ text: `── ${e.data.Unit} 的回合 ──`, category: 'system' }),
  Action:        e => ({ text: `${e.data.Actor} 使用了 ${e.data.Name}`, category: 'action' }),
  Damage:        e => ({ text: `${e.data.Target} 受到 ${e.data.Value} 点伤害，剩余 ${e.data.HP} HP${e.data.Critical ? '（暴击！）' : ''}`, category: 'damage' }),
  Healing:       e => ({ text: `${e.data.Target} 恢复 ${e.data.Value} 点生命，当前 ${e.data.HP} HP`, category: 'heal' }),
  Dodge:         e => ({ text: `${e.data.Target} 闪避了攻击`, category: 'dodge' }),
  Passive:       e => ({ text: `${e.data.Unit} 触发被动：${e.data.SkillName}`, category: 'buff' }),
  BuffApply:     e => ({ text: `${e.data.Target} 获得效果 [${e.data.BuffName}]（Lv.${e.data.BuffLevel}，持续 ${e.data.LastRound} 回合）`, category: 'buff' }),
  BuffTick:      e => ({ text: `${e.data.Unit} 受到 [${e.data.BuffName}] 持续伤害 ${e.data.Damage} 点，剩余 ${e.data.HP} HP`, category: 'damage' }),
  BuffExpire:    e => ({ text: `[${e.data.BuffName}] 效果消散`, category: 'buff' }),
  ReactionBegin: e => ({ text: `↳ ${e.data.Actor} 触发${e.data.Type}`, category: 'system' }),
  BattleEnd:     e => ({ text: `${e.data.Winner} 获得胜利`, category: 'system' }),
}

export function useBattleReplay(battleData: Ref<BattleEvent[]>) {
  const state = ref<BattleState>(emptyState())
  const logs = ref<LogEntry[]>([])
  const currentIndex = ref(0)
  const isPlaying = ref(false)
  const speed = ref<1 | 2>(1)
  let _id = 0

  const wait = (ms: number) => new Promise<void>(r => setTimeout(r, ms / speed.value))

  function pushLog(event: BattleEvent) {
    const builder = LOG_BUILDERS[event.type]
    if (!builder) return
    const result = builder(event)
    if (result) logs.value.push({ id: _id++, ...result, depth: event.depth })
  }

  // Re-initialize whenever the battle data array is replaced.
  watch(
    battleData,
    (data) => {
      if (!data?.length) return
      const start = data.find(e => e.type === 'BattleStart')
      if (!start) return
      state.value = applyEvent(emptyState(), start)
      logs.value = [{ id: _id++, text: '战斗开始', category: 'system', depth: 0 }]
      currentIndex.value = 0
      isPlaying.value = false
    },
    { immediate: true },
  )

  async function play() {
    if (isPlaying.value) return
    isPlaying.value = true

    for (; currentIndex.value < battleData.value.length; currentIndex.value++) {
      if (!isPlaying.value) break // allow pause via stop()
      const event = battleData.value[currentIndex.value]
      if (!event) continue
      if (event.type === 'BattleStart') continue

      state.value = applyEvent(state.value, event)
      pushLog(event)

      const ms = DELAYS[event.type] ?? DELAYS.default
      if (ms > 0) await wait(ms)
    }

    isPlaying.value = false
  }

  function stop() {
    isPlaying.value = false
  }

  // Skip all remaining events instantly; no animations.
  function fastForward() {
    isPlaying.value = false
    for (let i = currentIndex.value; i < battleData.value.length; i++) {
      const event = battleData.value[i]
      if (!event) continue
      if (event.type === 'BattleStart') continue
      state.value = applyEvent(state.value, event)
      pushLog(event)
    }
    currentIndex.value = battleData.value.length
  }

  function reset() {
    isPlaying.value = false
    const start = battleData.value.find(e => e.type === 'BattleStart')
    state.value = start ? applyEvent(emptyState(), start) : emptyState()
    logs.value = [{ id: _id++, text: '战斗开始', category: 'system', depth: 0 }]
    currentIndex.value = 0
  }

  function toggleSpeed() {
    speed.value = speed.value === 1 ? 2 : 1
  }

  return { state, logs, currentIndex, isPlaying, speed, play, stop, fastForward, reset, toggleSpeed }
}
