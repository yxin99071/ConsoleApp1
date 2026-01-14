using BattleCore.DataModel;
using BattleCore.EntityObjects;
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
            double finalDamage = 0;
            finalDamage += source.Strength * 1;
            DamageInfo damageInfo = new DamageInfo(source, taker, finalDamage);

            source.CauseDamage(damageInfo);
            taker.TakeDamage(damageInfo);
       
        }
        public static void AttackWithWeapon(Fighter source,Fighter taker)
        {
            //todo 判断武器是否剩余
            double finalDamage = 0;
            var random = new Random();
            //int choice = random.Next(0, source.Weapons.Count());
            int choice = 1;
            Weapon weapon = source.Weapons[choice];
            BattleLogger.LogAction(source.Name, weapon);
            DamageInfo damageInfo;

            //todo 移除武器
            finalDamage += (weapon.CoefficientStrength * source.Strength + weapon.CoefficientAgility * source.Agility);
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
            if (taker == null)
                return;
            BuffEffection(source);
            var random = new Random();
            int choice = random.Next(1, 3);
            switch(choice)
            {
                case 1:
                    {
                        AttackWithFist(source,taker);
                        break;
                    }
                case 2:
                    {
                        AttackWithWeapon(source, taker);
                        break;
                    }
            }
            BuffSettle(source);

        }
        public static void BuffEffection(Fighter fighter)
        {
            if(fighter.BuffStatuses.Count()>0)
            {
                foreach(var buffStatus in fighter.BuffStatuses)
                {
                    if(buffStatus.buff.DirectDamage !=0)
                    {
                        BattleLogger.LogBuffDamage(buffStatus.buff.Name);
                        var damageTag = $"{CommonData.UnDodgeable},{CommonData.UnFightBackable}";
                        DamageInfo damageInfo = new DamageInfo(buffStatus.Source, fighter, buffStatus.buff.DirectDamage,damageTag:damageTag);
                        buffStatus.Source?.CauseDamage(damageInfo);
                        fighter.TakeDamage(damageInfo);
                    }
                }
            }
        }
        public static void BuffSettle(Fighter fighter)
        {
            var timeOutBuffs = new List<BuffStatus>();
            foreach(var buffStatus in fighter.BuffStatuses)
            {
                if (buffStatus.buff.IsNew)
                    buffStatus.buff.IsNew = false;
                else
                {
                    if (buffStatus.buff.LastRound > 0)
                        buffStatus.buff.LastRound--;
                    else
                        timeOutBuffs.Add(buffStatus);
                }
            }
            foreach(var buffStatus in timeOutBuffs)
            {
                BattleLogger.LogBuffTimeOut(buffStatus.buff.Name);
                fighter.BuffStatuses.Remove(buffStatus);
            }
        }
        public static void Initial(List<Fighter> fighters)
        {
            foreach(var fighter in fighters)
            {
                fighter.LoadBuffEA += LoadBuffHandlers.LoadBuff;
                fighter.CauseDamageEA += CauseDamageHandlers.CorrectDamageByBuff;
                fighter.TakeDamageEA += TakeDamageHandlers.CorrectDamage;
                fighter.TakeDamageEA += TakeDamageHandlers.AvoidanceOrDamage;
                fighter.TakeDamageEA += TakeDamageHandlers.DamageOnHp;
                fighter.TakeDamageEA += TakeDamageHandlers.FightBack;
            }
        }
        public static bool WhetherAction(Fighter fighter)
        {
            fighter.SpeedBar += (int)fighter.Agility;
            if (fighter.SpeedBar > 100)
            {
                fighter.SpeedBar -= 100;
                BattleLogger.LogRoundBegin(fighter.Name);
                return true;
            }

            return false;
        }

    }



}
