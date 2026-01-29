using BattleCore.DataModel.States;
using DataCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel.Fighters
{
    public class Warrior : Fighter
    {
        public Warrior(User user): base(user)
        {
            Profession = "WARRIOR";
            CraticalDamage += StaticDataHelper.CalculateCriticalDamage(Strength) / 2;
            CraticalRate += StaticDataHelper.CalculateCriticalRate(0.8 * Strength) / 2;
        }

        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Strength * 2.0;
            var detail = new DamageDetail
            {
                DamageType = StaticDataHelper.FistDamage,
                DirectSource = $"{this.Profession}'s Fist",
            };
            damageInfo.damageDetail = detail;

        }
        public override void LoadBuff(Buff buff, Fighter? source,int buffLevel)
        {
            var newBuff = buff.Clone();
            if (source is not null)
            {
                if (newBuff.CoefficientStrength > 0)
                    newBuff.DirectDamage = newBuff.CoefficientStrength * source.Strength;
                if (newBuff.CoefficientAgility > 0)
                    newBuff.DirectDamage = newBuff.CoefficientAgility * source.Agility;
                if (newBuff.CoefficientIntelligence > 0)
                    newBuff.DirectDamage = newBuff.CoefficientAgility * source.Intelligence;
            }
            base.LoadBuff(newBuff, source, buffLevel);
        }


    }
}
