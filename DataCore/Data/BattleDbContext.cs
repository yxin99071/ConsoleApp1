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

            // 2. 配置 User <-> Weapon (纯多对多，EF Core 会自动生成中间表 UserWeapon)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Weapons)
                .WithMany(w => w.Users);

            // 3. 配置 User <-> Skill (纯多对多，EF Core 会自动生成中间表 UserSkill)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Skills)
                .WithMany(s => s.Users);

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

            modelBuilder.Entity<User>()
                .HasMany(u => u.Weapons)
                .WithMany(w => w.Users)
                .UsingEntity(j => j.ToTable("UserWeapons")); // 显式命名中间表

            modelBuilder.Entity<User>()
                .HasMany(u => u.Skills)
                .WithMany(s => s.Users)
                .UsingEntity(j => j.ToTable("UserSkills"));

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
        }
    }
}
