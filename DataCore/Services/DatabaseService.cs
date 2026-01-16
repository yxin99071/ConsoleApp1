using DataCore.Data;
using DataCore.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services
{
    public static class DatabaseService
    {
        public static AppDbContext GetDbContext()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbDirectory = Path.Combine(folder, "MyProject");
            string dbPath = Path.Combine(dbDirectory, "game.db");

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            var context = new AppDbContext(optionsBuilder.Options);

            // 确保数据库和表已创建
            context.Database.EnsureCreated();

            // 【新增】调用种子数据初始化
            SeedData(context);

            return context;
        }
        private static void SeedData(AppDbContext db)
        {
            // 如果已经有 Buff 数据，说明已经初始化过了，直接返回
            if (db.Buffs.Any()) return;

            // --- 1. 初始化 Buff 池 ---
            var buffMap = new Dictionary<string, Buff>
            {
                ["流血"] = new Buff { Name = "流血", LastRound = 2, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientStrength = 0.35, SpecialTag = new List<string> { "流血" } },
                ["铁壁意志"] = new Buff { Name = "铁壁意志", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.0, WoundCorrection = 0.8, SpecialTag = new List<string> { "防御" } },
                ["破甲"] = new Buff { Name = "破甲", LastRound = 2, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.2, CoefficientStrength = 0.15, SpecialTag = new List<string> { "破甲" } },
                ["中毒"] = new Buff { Name = "中毒", LastRound = 3, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientAgility = 0.25, SpecialTag = new List<string> { "中毒" } },
                ["肾上腺素"] = new Buff { Name = "肾上腺素", LastRound = 2, IsOnSelf = true, DamageCorrection = 1.2, WoundCorrection = 1.1, CoefficientAgility = 0.1, SpecialTag = new List<string> { "爆发" } },
                ["鹰眼"] = new Buff { Name = "鹰眼", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.15, WoundCorrection = 1.0, SpecialTag = new List<string> { "专注" } },
                ["点燃"] = new Buff { Name = "点燃", LastRound = 3, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientIntelligence = 0.30, SpecialTag = new List<string> { "火焰" } },
                ["奥术专注"] = new Buff { Name = "奥术专注", LastRound = 2, IsOnSelf = true, DamageCorrection = 1.2, WoundCorrection = 0.95, CoefficientIntelligence = 0.05, SpecialTag = new List<string> { "魔法强化" } },
                ["冻结"] = new Buff { Name = "冻结", LastRound = 1, IsOnSelf = false, DamageCorrection = 0.8, WoundCorrection = 1.2, CoefficientIntelligence = 0.10, SpecialTag = new List<string> { "控制" } },
                ["虚弱"] = new Buff { Name = "虚弱", LastRound = 2, IsOnSelf = false, DamageCorrection = 0.85, WoundCorrection = 1.15, SpecialTag = new List<string> { "常规" } },
                ["强壮"] = new Buff { Name = "强壮", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.1, WoundCorrection = 0.9, SpecialTag = new List<string> { "常规" } },
                ["再生"] = new Buff { Name = "再生", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.0, WoundCorrection = 0.95, CoefficientAgility = 0.04, CoefficientStrength = 0.04, CoefficientIntelligence = 0.04, SpecialTag = new List<string> { "恢复" } }
            };

            db.Buffs.AddRange(buffMap.Values);
            db.SaveChanges(); // 先保存 Buff 才能拿到数据库 ID

            // --- 2. 初始化 武器库 ---
            var weapons = new List<Weapon>();

            // 定义一个辅助动作方便快速添加武器
            void AddWeapon(string name, double agi, double str, double intel, List<string> buffNames, string tag = "常规")
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
            AddSkill("假死", true, 0, 0, 0, new());
            AddSkill("亡灵意志", true, 0, 0, 0, new());
            // 战士
            AddSkill("地裂斩", false, 0.2, 2.5, 0.3, new() { "虚弱" });
            AddSkill("战斗怒吼", false, 0.4, 1.3, 1.3, new() { "强壮", "铁壁意志" });
            // 游侠
            AddSkill("音速刺击", false, 2.5, 0.3, 0.2, new() { "流血" });
            AddSkill("消失", false, 1.5, 0.5, 1.0, new() { "鹰眼", "肾上腺素" });
            // 法师
            AddSkill("火球术", false, 0, 0, 3.5, new() { "点燃" });
            AddSkill("霜冻新星", false, 0.5, 0, 3.0, new() { "冻结" });
            AddSkill("折磨", false, 0, 0, 1.8, new(), "折磨");
            // 通用
            AddSkill("回响打击", false, 1.2, 1.0, 0.9, new() { "强壮" });
            AddSkill("盾击", false, 0.9, 1.2, 1.0, new() { "破甲" });
            AddSkill("灵魂绑定", false, 1.0, 0.9, 1.2, new() { "虚弱" });

            db.SaveChanges(); // 最后保存所有的武器和技能及其关联

            // 翻译并填充武器数据
            AddWeapon("处刑者之斧", 0.2, 2.5, 0.3, new() { "流血" });
            AddWeapon("塔盾与重锤", 0.1, 2.2, 0.7, new() { "铁壁意志" });
            AddWeapon("反曲长弓", 2.5, 0.4, 0.1, new() { "鹰眼" });
            AddWeapon("暗影双刃", 2.1, 0.5, 0.4, new() { "肾上腺素" });
            AddWeapon("大法师长杖", 0.1, 0.4, 2.5, new() { "奥术专注" });
            AddWeapon("符文魔剑", 1.0, 0.5, 1.5, new() { "点燃" });

            // 通用武器
            AddWeapon("守护者长戟", 0.9, 1.2, 0.9, new() { "强壮", "再生" }, "通用");
            AddWeapon("重弩", 1.1, 1.1, 0.8, new() { "虚弱" }, "通用");
            AddWeapon("侦察兵短剑", 1.2, 0.9, 0.9, new() { "强壮" }, "通用");
            AddWeapon("沉重木棒", 0.8, 1.1, 1.1, new(), "通用");
            AddWeapon("平衡长棍", 1.0, 1.0, 1.0, new(), "通用");
            AddWeapon("蚀刻匕首", 1.1, 0.9, 1.0, new(), "通用");

            db.Weapons.AddRange(weapons);
            db.SaveChanges();

            //user
            if (db.Users.Any()) return;

            // --- 准备技能组 (完全复刻你的逻辑) ---
            var allSkills = db.Skills.ToList();

            // 辅助获取方法
            Skill GetS(string name) => allSkills.First(s => s.Name == name);
            List<Skill> GetList(params string[] names) => names.Select(GetS).ToList();

            var commonSkill = GetList("回响打击", "盾击", "灵魂绑定");

            var warriorSkill = commonSkill.Concat(GetList("地裂斩", "战斗怒吼", "亡灵意志")).ToList();
            var rangerSkill = commonSkill.Concat(GetList("音速刺击", "消失", "假死")).ToList();
            var magicianSkill = commonSkill.Concat(GetList("火球术", "霜冻新星", "折磨", "假死")).ToList();

            var mortalSkill = commonSkill
                .Concat(warriorSkill)
                .Concat(rangerSkill)
                .Concat(magicianSkill)
                .DistinctBy(s => s.Id).ToList(); // 使用 ID 去重

            // --- 准备武器组 ---
            var allWeapons = db.Weapons.ToList();
            Weapon GetW(string name) => allWeapons.First(w => w.Name == name);

            var universalWeapons = GetListW("守护者长戟", "重弩", "侦察兵短剑", "沉重木棒", "平衡长棍", "蚀刻匕首");
            List<Weapon> GetListW(params string[] names) => names.Select(GetW).ToList();

            // --- 2. 创建并保存 User ---
            db.Users.AddRange(
                new User
                {
                    Name = "战士_凯尔",
                    Health = 840,
                    Profession = "战士",
                    Agility = 12,
                    Strength = 55,
                    Intelligence = 10,
                    Weapons = universalWeapons.Concat(GetListW("处刑者之斧", "塔盾与重锤")).ToList(),
                    Skills = warriorSkill
                },
                new User
                {
                    Name = "游侠_莱拉",
                    Health = 320,
                    Profession = "游侠",
                    Agility = 58,
                    Strength = 15,
                    Intelligence = 15,
                    Weapons = universalWeapons.Concat(GetListW("反曲长弓", "暗影双刃")).ToList(),
                    Skills = rangerSkill
                },
                new User
                {
                    Name = "法师_赞",
                    Health = 620,
                    Profession = "法师",
                    Agility = 18,
                    Strength = 10,
                    Intelligence = 60,
                    Weapons = universalWeapons.Concat(GetListW("大法师长杖", "符文魔剑")).ToList(),
                    Skills = magicianSkill
                },
                new User
                {
                    Name = "凡人_艾里斯",
                    Health = 650,
                    Profession = "凡人",
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