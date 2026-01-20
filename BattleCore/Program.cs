using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using System.Text;

namespace BattleCore
{
    internal class Program
    {
        public static int MOVE_TIME_RANGER;
        public static int MOVE_TIME_WARRIOR;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            await StaticData.InitializeAsyncData();
            var F_B = 0;
            var F_A = 0;
            var totalRound = 0;
            var BtData = new BattleDataBridge();
            while (totalRound < 1)
            {
                Console.WriteLine("========NewRound========");
                Console.WriteLine("========NewRound========");
                Console.WriteLine("========NewRound========");
                var fighter_1 = await BtData.ConvertUserToFighter(1);
                var fighter_2 = await BtData.ConvertUserToFighter(2);
                var fighter_3 = await BtData.ConvertUserToFighter(3);
                var fighter_4 = await BtData.ConvertUserToFighter(4);
                
                if (fighter_1 != null && fighter_2!= null && fighter_3 != null && fighter_4 != null)
                {
                    BattleController.Initial(new List<Fighter> { fighter_1, fighter_2, fighter_3, fighter_4 });
                    if (BattleController.BattleSimulation(fighter_3, fighter_2))
                        F_A++;
                    else
                        F_B++;
                    totalRound++;
                }
            }
            Console.WriteLine("==========Statistics==========");
            Console.WriteLine($"F_A Win: {F_A} who is {await BtData.ConvertUserToFighter(1)}");
            Console.WriteLine($"F_B Win: {F_B}");
            Console.WriteLine(MOVE_TIME_RANGER);
            Console.WriteLine(MOVE_TIME_WARRIOR);
            JsonLogger.GetJson();
           
                
        }

    }
}
