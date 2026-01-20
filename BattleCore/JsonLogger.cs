using System.Text.Encodings.Web;
using System.Text.Json;

namespace BattleCore
{
    public static class JsonLogger
    {
        // 1. 账本：存储所有发生的战斗事件
        private static readonly List<BattleEvent> _events = new List<BattleEvent>();
        public static int _currentDepth = 0;
        private class DepthScope : IDisposable
        {
            public DepthScope() => _currentDepth++;
            public void Dispose() => _currentDepth--;
        }
        // ---  回合管理 ---
        public static void LogRoundBegin(string name)
        {
            Emit("RoundBegin", new Dictionary<string, object> { { "Unit", name } });
        }
        // ---  伤害性 Buff 结算 (结算名称、等级、伤害) ---
        public static void LogBuffTick(string unit, string buffName, int damage)
        {
            Emit("BuffTick", new Dictionary<string, object> {
                { "Unit", unit },
                { "BuffName", buffName },
                { "Damage", damage }
            });
        }
        // ---  Buff 时间结算 (结算名称、等级、剩余时间) ---
        public static void LogBuffUpdate(string unit, string buffName, int level, int remainRound)
        {
            Emit("BuffUpdate", new Dictionary<string, object> {
                { "Unit", unit },
                { "BuffName", buffName },
                { "Level", level },
                { "Remain", remainRound }
            });
        }

        // 2. 这里的 Emit 是一个具体的方法，负责把数据塞进账本
        public static void Emit(string type, Dictionary<string, object> data)
        {
            _events.Add(new BattleEvent
            {
                Type = type,
                Depth = _currentDepth, // 记录当前深度
                Data = data
            });
        }
        public static IDisposable StartReactionScope(string actor, string reactionType)
        {
            Emit("ReactionBegin", new Dictionary<string, object> {
                { "Actor", actor }, { "Type", reactionType }
            });
            return new DepthScope();
        }


        // 获取最终的 JSON 结果
        public static string GetJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            string json = JsonSerializer.Serialize(_events, options);
            Console.WriteLine("\n[JSON PREVIEW]\n" + json);
            return json;
        }

        public static void Clear() => _events.Clear();

        // A1: 选择行动
        public static void LogAction(string actor, string type, string name)
        {
            Emit("Action", new Dictionary<string, object> {
            { "Actor", actor }, { "Category", type }, { "Name", name }
        });
        }

        // A2 & R3: 挂载 Buff (对自己或对方)
        public static void LogBuffApply(string target, string buffName,int buffLevel)
        {
            Emit("BuffApply", new Dictionary<string, object> {
            { "Target", target }, { "BuffName", buffName },{"BuffLevel",buffLevel }
        });
        }

        // R1: 闪避
        public static void LogDodge(string target)
        {
            Emit("Dodge", new Dictionary<string, object> { { "Target", target } });
        }

        // R2: 扣血
        public static void LogDamage(string target, int value, int remain)
        {
            Emit("Damage", new Dictionary<string, object> {
            { "Target", target }, { "Value", value }, { "HP", remain }
        });
        }

        // C1: 被动保命
        public static void LogPassive(string unit, string skillName)
        {
            Emit("Passive", new Dictionary<string, object> {
            { "Unit", unit }, { "SkillName", skillName }
        });
        }

        // 事件的数据结构
        public class BattleEvent
        {
            public string Type { get; set; }
            public int Depth { get; set; }
            public Dictionary<string, object> Data { get; set; }
        }
    }
}