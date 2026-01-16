using BattleCore.BattleEvnetArgs;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic
{
    public static class LoadBuffHandlers
    {
        public static void LoadBuff(object? sender, LoadBuffEventArgs e)
        {
            if (((Fighter)sender!).BuffStatuses.Any(s => s.buff.Name == "FakeHealth"))
                return;
            ((Fighter)sender!).BuffStatuses.Add(new BuffStatus(e.buff, (Fighter)sender,e.Source));
        }
    }
}
