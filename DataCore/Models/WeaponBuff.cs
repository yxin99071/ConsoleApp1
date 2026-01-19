using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class WeaponBuff
    {
        public int WeaponId { get; set; }
        public Weapon Weapon { get; set; } = default!;

        public int BuffId { get; set; }
        public Buff Buff { get; set; } = default!;

        // 你的特殊属性
        public int Level { get; set; }

        public WeaponBuff Clone()
        {
            return new WeaponBuff
            {
                Level = this.Level
                // 注意：导航属性通常不需要在 Clone 中手动赋值，
                // 除非你在克隆后立刻需要通过 wb.Buff 访问数据。
                // 在单向查询中，通常由上层（Weapon/Skill）来重新分配这些关系。
            };
        }
    }
}
