using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public class BuffStatus
    {
        public Buff buff;
        public Fighter? Source;
        public Fighter Taker;

        public BuffStatus(Buff buff,Fighter taker,Fighter? source = null)
        {
            this.buff = buff;
            Taker = taker;
            Source = source;
        }
    }
}
