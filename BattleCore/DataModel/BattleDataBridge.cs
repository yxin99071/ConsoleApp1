
using BattleCore.DataModel.Fighters;
using DataCore.Services;
using Buff = DataCore.Models.Buff;

namespace BattleCore.DataModel
{
    public class BattleDataBridge
    {
        private static readonly DataService dataService = new DataService();
        /*
         * readonly dataservice
         */
        public async Task<Fighter?> ConvertUserToFighter(int userId)
        {
            var user = await dataService.GetUserById(userId);
            if (user is not null)
            {
                if (user.Profession == "MAGICIAN")
                    return new Magician(user);
                if (user.Profession == "WARRIOR")
                    return new Warrior(user);
                if (user.Profession == "RANGER")
                    return new Ranger(user);
                if (user.Profession == "MORTAL")
                    return new Mortal(user);
            }
            return null;
        }
        public static async Task<List<Buff>> GetBuffTools()
        {
            return await dataService.GetAllBuffs();
        }

        //dataservice.GetUserInfo => return User

        //dataservice.GetBuffInfo => return List<Buff>
        
        //dataservice.GetWeaponInfo => return List<Weapon>



    }
}
