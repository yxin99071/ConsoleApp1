using DataCore.Data;
using DataCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Metadata.Ecma335;


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
                // === 力量系 Buff ===
                ["BLEED"] = new Buff
                {
                    Name = "流血",
                    Description = "持续造成基于力量的流血伤害",
                    LastRound = 2,
                    IsOnSelf = false,
                    DirectDamage = 0,
                    DamageCorrection = 1.0,
                    WoundCorrection = 1.0,
                    CoefficientStrength = 0.4,
                    SpecialTag = new List<string>()
                },

                ["ARMOR_BREAK"] = new Buff
                {
                    Name = "破甲",
                    Description = "降低目标防御,使其受到更多伤害",
                    LastRound = 2,
                    IsOnSelf = false,
                    DamageCorrection = 1.0,
                    WoundCorrection = 1.10,
                    SpecialTag = new List<string>()
                },

                ["IRON_WILL"] = new Buff
                {
                    Name = "钢铁意志",
                    Description = "基于力量持续恢复生命,并减少受到的伤害",
                    LastRound = 2,
                    IsOnSelf = true,
                    DamageCorrection = 1.0,
                    WoundCorrection = 0.9,
                    CoefficientStrength = -0.15,
                    SpecialTag = new List<string>()
                },

                // === 敏捷系 Buff ===
                ["ADRENALINE"] = new Buff
                {
                    Name = "肾上腺素",
                    Description = "爆发状态,提升伤害和受伤",
                    LastRound = 2,
                    IsOnSelf = true,
                    DamageCorrection = 1.20,
                    WoundCorrection = 1.08,
                    SpecialTag = new List<string>()
                },

                ["EAGLE_EYE"] = new Buff
                {
                    Name = "鹰眼",
                    Description = "提升伤害精准度",
                    LastRound = 2,
                    IsOnSelf = true,
                    DamageCorrection = 1.2,
                    WoundCorrection = 1.0,
                    CoefficientAgility = 0.08,
                    SpecialTag = new List<string>()
                },

                ["EVASION"] = new Buff
                {
                    Name = "闪避",
                    Description = "增加闪避概率",
                    LastRound = 2,
                    IsOnSelf = true,
                    DamageCorrection = 1.0,

                    SpecialTag = new List<string>()
                },

                // === 智力系 Buff ===
                ["IGNITE"] = new Buff
                {
                    Name = "点燃",
                    Description = "持续造成基于智力的火焰伤害",
                    LastRound = 2,
                    IsOnSelf = false,
                    DamageCorrection = 1.0,
                    WoundCorrection = 1.0,
                    CoefficientIntelligence = 0.35,
                    SpecialTag = new List<string>()
                },

                ["FROZEN"] = new Buff
                {
                    Name = "冻结",
                    Description = "大幅降低目标伤害能力,并提升其受伤",
                    LastRound = 1,
                    IsOnSelf = false,
                    DamageCorrection = 0.6,
                    WoundCorrection = 1.1,
                    SpecialTag = new List<string>()
                },

                ["MANA_SHIELD"] = new Buff
                {
                    Name = "法力治疗",
                    Description = "持续恢复生命",
                    LastRound = 2,
                    IsOnSelf = true,
                    DamageCorrection = 1.0,
                    WoundCorrection = 0.8,
                    CoefficientIntelligence = -0.15,
                    SpecialTag = new List<string>()
                },

                // === 通用 Buff ===
                ["REGENERATION"] = new Buff
                {
                    Name = "再生",
                    Description = "基于三维属性均衡恢复生命",
                    LastRound = 3,
                    IsOnSelf = true,
                    DamageCorrection = 1.0,
                    WoundCorrection = 0.95,
                    CoefficientAgility = -0.01,
                    CoefficientStrength = -0.01,
                    CoefficientIntelligence = -0.01,
                    SpecialTag = new List<string>()
                },
                ["POISON"] = new Buff
                {
                    Name = "中毒",
                    Description = "持续造成伤害",
                    LastRound = 3,
                    IsOnSelf = false,
                    DamageCorrection = 1.0,
                    WoundCorrection = 0.95,
                    CoefficientAgility = 0.01,
                    CoefficientStrength = 0.01,
                    CoefficientIntelligence = 0.01,
                    SpecialTag = new List<string>()
                },
                ["STRENGTH"] = new Buff
                {
                    Name = "振奋",
                    Description = "增加造成的伤害",
                    LastRound = 2,
                    IsOnSelf = true,
                    DamageCorrection = 1.2,
                    WoundCorrection = 1.0,
                    SpecialTag = new List<string>()
                },
                ["WEAKNESS"] = new Buff
                {
                    Name = "虚弱",
                    Description = "减少造成的伤害",
                    LastRound = 3,
                    IsOnSelf = false,
                    DamageCorrection = 0.8,
                    WoundCorrection = 1.0,
                    SpecialTag = new List<string>()
                },
                
                ["BLOCK"] = new Buff
                {
                    Name = "封印",
                    Description = "下回合无法行动",
                    LastRound = 0,
                    IsOnSelf = false,
                    SpecialTag = new List<string>() { "Block" }
                },

            };


            _context.Buffs.AddRange(buffMap.Values);
            _context.SaveChanges(); // 先保存 Buff 才能拿到数据库 ID

            // --- 2. 初始化 武器库 ---
            var weapons = new List<Weapon>();
            var skills = new List<Skill>();  // 在函数外声明

            // 定义一个辅助动作方便快速添加武器
            void AddWeapon(string name, string profession, double agi, double str, double intel, List<string> buffNames, int rareLevel = 1, string? secondProf = null, string desc = "暂无描述")
            {
                var w = new Weapon
                {
                    Name = name,
                    Description = desc,
                    Profession = profession,
                    SecondProfession = secondProf,
                    RareLevel = rareLevel,
                    CoefficientAgility = agi,
                    CoefficientStrength = str,
                    CoefficientIntelligence = intel,
                    Tags = new List<string>()  // Tags可以后续根据需要添加
                };
                foreach (var bName in buffNames)
                {
                    if (buffMap.TryGetValue(bName, out var bEntity))
                    {
                        w.WeaponBuffs.Add(new WeaponBuff { Weapon = w, Buff = bEntity, Level = 1 });
                    }
                }
                weapons.Add(w);
            }
            

            void AddSkill(string name, string profession, bool isPassive, double agi, double str, double intel, List<string> buffNames, int rareLevel = 1, string? secondProf = null, string desc = "暂无描述")
            {
                var s = new Skill
                {
                    Name = name,
                    Description = desc,
                    Profession = profession,
                    SecondProfession = secondProf,
                    IsPassive = isPassive,
                    RareLevel = rareLevel,
                    CoefficientAgility = agi,
                    CoefficientStrength = str,
                    CoefficientIntelligence = intel,
                    Tags = new List<string>()
                };
                foreach (var bName in buffNames)
                {
                    if (buffMap.TryGetValue(bName, out var bEntity))
                    {
                        s.SkillBuffs.Add(new SkillBuff { Skill = s, Buff = bEntity, Level = 1 });
                    }
                }
                skills.Add(s);
            }



            // === 种族/被动技能 ===
            AddSkill("假死", "MORTAL", true, 0, 0, 0, new(), desc: "濒死时触发,伪装死亡以躲避致命伤害");
            AddSkill("亡者意志", "MORTAL", true, 0, 0, 0, new(), desc: "生命值过低时提升所有属性");
            #region 主职业技能
            // === 战士技能 (WARRIOR) - 力量系 ===
            // Level 1 - 基础技能
            AddSkill("重劈", "WARRIOR", false, 0.2, 1.5, 0.3, new(), 1, desc: "基础的重型武器攻击");
            AddSkill("冲锋", "WARRIOR", false, 0.3, 1.4, 0.3, new(), 1, desc: "向前冲锋撞击敌人");

            // Level 2 - 精良技能
            AddSkill("破甲斩", "WARRIOR", false, 0.2, 1.8, 0.3, new() { "ARMOR_BREAK" }, 2, desc: "强力攻击并破坏敌人护甲");
            AddSkill("战吼", "WARRIOR", false, 0.3, 1.5, 0.2, new() { "IRON_WILL" }, 2, desc: "激励自身获得钢铁意志");
            AddSkill("撕裂", "WARRIOR", false, 0.3, 1.6, 0.3, new() { "BLEED" }, 2, desc: "造成深度伤口使敌人持续流血");

            // Level 3 - 稀有技能
            AddSkill("毁灭打击", "WARRIOR", false, 0.2, 3.0, 0.3, new(), 3, desc: "集中全部力量的毁灭性一击");
            AddSkill("战争践踏", "WARRIOR", false, 0.3, 2.2, 0.3, new() { "BLEED", "ARMOR_BREAK" }, 3, desc: "践踏地面造成范围伤害并削弱敌人");
            AddSkill("不屈之志", "WARRIOR", false, 0.4, 1.8, 0.3, new() { "IRON_WILL", "REGENERATION" }, 3, desc: "激发潜能获得持续恢复和防护");


            // === 游侠技能 (RANGER) - 敏捷系 ===
            // Level 1 - 基础技能
            AddSkill("快速射击", "RANGER", false, 1.5, 0.2, 0.3, new(), 1, desc: "快速的远程攻击");
            AddSkill("匕首连击", "RANGER", false, 1.4, 0.3, 0.3, new(), 1, desc: "敏捷的近身连续攻击");

            // Level 2 - 精良技能
            AddSkill("精准射击", "RANGER", false, 1.8, 0.2, 0.3, new() { "EAGLE_EYE" }, 2, desc: "瞄准要害提升精准度");
            AddSkill("毒刃", "RANGER", false, 1.6, 0.3, 0.4, new() { "POISON" }, 2, desc: "涂毒的武器造成持续中毒伤害");
            AddSkill("影袭", "RANGER", false, 1.6, 0.3, 0.3, new() { "ADRENALINE" }, 2, desc: "从阴影中突袭并进入爆发状态");
            AddSkill("致命一击", "RANGER", false, 1.5, 0.4, 0.3, new() { "BLEED" }, 2, desc: "攻击要害造成流血伤害");

            // Level 3 - 稀有技能
            AddSkill("死亡之舞", "RANGER", false, 3.0, 0.2, 0.3, new(), 3, desc: "如舞蹈般的致命连击");
            AddSkill("猎杀时刻", "RANGER", false, 2.2, 0.3, 0.3, new() { "EAGLE_EYE", "ADRENALINE" }, 3, desc: "进入猎杀状态大幅提升战斗能力");
            AddSkill("幻影步", "RANGER", false, 1.9, 0.4, 0.3, new() { "EVASION", "ADRENALINE" }, 3, desc: "高速移动获得闪避和爆发");



            // === 法师技能 (MAGE) - 智力系 ===
            // Level 1 - 基础技能
            AddSkill("魔法飞弹", "MAGICIAN", false, 0.2, 0.3, 1.5, new(), 1, desc: "基础的魔法攻击");
            AddSkill("能量冲击", "MAGICIAN", false, 0.3, 0.2, 1.5, new(), 1, desc: "释放纯粹的魔法能量");

            // Level 2 - 精良技能
            AddSkill("火球术", "MAGICIAN", false, 0.2, 0.2, 1.8, new() { "IGNITE" }, 2, desc: "发射火球点燃敌人");
            AddSkill("冰霜箭", "MAGICIAN", false, 0.2, 0.3, 1.7, new() { "FROZEN" }, 2, desc: "冰冻箭矢减缓敌人");
            AddSkill("奥术护盾", "MAGICIAN", false, 0.3, 0.2, 1.6, new() { "MANA_SHIELD" }, 2, desc: "召唤法力护盾保护自身");

            // Level 3 - 稀有技能
            AddSkill("末日审判", "MAGICIAN", false, 0.2, 0.3, 3.0, new(), 3, desc: "毁灭性的魔法爆发");
            AddSkill("元素风暴", "MAGICIAN", false, 0.3, 0.2, 2.2, new() { "IGNITE", "FROZEN" }, 3, desc: "召唤冰火元素风暴");
            AddSkill("法力涌动", "MAGICIAN", false, 0.3, 0.3, 1.9, new() { "MANA_SHIELD", "REGENERATION" }, 3, desc: "魔力涌动提供持续保护和恢复");

            // === 通用技能 (GENERAL) ===
            // Level 1 - 基础技能
            AddSkill("回声打击", "MORTAL", false, 0.7, 0.7, 0.6, new(), 1, desc: "均衡的基础攻击");
            AddSkill("武技连击", "MORTAL", false, 0.6, 0.8, 0.6, new(), 1, desc: "结合武技与技巧的连续攻击");

            // Level 2 - 精良技能
            AddSkill("削弱打击", "MORTAL", false, 0.8, 0.8, 0.7, new() { "WEAKNESS" }, 2, desc: "削弱敌人战斗力的攻击");
            AddSkill("生命汲取", "MORTAL", false, 0.8, 0.9, 0.6, new() { "REGENERATION" }, 2, desc: "攻击并获得持续恢复");
            AddSkill("坚韧打击", "MORTAL", false, 0.7, 1.0, 0.6, new() { "IRON_WILL" }, 2, desc: "坚定的攻击并强化自身");
            AddSkill("全能爆发", "MORTAL", false, 0.9, 0.7, 0.7, new() { "ADRENALINE" }, 2, desc: "短暂提升全面战斗能力");

            // Level 3 - 稀有技能
            AddSkill("究极一击", "MORTAL", false, 1.2, 1.1, 1.2, new(), 3, desc: "集三维属性之大成的强力攻击");
            AddSkill("完美平衡", "MORTAL", false, 1.0, 1.0, 0.8, new() { "REGENERATION", "IRON_WILL" }, 3, desc: "达到身心完美平衡状态");
            AddSkill("万象归一", "MORTAL", false, 0.9, 0.9, 1.0, new() { "MANA_SHIELD", "EVASION" }, 3, desc: "融合攻防于一体的终极技能");
            #endregion

            #region 副职业技能
            // Level 1 - 基础技能
            AddSkill("旋风斩", "WARRIOR", false, 0.8, 1.0, 0.2, new(), 1, "RANGER", desc: "快速旋转攻击周围敌人");
            AddSkill("突刺", "WARRIOR", false, 0.9, 0.9, 0.2, new(), 1, "RANGER", desc: "快速的刺击攻击");

            // Level 2 - 精良技能
            AddSkill("狂战冲锋", "WARRIOR", false, 1.0, 1.2, 0.2, new() { "ADRENALINE" }, 2, "RANGER", desc: "爆发式冲锋进入狂暴状态");
            AddSkill("流血突袭", "WARRIOR", false, 0.9, 1.1, 0.2, new() { "BLEED" }, 2, "RANGER", desc: "快速突袭造成流血");
            AddSkill("闪避反击", "WARRIOR", false, 1.1, 0.9, 0.2, new() { "EVASION" }, 2, "RANGER", desc: "闪避后的强力反击");

            // Level 3 - 稀有技能
            AddSkill("狂怒斩杀", "WARRIOR", false, 1.2, 1.8, 0.3, new(), 3, "RANGER", desc: "爆发全部力量与速度的斩击");
            AddSkill("血色风暴", "WARRIOR", false, 1.0, 1.5, 0.3, new() { "BLEED", "ADRENALINE" }, 3, "RANGER", desc: "快速旋转造成大范围流血");
            AddSkill("野性猎杀", "WARRIOR", false, 1.1, 1.3, 0.3, new() { "EAGLE_EYE", "STRENGTH" }, 3, "RANGER", desc: "结合力量与精准的猎杀技");

            //魔战士
            // Level 1 - 基础技能
            AddSkill("魔力强击", "WARRIOR", false, 0.3, 1.2, 0.5, new(), 1, "MAGICIAN", desc: "注入魔力的重击");
            AddSkill("元素之刃", "WARRIOR", false, 0.2, 1.3, 0.5, new(), 1, "MAGICIAN", desc: "附魔武器攻击");

            // Level 2 - 精良技能
            AddSkill("烈焰斩", "WARRIOR", false, 0.3, 1.3, 0.6, new() { "IGNITE" }, 2, "MAGICIAN", desc: "火焰附魔的斩击");
            AddSkill("冰霜重击", "WARRIOR", false, 0.2, 1.4, 0.6, new() { "FROZEN" }, 2, "MAGICIAN", desc: "冰冻附魔的重击");
            AddSkill("魔法护甲", "WARRIOR", false, 0.3, 1.1, 0.6, new() { "MANA_SHIELD" }, 2, "MAGICIAN", desc: "魔法强化的防御姿态");

            // Level 3 - 稀有技能
            AddSkill("毁灭魔刃", "WARRIOR", false, 0.4, 1.8, 1.0, new(), 3, "MAGICIAN", desc: "魔力与力量完美融合的一击");
            AddSkill("元素爆破", "WARRIOR", false, 0.3, 1.5, 0.9, new() { "IGNITE", "FROZEN" }, 3, "MAGICIAN", desc: "引爆冰火元素造成爆炸伤害");
            AddSkill("符文守护", "WARRIOR", false, 0.4, 1.2, 0.9, new() { "IRON_WILL", "MANA_SHIELD" }, 3, "MAGICIAN", desc: "符文加护提供双重防护");
            //魔游
            // Level 1 - 基础技能
            AddSkill("魔法箭", "RANGER", false, 0.9, 0.2, 0.9, new(), 1, "MAGICIAN", desc: "注入魔力的箭矢");
            AddSkill("奥术刺击", "RANGER", false, 1.0, 0.2, 0.8, new(), 1, "MAGICIAN", desc: "魔力强化的快速攻击");

            // Level 2 - 精良技能
            AddSkill("烈焰箭雨", "RANGER", false, 1.0, 0.3, 0.9, new() { "IGNITE" }, 2, "MAGICIAN", desc: "火焰箭矢的连续射击");
            AddSkill("冰霜陷阱", "RANGER", false, 0.9, 0.3, 1.0, new() { "FROZEN" }, 2, "MAGICIAN", desc: "设置冰霜陷阱冻结敌人");
            AddSkill("奥术聚焦", "RANGER", false, 1.1, 0.2, 0.9, new() { "EAGLE_EYE" }, 2, "MAGICIAN", desc: "魔力增强精准度");

            // Level 3 - 稀有技能
            AddSkill("虚空穿刺", "RANGER", false, 1.5, 0.3, 1.4, new(), 3, "MAGICIAN", desc: "穿透虚空的致命一击");
            AddSkill("元素连射", "RANGER", false, 1.3, 0.2, 1.2, new() { "IGNITE", "FROZEN" }, 3, "MAGICIAN", desc: "快速射出冰火元素箭");
            AddSkill("幻影魔刃", "RANGER", false, 1.2, 0.3, 1.2, new() { "ADRENALINE", "MANA_SHIELD" }, 3, "MAGICIAN", desc: "幻影般的魔法攻击并获得保护");
            //力游
            // Level 1 - 基础技能
            AddSkill("强力射击", "RANGER", false, 0.9, 0.9, 0.2, new(), 1, "WARRIOR", desc: "力量加持的远程攻击");
            AddSkill("猛击", "RANGER", false, 1.0, 0.8, 0.2, new(), 1, "WARRIOR", desc: "快速而有力的近战攻击");

            // Level 2 - 精良技能
            AddSkill("破甲射击", "RANGER", false, 1.0, 0.9, 0.2, new() { "ARMOR_BREAK" }, 2, "WARRIOR", desc: "破坏护甲的强力射击");
            AddSkill("撕裂连击", "RANGER", false, 1.1, 0.8, 0.2, new() { "BLEED" }, 2, "WARRIOR", desc: "快速连击造成撕裂伤");
            AddSkill("战斗本能", "RANGER", false, 1.0, 0.7, 0.3, new() { "STRENGTH" }, 2, "WARRIOR", desc: "激发战斗本能提升伤害");

            // Level 3 - 稀有技能
            AddSkill("毁灭连射", "RANGER", false, 1.6, 1.2, 0.3, new(), 3, "WARRIOR", desc: "力量与速度的完美结合");
            AddSkill("狩猎狂潮", "RANGER", false, 1.3, 1.0, 0.3, new() { "BLEED", "EVASION" }, 3, "WARRIOR", desc: "狂野的攻击并保持灵活");
            AddSkill("铁血突袭", "RANGER", false, 1.2, 1.1, 0.3, new() { "IRON_WILL", "ADRENALINE" }, 3, "WARRIOR", desc: "坚韧而爆发的突袭");
            //力法
            // Level 1 - 基础技能
            AddSkill("魔力冲击", "MAGICIAN", false, 0.2, 0.9, 1.0, new(), 1, "WARRIOR", desc: "物理与魔法的双重冲击");
            AddSkill("元素锤击", "MAGICIAN", false, 0.3, 0.8, 1.0, new(), 1, "WARRIOR", desc: "魔法强化的重击");

            // Level 2 - 精良技能
            AddSkill("炎爆术", "MAGICIAN", false, 0.2, 0.8, 1.2, new() { "IGNITE" }, 2, "WARRIOR", desc: "强力的火焰爆破");
            AddSkill("寒冰打击", "MAGICIAN", false, 0.3, 0.9, 1.0, new() { "FROZEN" }, 2, "WARRIOR", desc: "冰霜魔法的强力攻击");
            AddSkill("战法护盾", "MAGICIAN", false, 0.3, 0.7, 1.1, new() { "IRON_WILL" }, 2, "WARRIOR", desc: "结合物理与魔法的防护");

            // Level 3 - 稀有技能
            AddSkill("泰坦之怒", "MAGICIAN", false, 0.3, 1.3, 1.6, new(), 3, "WARRIOR", desc: "如泰坦般的毁灭之力");
            AddSkill("元素粉碎", "MAGICIAN", false, 0.3, 1.0, 1.4, new() { "IGNITE", "ARMOR_BREAK" }, 3, "WARRIOR", desc: "粉碎一切的元素攻击");
            AddSkill("符文战甲", "MAGICIAN", false, 0.4, 0.9, 1.2, new() { "MANA_SHIELD", "REGENERATION" }, 3, "WARRIOR", desc: "符文强化的战斗姿态");

            //敏捷法
            // Level 1 - 基础技能
            AddSkill("疾速魔弹", "MAGICIAN", false, 0.9, 0.2, 1.0, new(), 1, "RANGER", desc: "快速连发的魔法弹");
            AddSkill("灵巧施法", "MAGICIAN", false, 0.8, 0.3, 1.0, new(), 1, "RANGER", desc: "灵活快速的魔法攻击");

            // Level 2 - 精良技能
            AddSkill("火焰疾走", "MAGICIAN", false, 0.9, 0.3, 1.1, new() { "IGNITE" }, 2, "RANGER", desc: "快速移动并释放火焰");
            AddSkill("寒冰箭雨", "MAGICIAN", false, 1.0, 0.2, 1.0, new() { "FROZEN" }, 2, "RANGER", desc: "冰霜魔法的连续攻击");
            AddSkill("幻术闪避", "MAGICIAN", false, 0.8, 0.3, 1.1, new() { "EVASION" }, 2, "RANGER", desc: "幻术辅助的闪避技巧");

            // Level 3 - 稀有技能
            AddSkill("流星术", "MAGICIAN", false, 1.4, 0.3, 1.5, new(), 3, "RANGER", desc: "如流星般快速而强大的魔法");
            AddSkill("疾风烈焰", "MAGICIAN", false, 1.2, 0.3, 1.2, new() { "IGNITE", "ADRENALINE" }, 3, "RANGER", desc: "风助火势的狂暴魔法");
            AddSkill("影舞魔光", "MAGICIAN", false, 1.1, 0.3, 1.3, new() { "EAGLE_EYE", "MANA_SHIELD" }, 3, "RANGER", desc: "如影随形的精准魔法");
            #endregion
            _context.Skills.AddRange(skills);
            _context.SaveChanges(); // 最后保存所有的武器和技能及其关联


            #region 主职武器
            // Level 1 - 基础武器
            AddWeapon("铁斧", "WARRIOR", 0.2, 1.5, 0.3, new(), 1, desc: "沉重的战斧");
            AddWeapon("重锤", "WARRIOR", 0.3, 1.4, 0.3, new(), 1, desc: "破坏力强的重型战锤");

            // Level 2 - 精良武器
            AddWeapon("破甲战斧", "WARRIOR", 0.2, 1.8, 0.3, new() { "ARMOR_BREAK" }, 2, desc: "能破坏护甲的战斧");
            AddWeapon("嗜血巨剑", "WARRIOR", 0.3, 1.6, 0.3, new() { "BLEED" }, 2, desc: "造成深度伤口的巨剑");
            AddWeapon("守护者之盾", "WARRIOR", 0.2, 1.5, 0.4, new() { "IRON_WILL" }, 2, desc: "提供强大防护的盾牌");

            // Level 3 - 稀有武器
            AddWeapon("泰坦战锤", "WARRIOR", 0.2, 3.0, 0.3, new(), 3, desc: "传说中泰坦使用的战锤");
            AddWeapon("屠戮之斧", "WARRIOR", 0.3, 2.2, 0.3, new() { "BLEED", "ARMOR_BREAK" }, 3, desc: "浸满鲜血的恐怖战斧");
            AddWeapon("不朽之刃", "WARRIOR", 0.4, 1.9, 0.3, new() { "IRON_WILL", "STRENGTH" }, 3, desc: "永不折断的传奇之刃");

            // Level 1 - 基础武器
            AddWeapon("猎弓", "RANGER", 1.5, 0.2, 0.3, new(), 1, desc: "适合狩猎的长弓");
            AddWeapon("双刃匕首", "RANGER", 1.4, 0.3, 0.3, new(), 1, desc: "锋利的双刃匕首");

            // Level 2 - 精良武器
            AddWeapon("鹰眼长弓", "RANGER", 1.8, 0.2, 0.3, new() { "EAGLE_EYE" }, 2, desc: "提升精准度的精制长弓");
            AddWeapon("影袭双刀", "RANGER", 1.6, 0.3, 0.3, new() { "ADRENALINE" }, 2, desc: "激发爆发力的双刀");
            AddWeapon("毒刃", "RANGER", 1.5, 0.4, 0.3, new() { "POISON" }, 2, desc: "淬毒的致命刀刃");

            // Level 3 - 稀有武器
            AddWeapon("风神之弓", "RANGER", 3.0, 0.2, 0.3, new(), 3, desc: "快如疾风的神弓");
            AddWeapon("暗影獠牙", "RANGER", 2.2, 0.3, 0.3, new() { "EVASION", "POISON" }, 3, desc: "来自暗影的致命獠牙");
            AddWeapon("猎杀者弩", "RANGER", 2.0, 0.4, 0.3, new() { "EAGLE_EYE", "BLEED" }, 3, desc: "狩猎大师的传奇弩弓");

            // Level 1 - 基础武器
            AddWeapon("学徒法杖", "MAGICIAN", 0.2, 0.3, 1.5, new(), 1, desc: "初学者使用的法杖");
            AddWeapon("水晶魔杖", "MAGICIAN", 0.3, 0.2, 1.5, new(), 1, desc: "水晶制成的魔力导体");

            // Level 2 - 精良武器
            AddWeapon("烈焰法杖", "MAGICIAN", 0.2, 0.2, 1.8, new() { "IGNITE" }, 2, desc: "蕴含火焰之力的法杖");
            AddWeapon("寒霜魔典", "MAGICIAN", 0.2, 0.3, 1.7, new() { "FROZEN" }, 2, desc: "记载冰霜魔法的古籍");
            AddWeapon("守护法珠", "MAGICIAN", 0.3, 0.2, 1.6, new() { "MANA_SHIELD" }, 2, desc: "提供魔法护盾的宝珠");

            // Level 3 - 稀有武器
            AddWeapon("毁灭权杖", "MAGICIAN", 0.2, 0.3, 3.0, new(), 3, desc: "毁灭一切的禁忌法杖");
            AddWeapon("元素之心", "MAGICIAN", 0.3, 0.2, 2.2, new() { "IGNITE", "FROZEN" }, 3, desc: "掌控冰火元素的核心");
            AddWeapon("贤者之书", "MAGICIAN", 0.3, 0.3, 1.9, new() { "MANA_SHIELD", "REGENERATION" }, 3, desc: "智者留下的魔法典籍");

            // Level 1 - 基础武器
            AddWeapon("长枪", "MORTAL", 0.7, 0.7, 0.6, new(), 1, desc: "平衡的长柄武器");
            AddWeapon("硬木棍", "MORTAL", 0.6, 0.8, 0.6, new(), 1, desc: "结实的木制武器");

            // Level 2 - 精良武器
            AddWeapon("符文长戟", "MORTAL", 0.8, 0.9, 0.6, new() { "STRENGTH" }, 2, desc: "刻有符文的战戟");
            AddWeapon("守护战棍", "MORTAL", 0.7, 1.0, 0.6, new() { "IRON_WILL" }, 2, desc: "提供防护的战斗棍棒");
            AddWeapon("生命之杖", "MORTAL", 0.9, 0.7, 0.7, new() { "REGENERATION" }, 2, desc: "蕴含生命之力的法杖");

            // Level 3 - 稀有武器
            AddWeapon("平衡之刃", "MORTAL", 1.2, 1.1, 1.2, new(), 3, desc: "完美平衡的传奇之刃");
            AddWeapon("万象之枪", "MORTAL", 1.0, 1.0, 0.8, new() { "REGENERATION", "STRENGTH" }, 3, desc: "包容万象的神枪");
            AddWeapon("归一战戟", "MORTAL", 0.9, 0.9, 1.0, new() { "IRON_WILL", "MANA_SHIELD" }, 3, desc: "融合攻防的终极武器");


            #endregion

            #region 双职武器
            // Level 1 - 基础武器
            AddWeapon("战斗短弓", "WARRIOR", 0.8, 1.0, 0.2, new(), 1, "RANGER", desc: "力量型的短弓");
            AddWeapon("重型战刃", "WARRIOR", 0.9, 0.9, 0.2, new(), 1, "RANGER", desc: "兼具力量与速度的战刃");

            // Level 2 - 精良武器
            AddWeapon("狂战双斧", "WARRIOR", 1.0, 1.2, 0.2, new() { "ADRENALINE" }, 2, "RANGER", desc: "激发狂暴的双手斧");
            AddWeapon("撕裂战刀", "WARRIOR", 0.9, 1.1, 0.2, new() { "BLEED" }, 2, "RANGER", desc: "造成撕裂伤的弯刀");
            AddWeapon("掠夺者之刃", "WARRIOR", 1.1, 0.9, 0.2, new() { "EVASION" }, 2, "RANGER", desc: "灵活而强力的武器");

            // Level 3 - 稀有武器
            AddWeapon("狂怒战弓", "WARRIOR", 1.2, 1.8, 0.3, new(), 3, "RANGER", desc: "需要巨大力量才能拉开的战弓");
            AddWeapon("血色风暴", "WARRIOR", 1.0, 1.5, 0.3, new() { "BLEED", "STRENGTH" }, 3, "RANGER", desc: "快速挥舞造成血雨的双刀");
            AddWeapon("野性之牙", "WARRIOR", 1.1, 1.3, 0.3, new() { "ARMOR_BREAK", "ADRENALINE" }, 3, "RANGER", desc: "野兽般凶猛的武器");



            // Level 1 - 基础武器
            AddWeapon("魔纹战剑", "WARRIOR", 0.3, 1.2, 0.5, new(), 1, "MAGICIAN", desc: "刻有魔纹的长剑");
            AddWeapon("符文战锤", "WARRIOR", 0.2, 1.3, 0.5, new(), 1, "MAGICIAN", desc: "附魔的重型战锤");

            // Level 2 - 精良武器
            AddWeapon("烈焰重剑", "WARRIOR", 0.3, 1.3, 0.6, new() { "IGNITE" }, 2, "MAGICIAN", desc: "燃烧着火焰的巨剑");
            AddWeapon("寒冰战斧", "WARRIOR", 0.2, 1.4, 0.6, new() { "FROZEN" }, 2, "MAGICIAN", desc: "散发寒气的战斧");
            AddWeapon("魔盾战锤", "WARRIOR", 0.3, 1.1, 0.6, new() { "MANA_SHIELD" }, 2, "MAGICIAN", desc: "提供魔法护盾的锤盾");

            // Level 3 - 稀有武器
            AddWeapon("毁灭魔剑", "WARRIOR", 0.4, 1.8, 1.0, new(), 3, "MAGICIAN", desc: "力量与魔力的完美结合");
            AddWeapon("元素战刃", "WARRIOR", 0.3, 1.5, 0.9, new() { "IGNITE", "FROZEN" }, 3, "MAGICIAN", desc: "操控冰火元素的魔剑");
            AddWeapon("符文圣剑", "WARRIOR", 0.4, 1.2, 0.9, new() { "IRON_WILL", "MANA_SHIELD" }, 3, "MAGICIAN", desc: "神圣符文加持的圣剑");

            // Level 1 - 基础武器
            AddWeapon("魔力短弓", "RANGER", 0.9, 0.2, 0.9, new(), 1, "MAGICIAN", desc: "附魔的短弓");
            AddWeapon("奥术刺剑", "RANGER", 1.0, 0.2, 0.8, new(), 1, "MAGICIAN", desc: "魔力增强的细剑");

            // Level 2 - 精良武器
            AddWeapon("炎弓", "RANGER", 1.0, 0.3, 0.9, new() { "IGNITE" }, 2, "MAGICIAN", desc: "射出火焰箭矢的长弓");
            AddWeapon("寒霜刺剑", "RANGER", 0.9, 0.3, 1.0, new() { "FROZEN" }, 2, "MAGICIAN", desc: "冰冻敌人的魔法剑");
            AddWeapon("奥术连弩", "RANGER", 1.1, 0.2, 0.9, new() { "EAGLE_EYE" }, 2, "MAGICIAN", desc: "魔力引导的精准弩");

            // Level 3 - 稀有武器
            AddWeapon("虚空长弓", "RANGER", 1.5, 0.3, 1.4, new(), 3, "MAGICIAN", desc: "穿越虚空的神秘弓");
            AddWeapon("元素双刃", "RANGER", 1.3, 0.2, 1.2, new() { "IGNITE", "FROZEN" }, 3, "MAGICIAN", desc: "冰火缠绕的双刀");
            AddWeapon("幻影魔弓", "RANGER", 1.2, 0.3, 1.2, new() { "ADRENALINE", "MANA_SHIELD" }, 3, "MAGICIAN", desc: "幻影般的魔法弓");

            // Level 1 - 基础武器
            AddWeapon("强力复合弓", "RANGER", 0.9, 0.9, 0.2, new(), 1, "WARRIOR", desc: "需要力量的复合弓");
            AddWeapon("战斗匕首", "RANGER", 1.0, 0.8, 0.2, new(), 1, "WARRIOR", desc: "强化过的战斗匕首");

            // Level 2 - 精良武器
            AddWeapon("破甲弩", "RANGER", 1.0, 0.9, 0.2, new() { "ARMOR_BREAK" }, 2, "WARRIOR", desc: "能穿透护甲的强弩");
            AddWeapon("裂伤双刀", "RANGER", 1.1, 0.8, 0.2, new() { "BLEED" }, 2, "WARRIOR", desc: "造成撕裂的双刀");
            AddWeapon("猎杀长弓", "RANGER", 1.0, 0.7, 0.3, new() { "STRENGTH" }, 2, "WARRIOR", desc: "强化射击的长弓");

            // Level 3 - 稀有武器
            AddWeapon("毁灭战弩", "RANGER", 1.6, 1.2, 0.3, new(), 3, "WARRIOR", desc: "毁灭性的重型弩炮");
            AddWeapon("狩猎双刃", "RANGER", 1.3, 1.0, 0.3, new() { "BLEED", "EVASION" }, 3, "WARRIOR", desc: "狩猎大师的双刀");
            AddWeapon("铁血战弓", "RANGER", 1.2, 1.1, 0.3, new() { "IRON_WILL", "EAGLE_EYE" }, 3, "WARRIOR", desc: "坚韧而精准的战弓");

            // Level 1 - 基础武器
            AddWeapon("重型法杖", "MAGICIAN", 0.2, 0.9, 1.0, new(), 1, "WARRIOR", desc: "可当棍棒使用的法杖");
            AddWeapon("战斗魔典", "MAGICIAN", 0.3, 0.8, 1.0, new(), 1, "WARRIOR", desc: "厚重的魔法书");

            // Level 2 - 精良武器
            AddWeapon("炎爆法杖", "MAGICIAN", 0.2, 0.8, 1.2, new() { "IGNITE" }, 2, "WARRIOR", desc: "爆发火焰的重杖");
            AddWeapon("寒冰战杖", "MAGICIAN", 0.3, 0.9, 1.0, new() { "FROZEN" }, 2, "WARRIOR", desc: "可近战的冰杖");
            AddWeapon("护盾法典", "MAGICIAN", 0.3, 0.7, 1.1, new() { "IRON_WILL" }, 2, "WARRIOR", desc: "提供防护的魔典");

            // Level 3 - 稀有武器
            AddWeapon("泰坦法杖", "MAGICIAN", 0.3, 1.3, 1.6, new(), 3, "WARRIOR", desc: "如泰坦般强大的法杖");
            AddWeapon("元素战杖", "MAGICIAN", 0.3, 1.0, 1.4, new() { "IGNITE", "ARMOR_BREAK" }, 3, "WARRIOR", desc: "元素与力量的融合");
            AddWeapon("符文圣典", "MAGICIAN", 0.4, 0.9, 1.2, new() { "MANA_SHIELD", "REGENERATION" }, 3, "WARRIOR", desc: "符文加护的神圣魔典");

            // Level 1 - 基础武器
            AddWeapon("轻型魔杖", "MAGICIAN", 0.9, 0.2, 1.0, new(), 1, "RANGER", desc: "轻便灵活的魔杖");
            AddWeapon("迅捷法珠", "MAGICIAN", 0.8, 0.3, 1.0, new(), 1, "RANGER", desc: "快速施法的宝珠");

            // Level 2 - 精良武器
            AddWeapon("疾火魔杖", "MAGICIAN", 0.9, 0.3, 1.1, new() { "IGNITE" }, 2, "RANGER", desc: "快速释放火焰的魔杖");
            AddWeapon("寒霜法珠", "MAGICIAN", 1.0, 0.2, 1.0, new() { "FROZEN" }, 2, "RANGER", desc: "冰霜魔法的宝珠");
            AddWeapon("幻影法杖", "MAGICIAN", 0.8, 0.3, 1.1, new() { "EVASION" }, 2, "RANGER", desc: "幻术增强的法杖");

            // Level 3 - 稀有武器
            AddWeapon("流星法杖", "MAGICIAN", 1.4, 0.3, 1.5, new(), 3, "RANGER", desc: "快如流星的魔法杖");
            AddWeapon("风火之心", "MAGICIAN", 1.2, 0.3, 1.2, new() { "IGNITE", "ADRENALINE" }, 3, "RANGER", desc: "风助火势的魔法核心");
            AddWeapon("影月法典", "MAGICIAN", 1.1, 0.3, 1.3, new() { "EAGLE_EYE", "MANA_SHIELD" }, 3, "RANGER", desc: "精准而神秘的魔典");
            #endregion

            _context.Weapons.AddRange(weapons);
            _context.SaveChanges();

            //user
            if (_context.Users.Any()) return;

            // --- 准备技能组 ---
            var allSkills = _context.Skills.ToList();

            // 辅助获取方法
            Skill GetS(string name) => allSkills.First(s => s.Name == name);
            List<Skill> GetList(params string[] names) => names.Select(GetS).ToList();

            // 通用技能 (GENERAL职业的技能)
            var commonSkill = GetList("回声打击", "生命汲取", "究极一击");

            // 战士技能 (WARRIOR + 通用 + 被动)
            var warriorSkill = commonSkill
                .Concat(GetList("破甲斩", "战吼", "毁灭打击", "亡者意志"))
                .ToList();

            // 游侠技能 (RANGER + 通用 + 被动)
            var rangerSkill = commonSkill
                .Concat(GetList("快速射击", "影袭", "幻影步", "假死"))
                .ToList();

            // 法师技能 (MAGICIAN + 通用 + 被动)
            var magicianSkill = commonSkill
                .Concat(GetList("火球术", "冰霜箭", "奥术护盾", "假死"))
                .ToList();

            // 凡人技能 (MORTAL所有技能 + 被动)
            var mortalSkill = GetList("回声打击", "武技连击", "生命汲取", "坚韧打击", "全能爆发", "究极一击", "完美平衡", "万象归一", "假死", "亡者意志");

            // --- 准备武器组 ---
            var allWeapons = _context.Weapons.ToList();
            Weapon GetW(string name) => allWeapons.First(w => w.Name == name);
            List<Weapon> GetListW(params string[] names) => names.Select(GetW).ToList();

            // 通用武器列表 (MORTAL的武器)
            var universalWeapons = GetListW("长枪", "硬木棍", "符文长戟", "守护战棍", "生命之杖", "平衡之刃");

            // --- 创建并保存 User ---
            _context.Users.AddRange(
                new User
                {
                    Id = 1,
                    Account = "1",
                    Name = "战士_凯尔",
                    Password = "1234",
                    Health = 820,
                    Exp = 0,
                    Level = 10,
                    Profession = "WARRIOR",
                    Agility = 15,
                    Strength = 57,
                    Intelligence = 15,
                    Weapons = universalWeapons.Concat(GetListW("铁斧", "重锤", "破甲战斧", "嗜血巨剑")).ToList(),
                    Skills = warriorSkill
                },
                new User
                {
                    Id = 2,
                    Account = "2",
                    Name = "游侠_莱拉",
                    Password = "1234",
                    Health = 590,
                    Exp = 0,
                    Level = 10,
                    Profession = "RANGER",
                    Agility = 57,
                    Strength = 15,
                    Intelligence = 15,
                    Weapons = universalWeapons.Concat(GetListW("猎弓", "双刃匕首", "鹰眼长弓", "影袭双刀")).ToList(),
                    Skills = rangerSkill
                },
                new User
                {
                    Id = 3,
                    Account = "3",
                    Name = "法师_赞",
                    Password = "1234",
                    Health = 674,
                    Exp = 0,
                    Level = 10,
                    Profession = "MAGICIAN",
                    Agility = 15,
                    Strength = 15,
                    Intelligence = 57,
                    Weapons = universalWeapons.Concat(GetListW("学徒法杖", "水晶魔杖", "烈焰法杖", "寒霜魔典")).ToList(),
                    Skills = magicianSkill
                },
                new User
                {
                    Id = 4,
                    Account = "4",
                    Name = "凡人_艾里斯",
                    Password = "1234",
                    Health = 754,
                    Exp = 0,
                    Level = 10,
                    Profession = "MORTAL",
                    Agility = 32,
                    Strength = 32,
                    Intelligence = 32,
                    Weapons = allWeapons, // 凡人掌握所有武器
                    Skills = mortalSkill
                }
            );

            _context.SaveChanges();

        }

        public async Task<List<User>> GetAllUser()=> await _context.Users.AsNoTracking().Where(u=>u.Profession!=null).ToListAsync();

        public async Task<List<TempAwardList>> GetAwardList(int id)
        {
            return await _context.TempAwardLists.Where(t => t.UserId == id).ToListAsync();
        }

        public async Task<Skill?> GetSkillById(int skillId, bool noTracking = true) =>await _context.Skills.SingleOrDefaultAsync(s=>s.Id == skillId);
        public async Task<Weapon?> GetWeaponById(int weaponId, bool noTracking = true) => await _context.Weapons.SingleOrDefaultAsync(w => w.Id == weaponId);
    }
}
