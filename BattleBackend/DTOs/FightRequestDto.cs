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
    }
}
