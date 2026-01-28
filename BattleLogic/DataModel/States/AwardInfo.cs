using DataCore.Models;


namespace BattleCore.DataModel.States
{
    public record AwardInfo
    {
        public User user { get; set; } = default!;
        public int SpecialWeaponCount { get; set; }
        public int NormalWeaponCount { get; set; }
        public int SpecialSkillCount { get; set; }
        public int NormalSkillCount { get; set; }
        public bool HasAward => SpecialWeaponCount + NormalWeaponCount + SpecialSkillCount + NormalSkillCount > 0;
    }
}
