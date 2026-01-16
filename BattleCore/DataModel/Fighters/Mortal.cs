using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel.Fighters
{
    public class Mortal :Fighter
    {
        public Mortal(string name, double health, double agility, double strength, double intelligence, List<BuffStatus> buffStatuses, List<Weapon> weapons,List<Skill> skills)
: base(name, health, agility, strength, intelligence, buffStatuses, weapons,skills)
        {
            Profession = "Magician";
        }
        public string Profession { get; set; }

        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Agility*0.71 + Strength*0.71 + Intelligence * 0.71;
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
            base.LoadBuff(newBuff, source);
        }

    }
}
