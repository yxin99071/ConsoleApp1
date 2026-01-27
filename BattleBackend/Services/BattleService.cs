using BattleBackend.Controllers;
using BattleBackend.DTOs;
using BattleCore;
using BattleCore.BattleLogic;
using BattleCore.DataModel.Fighters;
using DataCore.Models;
using DataCore.Services;
using Microsoft.IdentityModel.Tokens;
using System;

namespace BattleBackend.Services
{
    public class BattleService
    {
        private readonly DataHelper _dataHelper;
        public BattleService(DataHelper dataHelper)
        {
            _dataHelper = dataHelper;
        }

        private static Fighter InitialFighter(User? user)
        {
            if (user is not null)
            {
                if (user.Profession == "MAGICIAN")
                    return new Magician(user);
                if (user.Profession == "WARRIOR")
                    return new Warrior(user);
                if (user.Profession == "RANGER")
                    return new Ranger(user);
                else
                    return new Mortal(user);
            }
            else
                throw new Exception();//出错

        }

        internal async Task<string> ExecuteFight(int id, int enemyId)
        {
            var user = await _dataHelper.GetUserById(id);
            var enemy = await _dataHelper.GetUserById(enemyId);
            if (user is null || enemy is null)
                return "";
            Fighter user_fighter = InitialFighter(user);
            Fighter enemy_fighter = InitialFighter(enemy);
            //战斗与结果

            var isWin =BattleManager.BattleSimulation(user_fighter, enemy_fighter);
            BattleManager.SetBattleResult(user, enemy, isWin);
            BattleManager.SetBattleResult(enemy, user, !isWin, false);
            await _dataHelper.UpgradeSinlgeUser(user);
            await _dataHelper.UpgradeSinlgeUser(enemy);

            return JsonLogger.GetJson();
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

        internal async Task<AwardListDto> GetAwardsList(int id)
        {
            //返回奖励列表
            var result = new AwardListDto();
            var user = await _dataHelper.GetUserById(id);
            if (user == null)
                return new AwardListDto();
            var awardList = await _dataHelper.GetAwardList(id);
            foreach (var award in awardList)
            {
                if (award.skillIds.Count > 0)
                {
                    foreach (var skillId in award.skillIds)
                    {
                        var skill = await _dataHelper.GetSkillById(skillId);
                        if (skill != null)
                            result.Skills.Add(skill);
                    }
                }
                if (award.weaponIds.Count > 0)
                {
                    foreach (var weaponId in award.weaponIds)
                    {
                        var weapon = await _dataHelper.GetSkillById(weaponId);
                        if (weapon != null)
                            result.Skills.Add(weapon);
                    }
                }
            }
            return result;
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
