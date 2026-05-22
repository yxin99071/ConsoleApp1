
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
        // ── 货币 & 限流 ──────────────────────────────────
        public int LotteryPoint { get; set; } = 0;
        public DateTime? LastBattleTime { get; set; } = null;

        // ── 默认出战卡组（JSON 序列化存储，如 "[1,2,3]"）─────────────
        public string DefaultDeckWeaponIds { get; set; } = "[]";
        public string DefaultDeckSkillIds { get; set; } = "[]";

        public List<UserWeapon> UserWeaponLinks { get; set; } = new List<UserWeapon>();
        public List<UserSkill> UserSkillLinks { get; set; } = new List<UserSkill>();
        public List<UserDailyShopSlot> DailyShopSlots { get; set; } = new();

        public User Copy()
        {
            var wlists = new List<UserWeapon>();
            foreach (var w in this.UserWeaponLinks) wlists.Add(w.Clone());
            var slists = new List<UserSkill>();
            foreach (var s in this.UserSkillLinks) slists.Add(s.Clone());

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
                LotteryPoint = this.LotteryPoint,
                LastBattleTime = this.LastBattleTime,
                UserSkillLinks = slists,
                UserWeaponLinks = wlists,
            };
        }
    }
}
