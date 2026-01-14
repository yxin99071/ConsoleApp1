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
            ((Fighter)sender!).BuffStatuses.Add(new BuffStatus(e.buff, (Fighter)sender,e.Source));
        }
    }
}
