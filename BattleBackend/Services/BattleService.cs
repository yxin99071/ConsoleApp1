using BattleBackend.Controllers;
using BattleCore.BattleLogic;
using DataCore.Models;
using DataCore.Services;

namespace BattleBackend.Services
{
    public class BattleService
    {
        private readonly DataHelper _dataHelper;
        public BattleService(DataHelper dataHelper)
        {
            _dataHelper = dataHelper;
        }

        internal async Task<List<User>> GetAllFighter(string id)
        {
            var users = new List<User>();
            if (int.TryParse(id, out int userId))
            {
                var allUsers = await _dataHelper.GetAllUser();
                allUsers.Remove(allUsers.SingleOrDefault(u => u.Id == userId)!);
                users.AddRange(allUsers);
            }
            return users;
        }

        internal async Task<User?> GetUserById(int id)
        {
            return await _dataHelper.GetUserById(id);
        }

        internal async Task<User?> IdentifyUser(string account, string password)
        {

            return await _dataHelper.IdentifyUser(account, password);
        }

        internal async Task<bool> InitializeUserProfile(string userId, string? name, string account, string profession, string? secondProfession, List<string> initialSkills)
        {
            if (!int.TryParse(userId, out int id))
                return false;
            var user = await _dataHelper.GetUserById(id, true);
            if (user is null)
                return false;
            user.Profession = profession;
            user.SecondProfession = secondProfession;
            user.Level = 1;
            user.Name = name ?? user.Id.ToString();
            user.Account = account ?? user.Id.ToString();

            user.Agility = 10;
            user.Strength = 10;
            user.Intelligence = 10;
            user.Exp = 0;
            //获得两级的属性点
            BattleManager.LevelUp(user, 2);
            user.Level = 1;
            var skillPD = await _dataHelper.FindSkillByName("假死");
            var skillUD = await _dataHelper.FindSkillByName("亡者意志");
            if (skillPD == null && skillUD == null)
                return false;
            List<string> skills = ["FEIGN_DEATH", "WILL_OF_THE_DEAD"] ;
            if (initialSkills.Count == 2)
                user.Skills.AddRange([skillUD!, skillPD!]);
            else
            {
                if (initialSkills.Contains(skills[0]))
                    user.Skills.Add(skillPD!);
                else
                    user.Skills.Add(skillUD!);
            }
            //todo 添加武器认领
            if (await _dataHelper.SaveChangesAsync() > 0)
                return true;
            return false;
            
        }
    }
}
