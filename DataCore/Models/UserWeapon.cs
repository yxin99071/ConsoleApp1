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
        public int WeaponId { get; set; }
        public int Count { get; set; } // 数量
    }
}
