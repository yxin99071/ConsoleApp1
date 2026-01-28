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
    }
}
