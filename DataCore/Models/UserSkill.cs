using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class UserSkill
    {
        public int UserId { get; set; }
        public int Skill { get; set; }
        public int Count { get; set; } // 数量
    }
}
