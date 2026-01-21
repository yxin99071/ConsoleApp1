using BattleLogic;
using BattleLogic.DataModel;
using BattleLogic.DataModel.Fighters;
using DataCore.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static BattleLogic.JsonLogger;


namespace BattleTestProject
{
    public class UnitTest1
    {
        
        [Fact]
        public async Task GetBuffTools_ShouldReturnData()
        {
            // Act
            var buffs = await BattleDataBridge.GetBuffTools();

            // Assert
            Assert.NotNull(buffs);
            Assert.True(buffs.Count >= 12); // 我们之前定义了12个
            Assert.Contains(buffs, b => b.Name == "BLEED");
        }
        
    }
}