using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BattleCore
{
    public class BattleLogger
    {
        public static void LogRoundBegin(string name)
        {
            Console.WriteLine("=====================");
            Console.WriteLine($"It's {name}'s turn!");
        }
        public static void LogAction(string name,Object action)
        {
            var type = action.GetType();
            var prop = type.GetProperty("Name");

            if (prop != null)
            {
                Console.WriteLine($"{name} chooses action: {prop.GetValue(action)}");
            }
            else
                Console.WriteLine("UNKNOWN ERROR IN LogAction");

        }
        public static void LogReaction(string name, string reaction)
        {
            Console.WriteLine($"{name} has reaction {reaction}, ");
        }
        public static void LogDamage(string name, double damage)
        {
            Console.WriteLine($"{name} get damaged:{(int)damage}");
        }
        public static void LogBuffDamage(string buffName)
        {
            Console.WriteLine($"Settling damage from buff {buffName}");
        }

        public static void LogBuffTimeOut(string buffName)
        {
            Console.WriteLine($"Buff {buffName} times out");
        }

        internal static void PassiveSkillInvoke(string skillName)
        {
            Console.WriteLine($"Passive Skill Invoked: {skillName}");
        }
    }
}
