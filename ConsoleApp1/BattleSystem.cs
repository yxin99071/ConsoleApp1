using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class BattleSystem
    {
        public event EventHandler<DamageJudgeEventArgs>? DamageJudgeEv;
        public event EventHandler<BeforeDamageEventArgs>? BeforeDamageEv;
        public event EventHandler<AfterDamageEventArgs>? AfterDamageEv;

        public void BasicAttack(Character attacker, Character defender)
        {
            DamageJudgeEv?.Invoke(this, new DamageJudgeEventArgs(attacker, defender));
        }

        public void PreAdjust(Character attacker, Character defender)
        {
            if (attacker.Buffers != null && attacker.Buffers.Count != 0)
            {
                //BeforeDamageEv.Invoke(attacker, defender);
            }
            //修正防御
            if (defender.Buffers != null && defender.Buffers.Count != 0)
            {
                for (int i = defender.Buffers.Count - 1; i >= 0; i--)
                {
                    if (defender.Buffers[i].lastRound == 0)
                    {
                        defender.RemoveBuffer(defender.Buffers[i]);
                        continue;
                    }
                }
            }
        }

        public void BattleSettle(Character attacker, Character defender)
        {
            
        }
    }
}
