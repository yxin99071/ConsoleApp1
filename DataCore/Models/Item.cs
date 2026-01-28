using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public abstract class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = "No Description";
        public string Profession { get; set; } = "GENERAL";
        public string? SecondProfession { get; set; }
        public int RareLevel { get; set; }
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }
        public List<string> Tags { get; set; } = default!;
        public List<User> Users { get; set; } = new();
    }
}
