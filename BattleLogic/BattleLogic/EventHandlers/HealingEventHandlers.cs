using BattleCore.BattleEventArgs;
using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic.EventHandlers
{
    public class HealingEventHandlers
    {
        public static void HealingOnHp(object? sender,HealingEventArgs e)
        {
            ((Fighter)sender!).Health += e.HealingValue;
            JsonLogger.LogHealing(((Fighter)sender!).Name, (int)e.HealingValue, (int)((Fighter)sender!).Health);
        }
    }
}
