using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class TempAwardList
    {
        public int UserId { get; set; }
        public List<int> weaponIds { get; set; } = new List<int>();
        public List<int> skillIds { get; set; } = new List<int>();
        public int AwardLevel { get; set; }
    }
}
