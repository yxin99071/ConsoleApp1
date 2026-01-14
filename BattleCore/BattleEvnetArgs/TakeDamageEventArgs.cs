using BattleCore.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleEvnetArgs
{
    public class TakeDamageEventArgs:EventArgs
    {
        public DamageInfo damageInfo { get; set; }
        public TakeDamageEventArgs(DamageInfo info) => damageInfo = info;
    }
}
