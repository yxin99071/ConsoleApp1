using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public static class CommonData
    {
        public static readonly string FireDamage = "FireDamage";
        public static readonly string IceDamage = "IceDamage";
        public static readonly string GroundDamage = "GroundDamage";
        public static readonly string WindDamage = "WindDamage";
        public static readonly string NormalDamage = "NormalDamage";


        public static readonly string UnDodgeable = "UnDogeable";
        public static readonly string UnFightBackable = "UnFightBackable";

        public static readonly List<Buff> BuffPools = [new Buff("Bleed", 2, false, coefficientAgility: 0.3)
            ,new Buff("Weak", 2, false, 0.7, 1.1)
            ,new Buff("Strong", 2, true, 1.2,0.8)];
    }
}
