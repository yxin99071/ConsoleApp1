
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

        public User Copy()
        {
            return new User
            {
                Id = this.Id,
                Account = this.Account,
                Password = this.Password,
                Name = this.Name,
                Profession = this.Profession,
                SecondProfession = this.SecondProfession,
                Exp = this.Exp,
                Level = this.Level,
                LastWeaponAward = this.LastWeaponAward,
                LastSkillAward = this.LastSkillAward,
                Health = this.Health,
                Agility = this.Agility,
                Strength = this.Strength,
                Intelligence = this.Intelligence,
                Weapons = new List<Weapon>(this.Weapons), // 浅拷贝列表
                Skills = new List<Skill>(this.Skills) // 浅拷贝列表
            };
        }
    }
}
