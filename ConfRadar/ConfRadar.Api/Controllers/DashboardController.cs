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
        
        [HttpGet("conferences-group-by-status")]
        public async Task<IActionResult> GetConferenceStatsByUserId([FromQuery] string userId)
        {
            var result = await _serviceManager.DashboardService.GetConferenceStatsByUserIdAsync(userId);
            return Ok(ApiResponse<ConferenceStatsResponse>.SuccessResponse(result, "Thống kê hội nghị status theo người tạo"));
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetConferenceStatsByUserId([FromQuery] string userId, [FromQuery]int monthBack)
        {
            var result = await _serviceManager.DashboardService.GetRevenueAnalyticsAsync(userId,monthBack);
            return Ok(ApiResponse<RevenueAnalyticsResponse>.SuccessResponse(result, "Thống kê doanh thu theo người tạo"));
        }

        [HttpGet("upcoming-conferences")]
        public async Task<IActionResult> GetUpcomingConferenceByUserId([FromQuery] string userId, [FromQuery] int nextmonths)
        {
            var result = await _serviceManager.DashboardService.GetUpcomingConferencesAsync(userId, nextmonths);
            return Ok(ApiResponse<List<ConferenceReminderDto>>.SuccessResponse(result, "Lấy hội nghị sắp diễn ra"));
        }

        [HttpGet("top-registered-conferences")]
        public async Task<IActionResult> MostRegisterConference([FromQuery] string userId, [FromQuery] int numberToTake)
        {
            if (numberToTake <= 0) numberToTake = 5;
            var result = await _serviceManager.DashboardService.GetTopRegisteredConferencesAsync(userId, numberToTake);
            return Ok(ApiResponse<RegisterConferenceResponse>.SuccessResponse(result, "Lấy hội nghị được mua vé nhiều nhất"));
        }

        [HttpGet("get-contract-with-confradar")]
        public async Task<IActionResult> GetContract([FromQuery] string userId)
        {
            var result = await _serviceManager.DashboardService.GetCollaboratorContractsAsync(userId);
            return Ok(ApiResponse<List<ConferenceContractResponse>>.SuccessResponse(result, "Lấy hợp đồng cho cộng tác viên"));
        }
    }
}