
namespace DataCore.Models
{
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }
        public List<string> Tags { get; set; } = default!;
        public List<WeaponBuff> WeaponBuffs { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }

}
