using BattleCore.BattleEventArgs;
using BattleCore.DataModel;
using DataCore.Models;
namespace BattleCore.BattleLogic.EventHandlers
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
            if (e.damageInfo.damageDetail.tags.Contains(StaticDataHelper.UnDodgeable))
                return;

            double AvoidChance = StaticDataHelper.CalculateDodge(e.damageInfo.Target.Agility);
            var random = new Random();
            int choice = random.Next(0, 101);       
            if (choice / 100.0 < AvoidChance)
            {
                BattleLogger.LogReaction(e.damageInfo.Target.Name, "Dodge");
                JsonLogger.LogDodge(e.damageInfo.Target.Name);
                e.damageInfo.Damage = -1;
                e.damageInfo.damageDetail.buffs.Clear();
            }
        }
        public static void DamageOnHp(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.Damage <= 0)
                return;
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
            //伤害结算逻辑
            e.damageInfo.Target.Health -= e.damageInfo.Damage;
            BattleLogger.LogDamage(e.damageInfo.Target.Name, e.damageInfo.Damage, e.damageInfo.Target.Health);
            #region 日志逻辑
            //如果是来自Buff的
            if (e.damageInfo.damageDetail.tags.Contains(StaticDataHelper.BuffDamage))
            {
                JsonLogger.LogBuffTick(e.damageInfo.Target.Name, e.damageInfo.damageDetail.DirectSource, (int)e.damageInfo.Damage);
                //来自buff的伤害，buff会被添加进伤害信息
            }
            else
            {

                JsonLogger.LogDamage(e.damageInfo.Target.Name
                    , (int)e.damageInfo.Damage
                    , (int)e.damageInfo.Target.Health
                    , e.damageInfo.damageDetail.tags.Contains(StaticDataHelper.CriticalDamage));

            }
            #endregion
            if (e.damageInfo.damageDetail.buffs != null)
            {
                foreach (Buff buff in e.damageInfo.damageDetail.buffs)
                {
                    e.damageInfo.Target.LoadBuff(buff, e.damageInfo.Source);
                }
            }
            //判断吸血类buff生效
            if(e.damageInfo.Source is not null)
            {
                if (e.damageInfo.Source.BuffStatuses.Any(b => b.buff.Name == "亡者意志"))
                {
                    //UndeadWilling有50%的吸血
                    e.damageInfo.Source.Heal(e.damageInfo.Damage/2,new List<string>());
                }
            }

        }
        public static void PassivePretendDeath(object? sender, TakeDamageEventArgs e)
        {
            
            if (e.damageInfo.Target.Health > 0)
                return;
            BattleLogger.PassiveSkillInvoke("假死");
            JsonLogger.LogPassive(e.damageInfo.Target.Name, "假死");
            //生命值回复至1，清除所有buff
            e.damageInfo.Target.Heal(Math.Abs(e.damageInfo.Target.Health)+1,new List<string>());
            foreach (var buffstatus in e.damageInfo.Target.BuffStatuses)
                JsonLogger.LogBuffUpdate(e.damageInfo.Target.Name, buffstatus.buff.Name);

            e.damageInfo.Target.BuffStatuses.Clear();
            e.damageInfo.Target.TakeDamageEA -= PassivePretendDeath;
            e.damageInfo.damageDetail.tags.Add(StaticDataHelper.UnFightBackable);
        }
        public static void PassiveUndeadWill(object? sender, TakeDamageEventArgs e)
        {
            
            if (e.damageInfo.Target.Health >= 0)
                return;
            if (e.damageInfo.Source == null)
                return;
            BattleLogger.PassiveSkillInvoke("亡者意志");
            JsonLogger.LogPassive(e.damageInfo.Target.Name, "亡者意志");

            var damageCorrection = 2*Math.Abs(e.damageInfo.Target.Health) / e.damageInfo.Target.MaxHealth;
            e.damageInfo.Target.LoadBuff(new Buff { Name="亡者意志",LastRound = 0,IsOnSelf = true,DamageCorrection = 1 + damageCorrection },null);
            e.damageInfo.damageDetail.tags.Add(StaticDataHelper.UnFightBackable);
            e.damageInfo.Target.TakeDamageEA -= PassiveUndeadWill;

            BattleHelper.DecideAction(e.damageInfo.Target, e.damageInfo.Source);

            //跳过一个回合
            e.damageInfo.Target.LoadBuff(new Buff { Name = "锁定", LastRound = 0, IsOnSelf = true, DamageCorrection = 1 + damageCorrection }, null);
            

            if (e.damageInfo.Source.IsDead)
                e.damageInfo.Target.Heal(Math.Abs(e.damageInfo.Target.Health)+1,new List<string>());
        }

        public static void FightBack(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.damageDetail.tags.Contains(StaticDataHelper.UnFightBackable))
                return;
                
            if (e.damageInfo.Damage == -1)
            {
                e.damageInfo.Damage = 0;
                return;
            }
            double FightBackChance = StaticDataHelper.CalculateCounterRate(e.damageInfo.Target.Agility, e.damageInfo.Target.Strength, e.damageInfo.Target.Intelligence);
            //最高20%概率反击
            var random = new Random();
            int choice = random.Next(0, 101);
            if (choice / 100.0 < FightBackChance)
            {
                using (JsonLogger.StartReactionScope(e.damageInfo.Target.Name, "还击"))
                {
                    BattleLogger.LogReaction(e.damageInfo.Target.Name, "还击");
                    BattleHelper.DecideAction(e.damageInfo.Target, e.damageInfo.Source);
                }
            }
        }

        public static void JudgeDeath(object? sender, TakeDamageEventArgs e)
        {
            if (e.damageInfo.Target.Health <= 0)
                e.damageInfo.Target.IsDead = true;
        }
       
    }
}
