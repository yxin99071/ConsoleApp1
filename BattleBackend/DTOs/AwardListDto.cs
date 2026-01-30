using DataCore.Models;

namespace BattleBackend.DTOs
{
    public record AwardListDto 
    {
        public String Type { get; set; } = string.Empty;
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
    
