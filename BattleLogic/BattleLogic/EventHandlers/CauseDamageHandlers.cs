using BattleCore.BattleEventArgs;
using BattleCore.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCore.BattleLogic.EventHandlers
{
    public static class CauseDamageHandlers
    {
        public static void CorrectDamageByBuff(object? sender, CauseDamageEventArgs e)
        {
            if (e.damageInfo.Source is null)
                return;
            if(e.damageInfo.Source!.BuffStatuses.Count>0)
            {
                foreach (var buffStatus in e.damageInfo.Source.BuffStatuses)
                {
                    e.damageInfo.Damage *= buffStatus.buff.DamageCorrection;
                }
            }
        }
        public static void CorrectDamageByCritical(object? sender, CauseDamageEventArgs e)
        {
            if (e.damageInfo.Source is null)
                return;
            Random random = new Random();
            int choice = random.Next(0, 101);
            if (choice <= e.damageInfo.Source.CraticalRate*100)
            {
                e.damageInfo.Damage *= e.damageInfo.Source.CraticalDamage;
                e.damageInfo.damageDetail.tags.Add(StaticData.CriticalDamage);
            }
        }
        public static void CorrectDamageByIncreasement(object? sender, CauseDamageEventArgs e)
        {
            if (e.damageInfo.Source is null)
                return;
            e.damageInfo.Damage *= e.damageInfo.Source.DamageInreasement;
        }

    }
}
