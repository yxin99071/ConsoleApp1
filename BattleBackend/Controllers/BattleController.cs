using BattleBackend.Services;
using BattleCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BattleBackend.Controllers
{
    public class BattleController : Controller
    {
        private readonly JwtService _jwtService;
        private readonly BattleService _battleService;

        // 通过构造函数注入
        public BattleController(JwtService jwtService, BattleService battleService)
        {
            _jwtService = jwtService;
            _battleService = battleService;

        }
        [HttpGet("start")]
        public IActionResult StartBattle([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("缺少或无效的 Authorization");
            var principal = _jwtService.ValidateToken(authHeader);
            //


            if (principal == null)
                return Unauthorized("Token 无效或过期");

            var username = principal.Identity?.Name;
            return Ok($"战斗开始，当前用户：{username}");
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
        public async Task<IActionResult> Fight(int enemyId)
        {
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id))
            {
                await _battleService.ExecuteFight(id, enemyId);//json
                return Ok(JsonLogger.GetEvents());
            }
            return BadRequest("无法战斗");
        }



    }
}
