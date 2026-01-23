using BattleCore.BattleLogic.EventHandlers;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using DataCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic
{
    public class BattleManager
    {
        #region BattleManage
        public static async Task BattleSimulation(Fighter source, Fighter taker)
        {
            Console.Write("");
            while (true)
            {
                BattleHelper.CalcuAction(source, taker);
                var Max_SpeedBar = (int)(source.Agility + taker.Agility) * 2;
                source.Max_SpeedBar = Max_SpeedBar;
                taker.Max_SpeedBar = Max_SpeedBar;
                if (source.SpeedBar >= Max_SpeedBar)
                {
                    Console.WriteLine($"New Round:======{source.Name}========");
                    //Thread.Sleep(1000);
                    BattleHelper.BuffEffection(source);
                    BattleHelper.DecideAction(source, taker);
                    BattleHelper.BuffSettle(source);
                    Console.WriteLine($"{taker.Name}'s Health:{(int)taker.Health}");
                    source.SpeedBar -= Max_SpeedBar;

                }

                if (source.IsDead)
                {
                    Console.WriteLine($"Game Over, {taker.Name} win!");
                    await BattleDataBridge.SetBattleResult(source.Id, taker.Id, false);
                    break;
                }
                if (taker.IsDead)
                {
                    Console.WriteLine($"Game Over, {source.Name} win!");
                    await BattleDataBridge.SetBattleResult(source.Id, taker.Id, true);
                    break;
                }


                if (taker.SpeedBar >= Max_SpeedBar)
                {
                    Console.WriteLine($"New Round:======{taker.Name}========");
                    //Thread.Sleep(1000);
                    BattleHelper.BuffEffection(taker);
                    BattleHelper.DecideAction(taker, source);
                    BattleHelper.BuffSettle(taker);
                    Console.WriteLine($"{source.Name}'s Health:{(int)source.Health}");
                    taker.SpeedBar -= Max_SpeedBar;//放在最后可以在方法中判断是否正在自己的回合

                }

                if (source.IsDead)
                {
                    Console.WriteLine($"Game Over, {taker.Name} win!");
                    await BattleDataBridge.SetBattleResult(source.Id, taker.Id, false);
                    break;
                }
                if (taker.IsDead)
                {
                    Console.WriteLine($"Game Over, {source.Name} win!");
                    await BattleDataBridge.SetBattleResult(source.Id, taker.Id, true);
                    break;
                }

            }
        }
        /// <summary>
        /// 重要：fighters的长度只能为2，并且第一个为挑战者，第二个为被挑战者
        /// </summary>
        /// <param name="fighters"></param>
        public static void Initial(List<Fighter> fighters)
        {
            if (fighters.Count != 2)
                return;
            for (int i = 0; i < fighters.Count; i++)
            {
                fighters[i].ClearAllEA();

                //绑定顺序很重要
                fighters[i].LoadBuffEA += LoadBuffHandlers.LoadBuff;

                fighters[i].HealingEA += HealingEventHandlers.HealingOnHp;

                fighters[i].CauseDamageEA += CauseDamageHandlers.CorrectDamageByBuff;
                fighters[i].CauseDamageEA += CauseDamageHandlers.CorrectDamageByCritical;
                fighters[i].CauseDamageEA += CauseDamageHandlers.CorrectDamageByIncreasement;

                fighters[i].TakeDamageEA += TakeDamageHandlers.CorrectDamage;
                fighters[i].TakeDamageEA += TakeDamageHandlers.AvoidanceOrDamage;
                fighters[i].TakeDamageEA += TakeDamageHandlers.DamageOnHp;


                if (fighters[i].Skills.Any(s => s.Name == StaticData.PassivePretendDeath))
                    fighters[i].TakeDamageEA += TakeDamageHandlers.PassivePretendDeath;
                if (fighters[i].Skills.Any(s => s.Name == StaticData.PassiveUndeadWill))
                    fighters[i].TakeDamageEA += TakeDamageHandlers.PassiveUndeadWill;

                fighters[i].TakeDamageEA += TakeDamageHandlers.FightBack;
                fighters[i].TakeDamageEA += TakeDamageHandlers.JudgeDeath;
            }
            JsonLogger.LogBattleStart(fighters[0], fighters[1], StaticData.BuffPool);
        }
        /**
         * 【经验值与等级核心公式】
         * * 1. 累计经验门槛 (Total Experience Threshold):
         * TotalExp = 50 * L^2 + 50 * L
         * (注：L 为目标等级。Exp 不清零，达到该累计值即升至 L 级)
         * * 2. 升级所需增量经验 (Required Delta Exp):
         * NextLevelRequired = 100 * (CurrentLevel + 1)
         * * 3. 战斗基础经验 (Base Battle Exp):
         * BaseExp = 20 + (EnemyLevel * 5)
         * * 4. 等级差修正系数 (Level Difference Factor):
         * 设 ΔL = EnemyLevel - PlayerLevel
         * - 当 ΔL <= -5 (碾压): Factor = 0.05
         * - 当 -5 < ΔL < 0    : Factor = 0.05 + (0.95 / 5) * (ΔL + 5)
         * - 当 ΔL = 0  (同级): Factor = 1.0
         * - 当 0 < ΔL < 5     : Factor = 1.0 + (1.5 / 5) * ΔL
         * - 当 ΔL >= 5  (挑战): Factor = 2.5
         * * 5. 最终单场经验获得 (Final Exp Gain):
         * FinalExp = BaseExp * Factor * (IsWin ? 1.0 : 0.5)
         */
        public static double CalculateGainedExp(int playerLvl, int enemyLvl, bool isWin)
        {
            // 1. 基础经验基数 (保持不变)
            // 限制敌人等级对经验贡献的有效上限（玩家等级 + 5）
            int effectiveEnemyLvl = Math.Min(enemyLvl, playerLvl + 5);

            // 基于有效等级计算基础经验
            double baseExp = 20.0 + (effectiveEnemyLvl * 5.0);

            // 2. 计算等级差
            double deltaL = enemyLvl - playerLvl;
            double factor;

            double levelBonus = 1;
            if(playerLvl < 15)
            {
                levelBonus += (15 - playerLvl) * 0.2;
            }
            

            if (isWin)
            {
                // 胜利时的逻辑：保留原有的越级激励
                if (deltaL <= -5) factor = 0.05;
                else if (deltaL >= 5) factor = 2.5;
                else if (deltaL < 0) factor = 0.05 + (0.95 / 5.0) * (deltaL + 5);
                else factor = 1.0 + (1.5 / 5.0) * deltaL;
            }
            else
            {
                // 失败时的逻辑：严厉惩罚越级失败
                // 如果是虐菜失败(deltaL < 0)，给少量经验；如果是越级失败(deltaL > 0)，经验衰减
                // 核心：失败时 factor 最高只能是 0.8 (甚至更低)，且等级差越大，失败收益占比越小
                if (deltaL < 0)
                    factor = 0.4; // 虐菜居然输了，给点低保
                else
                    // 越级失败惩罚：等级差越大，factor越低。
                    // 例子：越1级失败 factor=0.35, 越5级失败 factor=0.1
                    factor = Math.Max(0.1, 0.4 - (deltaL * 0.06));
            }

            // 3. 最终计算
            return baseExp * factor * levelBonus;
        }
        /// <summary>
        /// 如果upgradeLevel为0返回false，否则就处理升级逻辑，包括加点与等级同步
        /// </summary>
        /// <param name="user"></param>
        /// <param name="upgradeLevel"></param>
        /// <returns></returns>
        public static bool LevelUp(User user, int upgradeLevel)
        {
            if (upgradeLevel == 0)
                return true;
            int GetIndex(string? jobName) => jobName switch
            {
                "WARRIOR" => 0, // 力量
                "RANGER" => 1, // 敏捷
                "MAGICIAN" => 2, // 智力
                "MORTAL" => 3,//平均
                _ => -1 // 未知或无
            };
            //升级点数
            int propertyPoint = upgradeLevel * 6;
            int mainIdx = GetIndex(user.Profession!.ToUpper());
            int subIdx = GetIndex(user.SecondProfession?.ToUpper());

            // 如果主职业无效，直接返回不处理
            if (mainIdx == -1) return false;

            //  确定权重比例=>0力量，1敏捷，2智力
            double[] weights = new double[3];

            if (subIdx == -1 || subIdx == mainIdx)
            {
                // 纯色职：主属性 70%，其余各 15%
                for (int i = 0; i < 3; i++)
                    weights[i] = (i == mainIdx) ? 42 : 9;

            }
            else if (mainIdx == 4)
            {
                for (int i = 0; i < 3; i++)
                    weights[i] = 20;
                //该职业需要修正点数
                propertyPoint =upgradeLevel * 7;
            }
            else
            {
                // 双修职：主 60%, 副 25%, 剩余 15%
                for (int i = 0; i < 3; i++)
                {
                    if (i == mainIdx) weights[i] = 36;
                    else if (i == subIdx) weights[i] = 15;
                    else weights[i] = 9;
                }
            }
             
            var random = new Random();
            while (propertyPoint > 0)
            {
                var choice = random.Next(0, 61);
                if (choice <= weights[0])
                    user.Strength++;
                else if (choice <= weights[1]+ weights[0])
                    user.Agility++;
                else
                    user.Intelligence++;
                propertyPoint--;
            }
            user.Level += upgradeLevel;
            user.Health = 5 * user.Level + 10 * user.Strength + 7 * user.Intelligence + 5 * user.Agility;
            return true;
        }


        #endregion

    }
}
