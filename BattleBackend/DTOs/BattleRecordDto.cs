namespace BattleBackend.DTOs
{
    public record BattleRecordDto
    {
        public int Id { get; set; }
        public bool IsWin { get; set; }
        public string OpponentName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }
}
