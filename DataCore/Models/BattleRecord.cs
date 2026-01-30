using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class BattleRecord
    {
        public int Id { get; set; }

        // 关联用户
        public string WinnerId { get; set; } = string.Empty;
        public string LoserId { get; set; } = string.Empty;

        // 基础信息
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        // 物理存储信息
        public string JsonFileName { get; set; } = string.Empty; // 存储文件名，如: record_20260130.json
    }
}
