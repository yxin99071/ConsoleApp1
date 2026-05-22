
namespace DataCore.Models
{
    public class UserDailyShopSlot
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int SlotIndex { get; set; }          // 0–3
        public string ItemType { get; set; } = string.Empty;   // "WEAPON" | "SKILL"
        public int ItemId { get; set; }
        public int Price { get; set; }
        public bool IsPurchased { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public DateTime GeneratedAt { get; set; }
    }
}
