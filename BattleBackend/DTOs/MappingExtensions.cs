using BattleCore.BattleLogic;
using BattleCore.DataModel.Fighters;
using DataCore.Models;
using System;
using System.Security.Cryptography.X509Certificates;
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
        public static InformationDto ToDto(this User user)
        {

            var fighter = UserToFighter(user);
            if (fighter is null)
                return new InformationDto();
            return new InformationDto
            {
                Id = fighter.Id,
                Name = fighter.Name,
                Exp = user.Exp,
                Level = user.Level,
                Profession = fighter.Profession,
                Health = fighter.Health,
                Agility = fighter.Agility,
                Strength = fighter.Strength,
                Intelligence = fighter.Intelligence,
                SecondProfession = user.SecondProfession,
                Skills = fighter.Skills.Select(s => s.ToDto()).ToList(),
                Weapons = fighter.Weapons.Select(w => w.ToDto()).ToList()
            };
            Fighter? UserToFighter(User user)
            {
                if (user.Profession == "WARRIOR")
                    return new Warrior(user);
                if (user.Profession == "RANGER")
                    return new Ranger(user);
                if (user.Profession == "MAGICIAN")
                    return new Magician(user);
                if (user.Profession == "MORTAL")
                    return new Mortal(user);
                return null;
            }
        }
        

        public static SkillDto ToDto(this Skill skill) => new()
        {
            Name = skill.Name,
            Profession = skill.Profession,
            Description = skill.Description,
            isPassive = skill.IsPassive,
            RareLevel = skill.RareLevel,
            SecondProfession = skill.SecondProfession,
            Buffs = skill.SkillBuffs.Select(sb => sb.Buff.ToDto()).ToList()
        };
        public static WeaponDto ToDto(this Weapon weapon) => new()
        {
            RareLevel = weapon.RareLevel,
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
            IsDamage = (buff.CoefficientAgility + buff.CoefficientIntelligence + buff.CoefficientStrength) >0,
            LastRound = buff.LastRound
        };
    }
}
