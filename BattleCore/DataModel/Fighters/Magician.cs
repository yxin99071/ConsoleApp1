using DataCore.Models;


namespace BattleCore.DataModel.Fighters
{
    public class Magician : Fighter
    {
        public Magician(User user) : base(user)
        {
            Profession = "Magician";
            //法师开局有护盾
            BuffStatuses.Add(new BuffStatus(
                new Buff {Name="FakeHealth", LastRound=2,IsOnSelf= true, DamageCorrection = 1.2,
                    SpecialTag= (Intelligence * 2.5).ToString().Split(',').ToList()},this,null
                ));
        }

        public override void SetFitDamage(DamageInfo damageInfo)
        {
            damageInfo.Damage += Intelligence * 1.0;
            //添加buff
            Random random = new Random();
            var buff = StaticData.BuffPool[random.Next(0, StaticData.BuffPool.Count)];
            if (buff.IsOnSelf)
                this.LoadBuff(buff, null);
            else
                damageInfo.Target.LoadBuff(buff, this);
        }
        public override void LoadBuff(Buff buff, Fighter? source)
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
            
            base.LoadBuff(newBuff, source);
        }

    }
}
