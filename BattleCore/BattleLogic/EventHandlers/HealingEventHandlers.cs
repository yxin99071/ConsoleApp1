using BattleCore.BattleEventArgs;
using BattleCore.DataModel.Fighters;

namespace BattleCore.BattleLogic.EventHandlers
{
    public class HealingEventHandlers
    {
        public static void HealingOnHp(object? sender,HealingEventArgs e)
        {
            ((Fighter)sender!).Health += Math.Abs(e.HealingValue);
            JsonLogger.LogHealing(((Fighter)sender!).Name, (int)e.HealingValue, (int)((Fighter)sender!).Health);
        }
    }
}
