using BattleCore.DataModel;
using BattleCore.EntityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleEvnetArgs
{
    public class LoadBuffEventArgs:EventArgs
    {
        public Buff buff { get; set; }
        public Fighter? Source { get; set; }
        public LoadBuffEventArgs(Buff buff, Fighter? source = null)
        {
            this.buff = buff;
            Source = source;
        }
    }
}
