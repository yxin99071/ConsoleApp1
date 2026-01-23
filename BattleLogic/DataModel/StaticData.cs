using DataCore.Models;
using BattleCore.DataModel.Fighters;
using BattleCore.BattleLogic;
using System.Security.Cryptography.X509Certificates;

namespace BattleCore.DataModel
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
        public static readonly string CriticalDamage = "CriticalDamage";


        #endregion
        #region SpecialSkillTag
        public static readonly string SkillTagTorture = "折磨";
        #endregion
        #region PassiveSkillName
        public static readonly string PassiveUndeadWill = "UNDEAD_WILL";
        public static readonly string PassivePretendDeath = "FEIGN_DEATH";
        #endregion

        public static readonly Dictionary<string, Action<Fighter, Fighter, Skill>> SpecialSkillMap
            = new Dictionary<string, Action<Fighter, Fighter, Skill>>
            {
                [SkillTagTorture] = BattleHelper.ActionWithSkillTorture
            };

        public static List<Buff> BuffPool = new List<Buff>();



        public async static Task InitializeAsyncData()
        {
            var pool = await BattleDataBridge.GetBuffTools();
            if (pool is not null)
                BuffPool.AddRange(pool);
        }

        public static double CalculateDodge(double agility)
        {
            // --- 第一段：基础身法 (敏捷早期收益) ---
            // 效果：敏捷 20 左右爆发，提供最高 15% 的闪避
            double phase1 = Logistic(agility, 0.15, 20, 0.1, clampToZero: true);

            // --- 第二段：超凡入圣 (高敏捷进阶收益) ---
            // 效果：敏捷 100 左右二次爆发，额外提供最高 20% 的闪避
            double phase2 = Logistic(agility, 0.2, 100, 0.05, clampToZero: true);

            // 叠加结果：总上限为 0.15 + 0.2 = 0.35 (35%)
            double totalDodge = phase1 + phase2;

            // 最终一道保险，确保不越界
            return Math.Clamp(totalDodge, 0, 0.35);
        }
        public static double CalculateCounterRate(double agi, double str, double intel)
        {
            // 1. 计算加权属性值 (保持原有的权重比例)
            double weightedAttr = (14.0 * agi + 7.0 * str + 9.0 * intel) / 30.0;

            // 我们需要将 15% 的增量空间拆分为两段：
            // 参数：上限 8%, 中点 35, 陡峭度 0.08
            // 作用：在总属性约 50 点时（游侠加权约 40+）迅速拉开职业间的反击率差距
            double phase1 = Logistic(weightedAttr, 0.08, 35, 0.08, clampToZero: true);
            // 参数：上限 7%, 中点 90, 陡峭度 0.05
            // 作用：在总属性超过 150 点后（游侠加权约 100+）开启二次增长，向 20% 最终上限靠拢
            double phase2 = Logistic(weightedAttr, 0.07, 90, 0.05, clampToZero: true);
            // 2. 汇总增量
            double totalBonus = phase1 + phase2;

            // 3. 最终映射：基础 5% + 动态增量，并锁定在 [5%, 20%] 区间
            return Math.Clamp(0.05 + totalBonus, 0.05, 0.20);
        }
        public static double CalculateCriticalRate(double agility)
        {
            var CR_max = 0.6; 
            return Logistic(agility, CR_max, 100, 0.04, clampToZero: false);
        }
        public static double CalculateCriticalDamage(double strength)
        {
            var CD_max = 0.8; 
            return Logistic(strength, CD_max, 120, 0.03, clampToZero: false);
        }
        public static double CalculateDamageIncreasement(double intelligence)
        {
            var IC_max = 1.0; 
            return Logistic(intelligence, IC_max, 100, 0.02, clampToZero: false);
        }

        private static double Logistic(double x, double max, double midPoint, double k, bool clampToZero = true)
        {
            // 核心公式：L / (1 + exp(-k * (x - x0)))
            double GetValue(double val) => max / (1.0 + Math.Exp(-k * (val - midPoint)));

            if (!clampToZero)
            {
                return GetValue(x);
            }

            // 偏移修正逻辑：计算 x=0 时的原始值，并从结果中减去它，再重新缩放以保证上限依然是 max
            double offset = GetValue(0);
            double result = (GetValue(x) - offset) / (1.0 - offset / max);

            return Math.Max(0, result);
        }

    }
}
