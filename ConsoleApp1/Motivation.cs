using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Motivation
    {
        public static void DoAttack(object? sender, DamageJudgeEventArgs e)
        {
            Console.WriteLine($"Attacker:\t\t {e.attacker}");
            Console.WriteLine($"Defender:\t\t {e.defender}");
            //修正攻击力

            int finalDamage = e.attacker.Damage + e.attacker.DamageCorrection;
            //是否破防
            if (finalDamage > e.defender.HealthCorrection)
            {
                e.defender.Health -= (finalDamage - e.defender.HealthCorrection);
                e.defender.HealthCorrection = 0;
            }
            else
                e.defender.HealthCorrection -= finalDamage;

        }
        public static void NormalBuff(object? sender, BeforeDamageEventArgs e)
        {
            
        }

    }
}
