using DataCore.Models;

namespace BattleBackend.DTOs
{
    public record AwardListDto 
    {
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public List<Skill> Skills { get; set; } = new List<Skill>();
    }
}
    
