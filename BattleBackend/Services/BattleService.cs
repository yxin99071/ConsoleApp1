using BattleBackend.Controllers;
using BattleBackend.DTOs;
using BattleCore;
using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using BattleCore.DataModel.States;
using DataCore.Models;
using DataCore.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;

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
            StaticDataHelper.BuffPool = await _dataHelper.GetAllBuffs();
            var user = await _dataHelper.GetUserById(id);
            var enemy = await _dataHelper.GetUserById(enemyId);
            
            if (user is null || enemy is null)
                return "";
            Fighter user_fighter = InitialFighter(user);
            Fighter enemy_fighter = InitialFighter(enemy);

            BattleManager.Initial(new List<Fighter> { user_fighter, enemy_fighter });
            //战斗与结果

            var isWin =BattleManager.BattleSimulation(user_fighter, enemy_fighter);
            //包含了《双方》的对战结果的奖励,如果是影子挑战则只包含一个元素
            var awardInfo  =  BattleManager.SetBattleResult(user, enemy, isWin);
            //是否升级，选择是否有奖励
            if (awardInfo.Count > 0)
                foreach (var award in awardInfo)
                    await SetAward(award);
            //更新数据库
            await _dataHelper.UpgradeSinlgeUser(user);
            if (user.Id != enemy.Id)
                await _dataHelper.UpgradeSinlgeUser(enemy);

            return JsonLogger.GetJson();
            async Task SetAward(AwardInfo award)
            {
                if (!award.HasAward)
                    return;
                //基础数据准备
                var allLockedWeapons = await _dataHelper.GetLockedWeapons(award.user);
                var allLockedSkills = await _dataHelper.GetLockedSkills(award.user);
                Random random = new Random();

                //处理非职业武器
                if (award.NormalWeaponCount > 0)
                {
                    for (int i = 0; i < award.NormalWeaponCount; i++)
                    {
                        var selectedWeapons = await GetAvailibleItems<Weapon>(allLockedWeapons, new List<string>());
                        if (selectedWeapons.Count == 0)
                            break;
                        await _dataHelper.InsertAwardList(new TempAwardList
                        {
                            Weapons = selectedWeapons,
                            UserId = award.user.Id,
                            AwardLevel = award.user.Level
                        });
                    }
                }
                //处理职业武器
                if (award.SpecialWeaponCount > 0)
                {
                    for (int i = 0; i < award.SpecialWeaponCount; i++)
                    {
                        var selectedWeapons = await GetAvailibleItems<Weapon>(allLockedWeapons, new List<string> { award.user.Profession! });
                        if (selectedWeapons.Count == 0)
                            break;
                        await _dataHelper.InsertAwardList(new TempAwardList
                        {
                            Weapons = selectedWeapons,
                            UserId = award.user.Id,
                            AwardLevel = award.user.Level
                        });
                    }
                }
                //处理非职业技能
                if (award.NormalSkillCount > 0)
                {
                    for (int i = 0; i < award.NormalWeaponCount; i++)
                    {
                        var selectedSkills = await GetAvailibleItems<Skill>(allLockedSkills, new List<string> { award.user.Profession! });
                        if (selectedSkills.Count == 0)
                            break;
                        await _dataHelper.InsertAwardList(new TempAwardList
                        {
                            Skills = selectedSkills,
                            UserId = award.user.Id,
                            AwardLevel = award.user.Level
                        });
                    }
                }
                //处理职业技能
                if (award.SpecialSkillCount > 0)
                {
                    for (int i = 0; i < award.NormalWeaponCount; i++)
                    {
                        var selectedSkills = await GetAvailibleItems<Skill>(allLockedSkills, new List<string>());
                        if (selectedSkills.Count == 0)
                            break;
                        await _dataHelper.InsertAwardList(new TempAwardList
                        {
                            Skills = selectedSkills,
                            UserId = award.user.Id,
                            AwardLevel = award.user.Level
                        });
                    }
                }

                int getRareLevel()
                {
                    int chance = random.Next(1, 101);
                    if (chance < 5) return 3;
                    else if (chance < 30) return 2;
                    else return 1;
                }
                async Task<List<T>> GetAvailibleItems<T>(List<T> lockedItems, List<string> profession) where T : Item
                {
                    if (profession.Count == 0)
                        profession.AddRange(new List<String> { "MORTAL", "MAGICIAN", "WARRIOR", "RANGER" });
                    var finalChoice = new List<T>();
                    //获得3个稀有度
                    var rares = new int[] { getRareLevel(), getRareLevel(), getRareLevel() };
                    //根据三个稀有度和职业筛选获得一个或多个item池
                    List<List<T>> itemPools = new List<List<T>>();
                    foreach (var rare in rares)
                    {
                        var filtedPool = new List<T>();
                        var tempRare = rare;
                        //如果该稀有度的武器全部领完，则给另一个等级的池子
                        do
                        {
                            filtedPool = FilterSkillOrWeapon(lockedItems, profession, new List<int> { tempRare });
                            tempRare = tempRare % 3 + 1;
                            if (tempRare == rare)
                                break;
                        } while (filtedPool.Count == 0);
                        //只要有一个空池子就全部为空池子
                        if (filtedPool.Count == 0)
                        {
                            if (finalChoice.Count == 0)
                                return new List<T>();
                            else
                                break;
                        }
                        //找一个池子抽一个
                        var selectedItem = filtedPool.OrderBy(x => random.Next()).First();
                        lockedItems.Remove(selectedItem);
                        finalChoice.Add(selectedItem);
                    }
                    return finalChoice;
                }

            }
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

        internal async Task<List<AwardListDto>> GetAwardsList(int id)
        {
            //返回奖励列表
            var result = new List<AwardListDto>();
            var user = await _dataHelper.GetUserById(id);
            if (user == null)
                return new List<AwardListDto>();

            var awardList = await _dataHelper.GetAwardList(id);
            foreach (var award in awardList)
            {
                var tempAward = new AwardListDto();
                if (award.Skills.Count > 0)
                {
                    tempAward.Type = "SKILL";
                    foreach (var skill in award.Skills)
                    {
                        tempAward.Items.Add(skill);
                    }
                }
                if (award.Weapons.Count > 0)
                {
                    
                    foreach (var weapon in award.Weapons)
                    {
                        tempAward.Type = "WEAPON";
                        tempAward.Items.Add(weapon);
                    }
                }
                result.Add(tempAward);
            }
            return result;
        }

        internal async Task<User?> GetUserById(int id)
        {
            return await _dataHelper.GetUserById(id);
        }

        internal async Task<User?> IdentifyUser(string account, string password) => await _dataHelper.IdentifyUser(account, password);
        
        internal async Task<bool> InitializeUserProfile(int userId, InitProfileDto dto)
        {
            var user = await _dataHelper.GetUserById(userId, true);
            if (user is null)
                return false;
            //职业转换，这列可能会出错
            Console.WriteLine(dto.ToString());
            user.Profession = MappingExtensions.professionDict.GetValueOrDefault(dto.profession);
            if (dto.secondProfession == null ||
                MappingExtensions.professionDict.GetValueOrDefault(dto.secondProfession) == user.Profession)
                user.SecondProfession = null;
            else
                user.SecondProfession = MappingExtensions.professionDict.GetValueOrDefault(dto.secondProfession);
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
            await InitialWeaponAndSkillList(user);
            if (await _dataHelper.SaveChangesAsync() > 0)
                return true;
            return false;
            
        }
        //初始化武器和技能
        private async Task InitialWeaponAndSkillList(User user)
        {
            var unLockedWeapons = await _dataHelper.GetLockedWeapons(user);
            var unLockedSkills = await _dataHelper.GetLockedSkills(user);
            if (unLockedWeapons.Count <= 2 || unLockedSkills.Count<=2)
                throw new Exception("InitialError");
            var initialWeapons = FilterSkillOrWeapon<Weapon>(unLockedWeapons, new List<string> { user.Profession!, "MORTAL" }, [1]);
            var initialSkills = FilterSkillOrWeapon<Skill>(unLockedSkills, new List<string> { user.Profession!, "MORTAL" }, [1]);

            if (initialWeapons.Count <= 2 || initialSkills.Count <= 2)
                throw new Exception("InitialError");
            var random = new Random();
            // 随机打乱并取前 N 个
            var selectedWeapons = initialWeapons.OrderBy(x => random.Next()).Take(1).ToList();
            var selectedSkills = initialSkills.OrderBy(x => random.Next()).Take(1).ToList();
        }

        private List<T> FilterSkillOrWeapon<T>(List<T> origin, List<string> professions, List<int> rareLevels) 
            where T: Item
        {
            if (origin == null || !origin.Any()) return new List<T>();
            return origin.Where(item =>
                professions.Contains(item.Profession) &&
                rareLevels.Contains(item.RareLevel)
            ).ToList();
        }



    }
}
