using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public NotificationController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [HttpGet("own-notification")]
        public async Task<IActionResult> GetOwnNotification()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.NotificationService.GetOwnNotification(userId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Danh sách thông báo của bạn"));
        }
    }
}
