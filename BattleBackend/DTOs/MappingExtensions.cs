using DataCore.Models;
using System;
using static BattleBackend.DTOs.InformationDTO;

namespace BattleBackend.DTOs
{
    public static class MappingExtensions
    {
        public static Dictionary<string, string> professionDict = new Dictionary<string, string>
        {
            {"战士","WARRIOR"},
            {"游侠","RANGER"},
            {"法师","MAGICIAN"},
            { "凡人","MORTAL"}
        };
        public static InformationDto ToDto(this User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Exp = user.Exp,
            Level = user.Level,
            Profession = user.Profession,
            Health = user.Health,
            Agility = user.Agility,
            Strength = user.Strength,
            Intelligence = user.Intelligence,
            SecondProfession = user.SecondProfession,
            Skills = user.Skills.Select(s => s.ToDto()).ToList(),
            Weapons = user.Weapons.Select(w => w.ToDto()).ToList()
        };

        public static SkillDto ToDto(this Skill skill) => new()
        {
            Name = skill.Name,
            Profession = skill.Profession,
            Description = skill.Description,
            isPassive = skill.IsPassive,
            SecondProfession = skill.SecondProfession,
            Buffs = skill.SkillBuffs.Select(sb => sb.Buff.ToDto()).ToList()
        };
        public static WeaponDto ToDto(this Weapon weapon) => new()
        {
            Name = weapon.Name,
            Profession = weapon.Profession,
            Description = weapon.Description,
            SecondProfession = weapon.SecondProfession,
            Buffs = weapon.WeaponBuffs.Select(sb => sb.Buff.ToDto()).ToList()
        };
        public static BuffSummaryDto ToDto(this Buff buff) => new()
        {
            Name = buff.Name,
            Description = buff.Description,
            IsBuff = buff.DamageCorrection > 1 || buff.WoundCorrection < 1,
            IsDeBuff = buff.DamageCorrection < 1 || buff.WoundCorrection > 1,
            IsDamage = (buff.CoefficientAgility + buff.CoefficientIntelligence + buff.CoefficientStrength) >1,
            LastRound = buff.LastRound
        };
    }
}
