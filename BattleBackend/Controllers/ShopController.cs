using BattleBackend.DTOs;
using BattleBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BattleBackend.Controllers
{
    [ApiController]
    [Route("shop")]
    [Authorize]
    public class ShopController : ControllerBase
    {
        private readonly ShopService _shopService;

        public ShopController(ShopService shopService) => _shopService = shopService;

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

        // ── 每日商店 ─────────────────────────────────────────────────────────

        /// <summary>GET /shop/daily — 获取（并自动刷新）每日商店</summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyShop()
        {
            var dto = await _shopService.GetOrRefreshDailyShopAsync(GetUserId());
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>POST /shop/daily/refresh — 手动刷新（消耗 20 碎片）</summary>
        [HttpPost("daily/refresh")]
        public async Task<IActionResult> ManualRefresh()
        {
            var dto = await _shopService.ManualRefreshShopAsync(GetUserId());
            if (dto == null) return BadRequest(new { message = "碎片不足或操作失败" });
            return Ok(dto);
        }

        /// <summary>POST /shop/daily/lock — 锁定/解锁槽位 { slotId: 0 = 解锁全部 }</summary>
        [HttpPost("daily/lock")]
        public async Task<IActionResult> LockSlot([FromBody] LockSlotRequest req)
        {
            var dto = await _shopService.LockSlotAsync(GetUserId(), req.SlotId);
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>POST /shop/daily/purchase — 购买槽位</summary>
        [HttpPost("daily/purchase")]
        public async Task<IActionResult> PurchaseSlot([FromBody] PurchaseSlotRequest req)
        {
            var (result, error) = await _shopService.PurchaseSlotAsync(GetUserId(), req.SlotId);
            if (error != null) return BadRequest(new { message = error });
            return Ok(result);
        }

        // ── 抽卡 ─────────────────────────────────────────────────────────────

        /// <summary>POST /shop/draw/profession — 职业抽（40 碎片，稀有度随机）</summary>
        [HttpPost("draw/profession")]
        public async Task<IActionResult> DrawByProfession([FromBody] ProfDrawRequest req)
        {
            var (result, error) = await _shopService.DrawByProfessionAsync(GetUserId(), req.Profession);
            if (error != null) return BadRequest(new { message = error });
            return Ok(result);
        }

        /// <summary>POST /shop/draw/rarity — 稀有度抽（费用固定，职业随机）</summary>
        [HttpPost("draw/rarity")]
        public async Task<IActionResult> DrawByRarity([FromBody] RarityDrawRequest req)
        {
            var (result, error) = await _shopService.DrawByRarityAsync(GetUserId(), req.Rarity);
            if (error != null) return BadRequest(new { message = error });
            return Ok(result);
        }

        // ── 熔炼 ─────────────────────────────────────────────────────────────

        /// <summary>POST /shop/smelt — 熔炼一张卡牌</summary>
        [HttpPost("smelt")]
        public async Task<IActionResult> SmeltItem([FromBody] SmeltRequest req)
        {
            var (result, error) = await _shopService.SmeltItemAsync(GetUserId(), req.ItemType, req.ItemId);
            if (error != null) return BadRequest(new { message = error });
            return Ok(result);
        }

        // ── 背包 ─────────────────────────────────────────────────────────────

        /// <summary>GET /shop/inventory — 背包列表（含 Count，用于熔炼 Tab）</summary>
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var dto = await _shopService.GetInventoryAsync(GetUserId());
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>GET /shop/r4-status — 检查是否集齐所有 R4（稀有度抽按钮禁用判断）</summary>
        [HttpGet("r4-status")]
        public async Task<IActionResult> GetR4Status()
        {
            bool allOwned = await _shopService.AllR4OwnedAsync(GetUserId());
            return Ok(new { allR4Owned = allOwned });
        }
    }
}
