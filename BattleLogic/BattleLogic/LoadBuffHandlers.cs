using BattleCore.BattleEventArgs;
using BattleCore.DataModel.States;
using BattleLogic.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleLogic.BattleLogic
{
    public static class LoadBuffHandlers
    {
        public static void LoadBuff(object? sender, LoadBuffEventArgs e)
        {
            if (((Fighter)sender!).BuffStatuses.Any(s => s.buff.Name == "FakeHealth") && !e.buff.IsOnSelf)
                return;
            ((Fighter)sender!).BuffStatuses.Add(new BuffStatus(e.buff, (Fighter)sender,e.Source));
            BattleLogger.LoadBuffBegin(e.buff);
            JsonLogger.LogBuffApply(((Fighter)sender).Name, e.buff.Name,e.BuffLevel);
        }
    }
}
