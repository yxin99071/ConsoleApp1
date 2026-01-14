using BattleCore.BattleEvnetArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic
{
    public class GlobalEventBus
    {
        public event EventHandler<RoundBeginEventArgs>? RoundBeginEA;
        public event EventHandler<RoundEndEventArgs>? RoundEndEventArgs;

        public void BeginRound()
        {
            RoundBeginEA?.Invoke(this, new RoundBeginEventArgs());
        }
        public void EndRound()
        {
            RoundEndEventArgs?.Invoke(this, new RoundEndEventArgs());
        }
    }
}
