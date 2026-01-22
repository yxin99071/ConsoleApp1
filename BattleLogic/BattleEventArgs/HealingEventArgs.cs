using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleEventArgs
{
    public class HealingEventArgs
    {
        public List<string> Tags { get; set; }
        public Fighter HealedCharac { get; set; }
        public double HealingValue { get; set; }
        public HealingEventArgs(List<string> tags, Fighter healedCharac, double healingValue)
        {
            Tags = tags?? new List<string>();
            HealedCharac = healedCharac;
            HealingValue = healingValue;
        }
    }
}
