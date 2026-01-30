using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class UserWeapon
    {
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        public int WeaponId { get; set; }
        public Weapon Weapon { get; set; } = default!;
        public int Count { get; set; } = 1;// 数量
        public UserWeapon Clone()
        {
            return new UserWeapon
            {
                UserId = this.UserId,
                Weapon = this.Weapon.Clone(),
                Count = this.Count
            };
        }
    }
}
