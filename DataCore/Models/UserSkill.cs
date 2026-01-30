using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class UserSkill
    {
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        public int SkillId { get; set; }
        public Skill Skill { get; set; } = default!;
        public int Count { get; set; } = 1;// 数量
        public UserSkill Clone()
        {
            return new UserSkill
            {
                UserId = this.UserId,
                Skill = this.Skill.Clone(),
                Count = this.Count
            };
        }
    }
}
