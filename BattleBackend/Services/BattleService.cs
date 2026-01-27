using BattleBackend.Controllers;
using BattleBackend.DTOs;
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

        internal async Task<List<User>> GetAllFighter(string? exclusiveId)
        {
            var users = new List<User>();
            if (int.TryParse(exclusiveId, out int userId))
            {
                var allUsers = await _dataHelper.GetAllUser();
                allUsers.Remove(allUsers.SingleOrDefault(u => u.Id == userId)!);
                users.AddRange(allUsers);
            }
            else
            {
                var allUsers = await _dataHelper.GetAllUser();
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

        internal async Task<bool> InitializeUserProfile(int userId, InitProfileDto dto)
        {
            var user = await _dataHelper.GetUserById(userId, true);
            if (user is null)
                return false;
            //职业转换，这列可能会出错
            Console.WriteLine(dto.ToString());
            user.Profession = MappingExtensions.professionDict.GetValueOrDefault(dto.profession);
            user.SecondProfession = dto.secondProfession == null?null: MappingExtensions.professionDict.GetValueOrDefault(dto.secondProfession);
            user.Level = 1;
            user.Name = dto.name ?? user.Id.ToString();
            user.Account = dto.account ?? user.Id.ToString();
            user.Agility = 10;
            user.Strength = 10;
            user.Intelligence = 10;
            user.Exp = 0;
            //获得一级的属性点
            BattleManager.LevelUp(user, 1);
            user.Level = 1;
            var skillPD = await _dataHelper.FindSkillByName("假死");
            var skillUD = await _dataHelper.FindSkillByName("亡者意志");
            if (skillPD == null && skillUD == null)
                return false;
            //添加技能
            List<string> skills = ["FEIGN_DEATH", "WILL_OF_THE_DEAD"] ;
            if (dto.initialSkills.Count == 2)
                user.Skills.AddRange([skillUD!, skillPD!]);
            else
            {
                if (dto.initialSkills.Contains(skills[0]))
                    user.Skills.Add(skillPD!);
                else
                    user.Skills.Add(skillUD!);
            }
            //添加武器

            if (await _dataHelper.SaveChangesAsync() > 0)
                return true;
            return false;
            
        }
    }
}
