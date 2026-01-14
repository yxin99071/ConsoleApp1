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

            double AvoidChance = 0.5 - (0.5 / (0.05 * e.damageInfo.Target.Agility + 1))/1.3;
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
                //判断吸血类buff生效
                if(e.damageInfo.Source is not null)
                {
                    if (e.damageInfo.Source.BuffStatuses.Any(b => b.buff.Name == "UndeadWilling"))
                    {
                        //UndeadWilling有50%的吸血
                        e.damageInfo.Source.Health += e.damageInfo.Damage*0.5;
                    }
                }
            }
        }
        public static void PassivePretendDeath(object? sender, TakeDamageEventArgs e)
        {
            
            if (e.damageInfo.Target.Health > 0)
                return;
            BattleLogger.PassiveSkillInvoke("PretendDeath");
            //生命值置为1，清除所有buff
            e.damageInfo.Target.Health = 1;
            e.damageInfo.Target.BuffStatuses.Clear();
            e.damageInfo.Target.TakeDamageEA -= PassivePretendDeath;
        }
        public static void PassiveUndeadWilling(object? sender, TakeDamageEventArgs e)
        {
            
            if (e.damageInfo.Target.Health >= 0)
                return;
            BattleLogger.PassiveSkillInvoke("UndeadWilling");
            var damageCorrection = 2*Math.Abs(e.damageInfo.Target.Health) / e.damageInfo.Target.MaxHealth;
            e.damageInfo.Target.LoadBuff(new Buff("UndeadWilling", 1, true, 1+damageCorrection),null);
            e.damageInfo.DamageTag.Add(CommonData.UnFightBackable);
            e.damageInfo.Target.TakeDamageEA -= PassiveUndeadWilling;

            BattleController.DecideAction(e.damageInfo.Target, e.damageInfo.Source);

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
            double FightBackChance = (0.5 - (0.5 / (0.05 * e.damageInfo.Target.Agility + 1))) /1.5;
            //最高33%概率反击
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
