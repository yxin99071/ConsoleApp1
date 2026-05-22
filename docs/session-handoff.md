# Session Handoff — 2026-05-22

> 给下一个对话的 Claude 看的交接文档。  
> 当前分支：`Deperation`  
> 最新 commit：`b6e2297`（已 push 到 GitHub）

---

## 一、技术栈总览

| 层 | 技术 |
|----|------|
| 后端 | ASP.NET Core Web API (.NET 8)，EF Core + SQLite，JWT 认证 |
| 数据库 | SQLite，路径 `Documents/MyProject/game.db`，**使用 `EnsureCreated()` 而非 Migrations**，改表结构必须删 `game.db` 重启 |
| 前端 | Vue 3 + TypeScript + Tailwind CSS + GSAP |
| 核心逻辑 | `BattleCore` 项目（独立类库，负责战斗模拟与 JSON 日志） |

**项目结构**
```
ConsoleApp1/
├── DataCore/           # 数据模型 + EF Context + DataHelper
│   ├── Models/         # User, Weapon, Skill, UserWeapon, UserSkill,
│   │                   # UserDailyShopSlot, BattleRecord, TempAwardList, ...
│   ├── Data/           # BattleDbContext.cs
│   └── Services/       # DataHelper.cs（DB 操作封装）
├── BattleCore/         # 战斗引擎（Fighter, BattleManager, BattleHelper, JsonLogger）
├── BattleBackend/      # Web API
│   ├── Controllers/    # BattleController, UserController, ShopController
│   ├── Services/       # BattleService, ShopService, JwtService
│   └── DTOs/           # 各种 DTO + MappingExtensions
└── battle-frontend/src/
    ├── api/            # auth.ts, battle.ts, award.ts, shop.ts
    ├── types/          # battle.ts, award.ts, shop.ts, battleEvents.ts
    ├── components/game/# BackpackView.vue, ItemCard.vue, CharacterCard.vue
    ├── components/combat/ # FightReviewer.vue, FighterInfo.vue
    ├── views/          # LobbyView, FightCenterView, LoginView, CreateCharacterView
    └── utils/          # constants.ts（PROFESSION_MAP）
```

---

## 二、本次对话完成的工作

### 1. 商店系统（完整实现）

**设计文档**：`docs/shop-system-design.md`

**新增文件**
- `DataCore/Models/UserDailyShopSlot.cs`
- `BattleBackend/DTOs/ShopDtos.cs`（DailyShopDto, ShopSlotDto, DrawResultDto, SmeltResultDto, InventoryDto, OwnedItemDto）
- `BattleBackend/Services/ShopService.cs`
- `BattleBackend/Controllers/ShopController.cs`
- `battle-frontend/src/types/shop.ts`
- `battle-frontend/src/api/shop.ts`

**修改文件**
- `DataCore/Models/User.cs` — 新增 `LotteryPoint`（int=0）、`LastBattleTime`（DateTime?）、`DailyShopSlots` 导航属性；移除旧的 `WeaponLotteryPoint`/`SkillLotteryPoint`
- `DataCore/Data/BattleDbContext.cs` — 注册 `UserWeapons`/`UserSkills`/`UserDailyShopSlots` DbSet，配置 FK
- `DataCore/Services/DataHelper.cs` — `UpgradeSingleUser` 新增 LotteryPoint/LastBattleTime；新增 ~10 个商店/经济/背包方法
- `BattleBackend/DTOs/AwardListDto.cs` — `AwardItemDto` 加 `IsUnique` 字段
- `BattleBackend/DTOs/MappingExtensions.cs` — 新增 `ToAwardItemDto(Weapon/Skill)` 扩展
- `BattleBackend/Services/BattleService.cs` — 战斗后发 LotteryPoint（30s 冷却 +8W/+5L），升级 +15/级；ClaimAward 触发 Unique 槽位失效
- `battle-frontend/src/components/game/BackpackView.vue` — 实现熔炼 Tab 和换取 Tab（每日商店+职业抽+稀有度抽）

**API 端点**（均需 JWT）
```
GET  /shop/daily            — 获取/自动刷新每日商店（3小时 UTC 块）
POST /shop/daily/refresh    — 手动刷新 (-20 LotteryPoint)
POST /shop/daily/lock       — 锁定/解锁槽位 {slotId}
POST /shop/daily/purchase   — 购买槽位 {slotId}
POST /shop/draw/profession  — 职业抽 (-40) {profession}
POST /shop/draw/rarity      — 稀有度抽 (-20/50/120/400) {rarity}
POST /shop/smelt            — 熔炼 {itemType, itemId}
GET  /shop/inventory        — 背包列表（含 Count）
GET  /shop/r4-status        — 是否集齐所有 R4
```

**货币规则**（已实现，BattleService 里数值用户微调过）
- 战斗胜利 +8，失败 +5，30s 冷却
- 升级 +15/级
- 熔炼 R1=12, R2=30, R3=80, R4=200
- 每日商店手动刷新 -20，购买价 R1=20/R2=50/R3=120/R4=400
- 职业抽 -40，稀有度抽同购买价

### 2. Bug 修复

- `BattleHelper.cs` — `ActionWithSkill` 访问空 `Tags` 列表时 IndexOutOfRange → 改为 `Tags.Count == 0` 时走普通攻击分支
- `ShopService.cs` — SQLite 读回的 `DateTime` 是 `Kind=Unspecified`，`.ToUniversalTime()` 误当本地时间导致商店购买后触发刷新 → 改用 `DateTime.SpecifyKind(..., DateTimeKind.Utc)`
- `BackpackView.vue` — 购买后改为本地更新 `isPurchased=true` 而非重新 GET，避免再次触发后端刷新检查
- `ItemCard.vue` — 修复 TS strict null（`PROFESSION_MAP` index access）
- `useBattleReplay.ts` — 修复数组越界 undefined 访问

---

## 三、重要的现有设计

### 物品（Item）模型
```csharp
public abstract class Item {
    public bool IsUnique { get; set; } = false;  // R4 全部是 Unique
    public int RareLevel { get; set; }            // 1=Common,2=Rare,3=Epic,4=Legend
    public string Profession { get; set; }        // 主职业
    public string? SecondProfession { get; set; } // 副职业（可 null）
    public List<string> Tags { get; set; }        // 当前全部为 new List<string>()
}
```

### 职业体系
4 个职业：`MORTAL`、`WARRIOR`、`RANGER`、`MAGICIAN`  
对应 PROFESSION_MAP 在 `battle-frontend/src/utils/constants.ts`

### 战斗流程（当前）
1. `BattleManager.BattleSimulation` — 随机从 Fighter.Weapons + Fighter.Skills 里抽取行动
2. `DecideAction` — 根据武器/技能/拳头权重随机选择行动类型
3. `ActionWithSkill` → 检查 Tags（当前全空，走 NormalSkill 分支）
4. 战斗结束 → `SetBattleResult` 更新玩家属性/经验/等级 → `JsonLogger` 输出事件流

### 奖励领取系统
- 战斗/升级后产生 `TempAwardList`（待选奖励）
- 玩家在背包"奖励领取" Tab 从 3 选 1 → ClaimAward → 加入背包
- 领取 Unique 物品时自动调用 `InvalidateUniqueItemSlotAsync` 清除商店对应槽位

---

## 四、下一阶段工作：战斗系统重设计

### 核心变更方向

#### 4.1 出战卡组（Deck）系统
- 战斗前玩家**主动选择**携带的武器/技能卡牌
- 携带上限与等级相关（例：等级 ÷ 5 + 2，具体数值待定）
- 可以不拿满，也可以空手（此时只用拳头）
- **被挑战方**无法在线选择 → 在设置页面预设"默认出战 build"

#### 4.2 武器伤害类型
现有 `Weapon` 模型需新增字段：
```csharp
public string DamageType { get; set; }  // "SHARP"锐器 / "BLUNT"钝器 / "MAGIC"法器
```
这影响与被动技能的联动（详见 4.3）

#### 4.3 被动技能与伤害类型联动
被动技能将大幅增加，并与武器 DamageType 挂钩：  
例：「锐器专精」被动 → 使用 SHARP 武器时伤害 +X%  
例：「法器汲取」被动 → 使用 MAGIC 武器时附带回血  
当前被动技能字段：`Skill.IsPassive = true`，但战斗逻辑里尚未有被动触发机制，需要新增。

#### 4.4 卡牌互斥系统
- 某些卡牌组合互斥（逻辑层面，非代码硬限制）
- 互斥信息存在 Weapon/Skill 上，新增字段：
  ```csharp
  public string? ExclusiveGroup { get; set; }  // 互斥组名，同组只能带一张
  ```
- **例外**：某张 R4 被动技能「无法无天」→ 持有时无视所有互斥限制
- 互斥影响范围：
  - 组建 Deck 时的 UI 提示
  - 抽卡奖励池（已有同组 → 降低出现概率或提示）
  - 升级奖励展示时提示冲突

#### 4.5 设置页面（全新）
需新建 `SettingsView.vue`，功能：
- **默认出战 Build**：从背包里选择默认武器/技能组合（持久化到后端 User 表或新表）
- 其他设置占位（音量、显示等）

路由需要新增 `/settings`。

#### 4.6 战斗入口改造
当前：PK 按钮 → `FightCenterView` → `postFight(attackerId, defenderId)`  
改造后：PK 按钮 → **Deck 选择弹窗** → 选好卡牌 → `postFight` 附带卡牌列表

---

## 五、下一阶段需新建/修改的文件预估

### 后端
| 文件 | 操作 | 说明 |
|------|------|------|
| `DataCore/Models/Weapon.cs` | 修改 | 新增 `DamageType`、`ExclusiveGroup` |
| `DataCore/Models/Skill.cs`  | 修改 | 新增 `ExclusiveGroup` |
| `DataCore/Models/User.cs`   | 修改 | 可能新增 `DefaultDeck`（JSON 序列化存储卡牌 ID 列表） |
| `DataCore/Services/DataHelper.cs` | 修改 | 新增 Deck 读写方法 |
| `BattleCore/BattleLogic/BattleHelper.cs` | **大改** | 被动技能触发、伤害类型加成 |
| `BattleCore/DataModel/Fighters/Fighter.cs` | 修改 | Deck 机制替换当前全量武器/技能加载 |
| `BattleBackend/DTOs/` | 新增 | DeckDto, BattleRequestDto 扩展（携带卡牌列表）|
| `BattleBackend/Services/BattleService.cs` | 修改 | ExecuteFight 接收 Deck 参数 |
| `BattleBackend/Controllers/BattleController.cs` | 修改 | fight 端点接受 Deck 参数 |

### 前端
| 文件 | 操作 | 说明 |
|------|------|------|
| `views/SettingsView.vue` | **新建** | 默认 build 设置页 |
| `components/game/DeckSelectModal.vue` | **新建** | PK 前的 Deck 选择弹窗 |
| `views/LobbyView.vue` | 修改 | PK 按钮触发 DeckSelectModal |
| `router/index.ts` | 修改 | 新增 /settings 路由 |
| `api/battle.ts` | 修改 | postFight 携带 deckIds |
| `utils/constants.ts` | 修改 | 新增 DAMAGE_TYPE_MAP（锐器/钝器/法器颜色/图标）|

---

## 六、注意事项 & 易踩的坑

1. **删 game.db**：任何 EF Core 模型字段变更都必须删除 `Documents/MyProject/game.db`，服务重启时 `EnsureCreated()` 会自动建新表+跑种子数据
2. **SQLite DateTime**：从 DB 读回的 `DateTime` 是 `Kind=Unspecified`，比较前务必用 `DateTime.SpecifyKind(dt, DateTimeKind.Utc)` 而非 `.ToUniversalTime()`
3. **Tags 字段**：当前所有种子 Weapon/Skill 的 `Tags = new List<string>()`，`ActionWithSkill` 里已修复空列表访问，但未来如需真正区分特殊技能要给种子数据补 Tags
4. **DataHelper.cs SeedData**：非常长（~700行），改种子数据时注意 Buff/武器/技能三段的顺序依赖关系（Buff 先保存才能拿到 ID）
5. **UserWeapon/UserSkill 复合主键**：`{UserId, WeaponId}` / `{UserId, SkillId}`，BattleDbContext 里配置了两次（历史遗留），没有副作用但注意别再重复配置
6. **Fighter 初始化**：`BattleService.InitialFighter` 依赖 `user.Profession`，`MORTAL` 和 null 都走 `Mortal` 分支，未设置职业的用户不能参战
7. **Deck 机制与影子对决**：同一玩家打自己（影子对决）时，`user.Id == enemy.Id`，用 `enemy_fighter.Name += " (影)"` 区分。Deck 实现时注意不能直接克隆同一对象的 Fighter

---

## 七、代码库快速索引

```
# 战斗核心
BattleCore/BattleLogic/BattleManager.cs      — 战斗循环
BattleCore/BattleLogic/BattleHelper.cs       — 行动决策、技能/武器执行
BattleCore/BattleLogic/JsonLogger.cs         — 战斗事件 JSON 序列化

# 数据操作
DataCore/Services/DataHelper.cs              — 所有 DB 操作（很长）

# API 入口
BattleBackend/Controllers/BattleController.cs
BattleBackend/Controllers/ShopController.cs

# 前端核心组件
battle-frontend/src/components/game/BackpackView.vue    — 背包（奖励/熔炼/换取）
battle-frontend/src/components/combat/FightReviewer.vue — 战斗回放播放器
battle-frontend/src/views/LobbyView.vue                 — 大厅（选手/PK）
```
