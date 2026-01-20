using BattleLogic.DataModel.Fighters;
using DataCore.Models;

namespace BattleLogic.DataModel.States
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
