using DataCore.Models;


namespace BattleCore.DataModel.States
{
    public class DamageDetail
    {
        public DamageDetail()
        {
            buffs =  new List<Buff>();
            DamageType = "UNKNOW";
            DirectSource = "UNKNOW";
            tags = ["Normal"];
        }
        public string DamageType;
        public string DirectSource;
        public List<Buff> buffs;
        public List<String> tags;
    }
}
