using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public class Weapon
    {
        public Weapon(string name
            , double coefficientAgility
            , double coefficientStrength
            ,double coefficientIntelligence
            , List<Buff> buffs
            ,String tags = "Normal")
        {
            Name = name;
            CoefficientAgility = coefficientAgility;
            CoefficientStrength = coefficientStrength;
            CoefficientIntelligence = coefficientIntelligence;
            Buffs = buffs;
            Tags = tags.Split(',').ToList();

        }

        public string Name { get; set; }
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }
        public List<Buff> Buffs { get; set; } = new List<Buff>();
        public List<string> Tags { get; set; }

    }
}
