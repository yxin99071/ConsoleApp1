using BattleLogic.BattleLogic;
using DataCore.Models;
using BattleLogic.DataModel.Fighters;

namespace BattleLogic.DataModel
{
    public class StaticData
    {
        #region DamageTag
        public static readonly string UnDodgeable = "Undogeable".ToUpper();
        public static readonly string UnFightBackable = "UnFightBackable".ToUpper();
        public static readonly string BuffDamage = "BUFF_DAMAGE";
        public static readonly string WeaponDamage = "WEAPON_DAMAGE";
        public static readonly string SkillDamage = "SKILL_DAMAGE";
        public static readonly string FistDamage = "Fist_DAMAGE";


        #endregion
        #region SpecialSkillTag
        public static readonly string SkillTagTorture = "Torture".ToUpper();
        #endregion
        #region PassiveSkillName
        public static readonly string PassiveUndeadWilling = "UNDEAD_WILL";
        public static readonly string PassivePretendDeath = "FEIGN_DEATH";
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
                BuffPool.AddRange(pool);
        }

        public static double CalculateDodge(double agility, double k = 0.01)
        {
            // 设置中心点，原先是 60，现在改为 100，延迟收益爆发时机
            double center = 60;
            double maxDodge = 0.4; // 最大闪避率上限

            // 预计算常量
            // offset 用于确保当 agility 为 0 时，闪避率尽可能接近 0
            double offset = 1.0 / (1.0 + Math.Exp(k * center));
            double denominator = 1.0 - offset;

            // 核心逻辑回归公式
            double exponent = Math.Exp(-k * (agility - center));
            double sigmoid = 1.0 / (1.0 + exponent);

            // 计算最终闪避率，并将范围映射到 [0, 0.3]
            double dodgeRate = maxDodge * ((sigmoid - offset) / denominator);

            return Math.Clamp(dodgeRate, 0, maxDodge);
        }
        public static double CalculateCounterRate(double agi, double str, double intel)
        {
            // 1. 计算加权总值 
            // 分母 30 是 14+7+9 的总和
            double weightedAttr = (14.0 * agi + 7.0 * str + 9.0 * intel) / 30.0;
            // 2. 设定 Sigmoid 参数
            // k 从 0.05 降至 0.03：使收益曲线更平滑，避免反击率瞬间堆满
            double k = 0.03;
            // midPoint 从 40 移至 80：将爆发期大幅度延后。
            // 10级角色此时正处于曲线的上升准备期，20-30级左右才会迎来真正的高反击爆发。
            double midPoint = 60;
            // 3. 计算逻辑回归部分 (0 到 1 之间的 S 曲线)
            double exponent = Math.Exp(-k * (weightedAttr - midPoint));
            double sigmoid = 1.0 / (1.0 + exponent);
            // 4. 映射到 5% - 20% 区间
            double counterRate = 0.03 + 0.15 * sigmoid;

            return Math.Clamp(counterRate, 0.05, 0.20);
        }
    }
}
