using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;

namespace BattleCore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var F_B = 0;
            var F_A = 0;
            var totalRound = 0;
            while (totalRound < 100)
            {
                SeedData sd = new SeedData();
                BattleController.Initial(new List<Fighter> { sd.Fighter_Mortal
                        , sd.Fighter_Ranger
                        , sd.Fighter_Warrior
                        , sd.Fighter_Magician});
                if (BattleController.BattleSimulation(sd.Fighter_Warrior, sd.Fighter_Magician))
                    F_A++;
                else
                    F_B++;
                totalRound++;
            }
            Console.WriteLine("==========Statistics==========");
            Console.WriteLine($"F_A Win: {F_A}");
            Console.WriteLine($"F_B Win: {F_B}");
                
        }

    }
}
