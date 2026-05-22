using BattleCore;
using BattleCore.BattleLogic.EventHandlers;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using BattleCore.DataModel.States;
using DataCore.Models;
using System;
using System.Net;
namespace BattleCore.BattleLogic
{
    public static class BattleHelper
    {
        record Fist(string Name);
        #region Action in a round
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
                DamageType = StaticDataHelper.WeaponDamage,
                DirectSource = weapon.Name,
                tags = new List<String>(weapon.Tags),
            };
            //移除武器
            if (source.Weapons.Any(w=>w.Name == weapon.Name))
                source.Weapons.Remove(weapon);

            finalDamage += (weapon.CoefficientStrength * source.Strength 
                + weapon.CoefficientAgility * source.Agility
                + weapon.CoefficientIntelligence* source.Intelligence);
            damageInfo = new DamageInfo(source, taker, finalDamage);
            if (weapon.WeaponBuffs.Count > 0)
            {
                var allExtracted = StaticDataHelper.ExtractBuffs(null, weapon.WeaponBuffs);
                // 只有对方 buff 进入伤害详情，避免 IsOnSelf buff 错误挂给敌人
                detail.buffs = allExtracted.Where(b => !b.IsOnSelf).ToList();
                // 自身 buff 使用已缩放版本挂给 source
                foreach (var extracted in allExtracted.Where(b => b.IsOnSelf))
                {
                    var buffLevel = weapon.WeaponBuffs.SingleOrDefault(wb => wb.Buff.Name == extracted.Name)?.Level ?? 1;
                    source.LoadBuff(extracted, null, buffLevel);
                }
            }

            //挂载伤害细节
            damageInfo.damageDetail = detail;
            source.CauseDamage(damageInfo);
            taker.TakeDamage(damageInfo);

            
        }
        public static void DecideAction(Fighter source, Fighter? taker)
        {

            if (taker == null || source.BuffStatuses.Any(b => b.buff.Name == "锁定"))
                {
                //todo 跳过回合的json输出    
                return; 
            }

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
        public static void CalcuAction(Fighter fighter_1,Fighter fighter_2)
        {
            var balanceSumption = (int)(fighter_1.Agility + fighter_2.Agility) / 2;

            fighter_1.SpeedBar += balanceSumption + (int)(fighter_1.Agility * 0.5);
            fighter_2.SpeedBar += balanceSumption + (int)(fighter_2.Agility * 0.5);
            

        }
        public static void BuffEffection(Fighter fighter)
        {
            if(fighter.BuffStatuses.Count()>0)
            {
                List<BuffStatus> damagedBuffs = new List<BuffStatus>();
                List<BuffStatus> healingBuffs = new List<BuffStatus>();
                foreach(var buffStatus in fighter.BuffStatuses)
                {
                    if(buffStatus.buff.DirectDamage > 0)
                        damagedBuffs.Add(buffStatus);
                    if (buffStatus.buff.DirectDamage < 0)
                        healingBuffs.Add(buffStatus);
                }
                //先遍历完再结算伤害，否则可能出现遍历时清除buff的而错误
                foreach(var buffStatus in damagedBuffs)
                {
                    BattleLogger.LogBuffDamage(buffStatus.buff);
                    DamageInfo damageInfo = new DamageInfo(buffStatus.Source, fighter, buffStatus.buff.DirectDamage);
                    //来自Buff的伤害不可反击
                    var detail = new DamageDetail
                    {
                        DamageType = StaticDataHelper.BuffDamage,
                        DirectSource = buffStatus.buff.Name,
                        tags = [StaticDataHelper.UnDodgeable,StaticDataHelper.UnFightBackable,StaticDataHelper.BuffDamage]
                    };
                    damageInfo.damageDetail = detail;

                    buffStatus.Source?.CauseDamage(damageInfo);
                    fighter.TakeDamage(damageInfo);
                }
                foreach (var buffStatus in healingBuffs)
                {
                    BattleLogger.LogBuffDamage(buffStatus.buff);
                    fighter.Heal(Math.Abs(buffStatus.buff.DirectDamage), new List<string>());
                }
            }
        }
        public static void BuffSettle(Fighter fighter)
        {
            var timeOutBuffs = new List<BuffStatus>();
            foreach(var buffStatus in fighter.BuffStatuses)
            {
                if (buffStatus.buff.LastRound > 0)
                {
                    buffStatus.buff.LastRound--;
                    // 倒计时更新：记录剩余回合数
                    JsonLogger.LogBuffUpdate(fighter.Name, buffStatus.buff.Name, buffStatus.buff.LastRound);
                }
                else
                    timeOutBuffs.Add(buffStatus);
            }
            foreach(var buffStatus in timeOutBuffs)
            {
                BattleLogger.LogBuffTimeOut(buffStatus.buff.Name);
                // Buff 到期：用 Expire 而非 Update
                JsonLogger.LogBuffExpire(fighter.Name, buffStatus.buff.Name);
                fighter.BuffStatuses.Remove(buffStatus);
            }
        }
        public static void ActionWithSkill(Fighter source, Fighter taker)
        {
            var random = new Random();
            Skill chosenSkill;
            var SkillList = source.Skills.Where(s => !s.IsPassive).ToList();

            var choice = random.Next(0, SkillList.Count);
            chosenSkill = SkillList[choice];
            

            //normalSkill：Tags 为空或首 Tag 为"普通"都走普通攻击逻辑
            if (chosenSkill.Tags.Count == 0 || chosenSkill.Tags[0] == "普通")
                ActionWithNormalSkill(source, taker, chosenSkill);
            //specialSkill
            else
            {
                var todoSkills = new List<Skill>();
                foreach(var tag in chosenSkill.Tags)
                {
                    if (StaticDataHelper.SpecialSkillMap.TryGetValue(tag, out var action))
                        action(source, taker, chosenSkill);
                }
            }
            if (source.Skills.Any(s => s.Name == chosenSkill.Name))
                source.Skills.Remove(chosenSkill);


        }
        public static void ActionWithNormalSkill(Fighter source, Fighter taker, Skill skill)
        {
            BattleLogger.LogAction(source.Name, skill);
            JsonLogger.LogAction(source.Name, "Skill", skill.Name);
            double finalDamage = source.Agility * skill.CoefficientAgility
                + source.Intelligence * skill.CoefficientIntelligence
                + source.Strength * skill.CoefficientStrength;

            // 提取所有已缩放 buff（一次性），后续按 IsOnSelf 分流
            var allExtracted = skill.SkillBuffs.Count > 0
                ? StaticDataHelper.ExtractBuffs(skill.SkillBuffs)
                : new List<Buff>();

            if (finalDamage > 0)
            {
                var detail = new DamageDetail
                {
                    DirectSource = skill.Name,
                    DamageType = StaticDataHelper.SkillDamage,
                    tags = new List<string>(skill.Tags),
                    // 只有对方 buff 进入伤害详情，避免 IsOnSelf buff 错误挂给敌人
                    buffs = allExtracted.Where(b => !b.IsOnSelf).ToList()
                };
                var damageInfo = new DamageInfo(source, taker, finalDamage) { damageDetail = detail };
                source.CauseDamage(damageInfo);
                // 自身 buff 使用已缩放版本挂给 source
                foreach (var extracted in allExtracted.Where(b => b.IsOnSelf))
                {
                    var buffLevel = skill.SkillBuffs.SingleOrDefault(sb => sb.Buff.Name == extracted.Name)?.Level ?? 1;
                    source.LoadBuff(extracted, null, buffLevel);
                }
                taker.TakeDamage(damageInfo);
            }
            // 没有伤害，只挂自身 buff
            else
            {
                foreach (var extracted in allExtracted.Where(b => b.IsOnSelf))
                {
                    var buffLevel = skill.SkillBuffs.SingleOrDefault(sb => sb.Buff.Name == extracted.Name)?.Level ?? 1;
                    source.LoadBuff(extracted, null, buffLevel);
                }
            }
        }
        #endregion

        #region SpecialSkillAction
        public static void ActionWithSkillTorture(Fighter source,Fighter taker,Skill skill)
        {
            var random = new Random();
            var buffCount = 0;
            var newSkillBuffs = new List<SkillBuff>();
            do
            {
                var buffChoice = random.Next(0, StaticDataHelper.BuffPool.Count);
                if (StaticDataHelper.BuffPool[buffChoice].IsOnSelf)
                    continue;
                newSkillBuffs.Add(new SkillBuff {Buff = StaticDataHelper.BuffPool[buffChoice],Level = 3 });
                buffCount++;
            } while (buffCount < 4);
            skill.SkillBuffs.AddRange(newSkillBuffs);
            ActionWithNormalSkill(source, taker, skill);
        }
        #endregion

        

    }



}
