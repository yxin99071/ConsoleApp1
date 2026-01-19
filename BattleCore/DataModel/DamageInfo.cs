using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore.Models;

namespace BattleCore.DataModel
{
    public class DamageInfo
    {
        public DamageInfo(Fighter? source, Fighter target, double damage, List<Buff>? buffs = null, string damageTag = "Normal,")
        {
            Source = source;
            Target = target;
            Damage = damage;
            if(buffs!=null)
            {
                foreach (Buff buff in buffs)
                    Buffs.Add(buff);
            }
            DamageTag = damageTag.Split(',').ToList();
        }

        public Fighter? Source { get; set; }
        public Fighter Target { get; set; }
        public double Damage { get; set; }
        public List<Buff> Buffs { get; set; } = new List<Buff>();
        public List<String> DamageTag { get; set; }
        
    }
}
