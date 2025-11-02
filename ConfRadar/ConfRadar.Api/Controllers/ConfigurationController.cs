using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                return Ok(ApiResponse<SessionConfigurationResponse>.SuccessResponse(config, "Session configuration retrieved successfully"));
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
                var result = await _serviceManager.SystemConfigurationService.UpdateSessionConfigurationAsync(request);
                if (result > 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Session configuration updated successfully"));
                }
                return BadRequest(ApiResponse<object>.FailResponse("Failed to update session configuration"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
    }
}