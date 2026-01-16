
using BattleCore.DataModel.Fighters;
using DataCore.Services;
using Buff = DataCore.Models.Buff;
using Skill = DataCore.Models.Skill;
using User = DataCore.Models.User;
using Weapon = DataCore.Models.Weapon;

namespace BattleCore.DataModel
{
    public class BattleDataBridge
    {
        public readonly DataService dataService = new DataService();
        /*
         * readonly dataservice
         */
        public async Task<Fighter?> ConvertUserToFighter(int userId)
        {
            var user = await dataService.GetUserById(userId);
            return null;
        }


        //dataservice.GetUserInfo => return User

        //dataservice.GetBuffInfo => return List<Buff>
        
        //dataservice.GetWeaponInfo => return List<Weapon>



    }
}
