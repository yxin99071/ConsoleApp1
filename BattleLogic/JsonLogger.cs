using BattleCore.DataModel.Fighters;
using DataCore.Models;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BattleCore
{
    public static class JsonLogger
    {
        // 1. 账本：存储所有发生的战斗事件
        private static readonly List<BattleEvent> _events = new List<BattleEvent>();
        public static int _currentDepth = 0;

        #region 回合日志
        private class DepthScope : IDisposable
        {
            public DepthScope() => _currentDepth++;
            public void Dispose() => _currentDepth--;
        }
        // ---  回合管理:开始 ---
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
        public static void LogBuffUpdate(string unit, string buffName)
        {
            Emit("BuffTimeOut", new Dictionary<string, object> {
                { "Unit", unit },
                { "BuffName", buffName }
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
        public static void LogBuffApply(string target, string buffName,int buffLevel,int lastRound)
        {
            Emit("BuffApply", new Dictionary<string, object> {
            { "Target", target }, { "BuffName", buffName },{"BuffLevel",buffLevel },{"LastRound",lastRound }
        });
        }

        // R1: 闪避
        public static void LogDodge(string target)
        {
            Emit("Dodge", new Dictionary<string, object> { { "Target", target } });
        }

        // R2: 扣血
        public static void LogDamage(string target, int value, int remain,bool isCritical)
        {
            Emit("Damage", new Dictionary<string, object> {
            { "Target", target }, { "Value", value }, { "HP", remain },{"Critical", isCritical}
        });
        }
        // R3: 回血
        public static void LogHealing(string target, int value, int remain)
        {
            Emit("Healing", new Dictionary<string, object> {
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
        #endregion

        #region 对局开始与结束
        public static void LogBattleStart(Fighter p1, Fighter p2, List<Buff> fullBuffList)
        {

            _events.Clear();
            // 1. 玩家快照
            var players = new[] { SerializeFighter(p1), SerializeFighter(p2) };

            // 2. Buff 池快照（只提取 ID, 名称, 描述）
            var buffLibrary = fullBuffList.Select(b => new {
                b.Id,
                b.Name,
                b.Description
            }).ToList();

            Emit("BattleStart", new Dictionary<string, object> {
            { "Players", players },
            { "BuffLibrary", buffLibrary }
        });
        }
        private static object SerializeFighter(Fighter f)
        {
            return new
            {
                f.Id,
                f.Name,
                Stats = new
                {
                    f.MaxHealth,
                    f.Agility,
                    f.Strength,
                    f.Intelligence
                },
                Weapons = f.Weapons.Select(w => new {
                    w.Name,
                    w.Description,
                    // 这里只存 ID，前端通过 ID 去 BuffLibrary 匹配
                    BuffIds = w.WeaponBuffs.Select(wb => wb.BuffId)
                }),
                Skills = f.Skills.Select(s => new {
                    s.Name,
                    s.Description,
                    BuffIds = s.SkillBuffs.Select(sb => sb.BuffId)
                })
            };
        }
        public static void LogBattleEnd(User preBattleUser, User postBattleUser)
        {
            Emit("BattleEnd", new Dictionary<string, object> {
        { "UserId", postBattleUser.Id },
        { "UserName", postBattleUser.Name },
        // 等级与经验变化
        { "LevelChange", new {
            From = preBattleUser.Level,
            To = postBattleUser.Level,
            IsLeveledUp = postBattleUser.Level > preBattleUser.Level
        }},
        { "ExperienceChange", new {
            Before = preBattleUser.Exp,
            After = postBattleUser.Exp,
            Gained = postBattleUser.Exp - preBattleUser.Exp
        }},
        // 核心属性变化对比 (如果你的结算逻辑会影响这些值)
        { "StatsChange", new {
            Health = new { From = preBattleUser.Health, To = postBattleUser.Health },
            Strength = new { From = preBattleUser.Strength, To = postBattleUser.Strength },
            Agility = new { From = preBattleUser.Agility, To = postBattleUser.Agility },
            Intelligence = new { From = preBattleUser.Intelligence, To = postBattleUser.Intelligence }
        }}
    });
        }
        #endregion

        // 事件的数据结构
        public class BattleEvent
        {
            public string Type { get; set; } = default!;
            public int Depth { get; set; }
            public Dictionary<string, object> Data { get; set; } = default!;
        }
    }
}