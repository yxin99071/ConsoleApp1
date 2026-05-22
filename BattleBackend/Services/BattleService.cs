using BattleBackend.Controllers;
using BattleBackend.DTOs;
using BattleCore;
using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using BattleCore.DataModel.States;
using DataCore.Models;
using DataCore.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

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
        internal async Task<List<BattleRecord>> GetBattleRecordListAsync(int id) => await _dataHelper.GetUserBattleHistoryAsync(id);

        internal async Task<List<BattleRecordDto>> GetBattleRecordListDto(int userId)
        {
            var records = await _dataHelper.GetUserBattleHistoryAsync(userId);
            var result  = new List<BattleRecordDto>();

            foreach (var record in records)
            {
                int.TryParse(record.WinnerId, out int winnerId);
                int.TryParse(record.LoserId,  out int loserId);

                bool isWin     = winnerId == userId;
                int  opponentId = isWin ? loserId : winnerId;

                var opponent = await _dataHelper.GetUserById(opponentId);

                result.Add(new BattleRecordDto
                {
                    Id           = record.Id,
                    IsWin        = isWin,
                    OpponentName = opponent?.Name ?? "未知玩家",
                    CreatedTime  = record.CreatedTime,
                });
            }
            return result;
        }
        internal async Task<string> ExecuteFight(int id, int enemyId)
        {
            StaticDataHelper.BuffPool = await _dataHelper.GetAllBuffs();
            var user = await _dataHelper.GetUserById(id);
            var enemy = await _dataHelper.GetUserById(enemyId);

            if (user is null || enemy is null)
                return "";

            // 记录升级前等级，用于计算升级 LotteryPoint
            int userLevelBefore  = user.Level;
            int enemyLevelBefore = enemy.Level;

            Fighter user_fighter = InitialFighter(user);
            Fighter enemy_fighter = InitialFighter(enemy);
            // 影子对决：两个 Fighter 来自同一个 User，前端按名字区分，必须保证名字唯一
            if (user.Id == enemy.Id)
                enemy_fighter.Name += " (影)";

            BattleManager.Initial(new List<Fighter> { user_fighter, enemy_fighter });
            //战斗与结果
            var isWin =BattleManager.BattleSimulation(user_fighter, enemy_fighter);
            //包含了《双方》的对战结果的奖励,如果是影子挑战则只包含一个元素
            var awardInfo  =  BattleManager.SetBattleResult(user, enemy, isWin);
            //是否升级，选择是否有奖励
            if (awardInfo.Count > 0)
                foreach (var award in awardInfo)
                    await SetAward(award);

            // ── LotteryPoint 发放 ──────────────────────────────────────────
            var now = DateTime.UtcNow;

            // 玩家本人：战斗奖励（30s 冷却）+ 升级奖励
            bool userCooldownOk = user.LastBattleTime == null ||
                                  (now - user.LastBattleTime.Value.ToUniversalTime()).TotalSeconds >= 30;
            if (userCooldownOk)
            {
                user.LotteryPoint  += isWin ? 8 : 3;
                user.LastBattleTime = now;
            }
            user.LotteryPoint += (user.Level - userLevelBefore) * 15;

            // 对手（非影子对决）
            if (user.Id != enemy.Id)
            {
                bool enemyCooldownOk = enemy.LastBattleTime == null ||
                                       (now - enemy.LastBattleTime.Value.ToUniversalTime()).TotalSeconds >= 30;
                if (enemyCooldownOk)
                {
                    enemy.LotteryPoint  += isWin ? 3 : 8;   // enemy 输/赢与 isWin 相反
                    enemy.LastBattleTime = now;
                }
                enemy.LotteryPoint += (enemy.Level - enemyLevelBefore) * 15;
            }

            //更新数据库
            await _dataHelper.UpgradeSinlgeUser(user);
            if (user.Id != enemy.Id)
                await _dataHelper.UpgradeSinlgeUser(enemy);
            await SaveRecords(id, enemyId,isWin, JsonLogger.GetEvents());
            await _dataHelper.SaveChangesAsync();
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
                    for (int i = 0; i < award.NormalSkillCount; i++)
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
                //处理职业技能
                if (award.SpecialSkillCount > 0)
                {
                    for (int i = 0; i < award.SpecialSkillCount; i++)
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
            async Task SaveRecords(int id, int enemyId,bool isWin, List<JsonLogger.BattleEvent> battleEvents)
            {
                int winnerId = isWin ? id : enemyId;
                int loserId = isWin ? enemyId : id;

                // 2. 配置物理路径（建议从配置文件读取，这里演示硬编码）
                string folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MyProject", "Record");

                // 确保目录存在
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // 3. 序列化并写入 JSON 文件
                // 生成唯一文件名：战斗时间_胜者ID_随机码.json
                string fileName = $"battle_{DateTime.Now:yyyyMMddHHmmss}_{winnerId}.json";
                string fullPath = Path.Combine(folderPath, fileName);

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = false, // 生产环境建议关闭缩进以节省空间
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string jsonString = JsonSerializer.Serialize(battleEvents, jsonOptions);
                await File.WriteAllTextAsync(fullPath, jsonString);
                await _dataHelper.AddRecordAsync( winnerId, loserId, fileName, DateTime.Now);

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
            var awardList = await _dataHelper.GetAwardList(id);
            var result = new List<AwardListDto>();

            foreach (var award in awardList)
            {
                var dto = new AwardListDto { Id = award.Id, AwardLevel = award.AwardLevel };

                if (award.Weapons.Count > 0)
                {
                    dto.Type = "WEAPON";
                    dto.Items = award.Weapons.Select(w => new AwardItemDto
                    {
                        Id = w.Id,
                        Name = w.Name,
                        Description = w.Description,
                        Profession = w.Profession,
                        SecondProfession = w.SecondProfession,
                        RareLevel = w.RareLevel,
                        IsPassive = false,
                        Buffs = w.WeaponBuffs.Select(wb => wb.Buff.ToDto()).ToList()
                    }).ToList();
                }
                else if (award.Skills.Count > 0)
                {
                    dto.Type = "SKILL";
                    dto.Items = award.Skills.Select(s => new AwardItemDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Profession = s.Profession,
                        SecondProfession = s.SecondProfession,
                        RareLevel = s.RareLevel,
                        IsPassive = s.IsPassive,
                        Buffs = s.SkillBuffs.Select(sb => sb.Buff.ToDto()).ToList()
                    }).ToList();
                }

                result.Add(dto);
            }
            return result;
        }
        internal async Task<bool> ClaimAward(int userId, int awardListId, int itemId)
        {
            var award = await _dataHelper.GetTempAwardListByIdAsync(awardListId);
            if (award == null || award.UserId != userId)
                return false;

            var user = await _dataHelper.GetUserById(userId, isTracking: true);
            if (user == null) return false;

            bool isWeapon = award.Weapons.Any(w => w.Id == itemId);
            bool isSkill  = award.Skills.Any(s => s.Id == itemId);
            if (!isWeapon && !isSkill)
                return false;

            if (isWeapon)
            {
                var existing = user.UserWeaponLinks.FirstOrDefault(uw => uw.WeaponId == itemId);
                if (existing != null) existing.Count++;
                else user.UserWeaponLinks.Add(new UserWeapon { UserId = userId, WeaponId = itemId, Count = 1 });
                user.LastWeaponAward = award.AwardLevel;

                // Unique 卡获得 → 商店对应槽位失效
                var weapon = award.Weapons.First(w => w.Id == itemId);
                if (weapon.IsUnique)
                    await _dataHelper.InvalidateUniqueItemSlotAsync(userId, "WEAPON", itemId);
            }
            else
            {
                var existing = user.UserSkillLinks.FirstOrDefault(us => us.SkillId == itemId);
                if (existing != null) existing.Count++;
                else user.UserSkillLinks.Add(new UserSkill { UserId = userId, SkillId = itemId, Count = 1 });
                user.LastSkillAward = award.AwardLevel;

                // Unique 卡获得 → 商店对应槽位失效
                var skill = award.Skills.First(s => s.Id == itemId);
                if (skill.IsUnique)
                    await _dataHelper.InvalidateUniqueItemSlotAsync(userId, "SKILL", itemId);
            }

            _dataHelper.RemoveTempAwardList(award);
            await _dataHelper.SaveChangesAsync();
            return true;
        }

        internal async Task<int> GetPendingAwardCount(int userId)
            => await _dataHelper.GetPendingAwardCountAsync(userId);
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
            List<string> skills = ["假死", "亡者意志"] ;
            //创建
            var UDrecord = new UserSkill { User = user, SkillId = skillUD?.Id ?? 0, Count = 1 };
            var PDrecord = new UserSkill { User = user, SkillId = skillPD?.Id ?? 0, Count = 1 };

            if (dto.initialSkills.Count == 2)
                user.UserSkillLinks.AddRange([UDrecord, PDrecord]);
            else
            {
                if (dto.initialSkills.Contains(skills[0]))
                    user.UserSkillLinks.Add(PDrecord!);
                else
                    user.UserSkillLinks.Add(UDrecord!);
            }
            //todo 添加武器改到抽奖中
            
            if (await _dataHelper.SaveChangesAsync() > 0)
                return true;
            return false;
        }
        //初始化武器和技能
        private List<T> FilterSkillOrWeapon<T>(List<T> origin, List<string> professions, List<int> rareLevels) 
            where T: Item
        {
            if (origin == null || !origin.Any()) return new List<T>();
            return origin.Where(item =>
                professions.Contains(item.Profession) &&
                rareLevels.Contains(item.RareLevel)
            ).ToList();
        }
        internal async Task<string> GetBattleRecordByIdAsync(int id)
        {
            string _recordFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "MyProject", "Record");
            var record = await _dataHelper.GetBattleRecordByIdAsync(id);
            if (record == null)
                return "";

            // 2. 拼接物理路径
            string fullPath = Path.Combine(_recordFolder, record.JsonFileName);

            if (!System.IO.File.Exists(fullPath))
                return "";

            // 3. 读取并直接返回 JSON
            // 使用 ContentResult 确保前端接收到的是 application/json 格式
            var jsonContent = await System.IO.File.ReadAllTextAsync(fullPath);
            return jsonContent;
        }


    }
}
