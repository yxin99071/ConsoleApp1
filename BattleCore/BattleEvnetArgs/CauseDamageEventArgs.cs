using BattleCore.DataModel.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleEvnetArgs
{
    public class CauseDamageEventArgs:EventArgs
    {
        public DamageInfo damageInfo { get; set; }
        public CauseDamageEventArgs(DamageInfo info) => damageInfo = info;

    }
}
