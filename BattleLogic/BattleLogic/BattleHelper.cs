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
            //UNSURECHANGED : 原本没有导航属性
            if (weapon.WeaponBuffs.Count > 0)
            {
                damageInfo = new DamageInfo(source, taker, finalDamage);
                detail.buffs = StaticDataHelper.ExtractBuffs(null, weapon.WeaponBuffs);
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
                    buffStatus.buff.LastRound--;
                else
                    timeOutBuffs.Add(buffStatus);
            }
            foreach(var buffStatus in timeOutBuffs)
            {
                BattleLogger.LogBuffTimeOut(buffStatus.buff.Name);
                JsonLogger.LogBuffUpdate(fighter.Name, buffStatus.buff.Name);
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
            

            //normalSkill
            if (chosenSkill.Tags[0] == "普通")
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
                    DamageType = StaticDataHelper.SkillDamage,
                    tags = new List<string>(skill.Tags)
                };
                //UNSURECHANGED : 原本没有导航属性
                if (skill.SkillBuffs.Count() > 0)
                {
                    damageInfo = new DamageInfo(source, taker, finalDamage);
                    detail.buffs = StaticDataHelper.ExtractBuffs(skill.SkillBuffs);
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
            skill.SkillBuffs.Concat(newSkillBuffs);
            ActionWithNormalSkill(source, taker, skill);
        }
        #endregion

        

    }



}
