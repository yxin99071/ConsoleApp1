using DataCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public class DataHelper
    {
        public Buff CloneBuff(Buff buff)
        {
            return new Buff
            {
                Id = buff.Id,
                Name = buff.Name,
                LastRound = buff.LastRound,
                DirectDamage = buff.DirectDamage,
                DamageCorrection = buff.DamageCorrection,
                WoundCorrection = buff.WoundCorrection,
                IsOnSelf = buff.IsOnSelf,
                CoefficientAgility = buff.CoefficientAgility,
                CoefficientStrength = buff.CoefficientStrength,
                CoefficientIntelligence = buff.CoefficientIntelligence,

                // 引用类型的深拷贝
                SpecialTag = buff.SpecialTag?.ToList() ?? new List<string>(),

                // 如果 WeaponBuff 和 SkillBuff 也是类，建议也要实现它们的克隆
                WeaponBuffs = buff.WeaponBuffs?.Select(wb => new Weapon
                {
                    /* 根据 WeaponBuff 的属性赋值 */
                }).ToList() ?? new(),

                SkillBuffs = buff.SkillBuffs?.Select(sb => new SkillBuff
                {
                    /* 根据 SkillBuff 的属性赋值 */
                }).ToList() ?? new()
            };

        }
 
        public Weapon CloneWeapon(Weapon weapon)
        {

        }
    }
}
