using static BattleBackend.DTOs.InformationDTO;

namespace BattleBackend.DTOs
{
    // ── 每日商店 ─────────────────────────────────────────────────────────────

    public record DailyShopDto
    {
        public List<ShopSlotDto> Slots { get; set; } = new();
        public DateTime NextRefreshTime { get; set; }
        public int LotteryPoint { get; set; }
        public int ManualRefreshCost { get; set; } = 20;
    }

    public record ShopSlotDto
    {
        public int Id { get; set; }
        public int SlotIndex { get; set; }
        public bool IsLocked { get; set; }
        public bool IsPurchased { get; set; }
        public int Price { get; set; }
        public string ItemType { get; set; } = string.Empty;   // "WEAPON" | "SKILL"
        public AwardItemDto Item { get; set; } = null!;
    }

    // ── 抽卡结果 ─────────────────────────────────────────────────────────────

    public record DrawResultDto
    {
        public string ItemType { get; set; } = string.Empty;
        public AwardItemDto Item { get; set; } = null!;
        public int NewBalance { get; set; }
    }

    // ── 熔炼结果 ─────────────────────────────────────────────────────────────

    public record SmeltResultDto
    {
        public int Earned { get; set; }
        public int NewBalance { get; set; }
    }

    // ── 背包（熔炼 Tab 用）──────────────────────────────────────────────────

    public record InventoryDto
    {
        public List<OwnedItemDto> Weapons { get; set; } = new();
        public List<OwnedItemDto> Skills  { get; set; } = new();
        public int LotteryPoint { get; set; }
    }

    public record OwnedItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Profession { get; set; } = string.Empty;
        public string? SecondProfession { get; set; }
        public int RareLevel { get; set; }
        public bool IsPassive { get; set; }
        public bool IsUnique { get; set; }
        public int Count { get; set; }
        public string ItemType { get; set; } = string.Empty;   // "WEAPON" | "SKILL"
        public List<BuffSummaryDto> Buffs { get; set; } = new();
    }

    // ── 请求 DTO ─────────────────────────────────────────────────────────────

    public record LockSlotRequest  { public int SlotId { get; set; } }
    public record PurchaseSlotRequest { public int SlotId { get; set; } }
    public record SmeltRequest { public string ItemType { get; set; } = string.Empty; public int ItemId { get; set; } }
    public record ProfDrawRequest { public string Profession { get; set; } = string.Empty; }
    public record RarityDrawRequest { public int Rarity { get; set; } }
}
