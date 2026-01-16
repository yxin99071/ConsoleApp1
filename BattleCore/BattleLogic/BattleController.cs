using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic
{
    public static class BattleController
    {
        public static void AttackWithFist(Fighter source, Fighter taker)
        {
            BattleLogger.LogAction(source.Name, new {Name="Fist"});
            
            DamageInfo damageInfo = new DamageInfo(source, taker, 0);

            source.SetFitDamage(damageInfo);
            source.CauseDamage(damageInfo);
            taker.TakeDamage(damageInfo);
       
        }
        public static void AttackWithWeapon(Fighter source,Fighter taker)
        {
            //todo 判断武器是否剩余
            double finalDamage = 0;
            var random = new Random();
            int choice = random.Next(0, source.Weapons.Count());
            Weapon weapon = source.Weapons[choice];
            BattleLogger.LogAction(source.Name, weapon);
            DamageInfo damageInfo;

            //todo 移除武器
            finalDamage += (weapon.CoefficientStrength * source.Strength 
                + weapon.CoefficientAgility * source.Agility
                + weapon.CoefficientIntelligence* source.Intelligence);

            if (weapon.Buffs.Count() > 0)
            {
                damageInfo = new DamageInfo(source, taker, finalDamage,weapon.Buffs.Where(b=>b.IsOnSelf!=true).ToList());
            }
            else
                damageInfo = new DamageInfo(source, taker, finalDamage);

            
            source.CauseDamage(damageInfo);
            //结算自己伤害后，判定伤害前挂载自我buff
            foreach(var buff in weapon.Buffs)
            {
                if(buff.IsOnSelf)
                {
                    source.LoadBuff(buff,null);
                }
            }
            taker.TakeDamage(damageInfo);

            
        }
        public static void DecideAction(Fighter source, Fighter? taker)
        {

            if (taker == null || source.BuffStatuses.Any(b => b.buff.Name == "Block"))
                return;
            //计算权重
            var weaponRight = source.Weapons.Count;
            var skillRight = source.Skills.Count;
            var fistRight = (skillRight + weaponRight) / 2;
            var random = new Random();
            int choice = random.Next(0, fistRight+weaponRight + skillRight+1);
            if (choice <= fistRight)
                AttackWithFist(source, taker);
            else if (choice <= weaponRight + fistRight)
                AttackWithWeapon(source, taker);
            else
                ActionWithSkill(source,taker);
        }
        public static void BuffEffection(Fighter fighter)
        {
            if(fighter.BuffStatuses.Count()>0)
            {
                List<BuffStatus> damagedBuffs = new List<BuffStatus>();
                foreach(var buffStatus in fighter.BuffStatuses)
                {
                    if(buffStatus.buff.DirectDamage !=0)
                        damagedBuffs.Add(buffStatus);

                }
                //先遍历完再结算伤害，否则可能出现遍历时清除buff的而错误
                foreach(var buffStatus in damagedBuffs)
                {
                    BattleLogger.LogBuffDamage(buffStatus.buff.Name);
                    var damageTag = $"{CommonData.UnDodgeable},{CommonData.UnFightBackable}";
                    DamageInfo damageInfo = new DamageInfo(buffStatus.Source, fighter, buffStatus.buff.DirectDamage, damageTag: damageTag);
                    buffStatus.Source?.CauseDamage(damageInfo);
                    fighter.TakeDamage(damageInfo);
                }
            }
        }
        public static void BuffSettle(Fighter fighter)
        {
            var timeOutBuffs = new List<BuffStatus>();
            foreach(var buffStatus in fighter.BuffStatuses)
            {
                if (buffStatus.buff.LastRound > 0)
                    buffStatus.buff.LastRound--;
                else
                    timeOutBuffs.Add(buffStatus);
            }
            foreach(var buffStatus in timeOutBuffs)
            {
                BattleLogger.LogBuffTimeOut(buffStatus.buff.Name);
                fighter.BuffStatuses.Remove(buffStatus);
            }
        }
        public static void Initial(List<Fighter> fighters)
        {
            for (int i = 0; i < fighters.Count; i++)
            {
                fighters[i].ClearAllEA();

                //绑定顺序很重要
                fighters[i].LoadBuffEA += LoadBuffHandlers.LoadBuff;
                fighters[i].CauseDamageEA += CauseDamageHandlers.CorrectDamageByBuff;
                fighters[i].TakeDamageEA += TakeDamageHandlers.CorrectDamage;
                fighters[i].TakeDamageEA += TakeDamageHandlers.AvoidanceOrDamage;
                fighters[i].TakeDamageEA += TakeDamageHandlers.DamageOnHp;


                if (fighters[i].Skills.Any(s => s.Name == "PretendDeath"))
                    fighters[i].TakeDamageEA += TakeDamageHandlers.PassivePretendDeath;
                if (fighters[i].Skills.Any(s=>s.Name == "UndeadWilling"))
                    fighters[i].TakeDamageEA += TakeDamageHandlers.PassiveUndeadWilling;

                fighters[i].TakeDamageEA += TakeDamageHandlers.FightBack;
                fighters[i].TakeDamageEA += TakeDamageHandlers.JudgeDeath;
            }
        }
        public static void CalcuAction(Fighter fighter_1,Fighter fighter_2)
        {
            var balanceSumption = (int)(fighter_1.Agility + fighter_2.Agility) / 2;

            fighter_1.SpeedBar += balanceSumption + (int)fighter_1.Agility;
            fighter_2.SpeedBar += balanceSumption + (int)fighter_2.Agility;
            

        }
        public static void ActionWithSkill(Fighter source, Fighter taker)
        {
            var random = new Random();
            Skill chosenSkill;
            do
            {
                var choice = random.Next(0, source.Skills.Count);
                chosenSkill = source.Skills[choice];
            } while (chosenSkill.IsPassive);

            //normalSkill
            if(chosenSkill.Tags.Count == 0)
                ActionWithNormalSkill(source, taker, chosenSkill);
            //specialSkill
            else
            {
                var todoSkills = new List<Skill>();
                foreach(var tag in chosenSkill.Tags)
                {
                    if (CommonData.SpecialSkillMap.TryGetValue(tag, out var action))
                        action(source, taker, chosenSkill);
                }
            }


        }
        public static void ActionWithNormalSkill(Fighter source, Fighter taker,Skill skill)
        {
            double finalDamage = source.Agility * skill.CoefficientAgility
                + source.Intelligence * skill.CoefficientIntelligence
                + source.Strength * skill.CoefficientStrength;
            if (finalDamage > 0)
            {
                DamageInfo damageInfo;

                if (skill.Buffs.Count() > 0)
                {
                    damageInfo = new DamageInfo(source, taker, finalDamage, skill.Buffs.Where(b => b.IsOnSelf != true).ToList());
                    damageInfo.DamageTag = new List<string>(skill.Tags);
                }
                else
                    damageInfo = new DamageInfo(source, taker, finalDamage);
                source.CauseDamage(damageInfo);
                //结算自己可能打出的伤害后，判定对方伤害前挂载自我buff
                foreach (var buff in skill.Buffs)
                {
                    if (buff.IsOnSelf)
                    {
                        source.LoadBuff(buff, null);
                    }
                }
                taker.TakeDamage(damageInfo);
            }
            //没有伤害，只有buff
            else
            {
                foreach (var buff in skill.Buffs)
                {
                    if (buff.IsOnSelf)
                    {
                        source.LoadBuff(buff, null);
                    }
                }
            }

        }
        #region SpecialSkillAction
        public static void ActionWithSkillTorture(Fighter source,Fighter taker,Skill skill)
        {
            var random = new Random();
            List<Buff> tempBuff = new List<Buff>();
            do
            {
                var buffChoice = random.Next(0, CommonData.BuffPool.Count);
                if (CommonData.BuffPool[buffChoice].IsOnSelf)
                    continue;
                tempBuff.Add(CommonData.BuffPool[buffChoice]);
            } while (tempBuff.Count < 4);
            skill.Buffs = tempBuff;
            ActionWithNormalSkill(source, taker, skill);
        }
        #endregion
        public static bool BattleSimulation(Fighter source, Fighter taker)
        {
            while (true)
            {
                CalcuAction(source, taker);
                var Max_SpeedBar = (int)(source.Agility + taker.Agility) * 2;
                source.Max_SpeedBar = Max_SpeedBar;
                taker.Max_SpeedBar = Max_SpeedBar;
                if (source.SpeedBar >= Max_SpeedBar)
                {
                    Console.WriteLine($"New Round:======{source.Name}========");
                    //Thread.Sleep(1000);
                    BuffEffection(source);
                    DecideAction(source, taker);
                    BuffSettle(source);
                    Console.WriteLine($"{taker.Name}'s Health:{(int)taker.Health}");
                    source.SpeedBar -= Max_SpeedBar;

                }

                if (source.IsDead)
                {
                    Console.WriteLine($"Game Over, {taker.Name} win!");
                    return false;
                }
                if (taker.IsDead)
                {
                    Console.WriteLine($"Game Over, {source.Name} win!");
                    return true;
                }


                if (taker.SpeedBar >= Max_SpeedBar)
                {
                    Console.WriteLine($"New Round:======{taker.Name}========");
                    //Thread.Sleep(1000);
                    BuffEffection(taker);
                    DecideAction(taker, source);
                    BuffSettle(taker);
                    Console.WriteLine($"{source.Name}'s Health:{(int)source.Health}");
                    taker.SpeedBar -= Max_SpeedBar;//放在最后可以在方法中判断是否正在自己的回合

                }

                if (source.IsDead)
                {
                    Console.WriteLine($"Game Over, {taker.Name} win!");
                    return false;
                }
                if (taker.IsDead)
                {
                    Console.WriteLine($"Game Over, {source.Name} win!");
                    return true;
                }

            }
        }
        

    }



}
