using BattleCore.BattleEvnetArgs;
using BattleCore.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic
{
    public static class TakeDamageHandlers
    {
        public static void CorrectDamage(object? sender, TakeDamageEventArgs e)
        {
            if(e.damageInfo.Target.BuffStatuses.Count()!=0)
            {
                foreach(var buffStatus in e.damageInfo.Target.BuffStatuses)
                {
                    e.damageInfo.Damage *= buffStatus.buff.WoundCorrection;
                }
            }
        } 
        public static void AvoidanceOrDamage(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.DamageTag.Contains(CommonData.UnDodgeable))
                return;

            double AvoidChance = 0.5 - (0.5 / (0.05 * e.damageInfo.Target.Agility + 1));
            var random = new Random();
            int choice = random.Next(0, 101);       
            if (choice / 100.0 < AvoidChance)
            {
                BattleLogger.LogReaction(e.damageInfo.Target.Name, "Dodge");
                e.damageInfo.Damage = -1;
            }
        }
        public static void DamageOnHp(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.Damage >= 0)
            {
                e.damageInfo.Target.Health -= e.damageInfo.Damage;
                BattleLogger.LogDamage(e.damageInfo.Target.Name, e.damageInfo.Damage);
                if(e.damageInfo.Buffs!=null)
                {
                    foreach(Buff buff in e.damageInfo.Buffs)
                    {
                        e.damageInfo.Target.LoadBuff(buff,e.damageInfo.Source);
                    }
                }
            }
        }

        public static void FightBack(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.DamageTag.Contains(CommonData.UnFightBackable))
                return;
                
            if (e.damageInfo.Damage == -1)
            {
                e.damageInfo.Damage = 0;
                return;
            }
            double FightBackChance = (0.5 - (0.5 / (0.05 * e.damageInfo.Target.Agility + 1))) /2;
            var random = new Random();
            int choice = random.Next(0, 101);
            if (choice / 100.0 < FightBackChance)
            {
                BattleLogger.LogReaction(e.damageInfo.Target.Name, "FightBack");
                BattleController.DecideAction(e.damageInfo.Target, e.damageInfo.Source);
            }
        }
       
    }
}
