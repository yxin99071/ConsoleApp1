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
            Profession = "Ranger";
        }
        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Agility * 1.3;
            damageInfo.DamageTag = [StaticData.UnFightBackable, StaticData.UnDodgeable];
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
