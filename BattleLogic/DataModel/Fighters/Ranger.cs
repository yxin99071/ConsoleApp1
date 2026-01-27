using BattleCore.DataModel;
using BattleCore.DataModel.States;
using DataCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel.Fighters
{
    public class Ranger : Fighter
    {
        public Ranger(User user): base(user)
        {
            Profession = "RANGER";
            CraticalRate += StaticDataHelper.CalculateCriticalRate(Agility) / 2;
            CraticalDamage += StaticDataHelper.CalculateCriticalDamage(0.2*Agility) / 2;
        }
        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Agility * 1.3;
            var detail = new DamageDetail
            {
                DamageType = StaticDataHelper.FistDamage,
                DirectSource = $"{this.Profession}'s Fist",
                tags = [StaticDataHelper.UnFightBackable, StaticDataHelper.UnDodgeable]
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
            }
            base.LoadBuff(newBuff, source,buffLevel);
        }
    }
}
