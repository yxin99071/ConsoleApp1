using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataCore.Models
{
    public class Buff
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int LastRound { get; set; }
        public double DirectDamage { get; set; }
        public double DamageCorrection { get; set; }
        public double WoundCorrection { get; set; }
        public bool IsOnSelf { get; set; } = false;
        public List<String> SpecialTag { get; set; } = default!;
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }
        public List<WeaponBuff> WeaponBuffs { get; set; } = new();
        public List<SkillBuff> SkillBuffs { get; set; } = new();
    }
}
