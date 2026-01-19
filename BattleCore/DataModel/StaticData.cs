using BattleCore.BattleLogic;
using DataCore.Models;
using BattleCore.DataModel.Fighters;

namespace BattleCore.DataModel
{
    public class StaticData
    {
        #region DamageTag
        public static readonly string UnDodgeable = "UnDogeable".ToUpper();
        public static readonly string UnFightBackable = "UnFightBackable".ToUpper();
        #endregion
        #region SpecialSkillTag
        public static readonly string SkillTagTorture = "Torture".ToUpper();
        #endregion

        public static readonly Dictionary<string, Action<Fighter, Fighter, Skill>> SpecialSkillMap
            = new Dictionary<string, Action<Fighter, Fighter, Skill>>
            {
                [SkillTagTorture] = BattleController.ActionWithSkillTorture
            };

        public static List<Buff> BuffPool = new List<Buff>();



        public async static Task InitializeAsyncData()
        {
            var pool = await BattleDataBridge.GetBuffTools();
            if (pool is not null)
                BuffPool.Concat(pool);
        }

        public static double CalculateDodge(double agility, double k = 0.04)
        {
            // 预计算常量以提升性能
            double offset = 1.0 / (1.0 + Math.Exp(k * 60));
            double denominator = 1.0 - offset;

            // 核心逻辑回归公式
            double exponent = Math.Exp(-k * (agility - 60));
            double sigmoid = 1.0 / (1.0 + exponent);

            double dodgeRate = 0.4 * ((sigmoid - offset) / denominator);

            return Math.Clamp(dodgeRate, 0, 0.4); // 确保在 [0, 0.4] 区间
        }
        public static double CalculateCounterRate(double agi, double str, double intel)
        {
            // 1. 计算加权总值 (Weight Ratio 11:9:7)
            double weightedAttr = (14 * agi + 7 * str + 9 * intel) / 30.0;

            // 2. 设定 Sigmoid 参数
            double k = 0.05;      // 增长斜率
            double midPoint = 40; // 曲线中心点

            // 3. 计算逻辑回归部分 (0 到 1 之间的 S 曲线)
            double sigmoid = 1.0 / (1.0 + Math.Exp(-k * (weightedAttr - midPoint)));

            // 4. 映射到 5% - 20% 区间
            // Formula: Min + (Max - Min) * Sigmoid
            double counterRate = 0.05 + 0.15 * sigmoid;

            return Math.Clamp(counterRate, 0.05, 0.20);
        }
    }
}
