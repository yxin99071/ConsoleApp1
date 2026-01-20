using BattleCore.BattleEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic
{
    public static class CauseDamageHandlers
    {
        public static void CorrectDamageByBuff(object? sender, CauseDamageEventArgs e)
        {
            if (e.damageInfo.Source is null)
                return;
            if(e.damageInfo.Source!.BuffStatuses.Count>0)
            {
                foreach (var buffStatus in e.damageInfo.Source.BuffStatuses)
                {
                    e.damageInfo.Damage *= buffStatus.buff.DamageCorrection;
                }
            }
        }

    }
}
