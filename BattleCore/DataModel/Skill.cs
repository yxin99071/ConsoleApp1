using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.DataModel
{
    public class Skill
    {
        public Skill(string name, bool isPassive
            , double coefficientAgility, double coefficientStrength, double coefficientIntelligence
            , List<Buff> buffs, String? tags = null)
        {
            Name = name;
            IsPassive = isPassive;
            CoefficientAgility = coefficientAgility;
            CoefficientStrength = coefficientStrength;
            CoefficientIntelligence = coefficientIntelligence;
            Buffs = buffs;
            Tags = tags is null?new List<string>():tags.Split(",").ToList();
        }

        public Skill(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            // 复制简单类型和字符串（immutable）
            Name = skill.Name;
            IsPassive = skill.IsPassive;
            CoefficientAgility = skill.CoefficientAgility;
            CoefficientStrength = skill.CoefficientStrength;
            CoefficientIntelligence = skill.CoefficientIntelligence;

            // 深拷贝 Buffs 列表（逐项复制）
            Buffs = new List<Buff>();
            if (skill.Buffs != null)
            {
                for (int i = 0; i < skill.Buffs.Count; i++)
                {
                    var src = skill.Buffs[i];
                    Buffs.Add(new Buff(src));
                }
            }

            // 深拷贝 Tags 列表（逐项复制字符串）
            Tags = new List<string>();
            if (skill.Tags != null)
            {
                for (int i = 0; i < skill.Tags.Count; i++)
                {
                    Tags.Add(skill.Tags[i]);
                }
            }
        }


        public string Name { get; set; }
        public bool IsPassive { get; set; }
        public double CoefficientAgility { get; set; }
        public double CoefficientStrength { get; set; }
        public double CoefficientIntelligence { get; set; }
        public List<Buff> Buffs { get; set; } = new List<Buff>();
        public List<string> Tags { get; set; }

    }
}
