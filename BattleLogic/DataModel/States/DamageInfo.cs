using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore.Models;
using BattleLogic.DataModel.Fighters;

namespace BattleLogic.DataModel.States
{
    public class DamageInfo
    {
        public DamageInfo(Fighter? source, Fighter target, double damage)
        {
            Source = source;
            Target = target;
            damageDetail = new DamageDetail();
            Damage = damage;
        }

        public Fighter? Source { get; set; }
        public Fighter Target { get; set; }
        public DamageDetail damageDetail { get; set; }
        public double Damage { get; set; }

        
    }
}
