using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsPassive { get; set; }
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }
        public List<string> Tags { get; set; } = default!;
        public List<SkillBuff> SkillBuffs { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}
