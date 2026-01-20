using DataCore.Models;


namespace BattleCore.DataModel.States
{
    public class DamageDetail
    {
        public string DamageType;
        public string DirectSource;
        public double Damage;
        public List<Buff> buffs = new List<Buff>();
    }
}
