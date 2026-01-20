using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore.Models;

namespace BattleCore.BattleEvnetArgs
{
    public class LoadBuffEventArgs:EventArgs
    {
        public Buff buff { get; set; }
        public Fighter? Source { get; set; }
        public int BuffLevel { get; set; }
        public LoadBuffEventArgs(Buff buff, Fighter? source = null,int bufflevel = 1)
        {
            this.buff = buff;
            Source = source;
            BuffLevel = bufflevel;
        }
    }
}
