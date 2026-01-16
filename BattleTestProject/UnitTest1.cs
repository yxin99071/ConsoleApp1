using DataCore.Models;
using DataCore.Services;
using Microsoft.EntityFrameworkCore;

namespace BattleTestProject
{
    public class UnitTest1
    {
        [Fact]
        public void Should_Load_Seeded_Data_With_Complex_Relationships()
        {
            // 1. 初始化数据库并填充种子数据
            using var db = DatabaseService.GetDbContext();

            // 2. 验证基础池数据量
            Assert.True(db.Buffs.Any(), "Buff池应当不为空");
            Assert.Equal(12, db.Buffs.Count()); // 我们定义了12个Buff
            Assert.True(db.Weapons.Any(), "武器库应当不为空");
            Assert.True(db.Skills.Any(), "技能库应当不为空");

            // 3. 验证特定玩家：战士_凯尔
            var kael = db.Users
                .Include(u => u.Weapons)
                    .ThenInclude(w => w.WeaponBuffs)
                        .ThenInclude(wb => wb.Buff)
                .Include(u => u.Skills)
                    .ThenInclude(s => s.SkillBuffs)
                .FirstOrDefault(u => u.Name == "战士_凯尔");

            Assert.NotNull(kael);
            Assert.Equal(55, kael.Strength); // 验证基础属性

            // 4. 验证武器及其 Buff (带 Level 属性)
            // 凯尔应该有“处刑者之斧”
            var axe = kael.Weapons.FirstOrDefault(w => w.Name == "处刑者之斧");
            Assert.NotNull(axe);
            Assert.Equal(2.5, axe.CoefficientStrength);

            // 验证转换器：Tags 应该包含“常规”
            Assert.Contains("常规", axe.Tags);

            // 验证处刑者之斧是否带着“流血”Buff，且等级为1
            var axeBuff = axe.WeaponBuffs.FirstOrDefault(wb => wb.Buff.Name == "流血");
            Assert.NotNull(axeBuff);
            Assert.Equal(1, axeBuff.Level);
            Assert.Equal(0.35, axeBuff.Buff.CoefficientStrength);

            // 5. 验证技能逻辑
            // 战士应该有“战斗怒吼”
            var roar = kael.Skills.FirstOrDefault(s => s.Name == "战斗怒吼");
            Assert.NotNull(roar);
            // 战斗怒吼应该有两个 Buff：强壮、铁壁意志
            Assert.Equal(2, roar.SkillBuffs.Count);
            Assert.Contains(roar.SkillBuffs, sb => sb.Buff.Name == "强壮");
            Assert.Contains(roar.SkillBuffs, sb => sb.Buff.Name == "铁壁意志");

            // 6. 验证“凡人_艾里斯”的全能性
            var aris = db.Users
                .Include(u => u.Weapons)
                .Include(u => u.Skills)
                .FirstOrDefault(u => u.Name == "凡人_艾里斯");

            Assert.NotNull(aris);
            // 凡人拥有全部12把武器
            Assert.Equal(12, aris.Weapons.Count);
        }
    }
}