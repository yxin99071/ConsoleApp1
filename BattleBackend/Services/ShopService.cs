using BattleBackend.DTOs;
using DataCore.Models;
using DataCore.Services;
using static BattleBackend.DTOs.InformationDTO;

namespace BattleBackend.Services
{
    public class ShopService
    {
        private readonly DataHelper _dataHelper;

        // ── 常量 ──────────────────────────────────────────────────────────────
        private const int ManualRefreshCost = 20;
        private const int ProfDrawCost      = 40;

        // 权重数组：[R1, R2, R3, R4]
        private static readonly int[] DailyShopWeights = { 50, 30, 18, 2 };
        private static readonly int[] ProfDrawWeights  = { 55, 30, 12, 3 };

        // 按稀有度索引（1-based，0 号元素占位）
        private static readonly int[] RarityDrawCosts = { 0, 20,  50,  120, 400 };
        private static readonly int[] SmeltRewards    = { 0, 12,  30,   80, 200 };
        private static readonly int[] ItemPrices      = { 0, 20,  50,  120, 400 };

        public ShopService(DataHelper dataHelper) => _dataHelper = dataHelper;

        // ── 时间工具 ──────────────────────────────────────────────────────────

        private static DateTime GetCurrentBlockStart()
        {
            var now = DateTime.UtcNow;
            int blockHour = (now.Hour / 3) * 3;
            return new DateTime(now.Year, now.Month, now.Day, blockHour, 0, 0, DateTimeKind.Utc);
        }

        private static DateTime GetNextBlockStart() => GetCurrentBlockStart().AddHours(3);

        // ── 每日商店：获取/自动刷新 ───────────────────────────────────────────

        public async Task<DailyShopDto?> GetOrRefreshDailyShopAsync(int userId)
        {
            var slots      = await _dataHelper.GetShopSlotsAsync(userId);
            var blockStart = GetCurrentBlockStart();

            bool needsRefresh = slots.Count == 0 ||
                                slots.Max(s => DateTime.SpecifyKind(s.GeneratedAt, DateTimeKind.Utc)) < blockStart;

            if (needsRefresh)
                await DoRefreshAsync(userId, slots, forceFull: false);

            var fresh = await _dataHelper.GetShopSlotsAsync(userId);
            return await BuildDailyShopDtoAsync(userId, fresh);
        }

        // ── 每日商店：手动刷新 ────────────────────────────────────────────────

        public async Task<DailyShopDto?> ManualRefreshShopAsync(int userId)
        {
            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return null;
            if (user.LotteryPoint < ManualRefreshCost) return null;   // 余额不足

            await _dataHelper.UpdateUserLotteryPointAsync(userId, user.LotteryPoint - ManualRefreshCost);

            var slots = await _dataHelper.GetShopSlotsAsync(userId);
            await DoRefreshAsync(userId, slots, forceFull: false);

            await _dataHelper.SaveChangesAsync();

            var fresh = await _dataHelper.GetShopSlotsAsync(userId);
            return await BuildDailyShopDtoAsync(userId, fresh);
        }

        // ── 每日商店：锁定槽位 ────────────────────────────────────────────────

        /// <summary>
        /// 切换锁定：传入 slotId=0 表示解锁当前锁；传入有效 slotId 则先解锁旧锁再锁新槽。
        /// </summary>
        public async Task<DailyShopDto?> LockSlotAsync(int userId, int slotId)
        {
            var slots = await _dataHelper.GetShopSlotsAsync(userId);

            // 先解锁所有旧锁
            foreach (var s in slots.Where(s => s.IsLocked))
                s.IsLocked = false;

            // 锁定新槽（如果 slotId > 0）
            if (slotId > 0)
            {
                var target = slots.FirstOrDefault(s => s.Id == slotId && !s.IsPurchased);
                if (target != null) target.IsLocked = true;
            }

            await _dataHelper.SaveChangesAsync();
            return await BuildDailyShopDtoAsync(userId, await _dataHelper.GetShopSlotsAsync(userId));
        }

        // ── 每日商店：购买槽位 ────────────────────────────────────────────────

        public async Task<(ShopSlotDto? result, string? error)> PurchaseSlotAsync(int userId, int slotId)
        {
            var slot = await _dataHelper.GetShopSlotByIdAsync(slotId);
            if (slot == null || slot.UserId != userId)
                return (null, "槽位不存在");
            if (slot.IsPurchased)
                return (null, "该槽位已购买");

            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return (null, "用户不存在");
            if (user.LotteryPoint < slot.Price)
                return (null, "碎片不足");

            // 扣费
            await _dataHelper.UpdateUserLotteryPointAsync(userId, user.LotteryPoint - slot.Price);

            // 加入背包
            await _dataHelper.AddItemToUserAsync(userId, slot.ItemType, slot.ItemId);

            // 标记已购买、释放锁
            slot.IsPurchased = true;
            slot.IsLocked    = false;

            // Unique 失效检查（消除同一 Unique 可能存在于其他槽位的情况）
            bool isUnique = await IsItemUniqueAsync(slot.ItemType, slot.ItemId);
            if (isUnique)
                await _dataHelper.InvalidateUniqueItemSlotAsync(userId, slot.ItemType, slot.ItemId);

            await _dataHelper.SaveChangesAsync();

            // 构造返回
            AwardItemDto? itemDto = await GetItemDtoAsync(slot.ItemType, slot.ItemId);
            if (itemDto == null) return (null, "物品数据异常");

            return (new ShopSlotDto
            {
                Id         = slot.Id,
                SlotIndex  = slot.SlotIndex,
                IsLocked   = slot.IsLocked,
                IsPurchased= slot.IsPurchased,
                Price      = slot.Price,
                ItemType   = slot.ItemType,
                Item       = itemDto
            }, null);
        }

        // ── 职业抽 ────────────────────────────────────────────────────────────

        public async Task<(DrawResultDto? result, string? error)> DrawByProfessionAsync(int userId, string profession)
        {
            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return (null, "用户不存在");
            if (user.LotteryPoint < ProfDrawCost) return (null, "碎片不足");

            var availWeapons = await _dataHelper.GetLockedWeapons(user);
            var availSkills  = await _dataHelper.GetLockedSkills(user);

            // 按主职业过滤
            var profWeapons = availWeapons.Where(w => w.Profession == profession).ToList();
            var profSkills  = availSkills .Where(s => s.Profession == profession).ToList();

            var random = new Random();
            int rarity = RollRarity(random, ProfDrawWeights);

            // R4 池耗尽 → 重骰直到有货
            if (rarity == 4)
            {
                bool r4Available = profWeapons.Any(w => w.RareLevel == 4) ||
                                   profSkills .Any(s => s.RareLevel == 4);
                int tries = 0;
                while (!r4Available && tries++ < 50)
                {
                    rarity = RollRarity(random, ProfDrawWeights);
                    if (rarity != 4) break;
                }
                // 如果仍为 4 且无货，降到 R3
                if (rarity == 4 && !r4Available) rarity = 3;
            }

            // 构建候选池（武器 + 技能混合，50/50 靠 RandomOrder）
            var pool = new List<(string type, Item item)>();
            pool.AddRange(profWeapons.Where(w => w.RareLevel == rarity).Select(w => ("WEAPON", (Item)w)));
            pool.AddRange(profSkills .Where(s => s.RareLevel == rarity).Select(s => ("SKILL",  (Item)s)));

            if (pool.Count == 0) return (null, "当前职业该稀有度无可用卡牌");

            var (itemType, selected) = pool[random.Next(pool.Count)];

            // 扣费 + 加背包
            await _dataHelper.UpdateUserLotteryPointAsync(userId, user.LotteryPoint - ProfDrawCost);
            await _dataHelper.AddItemToUserAsync(userId, itemType, selected.Id);

            if (selected.IsUnique)
                await _dataHelper.InvalidateUniqueItemSlotAsync(userId, itemType, selected.Id);

            await _dataHelper.SaveChangesAsync();

            var itemDto = await GetItemDtoAsync(itemType, selected.Id);
            var newUser = await _dataHelper.GetUserById(userId);

            return (new DrawResultDto
            {
                ItemType   = itemType,
                Item       = itemDto!,
                NewBalance = newUser?.LotteryPoint ?? 0
            }, null);
        }

        // ── 稀有度抽 ─────────────────────────────────────────────────────────

        public async Task<(DrawResultDto? result, string? error)> DrawByRarityAsync(int userId, int rarity)
        {
            if (rarity < 1 || rarity > 4) return (null, "无效稀有度");

            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return (null, "用户不存在");

            int cost = RarityDrawCosts[rarity];
            if (user.LotteryPoint < cost) return (null, "碎片不足");

            var availWeapons = await _dataHelper.GetLockedWeapons(user);
            var availSkills  = await _dataHelper.GetLockedSkills(user);

            var pool = new List<(string type, Item item)>();
            pool.AddRange(availWeapons.Where(w => w.RareLevel == rarity).Select(w => ("WEAPON", (Item)w)));
            pool.AddRange(availSkills .Where(s => s.RareLevel == rarity).Select(s => ("SKILL",  (Item)s)));

            if (pool.Count == 0) return (null, "该稀有度已集齐，无可用卡牌");

            var random = new Random();
            var (itemType, selected) = pool[random.Next(pool.Count)];

            await _dataHelper.UpdateUserLotteryPointAsync(userId, user.LotteryPoint - cost);
            await _dataHelper.AddItemToUserAsync(userId, itemType, selected.Id);

            if (selected.IsUnique)
                await _dataHelper.InvalidateUniqueItemSlotAsync(userId, itemType, selected.Id);

            await _dataHelper.SaveChangesAsync();

            var itemDto = await GetItemDtoAsync(itemType, selected.Id);
            var newUser = await _dataHelper.GetUserById(userId);

            return (new DrawResultDto
            {
                ItemType   = itemType,
                Item       = itemDto!,
                NewBalance = newUser?.LotteryPoint ?? 0
            }, null);
        }

        // ── 熔炼 ─────────────────────────────────────────────────────────────

        public async Task<(SmeltResultDto? result, string? error)> SmeltItemAsync(int userId, string itemType, int itemId)
        {
            int rarity = await _dataHelper.SmeltItemAsync(userId, itemType, itemId);
            if (rarity == 0) return (null, "背包中找不到该物品");

            int earned = SmeltRewards[Math.Min(rarity, 4)];

            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return (null, "用户不存在");

            await _dataHelper.UpdateUserLotteryPointAsync(userId, user.LotteryPoint + earned);
            await _dataHelper.SaveChangesAsync();

            var newUser = await _dataHelper.GetUserById(userId);
            return (new SmeltResultDto { Earned = earned, NewBalance = newUser?.LotteryPoint ?? 0 }, null);
        }

        // ── 背包列表（熔炼 Tab）──────────────────────────────────────────────

        public async Task<InventoryDto?> GetInventoryAsync(int userId)
        {
            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return null;

            var weapons = user.UserWeaponLinks.Select(uw => new OwnedItemDto
            {
                Id               = uw.Weapon.Id,
                Name             = uw.Weapon.Name,
                Description      = uw.Weapon.Description,
                Profession       = uw.Weapon.Profession,
                SecondProfession = uw.Weapon.SecondProfession,
                RareLevel        = uw.Weapon.RareLevel,
                IsPassive        = false,
                IsUnique         = uw.Weapon.IsUnique,
                Count            = uw.Count,
                ItemType         = "WEAPON",
                DamageType       = uw.Weapon.DamageType,
                ExclusiveGroup   = uw.Weapon.ExclusiveGroup,
                Buffs            = uw.Weapon.WeaponBuffs
                                     .Select(wb => wb.Buff.ToDto())
                                     .ToList()
            }).ToList();

            var skills = user.UserSkillLinks.Select(us => new OwnedItemDto
            {
                Id               = us.Skill.Id,
                Name             = us.Skill.Name,
                Description      = us.Skill.Description,
                Profession       = us.Skill.Profession,
                SecondProfession = us.Skill.SecondProfession,
                RareLevel        = us.Skill.RareLevel,
                IsPassive        = us.Skill.IsPassive,
                IsUnique         = us.Skill.IsUnique,
                Count            = us.Count,
                ItemType         = "SKILL",
                ExclusiveGroup   = us.Skill.ExclusiveGroup,
                Buffs            = us.Skill.SkillBuffs
                                     .Select(sb => sb.Buff.ToDto())
                                     .ToList()
            }).ToList();

            return new InventoryDto
            {
                Weapons      = weapons,
                Skills       = skills,
                LotteryPoint = user.LotteryPoint
            };
        }

        // ── 是否所有 R4 都集齐（稀有度抽 R4 按钮禁用判断）────────────────────

        public async Task<bool> AllR4OwnedAsync(int userId) => await _dataHelper.AllR4OwnedAsync(userId);

        // ── 私有辅助方法 ──────────────────────────────────────────────────────

        /// <summary>（系统/手动）刷新非锁定槽位。</summary>
        private async Task DoRefreshAsync(int userId, List<UserDailyShopSlot> current, bool forceFull)
        {
            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return;

            // 找到锁定且未购买的槽位（最多 1 个）
            var lockedSlot = current.FirstOrDefault(s => s.IsLocked && !s.IsPurchased);

            // 删除非锁定槽位
            var toRemove = current.Where(s => !s.IsLocked).ToList();
            _dataHelper.RemoveShopSlots(toRemove);

            // 确定需要重新生成的 slotIndex
            var freeIndexes = Enumerable.Range(0, 4)
                .Where(i => lockedSlot == null || i != lockedSlot.SlotIndex)
                .ToArray();

            var newSlots = await GenerateSlotsAsync(user, freeIndexes);
            await _dataHelper.AddShopSlotsAsync(newSlots);
            await _dataHelper.SaveChangesAsync();
        }

        /// <summary>为指定 slotIndexes 生成新槽位。</summary>
        private async Task<List<UserDailyShopSlot>> GenerateSlotsAsync(User user, int[] slotIndexes)
        {
            var availWeapons = await _dataHelper.GetLockedWeapons(user);
            var availSkills  = await _dataHelper.GetLockedSkills(user);
            var random       = new Random();
            var result       = new List<UserDailyShopSlot>();
            var now          = DateTime.UtcNow;

            foreach (var index in slotIndexes)
            {
                int rarity   = RollRarity(random, DailyShopWeights);
                bool useWeapon = random.Next(2) == 0;

                var wPool = availWeapons.Where(w => w.RareLevel == rarity).ToList();
                var sPool = availSkills .Where(s => s.RareLevel == rarity).ToList();

                Item? chosen   = null;
                string itemType = "WEAPON";

                if (useWeapon && wPool.Count > 0)
                {
                    chosen   = wPool[random.Next(wPool.Count)];
                    itemType = "WEAPON";
                }
                else if (!useWeapon && sPool.Count > 0)
                {
                    chosen   = sPool[random.Next(sPool.Count)];
                    itemType = "SKILL";
                }
                else if (wPool.Count > 0)   // 备选
                {
                    chosen   = wPool[random.Next(wPool.Count)];
                    itemType = "WEAPON";
                }
                else if (sPool.Count > 0)
                {
                    chosen   = sPool[random.Next(sPool.Count)];
                    itemType = "SKILL";
                }

                if (chosen == null) continue;   // 该稀有度完全没有可用卡，跳过

                result.Add(new UserDailyShopSlot
                {
                    UserId      = user.Id,
                    SlotIndex   = index,
                    ItemType    = itemType,
                    ItemId      = chosen.Id,
                    Price       = ItemPrices[Math.Min(rarity, 4)],
                    GeneratedAt = now
                });
            }

            return result;
        }

        /// <summary>加权随机稀有度，weights 长度为 4，对应 R1-R4。</summary>
        private static int RollRarity(Random random, int[] weights)
        {
            int total      = weights.Sum();
            int roll       = random.Next(total);
            int cumulative = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative) return i + 1;
            }
            return 1;
        }

        private async Task<AwardItemDto?> GetItemDtoAsync(string itemType, int itemId)
        {
            if (itemType == "WEAPON")
            {
                var w = await _dataHelper.GetWeaponWithBuffsById(itemId);
                return w?.ToAwardItemDto();
            }
            else
            {
                var s = await _dataHelper.GetSkillWithBuffsById(itemId);
                return s?.ToAwardItemDto();
            }
        }

        private async Task<DailyShopDto> BuildDailyShopDtoAsync(int userId, List<UserDailyShopSlot> slots)
        {
            var user     = await _dataHelper.GetUserById(userId);
            var slotDtos = new List<ShopSlotDto>();

            foreach (var slot in slots)
            {
                var itemDto = await GetItemDtoAsync(slot.ItemType, slot.ItemId);
                if (itemDto == null) continue;

                slotDtos.Add(new ShopSlotDto
                {
                    Id          = slot.Id,
                    SlotIndex   = slot.SlotIndex,
                    IsLocked    = slot.IsLocked,
                    IsPurchased = slot.IsPurchased,
                    Price       = slot.Price,
                    ItemType    = slot.ItemType,
                    Item        = itemDto
                });
            }

            return new DailyShopDto
            {
                Slots            = slotDtos,
                NextRefreshTime  = GetNextBlockStart(),
                LotteryPoint     = user?.LotteryPoint ?? 0,
                ManualRefreshCost= ManualRefreshCost
            };
        }

        private async Task<bool> IsItemUniqueAsync(string itemType, int itemId)
        {
            if (itemType == "WEAPON")
            {
                var w = await _dataHelper.GetWeaponById(itemId);
                return w?.IsUnique ?? false;
            }
            else
            {
                var s = await _dataHelper.GetSkillById(itemId);
                return s?.IsUnique ?? false;
            }
        }

    }
}
