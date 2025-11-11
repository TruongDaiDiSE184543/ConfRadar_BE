using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.Ticket;
using ConfRadar.Shared.DTO.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public WalletController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [Authorize]
        [HttpGet("view-own-wallet")]
        public async Task<IActionResult> ViewOwnWallet()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.WalletService.ViewOwnWallet(userId);
            return Ok(ApiResponse<OwnWalletDetailResponse>.SuccessResponse(result, "Thông tin ví tiền của bạn"));
        }
    }
}
