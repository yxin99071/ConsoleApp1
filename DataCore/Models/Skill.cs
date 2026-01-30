using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class Skill:Item
    {
        public bool IsPassive { get; set; }
        public List<SkillBuff> SkillBuffs { get; set; } = new();
        public List<UserSkill> UserSkillLink { get; set; } = new();
        public Skill Clone()
        {
            return new Skill
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Profession = this.Profession,
                SecondProfession = this.SecondProfession,
                CoefficientAgility = this.CoefficientAgility,
                CoefficientStrength = this.CoefficientStrength,
                CoefficientIntelligence = this.CoefficientIntelligence,
                RareLevel = this.RareLevel,
                Tags = this.Tags?.ToList() ?? new(),

                // 重要：深拷贝 Buff，保证每个玩家拿到的武器 Buff 状态独立
                SkillBuffs = this.SkillBuffs?.Select(wb => wb.Clone()).ToList() ?? new(),

                // 重要：浅拷贝 User 列表（仅复制引用）
                // 因为查询是 玩家 -> 武器，武器不需要“拥有”全新的玩家副本
                UserSkillLink = new List<UserSkill>()
            };
        }
    }
}
