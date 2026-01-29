using DataCore.Models;

namespace BattleBackend.DTOs
{
    public class InformationDTO
    {
        public record InformationDto
        {
            public int Id { get; set; }
            public string Name { get; init; } = string.Empty;
            public double Exp { get; init; }
            public int Level { get; init; }
            public double Health { get; set; }
            public double Agility { get; set; }
            public double Strength { get; set; }
            public double Intelligence { get; set; }
            public string? Profession { get; init; }
            public string? SecondProfession { get; init; }
            public List<SkillDto> Skills { get; init; } = new();
            public List<WeaponDto> Weapons { get; init; } = new();
        }

        // 提取公共特性的基类
        public abstract record ItemDto
        {
            public string Name { get; init; } = string.Empty;
            public string Profession { get; init; } = string.Empty;
            public string? SecondProfession { get; init; }
            public string Description { get; init; } = string.Empty;
            public int RareLevel { get; init; }
            public List<BuffSummaryDto> Buffs { get; init; } = new();
        }

        public record SkillDto : ItemDto
        {
           public bool isPassive { get; set; }
        }
        public record WeaponDto : ItemDto;
        
        public record BuffSummaryDto
        {
            public string Name { get; init; } = string.Empty;
            public bool IsBuff { get; init; }
            public bool IsDeBuff { get; init; }
            public bool IsDamage { get; set; }
            public int LastRound { get; init; }
            public string Description { get; init; } = string.Empty;
        }

    }
}
