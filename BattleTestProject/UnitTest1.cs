using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using DataCore.Services;
using Microsoft.EntityFrameworkCore;


namespace BattleTestProject
{
    public class UnitTest1
    {
        private readonly BattleDataBridge _bridge = new BattleDataBridge();
        [Fact]
        public void Should_Load_Seeded_Data_With_Complex_Relationships()
        {
            // 1. 初始化数据库并填充种子数据
            using var db = DatabaseService.GetDbContext();

            // 2. 验证基础池数据量
            Assert.True(db.Buffs.Any(), "Buff pool should not be empty");
            Assert.Equal(12, db.Buffs.Count()); // 我们定义了12个Buff
            Assert.True(db.Weapons.Any(), "Weapon repository should not be empty");
            Assert.True(db.Skills.Any(), "Skill repository should not be empty");

            // 3. 验证特定玩家：战士_凯尔 (Name 保持中文)
            var kael = db.Users
                .Include(u => u.Weapons)
                    .ThenInclude(w => w.WeaponBuffs)
                        .ThenInclude(wb => wb.Buff)
                .Include(u => u.Skills)
                    .ThenInclude(s => s.SkillBuffs)
                .FirstOrDefault(u => u.Name == "战士_凯尔");

            Assert.NotNull(kael);
            Assert.Equal("WARRIOR", kael.Profession); // 验证职业英文名
            Assert.Equal(55, kael.Strength); // 验证基础属性

            // 4. 验证武器及其 Buff
            // 凯尔应该有“处刑者之斧” (Key: EXECUTIONER_AXE)
            var axe = kael.Weapons.FirstOrDefault(w => w.Name == "EXECUTIONER_AXE");
            Assert.NotNull(axe);
            Assert.Equal(2.5, axe.CoefficientStrength);

            // 验证转换器：Tags 应该包含英文标签（假设通用武器标为 GENERAL 或 COMMON）
            // 如果之前的 AddWeapon 逻辑中 "通用" 改为了 "GENERAL"，此处需对应
            Assert.Contains("GENERAL", axe.Tags);

            // 验证处刑者之斧是否带着“流血” (BLEED) Buff，且等级为1
            var axeBuff = axe.WeaponBuffs.FirstOrDefault(wb => wb.Buff.Name == "BLEED");
            Assert.NotNull(axeBuff);
            Assert.Equal(1, axeBuff.Level);
            Assert.Equal(0.35, axeBuff.Buff.CoefficientStrength);

            // 5. 验证技能逻辑
            // 战士应该有“战斗怒吼” (BATTLE_SHOUT)
            var roar = kael.Skills.FirstOrDefault(s => s.Name == "BATTLE_SHOUT");
            Assert.NotNull(roar);
            // 战斗怒吼应该有两个 Buff：STRENGTHEN (强壮)、IRON_WILL (铁壁意志)
            Assert.Equal(2, roar.SkillBuffs.Count);
            Assert.Contains(roar.SkillBuffs, sb => sb.Buff.Name == "STRENGTHEN");
            Assert.Contains(roar.SkillBuffs, sb => sb.Buff.Name == "IRON_WILL");

            // 6. 验证“凡人_艾里斯”的全能性
            var aris = db.Users
                .Include(u => u.Weapons)
                .Include(u => u.Skills)
                .FirstOrDefault(u => u.Name == "凡人_艾里斯");

            Assert.NotNull(aris);
            Assert.Equal("MORTAL", aris.Profession);
            // 凡人拥有全部12把武器
            Assert.Equal(12, aris.Weapons.Count);
        }

        [Theory]
        [InlineData(1, typeof(Warrior))]   // 假设 ID 1 是战士
        [InlineData(2, typeof(Ranger))]    // 假设 ID 2 是游侠
        [InlineData(3, typeof(Magician))]  // 假设 ID 3 是法师
        [InlineData(4, typeof(Mortal))]    // 假设 ID 4 是凡人
        public async Task ConvertUserToFighter_ShouldReturnCorrectSubclass(int userId, Type expectedType)
        {
            // Act
            var fighter = await _bridge.ConvertUserToFighter(userId);

            // Assert
            Assert.NotNull(fighter);
            Assert.IsType(expectedType, fighter);

            // 验证基础属性是否也带过来了
            Assert.True(fighter.MaxHealth > 0);
            Assert.NotNull(fighter.Skills);
            Assert.NotEmpty(fighter.Weapons);

            // 输出一下，方便调试看一眼
            Console.WriteLine($"User {userId} 成功转换为 {fighter.GetType().Name}, 拥有 {fighter.Skills.Count} 个技能");
        }

        [Fact]
        public async Task GetBuffTools_ShouldReturnData()
        {
            // Act
            var buffs = await BattleDataBridge.GetBuffTools();

            // Assert
            Assert.NotNull(buffs);
            Assert.True(buffs.Count >= 12); // 我们之前定义了12个
            Assert.Contains(buffs, b => b.Name == "BLEED");
        }
    }
}