using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class SystemController
    {
        private readonly BattleSystem _bs;
        public SystemController(BattleSystem bs)
        {
            _bs = bs;
        }

        public void BattleControll(Character c_1, Character c_2)
        {
            _bs.DamageJudgeEv += Motivation.DoAttack;
            c_1.AddBuffer(new NormalBuffer { damageCorrection = 10, healthCorrection = 10,lastRound = 3});
            c_2.AddBuffer(new NormalBuffer { damageCorrection = 0, healthCorrection = 100, lastRound = 10 });

            while(true)
            {
                if (c_1.Health <= 0)
                    break;
                _bs.BasicAttack(c_1, c_2);
                Thread.Sleep(1000);
                if (c_2.Health <= 0)
                    break;
                _bs.BasicAttack(c_2, c_1);
                Thread.Sleep(1000);
            }
        }

    }
}
