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
        #region buffSeed
        public static Buff Bleeding = new Buff("Bleeding",2, false, 1.0, 1.0, 0, 0.35, 0, "Bleed");
        public static Buff IronWill = new Buff("IronWill", 3, true, 1.0, 0.8, 0, 0, 0, "Defense");
        public static Buff ArmorCrush = new Buff("ArmorCrush", 2, false, 1.0, 1.2, 0, 0.15, 0, "Break");
        public static Buff Poison = new Buff("Poison", 3, false, 1.0, 1.0, 0.25, 0, 0, "Poison");
        public static Buff Adrenaline = new Buff("Adrenaline", 2, true, 1.2, 1.1, 0.1, 0, 0, "Burst");
        public static Buff EagleEye = new Buff("EagleEye", 3, true, 1.15, 1.0, 0, 0, 0, "Focus");
        public static Buff Ignite = new Buff("Ignite", 3, false, 1.0, 1.0, 0, 0, 0.30, "Fire");
        public static Buff ArcaneFocus = new Buff("ArcaneFocus", 2, true, 1.2, 0.95, 0, 0, 0.05, "MagicUp");
        public static Buff Frozen = new Buff("Frozen", 1, false, 0.8, 1.2, 0, 0, 0.10, "Freeze");
        public static Buff Weak = new Buff("Weak", 2, false, 0.85, 1.15, 0, 0, 0, "Normal");
        public static Buff Strong = new Buff("Strong", 3, true, 1.1, 0.9, 0, 0, 0, "Normal");
        public static Buff Regrow = new Buff("Regrow", 3, true, 1.0, 0.95, 0.04, 0.04, 0.04, "Regrow");

  
        #endregion

        #region WeaponSeed
        static Weapon ExecutionerAxe = new Weapon(
            "Executioner's Axe", 0.2, 2.5, 0.3,
            new List<Buff> { new Buff(Bleeding) }
        );
        static Weapon TowerShieldMace = new Weapon(
            "Tower Shield & Mace", 0.1, 2.2, 0.7,
            new List<Buff> { new Buff(IronWill) }
        );
        static Weapon RecurveLongbow = new Weapon(
            "Recurve Longbow", 2.5, 0.4, 0.1,
            new List<Buff> { new Buff(EagleEye) }
        );
        static Weapon TwinDaggers = new Weapon(
            "Shadow Daggers", 2.1, 0.5, 0.4,
            new List<Buff> { new Buff(Adrenaline) }
        );
        static Weapon ArchmageStaff = new Weapon(
            "Archmage Staff", 0.1, 0.4, 2.5,
            new List<Buff> { new Buff(ArcaneFocus) }
        );
        static Weapon RunicSpellblade = new Weapon(
            "Runic Spellblade", 1.0, 0.5, 1.5,
            new List<Buff> { new Buff(Ignite) }
        );

        static Weapon GuardiansHalberd = new Weapon(
            "Guardian's Halberd", 0.9, 1.2, 0.9,
            new List<Buff> { new Buff(Strong), new Buff(Regrow) },
            "Universal"
        );
        static Weapon HeavyCrossbow = new Weapon(
            "Heavy Crossbow", 1.1, 1.1, 0.8,
            new List<Buff> { new Buff(Weak) },
            "Universal"
        );
        static Weapon ScoutShortsword = new Weapon(
            "Scout's Shortsword", 1.2, 0.9, 0.9,
            new List<Buff> { new Buff(Strong) },
            "Universal"
        );
        static Weapon HeavyClub = new Weapon(
            "Heavy Club", 0.8, 1.1, 1.1,
            new List<Buff>(),
            "Universal"
        );
        static Weapon BalancedQuarterstaff = new Weapon(
            "Balanced Quarterstaff", 1.0, 1.0, 1.0,
            new List<Buff>(),
            "Universal"
        );
        static Weapon EtchedDagger = new Weapon(
            "Etched Dagger", 1.1, 0.9, 1.0,
            new List<Buff>(),
            "Universal"
        );
        #endregion

        #region SkillSeed
        //PassiveSkills
        static Skill PretendDeath = new Skill("PretendDeath", true, 0, 0, 0, new List<Buff>());
        static Skill UndeadWilling = new Skill("UndeadWilling", true, 0, 0, 0, new List<Buff>());
        //WarriorSkills
        static Skill GroundSlam = new Skill("Ground Slam", false, 0.2, 2.5, 0.3, new List<Buff> { new Buff(Weak) });
        static Skill BattleCry = new Skill("Battle Cry", false, 0.4, 1.3, 1.3, new List<Buff> { new Buff(Strong), new Buff(IronWill) });
        //RangerSkills
        static Skill SonicStab = new Skill("Sonic Stab", false, 2.5, 0.3, 0.2, new List<Buff> { new Buff(Bleeding) });
        static Skill Vanish = new Skill("Vanish", false, 1.5, 0.5, 1.0, new List<Buff> { new Buff(EagleEye), new Buff(Adrenaline) });
        //MagicianSkills
        static Skill Fireball = new Skill("Fireball", false, 0.0, 0.0, 3.5, new List<Buff> { new Buff(Ignite) });
        static Skill FrostNova = new Skill("Frost Nova", false, 0.5, 0.0, 3.0, new List<Buff> { new Buff(Frozen) });
        static Skill Torture = new Skill("Torture", false, 0.0, 0.0, 1.8, new List<Buff> { }, CommonData.SkillTagTorture);
        //CommonSkills
        static Skill EchoStrike = new Skill(
            "Echo Strike",
            false,
            1.2, 1.0, 0.9,
            new List<Buff> { new Buff(Strong) }
        );
        static Skill ShieldBash = new Skill(
            "Shield Bash",
            false,
            0.9, 1.2, 1.0,
            new List<Buff> { new Buff(ArmorCrush) }
        );
        static Skill SoulBind = new Skill(
            "Soul Bind",
            false,
            1.0, 0.9, 1.2,
            new List<Buff> { new Buff(Weak) }
        );
        #endregion


        public Fighter Fighter_Ranger;
        public Fighter Fighter_Warrior;
        public Fighter Fighter_Mortal;
        public Fighter Fighter_Magician;


        public SeedData()
        {

            var commonSkill = new List<Skill> { EchoStrike, ShieldBash, SoulBind };
            var warriorSkill = commonSkill.Concat(new List<Skill> { GroundSlam, BattleCry, UndeadWilling }).ToList();
            var rangerSkill = commonSkill.Concat(new List<Skill> { SonicStab, Vanish, PretendDeath }).ToList();
            var magicianSkill = commonSkill.Concat(new List<Skill> { Fireball, FrostNova, Torture, PretendDeath}).ToList();
            var tempSkill = commonSkill.Concat(warriorSkill).Concat(rangerSkill).Concat(magicianSkill);
            var mortalSkill = tempSkill.Distinct().ToList();

            // --- 1. Warrior: Specialized in Strength and survivability ---
            Fighter_Warrior = new Warrior(
                name: "Warrior_Kael",
                health: 840,
                agility: 12,
                strength: 55,
                intelligence: 10,
                buffStatuses: new List<BuffStatus>(),
                weapons: new List<Weapon>
                {
            new Weapon(ExecutionerAxe),
            new Weapon(TowerShieldMace),
            new Weapon(GuardiansHalberd),
            new Weapon(HeavyCrossbow),
            new Weapon(ScoutShortsword),
            new Weapon(HeavyClub),
            new Weapon(BalancedQuarterstaff),
            new Weapon(EtchedDagger)
                },warriorSkill
            );

            // --- 2. Ranger: Specialized in Agility and speed ---
            Fighter_Ranger = new Ranger(
                name: "Ranger_Lyra",
                health: 320,
                agility: 58,
                strength: 15,
                intelligence: 15,
                buffStatuses: new List<BuffStatus>(),
                weapons: new List<Weapon>
                {
            new Weapon(RecurveLongbow),
            new Weapon(TwinDaggers),
            new Weapon(GuardiansHalberd),
            new Weapon(HeavyCrossbow),
            new Weapon(ScoutShortsword),
            new Weapon(HeavyClub),
            new Weapon(BalancedQuarterstaff),
            new Weapon(EtchedDagger)
                },rangerSkill
            );

            // --- 3. Magician: Specialized in Intelligence and magic ---
            Fighter_Magician = new Magician(
                name: "Mage_Xan",
                health: 620,
                agility: 18,
                strength: 10,
                intelligence: 60,
                buffStatuses: new List<BuffStatus>(),
                weapons: new List<Weapon>
                {
            new Weapon(ArchmageStaff),
            new Weapon(RunicSpellblade),
            new Weapon(GuardiansHalberd),
            new Weapon(HeavyCrossbow),
            new Weapon(ScoutShortsword),
            new Weapon(HeavyClub),
            new Weapon(BalancedQuarterstaff),
            new Weapon(EtchedDagger)
                },magicianSkill
            );

            // --- 4. Mortal: The Jack of All Trades (Balanced Stats) ---
            Fighter_Mortal = new Mortal(
                name: "Mortal_Aris",
                health: 650,
                agility: 31,
                strength: 33,
                intelligence: 30,
                buffStatuses: new List<BuffStatus>(),
                weapons: new List<Weapon>
                { 
            // All 6 Specialized + 6 Universal
            new Weapon(ExecutionerAxe), new Weapon(TowerShieldMace),
            new Weapon(RecurveLongbow), new Weapon(TwinDaggers),
            new Weapon(ArchmageStaff), new Weapon(RunicSpellblade),
            new Weapon(GuardiansHalberd), new Weapon(HeavyCrossbow),
            new Weapon(ScoutShortsword), new Weapon(HeavyClub),
            new Weapon(BalancedQuarterstaff), new Weapon(EtchedDagger)
                },mortalSkill
            );
        }

    }
}
