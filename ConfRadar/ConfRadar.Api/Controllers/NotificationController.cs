using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.Notification;
using Microsoft.AspNetCore.Authorization;
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
            return Ok(ApiResponse<List<UserNotificationDetailResponse>>.SuccessResponse(result, "Danh sách thông báo của bạn"));
        }
        [Authorize]
        [HttpPut("update-read-status")]
        public async Task<IActionResult> UpdateReadStatus([FromBody] List<UpdateReadStatusRequest> request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.NotificationService.UpdateReadStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Cập nhật thành công thông báo"));
        }
    }
}
