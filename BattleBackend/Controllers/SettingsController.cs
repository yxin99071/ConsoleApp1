using BattleBackend.DTOs;
using DataCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BattleBackend.Controllers
{
    [Route("settings")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly DataHelper _dataHelper;

        public SettingsController(DataHelper dataHelper)
        {
            _dataHelper = dataHelper;
        }

        /// <summary>获取当前用户的默认出战卡组</summary>
        [HttpGet("default-deck")]
        public async Task<IActionResult> GetDefaultDeck()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                return BadRequest("找不到用户 Id");

            // 读取用户等级（用于计算容量）
            var user = await _dataHelper.GetUserById(userId);
            if (user == null) return NotFound();

            int capacity = user.Level / 5 + 2;

            var deck = await _dataHelper.GetDefaultDeckAsync(userId);
            return Ok(new DefaultDeckDto
            {
                WeaponIds = deck?.WeaponIds ?? new(),
                SkillIds  = deck?.SkillIds  ?? new(),
                Capacity  = capacity
            });
        }

        /// <summary>保存当前用户的默认出战卡组</summary>
        [HttpPost("default-deck")]
        public async Task<IActionResult> SetDefaultDeck([FromBody] DefaultDeckDto dto)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                return BadRequest("找不到用户 Id");

            await _dataHelper.SetDefaultDeckAsync(userId, dto.WeaponIds, dto.SkillIds);
            return Ok();
        }
    }
}
