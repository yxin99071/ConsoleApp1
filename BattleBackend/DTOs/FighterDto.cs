namespace BattleBackend.DTOs
{
    public record FighterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Profession { get; set; } = default!;
        public string? SecondProfession { get; set; }
    }
}
