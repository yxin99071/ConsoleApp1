using BattleCore.BattleEvnetArgs;
using BattleCore.DataModel.States;
using DataCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel.Fighters
{
    public abstract class Fighter
    {
        public Fighter(User user)
        {
            Id = user.Id;
            Name = user.Name;
            Health = user.Health;
            MaxHealth = user.Health;
            Agility = user.Agility;
            Strength = user.Strength;
            BuffStatuses = new List<BuffStatus>();
            Intelligence = user.Intelligence;
            Weapons = user.Weapons;
            Skills = user.Skills;
        }
        public readonly int Id;
        public readonly double MaxHealth;
        public string Name { get; set; }
        public double Health { get; set; }
        public double Agility { get; set; }
        public string Profession { get; set; } = default!;
        public double Strength { get; set; }
        public double Intelligence { get; set; }
        public int SpeedBar { get; set; }
        public int Max_SpeedBar { get; set; }
        public List<BuffStatus> BuffStatuses { get; set; } = new List<BuffStatus>();
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public List<Skill> Skills { get; set; } = new List<Skill>();
        public bool IsDead { get; set; } = false;
        

        public event EventHandler<CauseDamageEventArgs>? CauseDamageEA;
        public event EventHandler<TakeDamageEventArgs>? TakeDamageEA;
        public event EventHandler<LoadBuffEventArgs>? LoadBuffEA;

        public virtual void CauseDamage(DamageInfo damageInfo)
        {
            CauseDamageEA?.Invoke(this, new CauseDamageEventArgs(damageInfo));
        }
        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            TakeDamageEA?.Invoke(this, new TakeDamageEventArgs(damageInfo));  
        }
        public virtual void LoadBuff(Buff buff,Fighter? source,int buffLevel = 1)
        {
            LoadBuffEA?.Invoke(this, new LoadBuffEventArgs(buff,source,buffLevel));
        }
        public abstract void SetFitDamage(DamageInfo damageInfo);  
        
        public void ClearAllEA()
        {
            CauseDamageEA = null;
            TakeDamageEA = null;
            LoadBuffEA = null;
        }


    }
}
