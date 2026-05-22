using System.Text.Json.Serialization;

namespace BattleBackend.DTOs
{
    /// <summary>默认出战卡组 DTO</summary>
    public class DefaultDeckDto
    {
        [JsonPropertyName("weaponIds")]
        public List<int> WeaponIds { get; set; } = new();

        [JsonPropertyName("skillIds")]
        public List<int> SkillIds { get; set; } = new();

        /// <summary>卡组容量（level / 5 + 2），由后端计算后返回给前端。</summary>
        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }
    }
}
