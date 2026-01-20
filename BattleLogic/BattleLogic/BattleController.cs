using BattleLogic;
using BattleLogic.DataModel;
using BattleLogic.DataModel.Fighters;
using BattleLogic.DataModel.States;
using DataCore.Models;
using System;
using System.Net;
namespace BattleLogic.BattleLogic
{
    public static class BattleController
    {
        record Fist(string Name);
        public static void AttackWithFist(Fighter source, Fighter taker)
        {
            
            BattleLogger.LogAction(source.Name, new Fist(Name:source.Profession+"'s Fist"));
            JsonLogger.LogAction(source.Name, "Fist", source.Profession + "'s Fist");
            DamageInfo damageInfo = new DamageInfo(source, taker, 0);

            source.SetFitDamage(damageInfo);
            source.CauseDamage(damageInfo);
            taker.TakeDamage(damageInfo);
       
        }
        public static void AttackWithWeapon(Fighter source,Fighter taker)
        {
            //如果武器为空，不会进入到该函数
            double finalDamage = 0;
            var random = new Random();
            int choice = random.Next(0, source.Weapons.Count());
            Weapon weapon = source.Weapons[choice];

            BattleLogger.LogAction(source.Name, weapon);
            JsonLogger.LogAction(source.Name, "weapon", weapon.Name);

            DamageInfo damageInfo;
            //伤害细节
            var detail = new DamageDetail
            {
                DamageType = StaticData.WeaponDamage,
                DirectSource = weapon.Name,
                tags = new List<String>(weapon.Tags),
            };
            //移除武器
            if (source.Weapons.Any(w=>w.Name == weapon.Name))
                source.Weapons.Remove(weapon);

            finalDamage += (weapon.CoefficientStrength * source.Strength 
                + weapon.CoefficientAgility * source.Agility
                + weapon.CoefficientIntelligence* source.Intelligence);
            //UNSURECHANGED : 原本没有导航属性
            if (weapon.WeaponBuffs.Count > 0)
            {
                damageInfo = new DamageInfo(source, taker, finalDamage);
                detail.buffs = BattleDataBridge.ExtractBuffs(null, weapon.WeaponBuffs);
            }
            else
                damageInfo = new DamageInfo(source, taker, finalDamage);

            //挂载伤害细节 
            damageInfo.damageDetail = detail;
            source.CauseDamage(damageInfo);
            //结算自己伤害后，判定伤害前挂载自我buff
            //UNSURECHANGED : 原本没有导航属性
            var OnSelfBuffs = weapon.WeaponBuffs.Where(wb => wb.Buff.IsOnSelf).Select(wb => wb.Buff);
            
            foreach (var buff in OnSelfBuffs)
            {
                var buffLevel = weapon.WeaponBuffs.SingleOrDefault(b => b.Buff.Name == buff.Name)?.Level ?? 1;
                if (buff.IsOnSelf)
                {
                    source.LoadBuff(buff,null, buffLevel);
                }
            }
            taker.TakeDamage(damageInfo);

            
        }
        public static void DecideAction(Fighter source, Fighter? taker)
        {

            if (taker == null || source.BuffStatuses.Any(b => b.buff.Name == "BLOCK"))
                return;
            //计算权重
            var weaponRight = source.Weapons.Count;
            var skillRight = source.Skills.Where(s=>!s.IsPassive).ToList().Count;
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
                    BattleLogger.LogBuffDamage(buffStatus.buff);
                    DamageInfo damageInfo = new DamageInfo(buffStatus.Source, fighter, buffStatus.buff.DirectDamage);
                    //来自Buff的伤害不可反击
                    var detail = new DamageDetail
                    {
                        DamageType = StaticData.BuffDamage,
                        DirectSource = buffStatus.buff.Name,
                        tags = [StaticData.UnDodgeable,StaticData.UnFightBackable,StaticData.BuffDamage]
                    };
                    damageInfo.damageDetail = detail;

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


                if (fighters[i].Skills.Any(s => s.Name == StaticData.PassivePretendDeath))
                    fighters[i].TakeDamageEA += TakeDamageHandlers.PassivePretendDeath;
                if (fighters[i].Skills.Any(s=>s.Name == StaticData.PassiveUndeadWilling))
                    fighters[i].TakeDamageEA += TakeDamageHandlers.PassiveUndeadWilling;

                fighters[i].TakeDamageEA += TakeDamageHandlers.FightBack;
                fighters[i].TakeDamageEA += TakeDamageHandlers.JudgeDeath;
            }
        }
        public static void CalcuAction(Fighter fighter_1,Fighter fighter_2)
        {
            var balanceSumption = (int)(fighter_1.Agility + fighter_2.Agility) / 2;

            fighter_1.SpeedBar += balanceSumption + (int)(fighter_1.Agility * 0.5);
            fighter_2.SpeedBar += balanceSumption + (int)(fighter_2.Agility * 0.5);
            

        }
        public static void ActionWithSkill(Fighter source, Fighter taker)
        {
            var random = new Random();
            Skill chosenSkill;
            var SkillList = source.Skills.Where(s => !s.IsPassive).ToList();

            var choice = random.Next(0, SkillList.Count);
            chosenSkill = SkillList[choice];
            

            //normalSkill
            if (chosenSkill.Tags[0] == "GENERAL")
                ActionWithNormalSkill(source, taker, chosenSkill);
            //specialSkill
            else
            {
                var todoSkills = new List<Skill>();
                foreach(var tag in chosenSkill.Tags)
                {
                    if (StaticData.SpecialSkillMap.TryGetValue(tag, out var action))
                        action(source, taker, chosenSkill);
                }
            }
            if (source.Skills.Any(s => s.Name == chosenSkill.Name))
                source.Skills.Remove(chosenSkill);


        }
        public static void ActionWithNormalSkill(Fighter source, Fighter taker,Skill skill)
        {
            BattleLogger.LogAction(source.Name, skill);
            JsonLogger.LogAction(source.Name, "Skill", skill.Name);
            double finalDamage = source.Agility * skill.CoefficientAgility
                + source.Intelligence * skill.CoefficientIntelligence
                + source.Strength * skill.CoefficientStrength;
            if (finalDamage > 0)
            {

                DamageInfo damageInfo;
                var detail = new DamageDetail
                {
                    DirectSource = skill.Name,
                    DamageType = StaticData.SkillDamage,
                    tags = new List<string>(skill.Tags)
                };
                //UNSURECHANGED : 原本没有导航属性
                if (skill.SkillBuffs.Count() > 0)
                {
                    damageInfo = new DamageInfo(source, taker, finalDamage);
                    detail.buffs = BattleDataBridge.ExtractBuffs(skill.SkillBuffs);
                }
                else
                    damageInfo = new DamageInfo(source, taker, finalDamage);
                damageInfo.damageDetail = detail;
                source.CauseDamage(damageInfo);
                //结算自己可能打出的伤害后，判定对方伤害前挂载自我buff
                //UNSURECHANGED : 原本没有导航属性
                var selectedBuffs = skill.SkillBuffs
                    .Where(wb => wb.Buff.IsOnSelf)
                    .Select(wb=>wb.Buff);
                foreach (var buff in selectedBuffs)
                {
                    if (buff.IsOnSelf)
                    {
                        var buffLevel = skill.SkillBuffs.SingleOrDefault(b => b.Buff.Name == buff.Name)?.Level ?? 1;
                        source.LoadBuff(buff, null,buffLevel);
                    }
                }
                taker.TakeDamage(damageInfo);
            }
            //没有伤害，只有buff
            else
            {
                var selectedBuffs = skill.SkillBuffs
                    .Where(wb => wb.Buff.IsOnSelf)
                    .Select(wb => wb.Buff);
                foreach (var buff in selectedBuffs)
                {
                    if (buff.IsOnSelf)
                    {
                        var buffLevel = skill.SkillBuffs.SingleOrDefault(b => b.Buff.Name == buff.Name)?.Level ?? 1;
                        source.LoadBuff(buff, null,buffLevel);
                    }
                }
            }

        }
        #region SpecialSkillAction
        public static void ActionWithSkillTorture(Fighter source,Fighter taker,Skill skill)
        {
            var random = new Random();
            var buffCount = 0;
            var newSkillBuffs = new List<SkillBuff>();
            do
            {
                var buffChoice = random.Next(0, StaticData.BuffPool.Count);
                if (StaticData.BuffPool[buffChoice].IsOnSelf)
                    continue;
                newSkillBuffs.Add(new SkillBuff {Buff = StaticData.BuffPool[buffChoice],Level = 3 });
                buffCount++;
            } while (buffCount < 4);
            skill.SkillBuffs.Concat(newSkillBuffs);
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
                    if (source.Profession == "Warrior")
                        Program.MOVE_TIME_WARRIOR++;
                    else
                        Program.MOVE_TIME_RANGER++;
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
                    if (taker.Profession == "Warrior")
                        Program.MOVE_TIME_WARRIOR++;
                    else
                        Program.MOVE_TIME_RANGER++;
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
