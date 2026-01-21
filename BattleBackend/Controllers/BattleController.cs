using Microsoft.AspNetCore.Mvc;

namespace BattleBackend.Controllers
{
    public class BattleController : Controller
    {
        private readonly JwtService _jwtService;

        // 通过构造函数注入
        public BattleController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }
        [HttpGet("start")]
        public IActionResult StartBattle([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("缺少或无效的 Authorization");
            var principal = _jwtService.ValidateToken(authHeader);

            if (principal == null)
                return Unauthorized("Token 无效或过期");

            var username = principal.Identity?.Name;
            return Ok($"战斗开始，当前用户：{username}");
        }
    }
}
