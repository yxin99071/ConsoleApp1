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
        /// <summary>
        /// Initializes a new instance of the combat Buff class.
        /// </summary>
        /// <param name="name">The display name of the buff.</param>
        /// <param name="lastRound">Duration in rounds (typically 1-3).</param>
        /// <param name="isOnSelf">Targeting logic: true if applied to the caster, false if applied to the enemy.</param>
        /// <param name="damageCorrection">Multiplier for outgoing damage (e.g., 1.2 for +20% damage).</param>
        /// <param name="woundCorrection">Multiplier for incoming damage (e.g., 1.1 for +10% damage taken).</param>
        /// <param name="coefficientAgility">DOT (Damage Over Time) scaling factor based on Agility.</param>
        /// <param name="coefficientStrength">DOT scaling factor based on Strength.</param>
        /// <param name="coefficientIntelligence">DOT scaling factor based on Intelligence.</param>
        /// <param name="specialTag">Tag for non-numerical logic (e.g., "Stun", "Freeze").</param>
        public Buff(string name, int lastRound,
            bool isOnSelf,
            double damageCorrection = 1,
            double woundCorrection = 1,
            double coefficientAgility = 0,
            double coefficientStrength = 0,
            double coefficientIntelligence = 0,
            string specialTag = "Normal")
        {
            Name = name;
            LastRound = lastRound;
            IsOnSelf = isOnSelf;
            DamageCorrection = damageCorrection;
            WoundCorrection = woundCorrection;
            CoefficientAgility = coefficientAgility;
            CoefficientStrength = coefficientStrength;
            CoefficientIntelligence = coefficientIntelligence;
            SpecialTag = specialTag.Split(",").ToList();
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
        public List<String> SpecialTag { get; set; } = new List<string>();
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }

    }
}
