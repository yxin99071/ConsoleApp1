using DataCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataCore.Data  
{
    public class BattleDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Buff> Buffs { get; set; }
        public DbSet<TempAwardList> TempAwardLists { get; set; }
        public DbSet<BattleRecord> BattleRecords { get; set; }

        public BattleDbContext(DbContextOptions<BattleDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. 处理 Tags 的转换 (你原有的代码)
            modelBuilder.Entity<Weapon>()
                .Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<Skill>()
                .Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<Buff>()
                .Property(e => e.SpecialTag)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<UserWeapon>()
                .HasKey(uw => new { uw.UserId, uw.WeaponId });

            // 4. 配置 Weapon <-> Buff (显式中间表，带 Level)
            modelBuilder.Entity<WeaponBuff>()
                .HasKey(wb => new { wb.WeaponId, wb.BuffId }); // 复合主键

            modelBuilder.Entity<WeaponBuff>()
                .HasOne(wb => wb.Weapon)
                .WithMany(w => w.WeaponBuffs)
                .HasForeignKey(wb => wb.WeaponId);

            modelBuilder.Entity<WeaponBuff>()
                .HasOne(wb => wb.Buff)
                .WithMany(b => b.WeaponBuffs)
                .HasForeignKey(wb => wb.BuffId);

            // 5. 配置 Skill <-> Buff (显式中间表，带 Level)
            modelBuilder.Entity<SkillBuff>()
                .HasKey(sb => new { sb.SkillId, sb.BuffId });

            modelBuilder.Entity<SkillBuff>()
                .HasOne(sb => sb.Skill)
                .WithMany(s => s.SkillBuffs)
                .HasForeignKey(sb => sb.SkillId);

            modelBuilder.Entity<SkillBuff>()
                .HasOne(sb => sb.Buff)
                .WithMany(b => b.SkillBuffs)
                .HasForeignKey(sb => sb.BuffId);

            modelBuilder.Entity<TempAwardList>(entity =>
            {
                // 设置自增主键
                entity.HasKey(t => t.Id);

                // 配置多对多关系（EF Core 会自动创建中间表）
                entity.HasMany(t => t.Weapons)
                      .WithMany(); // 不需要 Weapon 类中有 List<TempAwardList>

                entity.HasMany(t => t.Skills)
                      .WithMany();
            });

            // --- UserWeapon 配置 ---
            modelBuilder.Entity<UserWeapon>(entity =>
            {
                // 1. 定义复合主键 (由用户ID和武器ID共同组成)
                entity.HasKey(uw => new { uw.UserId, uw.WeaponId });

                // 2. 配置 User -> UserWeapon (1:N)
                entity.HasOne(uw => uw.User)
                      .WithMany(u => u.UserWeaponLinks)
                      .HasForeignKey(uw => uw.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // 删除玩家时，自动删除其武器拥有记录

                // 3. 配置 Weapon -> UserWeapon (1:N)
                entity.HasOne(uw => uw.Weapon)
                      .WithMany(w => w.UserWeaponLink) // 对应你 Weapon 类里的属性名
                      .HasForeignKey(uw => uw.WeaponId)
                      .OnDelete(DeleteBehavior.Cascade); // 删除武器模板时，自动删除所有玩家拥有的该武器记录

                // 4. 设置 Count 默认值
                entity.Property(uw => uw.Count).HasDefaultValue(1);
            });

            // --- UserSkill 配置 ---
            modelBuilder.Entity<UserSkill>(entity =>
            {
                // 复合主键
                entity.HasKey(us => new { us.UserId, us.SkillId });

                // 用户 -> 技能关联
                entity.HasOne(us => us.User)
                      .WithMany(u => u.UserSkillLinks) // 确保 User 类中有 List<UserSkill> UserSkillLinks
                      .HasForeignKey(us => us.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 技能 -> 用户关联
                entity.HasOne(us => us.Skill)
                      .WithMany() // 如果 Skill 类里没写 List<UserSkill>，这里留空
                      .HasForeignKey(us => us.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(us => us.Count).HasDefaultValue(1);
            });


        }
    }
}
