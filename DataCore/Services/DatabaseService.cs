using DataCore.Data;
using DataCore.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services
{
    public static class DatabaseService
    {
        public static BattleDbContext GetDbContext()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbDirectory = Path.Combine(folder, "MyProject");
            string dbPath = Path.Combine(dbDirectory, "game.db");

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            var optionsBuilder = new DbContextOptionsBuilder<BattleDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            var context = new BattleDbContext(optionsBuilder.Options);

            // 确保数据库和表已创建
            context.Database.EnsureCreated();

            // 【新增】调用种子数据初始化
            SeedData(context);

            return context;
        }
        private static void SeedData(BattleDbContext db)
        {
            // 如果已经有 Buff 数据，说明已经初始化过了，直接返回
            if (db.Buffs.Any()) return;

            // --- 1. 初始化 Buff 池 ---
            var buffMap = new Dictionary<string, Buff>
            {
                ["BLEED"] = new Buff { Name = "BLEED", LastRound = 2, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientStrength = 0.35, SpecialTag = new List<string> { "BLEED" } },
                ["IRON_WILL"] = new Buff { Name = "IRON_WILL", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.0, WoundCorrection = 0.8, SpecialTag = new List<string> { "DEFENSE" } },
                ["ARMOR_BREAK"] = new Buff { Name = "ARMOR_BREAK", LastRound = 2, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.2, CoefficientStrength = 0.15, SpecialTag = new List<string> { "ARMOR_PIERCE" } },
                ["POISON"] = new Buff { Name = "POISON", LastRound = 3, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientAgility = 0.25, SpecialTag = new List<string> { "POISON" } },
                ["ADRENALINE"] = new Buff { Name = "ADRENALINE", LastRound = 2, IsOnSelf = true, DamageCorrection = 1.2, WoundCorrection = 1.1, CoefficientAgility = 0.1, SpecialTag = new List<string> { "BURST" } },
                ["EAGLE_EYE"] = new Buff { Name = "EAGLE_EYE", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.15, WoundCorrection = 1.0, SpecialTag = new List<string> { "FOCUS" } },
                ["IGNITE"] = new Buff { Name = "IGNITE", LastRound = 3, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientIntelligence = 0.30, SpecialTag = new List<string> { "FIRE" } },
                ["ARCANE_FOCUS"] = new Buff { Name = "ARCANE_FOCUS", LastRound = 2, IsOnSelf = true, DamageCorrection = 1.2, WoundCorrection = 0.95, CoefficientIntelligence = 0.05, SpecialTag = new List<string> { "MAGIC_BOOST" } },
                ["FROZEN"] = new Buff { Name = "FROZEN", LastRound = 1, IsOnSelf = false, DamageCorrection = 0.8, WoundCorrection = 1.2, CoefficientIntelligence = 0.10, SpecialTag = new List<string> { "CONTROL" } },
                ["WEAKNESS"] = new Buff { Name = "WEAKNESS", LastRound = 2, IsOnSelf = false, DamageCorrection = 0.85, WoundCorrection = 1.15, SpecialTag = new List<string> { "COMMON" } },
                ["STRENGTHEN"] = new Buff { Name = "STRENGTHEN", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.1, WoundCorrection = 0.9, SpecialTag = new List<string> { "COMMON" } },
                ["REGENERATION"] = new Buff { Name = "REGENERATION", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.0, WoundCorrection = 0.95, CoefficientAgility = 0.04, CoefficientStrength = 0.04, CoefficientIntelligence = 0.04, SpecialTag = new List<string> { "RECOVERY" } }
            };

            db.Buffs.AddRange(buffMap.Values);
            db.SaveChanges(); // 先保存 Buff 才能拿到数据库 ID

            // --- 2. 初始化 武器库 ---
            var weapons = new List<Weapon>();

            // 定义一个辅助动作方便快速添加武器
            void AddWeapon(string name, double agi, double str, double intel, List<string> buffNames, string tag = "GENERAL")
            {
                var w = new Weapon
                {
                    Name = name,
                    CoefficientAgility = agi,
                    CoefficientStrength = str,
                    CoefficientIntelligence = intel,
                    Tags = new List<string> { tag }
                };
                foreach (var bName in buffNames)
                {
                    if (buffMap.TryGetValue(bName, out var bEntity))
                    {
                        // 注意：这里默认将初始化的武器 Buff 等级设为 1
                        w.WeaponBuffs.Add(new WeaponBuff { Weapon = w, Buff = bEntity, Level = 1 });
                    }
                }
                weapons.Add(w);
            }
            void AddSkill(string name, bool isPassive, double agi, double str, double intel, List<string> buffNames, string tag = "常规")
            {
                var s = new Skill
                {
                    Name = name,
                    IsPassive = isPassive,
                    CoefficientAgility = agi,
                    CoefficientStrength = str,
                    CoefficientIntelligence = intel,
                    Tags = new List<string> { tag }
                };

                foreach (var bName in buffNames)
                {
                    if (buffMap.TryGetValue(bName, out var bEntity))
                    {
                        // 同样，建立中间表关联并赋予初始等级
                        s.SkillBuffs.Add(new SkillBuff { Skill = s, Buff = bEntity, Level = 1 });
                    }
                }
                db.Skills.Add(s);
            }
            // 种族/被动
            AddSkill("FEIGN_DEATH", true, 0, 0, 0, new());
            AddSkill("UNDEAD_WILL", true, 0, 0, 0, new());

            // 战士 (Warrior)
            AddSkill("GROUND_SLAM", false, 0.2, 2.5, 0.3, new() { "WEAKNESS" });
            AddSkill("BATTLE_SHOUT", false, 0.4, 1.3, 1.3, new() { "STRENGTHEN", "IRON_WILL" });

            // 游侠 (Ranger)
            AddSkill("SONIC_STAB", false, 2.5, 0.3, 0.2, new() { "BLEED" });
            AddSkill("VANISH", false, 1.5, 0.5, 1.0, new() { "EAGLE_EYE", "ADRENALINE" });

            // 法师 (Mage)
            AddSkill("FIREBALL", false, 0, 0, 3.5, new() { "IGNITE" });
            AddSkill("FROST_NOVA", false, 0.5, 0, 3.0, new() { "FROZEN" });
            AddSkill("TORMENT", false, 0, 0, 1.8, new(), "TORMENT");

            // 通用 (General)
            AddSkill("ECHO_STRIKE", false, 1.2, 1.0, 0.9, new() { "STRENGTHEN" });
            AddSkill("SHIELD_BASH", false, 0.9, 1.2, 1.0, new() { "ARMOR_BREAK" });
            AddSkill("SOUL_BIND", false, 1.0, 0.9, 1.2, new() { "WEAKNESS" });

            db.SaveChanges(); // 最后保存所有的武器和技能及其关联


            // 职业专属武器
            AddWeapon("EXECUTIONER_AXE", 0.2, 2.5, 0.3, new() { "BLEED" });
            AddWeapon("TOWER_SHIELD_HAMMER", 0.1, 2.2, 0.7, new() { "IRON_WILL" });
            AddWeapon("RECURVE_BOW", 2.5, 0.4, 0.1, new() { "EAGLE_EYE" });
            AddWeapon("SHADOW_BLADES", 2.1, 0.5, 0.4, new() { "ADRENALINE" });
            AddWeapon("ARCHMAGE_STAFF", 0.1, 0.4, 2.5, new() { "ARCANE_FOCUS" });
            AddWeapon("RUNIC_MAGIC_SWORD", 1.0, 0.5, 1.5, new() { "IGNITE" });

            // 通用武器 (General Weapons)
            AddWeapon("GUARDIAN_HALBERD", 0.9, 1.2, 0.9, new() { "STRENGTHEN", "REGENERATION" }, "GENERAL");
            AddWeapon("HEAVY_CROSSBOW", 1.1, 1.1, 0.8, new() { "WEAKNESS" }, "GENERAL");
            AddWeapon("SCOUT_DAGGER", 1.2, 0.9, 0.9, new() { "STRENGTHEN" }, "GENERAL");
            AddWeapon("HEAVY_CLUB", 0.8, 1.1, 1.1, new(), "GENERAL");
            AddWeapon("BALANCED_STAFF", 1.0, 1.0, 1.0, new(), "GENERAL");
            AddWeapon("ETCHED_DAGGER", 1.1, 0.9, 1.0, new(), "GENERAL");

            db.Weapons.AddRange(weapons);
            db.SaveChanges();

            //user
            if (db.Users.Any()) return;

            // --- 准备技能组 (完全复刻你的逻辑) ---
            var allSkills = db.Skills.ToList();

            // 辅助获取方法
            Skill GetS(string name) => allSkills.First(s => s.Name == name);
            List<Skill> GetList(params string[] names) => names.Select(GetS).ToList();

            // 通用技能
            var commonSkill = GetList("ECHO_STRIKE", "SHIELD_BASH", "SOUL_BIND");
            // 战士技能
            var warriorSkill = commonSkill.Concat(GetList("GROUND_SLAM", "BATTLE_SHOUT", "UNDEAD_WILL")).ToList();
            // 游侠技能
            var rangerSkill = commonSkill.Concat(GetList("SONIC_STAB", "VANISH", "FEIGN_DEATH")).ToList();
            // 法师技能
            var magicianSkill = commonSkill.Concat(GetList("FIREBALL", "FROST_NOVA", "TORMENT", "FEIGN_DEATH")).ToList();

            // 凡人/全量技能列表
            var mortalSkill = commonSkill
                .Concat(warriorSkill)
                .Concat(rangerSkill)
                .Concat(magicianSkill)
                .DistinctBy(s => s.Id).ToList(); // 使用 ID 去重

            // --- 准备武器组 ---
            var allWeapons = db.Weapons.ToList();
            Weapon GetW(string name) => allWeapons.First(w => w.Name == name);

            // 通用武器列表
            var universalWeapons = GetListW("GUARDIAN_HALBERD", "HEAVY_CROSSBOW", "SCOUT_DAGGER", "HEAVY_CLUB", "BALANCED_STAFF", "ETCHED_DAGGER");
            List<Weapon> GetListW(params string[] names) => names.Select(GetW).ToList();

            // --- 2. 创建并保存 User ---
            db.Users.AddRange(
                new User
                {
                    Name = "战士_凯尔",
                    Password = 1234.ToString(),
                    Health = 840,
                    Profession = "WARRIOR",
                    Agility = 12,
                    Strength = 55,
                    Intelligence = 10,
                    Weapons = universalWeapons.Concat(GetListW("EXECUTIONER_AXE", "TOWER_SHIELD_HAMMER")).ToList(),
                    Skills = warriorSkill
                },
                new User
                {
                    Name = "游侠_莱拉",
                    Password = 1234.ToString(),
                    Health = 320,
                    Profession = "RANGER",
                    Agility = 58,
                    Strength = 15,
                    Intelligence = 15,
                    Weapons = universalWeapons.Concat(GetListW("RECURVE_BOW", "SHADOW_BLADES")).ToList(),
                    Skills = rangerSkill
                },
                new User
                {
                    Name = "法师_赞",
                    Password = 1234.ToString(),
                    Health = 620,
                    Profession = "MAGICIAN",
                    Agility = 18,
                    Strength = 10,
                    Intelligence = 60,
                    Weapons = universalWeapons.Concat(GetListW("ARCHMAGE_STAFF", "RUNIC_MAGIC_SWORD")).ToList(),
                    Skills = magicianSkill
                },
                new User
                {
                    Name = "凡人_艾里斯",
                    Password = 1234.ToString(),
                    Health = 650,
                    Profession = "MORTAL",
                    Agility = 31,
                    Strength = 33,
                    Intelligence = 30,
                    Weapons = allWeapons, // 凡人精通所有武器
                    Skills = mortalSkill
                }
            );

            db.SaveChanges();

        }
    }
}