// utils/battleLogic.ts
import type { BuffSummaryDto, ItemDto ,PlayerBattleInstance} from '../types/battle';

/**
 * 解析 API 返回的原始 JSON 建立资源池
 */
export function buildStaticResourcePool(startNode: any) {
  const content = startNode?.data || {};
  const buffLibrary = content.BuffLibrary || [];
  const players = content.Players || [];

  const buffPool = new Map<number, BuffSummaryDto>();
  
  // 显式标注 b 为 any 或具体的后端原始类型
  buffLibrary.forEach((b: any) => { 
    buffPool.set(b.id, {
      name: b.name,
      isBuff: b.isBuff,
      isDeBuff: b.isDeBuff,
      isDamage: b.isDamage,
      lastRound: b.LastRound ?? b.lastRound ?? 1,
      description: b.description,
    });
  });

  const itemMap = new Map<string, ItemDto>();
  players.forEach((p: any) => {
    const allItems = [
      ...(p.weapons || []).map((w: any) => ({ ...w, type: '武器' })),
      ...(p.skills || []).map((s: any) => ({ ...s, type: '技能' }))
    ];

    allItems.forEach((item: any) => {
      if (!itemMap.has(item.name)) {
        itemMap.set(item.name, {
          id: item.id ?? 0,
          name: item.name,
          profession: item.profession,
          secondProfession: item.secondProfession,
          description: item.description,
          rareLevel: item.rareLevel,
          isPassive: item.isPassive,
          // 这里 b 也需要标注
          buffs: (item.buffIds || [])
            .map((id: number) => buffPool.get(id))
            .filter((b: BuffSummaryDto | undefined): b is BuffSummaryDto => !!b)
        });
      }
    });
  });

  return { buffPool, itemMap };
}
/**
 * 提取角色初始状态
 */
export function initPlayerState(startNode: any): PlayerBattleInstance[] {
  const content = startNode?.data || {};
  const players = content.Players || [];

  return players.map((p: any) => ({
    id: p.id,
    name: p.name,
    maxHealth: p.stats?.maxHealth || 0,
    currentHealth: p.stats?.maxHealth || 0,
    // 只存名字数组，减少响应式压力
    weapons: (p.weapons || []).map((w: any) => w.name),
    skills: (p.skills || []).map((s: any) => s.name),
    // 初始 Buff 栏为空
    activeBuffs: []
  }));
}