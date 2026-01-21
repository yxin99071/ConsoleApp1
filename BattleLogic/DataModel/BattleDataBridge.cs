using BattleCore.BattleLogic;
using BattleCore.DataModel.Fighters;
using DataCore.Models;
using DataCore.Services;
using Buff = DataCore.Models.Buff;

namespace BattleCore.DataModel
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
        public static async Task<bool> SetBattleResult(int challengerId, int opponentId,bool isWin)
        {
            return false;
            Console.Write("");
            var challenger = await dataService.GetUserById(challengerId);
            var challenger_copy = await dataService.GetUserById(challengerId);
            var opponent = await dataService.GetUserById(opponentId);
            if (challenger is null || opponent is null)
                return false;
            //当前经验
            var exp = challenger.Exp + BattleManager.CalculateGainedExp(challenger.Level, opponent.Level, isWin);
            var finalLevel = challenger.Level;
            while (true)
            {
                // 计算下一级所需的总经验门槛
                int nextLevel = finalLevel + 1;
                double threshold = 50.0 * nextLevel * nextLevel + 50.0 * nextLevel;

                // 如果当前总经验达到了下一级的门槛，则等级自增
                if (exp >= threshold)
                {
                    finalLevel++;
                }
                else
                {
                    break; // 未达到门槛，退出循环
                }
            }
            
            if(BattleManager.LevelUp(challenger,finalLevel - challenger.Level))
            {
                //同步经验值
                challenger.Exp = exp;
                await dataService.UpdateSinlgeUser(challenger);
                //日志结算信息
            }
            JsonLogger.LogBattleEnd(challenger_copy!, challenger);
            return true;
        }
        
        //Change Account
        //Choose Skill
        //Choose Profession

        //dataservice.GetWeaponInfo => return List<Weapon>



    }
}
