using BattleLogic.DataModel;
using BattleLogic.DataModel.States;
using DataCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleLogic.DataModel.Fighters
{
    public class Mortal :Fighter
    {
        public Mortal(User user): base(user)
        {
            Profession = "Magician";
        }

        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Agility*0.71 + Strength*0.71 + Intelligence * 0.71;
            var detail = new DamageDetail
            {
                DamageType = StaticData.FistDamage,
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
            }
            base.LoadBuff(newBuff, source, buffLevel);
        }

    }
}
