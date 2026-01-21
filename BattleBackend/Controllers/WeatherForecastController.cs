using BattleCore;
using BattleCore.BattleLogic;
using BattleCore.DataModel;
using BattleCore.DataModel.Fighters;
using DataCore.Services;
using Microsoft.AspNetCore.Mvc;
namespace BattleBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await StaticData.InitializeAsyncData();
            var BtData = new BattleDataBridge();

            var fighter_1 = await BtData.ConvertUserToFighter(1);
            var fighter_2 = await BtData.ConvertUserToFighter(2);
            var fighter_3 = await BtData.ConvertUserToFighter(3);
            var fighter_4 = await BtData.ConvertUserToFighter(4);

            if (fighter_1 != null && fighter_2 != null && fighter_3 != null && fighter_4 != null)
            {
                BattleManager.Initial(new List<Fighter> { fighter_1, fighter_2});
                await BattleManager.BattleSimulation(fighter_1, fighter_2);
            }

            return Content(JsonLogger.GetJson(), "application/json");
        }



    }
}
