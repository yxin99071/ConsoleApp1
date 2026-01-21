using Microsoft.AspNetCore.Mvc;

namespace BattleBackend.Controllers
{
    public class UserController : Controller
    {
        private readonly JwtService _jwtService;

        // 通过构造函数注入
        public UserController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }
        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            // 这里只是演示，实际应该校验数据库
            if (username != "admin" || password != "123456")
                return Unauthorized("用户名或密码错误");

            var token = _jwtService.GenerateToken(username, "Admin");
            return Ok(new { token });
        }

        [HttpGet("profile")]
        public IActionResult Profile([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("缺少或无效的 Authorization");
            var principal = _jwtService.ValidateToken(authHeader);

            if (principal == null)
                return Unauthorized("Token 无效或过期");

            var username = principal.Identity?.Name;
            var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new { username, role });
        }
    }
}
