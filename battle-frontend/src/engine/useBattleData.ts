export function useBattleData(rawJson: any) {
  // 1. 建立 Buff 全局索引字典 (Map)
  const buffMap = new Map();
  rawJson.data.BuffLibrary.forEach((b: any) => {
    buffMap.set(b.name, {
      name: b.name,
      description: b.description,
      isBuff: b.isBuff,
      isDeBuff: b.isDeBuff,
      isDamage: b.isDamage,
      lastRound: 0 // 初始状态默认为 0
    });
  });

  /**
   * 2. 内部转换函数：将原始武器/技能映射为 ItemDto
   */
  const mapToItemDto = (item: any) => ({
    name: item.name,
    profession: item.profession,
    secondProfession: item.secondProfession,
    description: item.description,
    rareLevel: item.rareLevel,
    // 通过 ID 数组从 BuffLibrary 匹配详细信息
    // 注意：原始数据里武器用的是 ID，战斗流里用的是 Name，这里兼容处理
    buffs: item.buffIds.map((id: number) => {
      const buffInLib = rawJson.data.BuffLibrary.find((lib: any) => lib.id === id);
      return buffInLib ? buffMap.get(buffInLib.name) : null;
    }).filter(Boolean)
  });

  /**
   * 3. 静态角色快照
   * 将玩家 1 和 玩家 2 解析为 attacker 和 defender
   */
  const attackerStatic = {
    name: rawJson.data.Players[0].name,
    stats: rawJson.data.Players[0].stats,
    weapons: rawJson.data.Players[0].weapons.map(mapToItemDto),
    skills: rawJson.data.Players[0].skills.map(mapToItemDto)
  };

  const defenderStatic = {
    name: rawJson.data.Players[1].name,
    stats: rawJson.data.Players[1].stats,
    weapons: rawJson.data.Players[1].weapons.map(mapToItemDto),
    skills: rawJson.data.Players[1].skills.map(mapToItemDto)
  };

  return {
    buffMap,        // 用于战斗过程中根据 BuffName 快速查表
    attackerStatic, // 用于 FighterInfo 初始化展示
    defenderStatic  // 用于 FighterInfo 初始化展示
  };
}