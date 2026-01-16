namespace DataCore.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Password { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Profession { get; set; } = default!;
        public double Exp { get; set; }
        public int Level { get; set; }
        public double Health { get; set; }
        public double Agility { get; set; }
        public double Strength { get; set; }
        public double Intelligence { get; set; }
        public List<Weapon> Weapons { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();
    }
}
