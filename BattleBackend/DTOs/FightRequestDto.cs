using System.Text.Json.Serialization;

namespace BattleBackend.DTOs
{
    public class FightRequestDto
    {
        [JsonPropertyName("attacker")]
        public string? attacker { get; set; }

        [JsonPropertyName("defender")]
        public string? defender { get; set; }

        [JsonPropertyName("history")]
        public string? history { get; set; }

        /// <summary>攻击方选择携带的武器 ID 列表（可重复，代表多份）</summary>
        [JsonPropertyName("deckWeaponIds")]
        public List<int>? DeckWeaponIds { get; set; }

        /// <summary>攻击方选择携带的技能 ID 列表</summary>
        [JsonPropertyName("deckSkillIds")]
        public List<int>? DeckSkillIds { get; set; }
    }
}
