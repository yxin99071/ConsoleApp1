namespace DataCore.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Account { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Profession { get; set; } = null;
        public string? SecondProfession { get; set; } = null;
        public double Exp { get; set; }
        public int Level { get; set; }
        public int LastWeaponAward { get; set; } = 1;
        public int LastSkillAward { get; set; } = 1;
        public double Health { get; set; }
        public double Agility { get; set; }
        public double Strength { get; set; }
        public double Intelligence { get; set; }
        public List<Weapon> Weapons { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();
    }
}
