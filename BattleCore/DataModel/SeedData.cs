using BattleCore.DataModel.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public class SeedData
    {
        public Buff Bleed;
        public Buff Weak;
        public Buff Strong;

        public Weapon Sword;
        public Weapon Knife;
        public Weapon Machete;

        public Fighter Fighter_Agility;
        public Fighter Fighter_Strength;
        public Fighter Fighter_Balacne;
        public Fighter Fighter_Wisdom;

        public SeedData()
        {
            Fighter_Agility = new Ranger("Fighter_Agility", 51, 9, 5, 5, new List<BuffStatus>(),new List<Weapon> ());
            Fighter_Strength = new Warrior("Fighter_Strength", 72, 5, 9,5, new List<BuffStatus>(),new List<Weapon>());
            Fighter_Balacne = new Mortal("Fighter_Balacne", 64, 7, 6, 6 , new List<BuffStatus>(),new List<Weapon>());
            Fighter_Wisdom = new Magician("Fighter_Wisdom", 47, 5, 5, 9 , new List<BuffStatus>(),new List<Weapon>());

            Bleed = new Buff("Bleed", 2, false,coefficientAgility:0.3);
            Weak = new Buff("Weak", 2, false, 0.9, 1.1);
            Strong = new Buff("Strong", 2, true, 1.1,0.9);

            Sword = new Weapon("Sword", 0.75, 0.75,0.3, new List<Buff> { Weak });
            Knife = new Weapon("Knife", 1.2, 0.3,0.3, new List<Buff> { Bleed });
            Machete = new Weapon("Machete", 0.3, 1.2, 0.3, new List<Buff> { Strong });

            Fighter_Agility.Weapons.Add(Knife);
            Fighter_Agility.Weapons.Add(Sword);
            Fighter_Strength.Weapons.Add(Sword);
            Fighter_Strength.Weapons.Add(Machete);
            Fighter_Balacne.Weapons.Add(Machete);
            Fighter_Balacne.Weapons.Add(Knife);
            Fighter_Balacne.Weapons.Add(Sword);



        }

    }
}
