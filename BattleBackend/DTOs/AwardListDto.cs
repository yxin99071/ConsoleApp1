using DataCore.Models;

namespace BattleBackend.DTOs
{
    public record AwardListDto 
    {
        public String Type { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
    
