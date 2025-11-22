using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        
        public DashboardController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        
        [HttpGet("conference-stats")]
        public async Task<IActionResult> GetConferenceStatsByUserId([FromQuery] string userId)
        {
            var result = await _serviceManager.DashboardService.GetConferenceStatsByUserIdAsync(userId);
            return Ok(ApiResponse<ConferenceStatsResponse>.SuccessResponse(result, "Thống kê hội nghị theo người dùng"));
        }
    }
}