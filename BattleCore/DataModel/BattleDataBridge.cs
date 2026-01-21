
using BattleLogic.DataModel.Fighters;
using DataCore.Models;
using DataCore.Services;
using Buff = DataCore.Models.Buff;

namespace BattleLogic.DataModel
{
    public class BattleDataBridge
    {
        private static readonly DataService dataService = new DataService();
        /*
         * readonly dataservice
         */
        public async Task<Fighter?> ConvertUserToFighter(int userId)
        {
            var user = await dataService.GetUserById(userId);
            if (user is not null)
            {
                if (user.Profession == "MAGICIAN")
                    return new Magician(user);
                if (user.Profession == "WARRIOR")
                    return new Warrior(user);
                if (user.Profession == "RANGER")
                    return new Ranger(user);
                if (user.Profession == "MORTAL")
                    return new Mortal(user);
            }
            return null;
        }
        public static async Task<List<Buff>> GetBuffTools()
        {
            return await dataService.GetAllBuffs();
        }

        private static Buff? ExtractBuff(SkillBuff? skillBuff = null,WeaponBuff? weaponBuff = null)
        {
            Buff? newBuff = null;
            if(skillBuff!=null)
            {
                var baseBuff = skillBuff.Buff;
                var enhencement = skillBuff.Level * 0.07+1;
                newBuff = new Buff
                {
                    CoefficientAgility = baseBuff.CoefficientAgility * enhencement,
                    CoefficientStrength = baseBuff.CoefficientStrength * enhencement,
                    CoefficientIntelligence = baseBuff.CoefficientIntelligence * enhencement,
                    DamageCorrection = 1 + ((baseBuff.DamageCorrection - 1) * enhencement),
                    WoundCorrection = 1 + ((baseBuff.WoundCorrection - 1) * enhencement),

                    Id = baseBuff.Id,
                    Name = baseBuff.Name,
                    IsOnSelf = baseBuff.IsOnSelf,
                    LastRound = baseBuff.LastRound + (skillBuff.Level >= 3 ? 1 : 0),
                    SpecialTag = baseBuff.SpecialTag,
                    SkillBuffs = baseBuff.SkillBuffs,
                    WeaponBuffs = baseBuff.WeaponBuffs
                };
            }
            if (weaponBuff != null)
            {
                var baseBuff = weaponBuff.Buff;
                var enhencement = weaponBuff.Level * 0.08 + 1;
                newBuff = new Buff
                {
                    CoefficientAgility = baseBuff.CoefficientAgility * enhencement,
                    CoefficientStrength = baseBuff.CoefficientStrength * enhencement,
                    CoefficientIntelligence = baseBuff.CoefficientIntelligence * enhencement,
                    DamageCorrection = 1 + ((baseBuff.DamageCorrection - 1) * enhencement),
                    WoundCorrection = 1 + ((baseBuff.WoundCorrection - 1) * enhencement),

                    Id = baseBuff.Id,
                    Name = baseBuff.Name,
                    IsOnSelf = baseBuff.IsOnSelf,
                    LastRound = baseBuff.LastRound + (weaponBuff.Level == 3 ? 1 : 0),
                    SpecialTag = baseBuff.SpecialTag,
                    SkillBuffs = baseBuff.SkillBuffs,
                    WeaponBuffs = baseBuff.WeaponBuffs
                };

            }

            return newBuff;
        }
        public static List<Buff> ExtractBuffs(List<SkillBuff>? skillBuffs = null, List<WeaponBuff>? weaponBuffs = null)
        {
            List<Buff> EnhencedBuffs = new List<Buff>();
            if(skillBuffs != null)
            {
                foreach (var skillBuff in skillBuffs)
                {
                    var enhencedBuff = ExtractBuff(skillBuff);
                    if (enhencedBuff != null)
                        EnhencedBuffs.Add(enhencedBuff);
                }
            }
            if(weaponBuffs != null)
            {
                foreach (var weaponBuff in weaponBuffs)
                {
                    var enhencedBuff = ExtractBuff(null,weaponBuff);
                    if (enhencedBuff != null)
                        EnhencedBuffs.Add(enhencedBuff);
                }
            }
            return EnhencedBuffs;
        }

        //dataservice.GetBuffInfo => return List<Buff>
        
        //dataservice.GetWeaponInfo => return List<Weapon>



    }
}
