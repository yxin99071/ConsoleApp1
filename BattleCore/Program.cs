using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.EntityObjects;

namespace BattleCore
{
    internal class Program
    {
        static void Main(string[] args)
        {

            SeedData sd = new SeedData();
            BattleController.Initial(new List<EntityObjects.Fighter> { sd.Fighter_Balacne
                    , sd.Fighter_Agility
                    , sd.Fighter_Strength });
            while (true)
            {
                if (BattleController.WhetherAction(sd.Fighter_Balacne))
                {
                    Thread.Sleep(1000);
                    BattleController.DecideAction(sd.Fighter_Balacne, sd.Fighter_Strength);
                    Console.WriteLine($"Fighter_Strength's Health:{(int)sd.Fighter_Strength.Health}");
                }
                if (BattleController.WhetherAction(sd.Fighter_Strength))
                {
                    Thread.Sleep(1000);
                    BattleController.DecideAction(sd.Fighter_Strength, sd.Fighter_Balacne);
                    Console.WriteLine($"Fighter_Balacne's Health:{(int)sd.Fighter_Balacne.Health}");
                }

                if (sd.Fighter_Balacne.Health <= 0 || sd.Fighter_Strength.Health <=0)
                    break;
            }
        }
    }
}
