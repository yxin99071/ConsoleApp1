using BattleCore.EntityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BattleCore.DataModel
{
    public class Buff
    {
        public Buff(string name, int lastRound,
            bool isOnSelf,
            double damageCorrection = 1,
            double woundCorrection = 1,
            double coefficientAgility = 0,
            double coefficientStrength = 0,
            string specialTag = "Normal")
        {
            Name = name;
            LastRound = lastRound;
            IsOnSelf = isOnSelf;
            DamageCorrection = damageCorrection;
            WoundCorrection = woundCorrection;
            CoefficientAgility = coefficientAgility;
            CoefficientStrength = coefficientStrength;
            SpecialTag = specialTag;
        }
        public Buff(Buff other)
        {
            Name = other.Name;
            LastRound = other.LastRound;
            IsOnSelf = other.IsOnSelf;
            DamageCorrection = other.DamageCorrection;
            WoundCorrection = other.WoundCorrection;
            SpecialTag = other.SpecialTag;
            CoefficientAgility = other.CoefficientAgility;
            CoefficientStrength = other.CoefficientStrength;
        }

        public string Name { get; set; }
        public int LastRound { get; set; }
        public double DirectDamage { get; set; } = 0;
        public double DamageCorrection { get; set; }
        public double WoundCorrection { get; set; }
        public bool IsOnSelf { get; set; } = false;
        public string SpecialTag { get; set; }
        public bool IsNew { get; set; } = true;
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }

    }
}
