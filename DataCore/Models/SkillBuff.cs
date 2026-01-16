using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class SkillBuff
    {
        public int SkillId { get; set; }
        public Skill Skill { get; set; } = default!;

        public int BuffId { get; set; }
        public Buff Buff { get; set; } = default!;

        // 你的特殊属性
        public int Level { get; set; }
    }
}
