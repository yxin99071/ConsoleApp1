namespace BattleBackend.DTOs
{
    public class InitProfileDto
    {
        public string? name { get; set; }
        public string? account { get; set; }
        public string profession { get; set; } = null!;
        public string? secondProfession { get; set; }

        // 对应前端传过来的数组
        public List<string> initialSkills { get; set; } = null!;
    }
}
