using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ConfigurationController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("session")]
        public async Task<IActionResult> GetSessionConfiguration()
        {
            try
            {
                var config = await _serviceManager.SystemConfigurationService.GetAllSessionConfigurationAsync();
                return Ok(ApiResponse<SessionConfigurationResponse>.SuccessResponse(config, " truy xu?t c?u hình phiên thành công "));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("session")]
        public async Task<IActionResult> UpdateSessionConfiguration([FromBody] SessionConfigurationRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _serviceManager.SystemConfigurationService.UpdateSessionConfigurationAsync(request);
                if (result > 0)
                {
                    await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"c?u hình phiên c?p nh?t thành công");
                    return Ok(ApiResponse<object>.SuccessResponse(null, "C?u hình phiên c?p nh?t thành công"));
                }
                return BadRequest(ApiResponse<object>.FailResponse("C?u hình phiên c?p nh?t th?t b?i"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
    }
}