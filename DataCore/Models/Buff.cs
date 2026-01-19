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

        public Buff Clone()
        {
            return new Buff
            {
                Id = this.Id,
                Name = this.Name,
                LastRound = this.LastRound,
                DirectDamage = this.DirectDamage,
                DamageCorrection = this.DamageCorrection,
                WoundCorrection = this.WoundCorrection,
                IsOnSelf = this.IsOnSelf,
                CoefficientAgility = this.CoefficientAgility,
                CoefficientStrength = this.CoefficientStrength,
                CoefficientIntelligence = this.CoefficientIntelligence,

                // 列表必须 new，防止引用污染
                SpecialTag = this.SpecialTag?.ToList() ?? new(),
                WeaponBuffs = this.WeaponBuffs?.Select(wb => wb.Clone()).ToList() ?? new(),
                SkillBuffs = this.SkillBuffs?.Select(sb => sb.Clone()).ToList() ?? new()
            };
        }
    }
}
