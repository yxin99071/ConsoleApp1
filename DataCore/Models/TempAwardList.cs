using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class TempAwardList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public List<Skill> Skills { get; set; } = new List<Skill>();
        public int AwardLevel { get; set; }
    }
}
