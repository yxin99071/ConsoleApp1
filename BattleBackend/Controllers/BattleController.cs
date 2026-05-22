using BattleBackend.DTOs;
using BattleBackend.Services;
using BattleCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


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
        
        [HttpGet("awards")]
        [Authorize]
        public async Task<IActionResult> GetAwards()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id))
                return BadRequest("找不到Id");
            return Ok(await _battleService.GetAwardsList(id));
        }

        [HttpGet("awards/count")]
        [Authorize]
        public async Task<IActionResult> GetAwardCount()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id))
                return BadRequest("找不到Id");
            return Ok(await _battleService.GetPendingAwardCount(id));
        }

        [HttpPost("awards/claim")]
        [Authorize]
        public async Task<IActionResult> ClaimAward([FromBody] ClaimAwardDto dto)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                return BadRequest("无法识别用户");
            var success = await _battleService.ClaimAward(userId, dto.AwardListId, dto.ItemId);
            return success ? Ok() : BadRequest("领取失败：奖励不存在或已过期");
        }

        // 保留旧路由兼容
        [HttpGet("GetWeaponAward")]
        [Authorize]
        public async Task<IActionResult> GetWeaponAward()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id))
                return BadRequest("找不到Id");
            return Ok(await _battleService.GetAwardsList(id));
        }
        [HttpPost("fight")]
        [Authorize]
        public async Task<IActionResult> Fight([FromBody]FightRequestDto fightRequestDto)
        {
            if (fightRequestDto == null)
                return BadRequest("Null Param");
            
            if (fightRequestDto.history.IsNullOrEmpty())
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
        public async Task<IActionResult> GetBattleList()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id))
                return BadRequest("找不到Id");
            var history = await _battleService.GetBattleRecordListDto(id);
            return Ok(history);
        }
        [HttpPost("replay")]
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
