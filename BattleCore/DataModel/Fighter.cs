using BattleCore.BattleEvnetArgs;
using BattleCore.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.EntityObjects
{
    public class Fighter
    {
        public Fighter(string name, double health, double agility, double strength, List<BuffStatus> buffStatuses, List<Weapon> weapons)
        {
            Name = name;
            Health = health;
            Agility = agility;
            Strength = strength;
            BuffStatuses = buffStatuses;
            Weapons = weapons;
        }

        public string Name { get; set; }
        public double Health { get; set; }
        public double Agility { get; set; }
        public double Strength { get; set; }
        public int SpeedBar { get; set; }
        public List<BuffStatus> BuffStatuses { get; set; } = new List<BuffStatus>();
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        

        public event EventHandler<CauseDamageEventArgs>? CauseDamageEA;
        public event EventHandler<TakeDamageEventArgs>? TakeDamageEA;
        public event EventHandler<LoadBuffEventArgs>? LoadBuffEA;

        public void CauseDamage(DamageInfo damageInfo)
        {
            CauseDamageEA?.Invoke(this, new CauseDamageEventArgs(damageInfo));
        }
        public void TakeDamage(DamageInfo damageInfo)
        {
            TakeDamageEA?.Invoke(this, new TakeDamageEventArgs(damageInfo));  
        }
        public void LoadBuff(Buff buff,Fighter? source)
        {
            var newBuff = new Buff(buff);
            if (source is not null)
            {
                if (newBuff.CoefficientStrength > 0)
                    newBuff.DirectDamage = newBuff.CoefficientStrength * source.Strength;
                if (newBuff.CoefficientAgility > 0)
                    newBuff.DirectDamage = newBuff.CoefficientAgility * source.Agility;
            }
            

            LoadBuffEA?.Invoke(this, new LoadBuffEventArgs(newBuff));
        }
        

    }
}
