using DataCore.Models;

namespace BattleBackend.DTOs
{
    public class InformationDTO
    {
        public InformationDTO(User user)
        {
            name = user.Name;
            exp = user.Exp;
            level = user.Level;
            profession = user.Profession;
            secondProfession = user.SecondProfession;
            foreach (var skill in user.Skills)
                skillDTO.Add(new SkillDTO(skill));
            foreach (var weapon in user.Weapons)
                weaponDTO.Add(new WeaponDTO(weapon)); 
        }
        public string name { get; set; }
        public double exp { get; set; }
        public int level { get; set; }
        public string? profession { get; set; }
        public string? secondProfession { get; set; }
        public List<SkillDTO> skillDTO { get; set; } = new List<SkillDTO>();
        public List<WeaponDTO> weaponDTO { get; set; } = new List<WeaponDTO>();
    }
    public class SkillDTO 
    {
        public SkillDTO(Skill skill)
        {
            name = skill.Name;
            profession = skill.Profession;
            description = skill.Description;
            foreach(var sb in skill.SkillBuffs)
            {
                buffs.Add(new BuffSummaryDTO(sb.Buff));
            }
        }
        public string name { get; set; }
        public string profession { get; set; }
        public string description { get; set; }
        public List<BuffSummaryDTO> buffs { get; set; } = new List<BuffSummaryDTO>();
    }
    public class WeaponDTO {
        public WeaponDTO(Weapon weapon)
        {
            name = weapon.Name;
            profession = weapon.Profession;
            description = weapon.Description;
            foreach (var sb in weapon.WeaponBuffs)
            {
                buffs.Add(new BuffSummaryDTO(sb.Buff));
            }
        }
        public string name { get; set; }
        public string profession { get; set; }
        public string description { get; set; }
        public List<BuffSummaryDTO> buffs { get; set; } = new List<BuffSummaryDTO>();
    }
    public class BuffSummaryDTO
    {
        public BuffSummaryDTO(Buff buff)
        {
            name = buff.Name;
            description = buff.Description;
        }
        public string name { get; set; }
        public string description { get; set; }


    }

}
