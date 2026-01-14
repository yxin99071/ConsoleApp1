using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;

namespace BattleCore
{
    internal class Program
    {
        static void Main(string[] args)
        {

            SeedData sd = new SeedData();
            BattleController.Initial(new List<Fighter> { sd.Fighter_Balacne
                    , sd.Fighter_Agility
                    , sd.Fighter_Strength });
            while (true)
            {
                if (BattleController.WhetherAction(sd.Fighter_Agility))
                {
                    Thread.Sleep(1000);
                    BattleController.BuffEffection(sd.Fighter_Agility);
                    BattleController.DecideAction(sd.Fighter_Agility, sd.Fighter_Strength);
                    BattleController.BuffSettle(sd.Fighter_Agility);
                    Console.WriteLine($"Fighter_Strength's Health:{(int)sd.Fighter_Strength.Health}");
                }
                if (sd.Fighter_Agility.Health <= 0 || sd.Fighter_Strength.Health <= 0)
                    break;
                if (BattleController.WhetherAction(sd.Fighter_Strength))
                {
                    Thread.Sleep(1000);
                    BattleController.BuffEffection(sd.Fighter_Strength);
                    BattleController.DecideAction(sd.Fighter_Strength, sd.Fighter_Agility);
                    BattleController.BuffSettle(sd.Fighter_Strength);
                    Console.WriteLine($"Fighter_Agility's Health:{(int)sd.Fighter_Agility.Health}");
                }

                if (sd.Fighter_Agility.Health <= 0 || sd.Fighter_Strength.Health <=0)
                    break;
            }
        }
    }
}
