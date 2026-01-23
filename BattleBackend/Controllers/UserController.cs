using BattleBackend.DTOs;
using BattleBackend.Services;
using DataCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BattleBackend.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        private readonly JwtService _jwtService;
        private readonly BattleService _battleService;

        // 通过构造函数注入
        public UserController(JwtService jwtService, BattleService battleService)
        {
            _jwtService = jwtService;
            _battleService = battleService;

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            if (request is null)
                return BadRequest("参数错误");
            var user = await _battleService.IdentifyUser(request.Account, request.Password);
            if (user is null)
                return Unauthorized("账号或密码错误");
            var token = _jwtService.GenerateToken(user);
            return Ok(
                new
                {
                    token = token,
                    id = user.Id,
                    profession = user.Profession,
                    name = user.Name
                });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetSkillAndWeapon()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Unauthorized("凭证过期");
            int.TryParse(claim.Value, out int id);

            var user = await _battleService.GetUserById(id);

            var information = new InformationDTO(user!);
            return Ok(information);
        }
        [HttpPost("init")]
        [Authorize]
        public async Task<IActionResult> InitProfile([FromBody] InitProfileDto dto)
        {
            // 从 JWT Token 中获取用户 ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            bool success = await _battleService.InitializeUserProfile(
                userId,
                dto.name,
                dto.account ?? userId, // 如果 account 为空则默认为 ID
                dto.profession,
                dto.secondProfession,
                dto.initialSkills
            );

            return success ? Ok(new { message = "初始化成功" }) : BadRequest();
        }
        [HttpGet("battle")]
        [Authorize]
        public async Task<IActionResult> BattleList()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return Unauthorized("未授权ID");
            var users = await _battleService.GetAllFighter(userId);
            foreach (var user in users)
            {
                
            }
        }
    }
}
