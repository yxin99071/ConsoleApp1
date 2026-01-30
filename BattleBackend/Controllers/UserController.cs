using BattleBackend.DTOs;
using BattleBackend.Services;
using DataCore.Models;
using DataCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using static BattleBackend.DTOs.InformationDTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<IActionResult> GetProfiles(string id)
        {
            var information = new InformationDto();
            if(id == null)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userId, out int SelectedId))
                {
                    var user = await _battleService.GetUserById(SelectedId);
                    if(user is null) return BadRequest("GetUserError");
                     information = MappingExtensions.ToDto(user);
                }
            }
            if(id != null)
            {
                if (int.TryParse(id, out int SelectedId))
                {
                    var user = await _battleService.GetUserById(SelectedId);
                    if (user is null) return BadRequest("GetUserError");
                    information = MappingExtensions.ToDto(user);
                }
            }
            return Ok(information);
        }
        [HttpPost("init")]
        [Authorize]
        public async Task<IActionResult> InitProfile([FromBody] InitProfileDto dto)
        {
            // 从 JWT Token 中获取用户 ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(int.TryParse(userId,out int id))
            {
                dto.account = id.ToString();
                await _battleService.InitializeUserProfile(id,dto);
            }
            return Ok(new { success = true });
        }
        [HttpGet("fighters")]
        [Authorize]
        public async Task<IActionResult> GetAllFighter()
        {
            //todo mapto dto
            var users = await _battleService.GetAllFighter(exclusiveId:null);
            List<FighterDto> fighters = new List<FighterDto>();
            foreach (var user in users)
            {
                fighters.Add(new FighterDto
                {
                    Id = user.Id,
                    Level = user.Level,
                    Name = user.Name,
                    Profession = user.Profession!,
                    SecondProfession = user.SecondProfession
                });
            }
            return Ok(fighters);
        }
    }
}
