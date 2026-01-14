using BattleCore.BattleEvnetArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel.Fighters
{
    public class Magician : Fighter
    {
        public Magician(string name, double health, double agility, double strength, double intelligence, List<BuffStatus> buffStatuses, List<Weapon> weapons)
    : base(name, health, agility, strength, intelligence, buffStatuses, weapons)
        {
            Profession = "Magician";
        }
        public string Profession { get; set; }

        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Intelligence * 1.0;
            //添加buff
            Random random = new Random();
            var buff = CommonData.BuffPools[random.Next(0, CommonData.BuffPools.Count)];
            if (buff.IsOnSelf)
                this.LoadBuff(buff, null);
            else
                damageInfo.Target.LoadBuff(buff, this);
        }
        public override void LoadBuff(Buff buff, Fighter? source)
        {
            var newBuff = new Buff(buff);
            if (source is not null)
            {
                if (newBuff.CoefficientStrength > 0)
                    newBuff.DirectDamage = newBuff.CoefficientStrength * source.Strength;
                if (newBuff.CoefficientAgility > 0)
                    newBuff.DirectDamage = newBuff.CoefficientAgility * source.Agility;
            }
            //法师被动
            if (newBuff.DamageCorrection < 1 || newBuff.WoundCorrection > 1 || newBuff.DirectDamage > 0)
                newBuff.LastRound--;
            if (newBuff.DamageCorrection > 1 || newBuff.WoundCorrection < 1)
                newBuff.LastRound++;
            base.LoadBuff(newBuff, source);
        }

    }
}
