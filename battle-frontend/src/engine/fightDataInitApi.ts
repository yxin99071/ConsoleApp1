// utils/battleLogic.ts
import type { BuffSummaryDto, ItemDto ,PlayerBattleInstance} from '../types/battle';

/**
 * 解析 API 返回的原始 JSON 建立资源池
 */
export function buildStaticResourcePool(rawData: any) {
  const { BuffLibrary = [], Players = [] } = rawData.data || {};
  const buffPool = new Map<number, BuffSummaryDto>();
  const itemMap = new Map<string, ItemDto>();

  BuffLibrary.forEach((b: any) => {
    buffPool.set(b.id, {
      name: b.name,
      isBuff: b.isBuff,
      isDeBuff: b.isDeBuff,
      isDamage: b.isDamage,
      lastRound: b.LastRound || b.lastRound || 1,
      description: b.description,
    });
  });

  Players.forEach((p: any) => {
    [...(p.weapons || []), ...(p.skills || [])].forEach((item: any) => {
      if (!itemMap.has(item.name)) {
        itemMap.set(item.name, {
          ...item,
          buffs: (item.buffIds || []).map((id: number) => buffPool.get(id)).filter(Boolean)
        });
      }
    });
  });

  return { buffPool, itemMap };
}

/**
 * 提取角色初始状态
 */
export function initPlayerState(rawData: any): PlayerBattleInstance[] {
  const players = rawData.data?.Players || [];
  return players.map((p: any) => ({
    id: p.id,
    name: p.name,
    maxHealth: p.stats?.maxHealth || 0,
    currentHealth: p.stats?.maxHealth || 0,
    weapons: (p.weapons || []).map((w: any) => w.name),
    skills: (p.skills || []).map((s: any) => s.name),
    activeBuffs: []
  }));
}