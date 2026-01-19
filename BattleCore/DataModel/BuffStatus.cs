using BattleCore.DataModel.Fighters;
using DataCore.Models;

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
