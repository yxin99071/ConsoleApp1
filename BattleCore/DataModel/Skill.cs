using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public class Skill
    {
        public string Name { get; set; }
        public bool IsPassive { get; set; }
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }
        public string DamageType { get; set; }
        public List<Buff> Buffs { get; set; } = new List<Buff>();
        public List<string> Tags { get; set; }

    }
}
