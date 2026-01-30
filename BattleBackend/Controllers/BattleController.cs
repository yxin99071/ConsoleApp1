using BattleBackend.DTOs;
using BattleBackend.Services;
using BattleCore;
using DataCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BattleBackend.Controllers
{
    [Route("battle")]
    public class BattleController : Controller
    {
        private readonly JwtService _jwtService;
        private readonly BattleService _battleService;
        //对局目录

        // 通过构造函数注入
        public BattleController(JwtService jwtService, BattleService battleService)
        {
            _jwtService = jwtService;
            _battleService = battleService;
        }
        
        [HttpGet("GetWeaponAward")]
        [Authorize]
        public async Task<IActionResult> GetWeaponAward()
        {
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id))
            {
                var Awards =await _battleService.GetAwardsList(id);
                return Ok(Awards);

            }
            return BadRequest("找不到Id");
        }
        [HttpPost("fight")]
        [Authorize]
        public async Task<IActionResult> Fight([FromBody]FightRequestDto fightRequestDto)
        {
            
            if (fightRequestDto.history == null)
            {
                if (fightRequestDto.attacker != null && fightRequestDto.defender != null)
                {
                    if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id)
                        && int.TryParse(fightRequestDto.defender,out int enemyId))
                    {
                        await _battleService.ExecuteFight(id, enemyId);//json
                        var jsonEvents = JsonLogger.GetEvents();
                        return Ok(jsonEvents);
                    }

                }
            }
            //todo查找历史对局

            return BadRequest("无法战斗");
        }
        [HttpGet("battlelist")]
        [Authorize]
        public async Task<IActionResult> GetBattleList([FromBody]int id)
        {
            var history = _battleService.GetBattleRecordListAsync(id);
            return Ok(history);
        }
        [HttpGet("replay")]
        public async Task<IActionResult> GetReplay([FromBody]int id)
        {
            // 1. 从数据库获取元数据
            var recordJson = await _battleService.GetBattleRecordByIdAsync(id);
            if (recordJson.IsNullOrEmpty())
                return BadRequest("can't find battle file");
            return Content(recordJson, "application/json");
        }

    }
}
