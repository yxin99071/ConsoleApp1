using BattleCore.BattleEvnetArgs;
using BattleCore.DataModel;
using DataCore.Models;
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
            if (e.damageInfo.DamageTag.Contains(StaticData.UnDodgeable))
                return;

            double AvoidChance = StaticData.CalculateDodge(e.damageInfo.Target.Agility);
            var random = new Random();
            int choice = random.Next(0, 101);       
            if (choice / 100.0 < AvoidChance)
            {
                BattleLogger.LogReaction(e.damageInfo.Target.Name, "Dodge");
                e.damageInfo.Damage = -1;
                e.damageInfo.Buffs.Clear();
            }
        }
        public static void DamageOnHp(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.Damage > 0)
            {
                //判断虚假生命值
                var FakeHealth = e.damageInfo.Target.BuffStatuses.SingleOrDefault(s => s.buff.Name == "FakeHealth");
                if (FakeHealth != null)
                {
                    //如果解析护盾值成功
                    if (Double.TryParse(FakeHealth.buff.SpecialTag[0], out double shield))
                    {
                        if (shield >= e.damageInfo.Damage)
                        {
                            shield -= e.damageInfo.Damage;
                            e.damageInfo.Damage = 0;
                            FakeHealth.buff.SpecialTag[0] = ((int)shield).ToString();
                        }
                        else
                        {
                            e.damageInfo.Damage -= shield;
                            e.damageInfo.Target.BuffStatuses.Remove(FakeHealth);
                        }
                    }
                        
                }

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
            e.damageInfo.DamageTag.Add(StaticData.UnFightBackable);
        }
        public static void PassiveUndeadWilling(object? sender, TakeDamageEventArgs e)
        {
            
            if (e.damageInfo.Target.Health >= 0)
                return;
            if (e.damageInfo.Source == null)
                return;
            BattleLogger.PassiveSkillInvoke("UndeadWilling");
            var damageCorrection = 2*Math.Abs(e.damageInfo.Target.Health) / e.damageInfo.Target.MaxHealth;
            e.damageInfo.Target.LoadBuff(new Buff { Name="UndeadWilling",LastRound = 1,IsOnSelf = true,DamageCorrection = 1 + damageCorrection },null);
            e.damageInfo.DamageTag.Add(StaticData.UnFightBackable);
            e.damageInfo.Target.TakeDamageEA -= PassiveUndeadWilling;

            BattleController.DecideAction(e.damageInfo.Target, e.damageInfo.Source);

            if(e.damageInfo.Target.SpeedBar>e.damageInfo.Target.Max_SpeedBar)
                e.damageInfo.Target.LoadBuff(new Buff {Name= "Block",LastRound = 0,IsOnSelf = true,DamageCorrection = 1 + damageCorrection }, null);

            if (e.damageInfo.Source.IsDead)
                e.damageInfo.Target.Health = 1;
        }

        public static void FightBack(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.DamageTag.Contains(StaticData.UnFightBackable))
                return;
                
            if (e.damageInfo.Damage == -1)
            {
                e.damageInfo.Damage = 0;
                return;
            }
            double FightBackChance = StaticData.CalculateCounterRate(e.damageInfo.Target.Agility, e.damageInfo.Target.Strength, e.damageInfo.Target.Intelligence);
            //最高20%概率反击
            var random = new Random();
            int choice = random.Next(0, 101);
            if (choice / 100.0 < FightBackChance)
            {
                BattleLogger.LogReaction(e.damageInfo.Target.Name, "FightBack");
                BattleController.DecideAction(e.damageInfo.Target, e.damageInfo.Source);
            }
        }

        public static void JudgeDeath(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.Target.Health <= 0)
                e.damageInfo.Target.IsDead = true;
        }
       
    }
}
