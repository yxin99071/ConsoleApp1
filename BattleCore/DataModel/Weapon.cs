using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public class Weapon
    {
        public Weapon(string name, double coefficientAgility, double coefficientStrength, List<Buff> buffs)
        {
            Name = name;
            CoefficientAgility = coefficientAgility;
            CoefficientStrength = coefficientStrength;
            Buffs = buffs;
        }

        public string Name { get; set; }
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public List<Buff> Buffs { get; set; } = new List<Buff>();


    }
}
