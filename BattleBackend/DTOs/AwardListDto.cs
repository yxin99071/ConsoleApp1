using static BattleBackend.DTOs.InformationDTO;

namespace BattleBackend.DTOs
{
    public record AwardListDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;   // "WEAPON" | "SKILL"
        public int AwardLevel { get; set; }
        public List<AwardItemDto> Items { get; set; } = new();
    }

    public record AwardItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Profession { get; set; } = string.Empty;
        public string? SecondProfession { get; set; }
        public int RareLevel { get; set; }
        public bool IsPassive { get; set; }
        public bool IsUnique { get; set; }
        public string? ExclusiveGroup { get; set; }
        /// <summary>仅武器有效：SHARP / BLUNT / MAGIC</summary>
        public string? DamageType { get; set; }
        public List<BuffSummaryDto> Buffs { get; set; } = new();
    }

    public record ClaimAwardDto
    {
        public int AwardListId { get; set; }
        public int ItemId { get; set; }
    }
}
