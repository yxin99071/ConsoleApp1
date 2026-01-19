using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using System.Threading.Tasks;

namespace BattleCore
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await StaticData.InitializeAsyncData();
            var F_B = 0;
            var F_A = 0;
            var totalRound = 0;
            var BtData = new BattleDataBridge();
            while (totalRound < 100)
            {
                var fighter_1 = await BtData.ConvertUserToFighter(1);
                var fighter_2 = await BtData.ConvertUserToFighter(2);
                if (fighter_1 != null && fighter_2!=null)
                {
                    if (BattleController.BattleSimulation(fighter_1, fighter_2))
                        F_A++;
                    else
                        F_B++;
                    totalRound++;
                }
            }
            Console.WriteLine("==========Statistics==========");
            Console.WriteLine($"F_A Win: {F_A}");
            Console.WriteLine($"F_B Win: {F_B}");
                
        }

    }
}
