using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Models
{
    public class LevelConfiguration
    {
        public double EXP_A { get; set; }  // 二次项系数
        public double EXP_B { get; set; }  // 一次项系数
        public double BASE_WIN_EXP { get; set; } // 基础战斗经验
        public double LVL_EXP_STEP { get; set; }  // 每级敌人增加的经验收益
        public double FAIL_EXP_RATE { get; set; }// 失败获得的经验比例
    }
}
