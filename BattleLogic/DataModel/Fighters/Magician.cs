using BattleCore;
using BattleCore.DataModel;
using BattleCore.DataModel.States;
using DataCore.Models;


namespace BattleCore.DataModel.Fighters
{
    public class Magician : Fighter
    {
        public Magician(User user) : base(user)
        {
            Profession = "MAGICIAN";
            //法师开局有护盾
            BuffStatuses.Add(new BuffStatus(
                new Buff {Name="FakeHealth", LastRound=2,IsOnSelf= true, DamageCorrection = 1.2,
                    SpecialTag= (Intelligence * 2.5).ToString().Split(',').ToList()},this,null
                ));
            DamageInreasement += StaticDataHelper.CalculateDamageIncreasement(Intelligence)/2;
        }

        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Intelligence * 1.0;
            //添加buff
            Random random = new Random();
            var buff_1 = StaticDataHelper.BuffPool[random.Next(0, StaticDataHelper.BuffPool.Count)].Clone();
            var buff_2 = StaticDataHelper.BuffPool[random.Next(0, StaticDataHelper.BuffPool.Count)].Clone();
            var buffs = StaticDataHelper.ExtractBuffs(new List<SkillBuff> { new SkillBuff { Buff = buff_1,Level=4 }, new SkillBuff { Buff = buff_2,Level=4 } });
            var detail = new DamageDetail
            {
                DamageType = StaticDataHelper.FistDamage,
                DirectSource = $"{this.Profession}'s Fist",
            };
            foreach(var buff in buffs)
            {
                if (buff.IsOnSelf)
                    this.LoadBuff(buff, null, 4);
                else
                    detail.buffs.Add(buff);
            }
            damageInfo.damageDetail = detail;

        }
        public override void LoadBuff(Buff buff, Fighter? source,int buffLevel)
        {
            var newBuff = buff.Clone();
            if (source is not null)
            {
                if (newBuff.CoefficientStrength > 0)
                    newBuff.DirectDamage = newBuff.CoefficientStrength * source.Strength;
                if (newBuff.CoefficientAgility > 0)
                    newBuff.DirectDamage = newBuff.CoefficientAgility * source.Agility;
            }
            //法师被动
            if (newBuff.DamageCorrection < 1 || newBuff.WoundCorrection > 1 || newBuff.DirectDamage > 0)
                { 
                newBuff.LastRound--;
                BattleLogger.PassiveSkillInvoke("法师的灵动");
                }
            if (newBuff.DamageCorrection > 1 || newBuff.WoundCorrection < 1)
                newBuff.LastRound++;
            
            base.LoadBuff(newBuff, source, buffLevel);
        }

    }
}
