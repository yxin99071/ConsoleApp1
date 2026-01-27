using DataCore.Data;
using DataCore.Models;
using Microsoft.EntityFrameworkCore;
using System;


namespace DataCore.Services
{
    public class DataHelper
    {
        private readonly BattleDbContext _context;
        public DataHelper()
        {
            _context = DatabaseHelper.GetDbContext();
        }
        public DataHelper(BattleDbContext context)
        {
            _context = context;
        }
        public async Task<int> SaveChangesAsync()
        {
            // 调用内部注入的 DbContext 保存更改
            return await _context.SaveChangesAsync();
        }
        public async Task<Skill?> FindSkillByName(string name)
        {
            return await _context.Skills.SingleOrDefaultAsync(s => s.Name == name);
        }

        public async Task<User?> GetUserById(int UserId,bool isTracking = false)
        {
            if(isTracking)
                return await _context.Users
                .Include(u => u.Weapons)
                .ThenInclude(w => w.WeaponBuffs)
                .ThenInclude(wb => wb.Buff)
                .Include(u => u.Skills)
                .ThenInclude(s => s.SkillBuffs)
                .ThenInclude(sb => sb.Buff)
                .SingleOrDefaultAsync(u => u.Id == UserId);

            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Weapons)
                .ThenInclude(w => w.WeaponBuffs)
                .ThenInclude(wb => wb.Buff)
                .Include(u => u.Skills)
                .ThenInclude(s => s.SkillBuffs)
                .ThenInclude(sb => sb.Buff)
                .SingleOrDefaultAsync(u => u.Id == UserId);
        }
        public async Task<List<Buff>> GetAllBuffs()
        {
            return await _context.Buffs.AsNoTracking().ToListAsync();
        }
        public async Task UpgradeSinlgeUser(User user)
        {
            var dbUser = await _context.Users.FindAsync(user.Id);

            if (dbUser != null)
            {

                // 核心：强制从数据库同步最新状态，刷新内存快照
                await _context.Entry(dbUser).ReloadAsync();

                _context.Entry(dbUser).CurrentValues.SetValues(new
                {
                    user.Exp,
                    user.Level,
                    user.Health,
                    user.Agility,
                    user.Strength,
                    user.Intelligence
                });

                await _context.SaveChangesAsync();
            }
        }
        public async Task<User?> IdentifyUser(string account, string password)
        {
            var user = _context.Users.SingleOrDefaultAsync(b => b.Account == account && b.Password == password);
            return await user;
        }




        //seedData
        public async Task SeedData()
        {
            //确认数据库
            _context.Database.EnsureCreated();
            // 如果已经有 Buff 数据，说明已经初始化过了，直接返回
            if (_context.Buffs.Any()) return;

            // --- 1. 初始化 Buff 池 ---
            var buffMap = new Dictionary<string, Buff>
            {
                ["BLEED"] = new Buff { Name = "流血", LastRound = 2, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientStrength = 0.35, SpecialTag = new List<string> { "流血" } },
                ["IRON_WILL"] = new Buff { Name = "钢铁意志", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.0, WoundCorrection = 0.8, SpecialTag = new List<string> { "防御" } },
                ["ARMOR_BREAK"] = new Buff { Name = "破甲", LastRound = 2, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.2, CoefficientStrength = 0.15, SpecialTag = new List<string> { "穿甲" } },
                ["POISON"] = new Buff { Name = "中毒", LastRound = 3, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientAgility = 0.25, SpecialTag = new List<string> { "毒" } },
                ["ADRENALINE"] = new Buff { Name = "肾上腺素", LastRound = 2, IsOnSelf = true, DamageCorrection = 1.2, WoundCorrection = 1.1, CoefficientAgility = 0.1, SpecialTag = new List<string> { "爆发" } },
                ["EAGLE_EYE"] = new Buff { Name = "鹰眼", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.15, WoundCorrection = 1.0, SpecialTag = new List<string> { "专注" } },
                ["IGNITE"] = new Buff { Name = "点燃", LastRound = 3, IsOnSelf = false, DamageCorrection = 1.0, WoundCorrection = 1.0, CoefficientIntelligence = 0.30, SpecialTag = new List<string> { "火焰" } },
                ["ARCANE_FOCUS"] = new Buff { Name = "奥术专注", LastRound = 2, IsOnSelf = true, DamageCorrection = 1.2, WoundCorrection = 0.95, CoefficientIntelligence = 0.05, SpecialTag = new List<string> { "魔力强化" } },
                ["FROZEN"] = new Buff { Name = "冻结", LastRound = 1, IsOnSelf = false, DamageCorrection = 0.8, WoundCorrection = 1.2, CoefficientIntelligence = 0.10, SpecialTag = new List<string> { "控制" } },
                ["WEAKNESS"] = new Buff { Name = "虚弱", LastRound = 2, IsOnSelf = false, DamageCorrection = 0.85, WoundCorrection = 1.15, SpecialTag = new List<string> { "普通" } },
                ["STRENGTHEN"] = new Buff { Name = "强化", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.1, WoundCorrection = 0.9, SpecialTag = new List<string> { "普通" } },
                ["REGENERATION"] = new Buff { Name = "再生", LastRound = 3, IsOnSelf = true, DamageCorrection = 1.0, WoundCorrection = 0.95, CoefficientAgility = 0.04, CoefficientStrength = 0.04, CoefficientIntelligence = 0.04, SpecialTag = new List<string> { "恢复" } }
            };


            _context.Buffs.AddRange(buffMap.Values);
            _context.SaveChanges(); // 先保存 Buff 才能拿到数据库 ID

            // --- 2. 初始化 武器库 ---
            var weapons = new List<Weapon>();

            // 定义一个辅助动作方便快速添加武器
            void AddWeapon(string name, double agi, double str, double intel, List<string> buffNames, string tag = "普通")
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
            void AddSkill(string name, bool isPassive, double agi, double str, double intel, List<string> buffNames, string tag = "普通")
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
                _context.Skills.Add(s);
            }
            // 种族/被动
            AddSkill("假死", true, 0, 0, 0, new());
            AddSkill("亡者意志", true, 0, 0, 0, new());

            // 战士 (Warrior)
            AddSkill("地面猛击", false, 0.2, 2.5, 0.3, new() { "虚弱" });
            AddSkill("战吼", false, 0.4, 1.3, 1.3, new() { "强化", "钢铁意志" });

            // 游侠 (Ranger)
            AddSkill("音速刺击", false, 2.5, 0.3, 0.2, new() { "流血" });
            AddSkill("消失", false, 1.5, 0.5, 1.0, new() { "鹰眼", "肾上腺素" });

            // 法师 (Mage)
            AddSkill("火球术", false, 0, 0, 3.5, new() { "点燃" });
            AddSkill("冰霜新星", false, 0.5, 0, 3.0, new() { "冻结" });
            AddSkill("折磨", false, 0, 0, 1.8, new(), "折磨");

            // 通用 (General)
            AddSkill("回声打击", false, 1.2, 1.0, 0.9, new() { "强化" });
            AddSkill("盾击", false, 0.9, 1.2, 1.0, new() { "破甲" });
            AddSkill("灵魂束缚", false, 1.0, 0.9, 1.2, new() { "虚弱" });

            _context.SaveChanges(); // 最后保存所有的武器和技能及其关联


            // 职业专属武器
            AddWeapon("行刑斧", 0.2, 2.5, 0.3, new() { "流血" });
            AddWeapon("塔盾锤", 0.1, 2.2, 0.7, new() { "钢铁意志" });
            AddWeapon("反曲弓", 2.5, 0.4, 0.1, new() { "鹰眼" });
            AddWeapon("影刃", 2.1, 0.5, 0.4, new() { "肾上腺素" });
            AddWeapon("大法师法杖", 0.1, 0.4, 2.5, new() { "奥术专注" });
            AddWeapon("符文魔剑", 1.0, 0.5, 1.5, new() { "点燃" });

            // 通用武器 (General Weapons)
            AddWeapon("守护者戟", 0.9, 1.2, 0.9, new() { "强化", "再生" }, "普通");
            AddWeapon("重型十字弩", 1.1, 1.1, 0.8, new() { "虚弱" }, "普通");
            AddWeapon("侦察匕首", 1.2, 0.9, 0.9, new() { "强化" }, "普通");
            AddWeapon("重型棍棒", 0.8, 1.1, 1.1, new(), "普通");
            AddWeapon("平衡法杖", 1.0, 1.0, 1.0, new(), "普通");
            AddWeapon("雕纹匕首", 1.1, 0.9, 1.0, new(), "普通");

            _context.Weapons.AddRange(weapons);
            _context.SaveChanges();

            //user
            if (_context.Users.Any()) return;

            // --- 准备技能组 (完全复刻你的逻辑) ---
            var allSkills = _context.Skills.ToList();

            // 辅助获取方法
            Skill GetS(string name) => allSkills.First(s => s.Name == name);
            List<Skill> GetList(params string[] names) => names.Select(GetS).ToList();
            // 通用技能
            var commonSkill = GetList("回声打击", "盾击", "灵魂束缚");
            // 战士技能
            var warriorSkill = commonSkill.Concat(GetList("地面猛击", "战吼", "亡者意志")).ToList();
            // 游侠技能
            var rangerSkill = commonSkill.Concat(GetList("音速刺击", "消失", "假死")).ToList();
            // 法师技能
            var magicianSkill = commonSkill.Concat(GetList("火球术", "冰霜新星", "折磨", "假死")).ToList();

            // 凡人/全量技能列表
            var mortalSkill = commonSkill
                .Concat(warriorSkill)
                .Concat(rangerSkill)
                .Concat(magicianSkill)
                .DistinctBy(s => s.Id).ToList(); // 使用 ID 去重

            // --- 准备武器组 ---
            var allWeapons = _context.Weapons.ToList();
            Weapon GetW(string name) => allWeapons.First(w => w.Name == name);

            // 通用武器列表
            var universalWeapons = GetListW("守护者戟", "重型十字弩", "侦察匕首", "重型棍棒", "平衡法杖", "雕纹匕首");
            List<Weapon> GetListW(params string[] names) => names.Select(GetW).ToList();


            // --- 2. 创建并保存 User ---
            _context.Users.AddRange(
                new User
                {
                    Id = 1,
                    Account = 1.ToString(),
                    Name = "战士_凯尔",
                    Password = 1234.ToString(),
                    Health = 820,
                    Exp = 0,
                    Level = 10,
                    Profession = "WARRIOR",
                    Agility = 15,
                    Strength = 57,
                    Intelligence = 15,
                    Weapons = universalWeapons.Concat(GetListW("行刑斧", "塔盾锤")).ToList(),
                    Skills = warriorSkill
                },
                new User
                {
                    Id = 2,
                    Account = 2.ToString(),
                    Name = "游侠_莱拉",
                    Password = 1234.ToString(),
                    Health = 590,
                    Exp = 0,
                    Level = 10,
                    Profession = "RANGER",
                    Agility = 57,
                    Strength = 15,
                    Intelligence = 15,
                    Weapons = universalWeapons.Concat(GetListW("反曲弓", "影刃")).ToList(),
                    Skills = rangerSkill
                },
                new User
                {
                    Id = 3,
                    Account = 3.ToString(),
                    Name = "法师_赞",
                    Password = 1234.ToString(),
                    Health = 674,
                    Exp = 0,
                    Level = 10,
                    Profession = "MAGICIAN",
                    Agility = 15,
                    Strength = 15,
                    Intelligence = 57,
                    Weapons = universalWeapons.Concat(GetListW("大法师法杖", "符文魔剑")).ToList(),
                    Skills = magicianSkill
                },
                new User
                {
                    Id = 4,
                    Account = 4.ToString(),
                    Name = "凡人_艾里斯",
                    Password = 1234.ToString(),
                    Health = 754,
                    Profession = "MORTAL",
                    Agility = 32,
                    Strength = 32,
                    Intelligence = 32,
                    Weapons = allWeapons, // 凡人精通所有武器
                    Skills = mortalSkill
                }
            );


            _context.SaveChanges();

        }

        public async Task<List<User>> GetAllUser()=> await _context.Users.AsNoTracking().Where(u=>u.Profession!=null).ToListAsync();

    }
}
