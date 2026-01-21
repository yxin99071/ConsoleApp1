using DataCore.Models;


namespace BattleCore
{
    public class BattleLogger
    {
        public static void LogRoundBegin(string name)
        {
            Console.WriteLine("=====================");
            Console.WriteLine($"It's {name}'s turn!");
        }
        public static void LogAction(string name, Object action)
        {
            var type = action.GetType();
            var prop = type.GetProperty("Name");
            if (prop != null)
            {
                var actionName = prop.GetValue(action);
                Console.WriteLine($"{name} chooses {action.GetType().Name}: {actionName}");
            }
            else
                Console.WriteLine("UNKNOWN ERROR IN LogAction");

        }
        public static void LogReaction(string name, string reaction)
        {
            Console.WriteLine($"{name} has reaction {reaction}, ");

        }
        public static void LogDamage(string name, double damage, double remaindHealth)
        {
            Console.WriteLine($"{name} get damaged: {(int)damage}, remaind{(int)remaindHealth}");

        }
        public static void LogBuffDamage(Buff buff)
        {
            Console.WriteLine($"Settling damage from buff {buff.Name}");

        }

        public static void LogBuffTimeOut(string buffName)
        {
            Console.WriteLine($"Buff {buffName} times out");
        }

        public static void PassiveSkillInvoke(string skillName)
        {
            Console.WriteLine($"Passive Skill Invoked: {skillName}");
        }
        public static void LoadBuffBegin(Buff buff)
        {
            Console.WriteLine($"BuffLoaded:{buff.Name}, Periord: {buff.LastRound}");

        }
    }
}
