using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Statistics;
using ConfRadar.Shared.DTO.General;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public StatisticsController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("sold-ticket")]
        public async Task<IActionResult> getSoldTicket([FromQuery] string confId)
        {
            var result = await _serviceManager.StatisticsService.GetSoldTicketStatisticsAsync(confId);
            return Ok(ApiResponse<ConferenceStatisticsResponse>.SuccessResponse(result, "Lấy thành công vé đã bán"));
        }

        [HttpGet("ticket-holders")]
        public async Task<IActionResult> getTicketHolders([FromQuery] TicketHolderSearchParam request)
        {
            var result = await _serviceManager.StatisticsService.GetTicketHoldersByConferenceIdAsync(request);
            return Ok(ApiResponse<PagedResultResponseDto<TicketHolderDetailResponse>>.SuccessResponse(result, "Lấy thành công thông tin vé đã bán và người mua"));
        }
        [HttpGet("export/sold-ticket")]
        public async Task<IActionResult> exportRevenue([FromQuery] string confId)
        {
            var fileBytes = await _serviceManager.StatisticsService.ExportDetailedConferenceStatisticsAsync(confId);
            var fileName = $"conference_statistics_{confId}{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("submitted-papers")]
        public async Task<IActionResult> getSubmittedPapers([FromQuery] string confId)
        {
            var result = await _serviceManager.StatisticsService.GetPaperStatisticsByConferenceIdAsync(confId);
            return Ok(ApiResponse<PaperStatisticsResponse>.SuccessResponse(result, "Lấy thành công thông tin papers của hội nghị"));
        }

        [HttpGet("assign-reviewers")]
        public async Task<IActionResult> getAssignedReviewers([FromQuery] string confId)
        {
            var result = await _serviceManager.StatisticsService.GetReviewersByConferenceIdAsync(confId);
            return Ok(ApiResponse<List<ConfRadar.Services.DTOs.Statistics.ReviewerAssignmentResponse>>.SuccessResponse(result, "Lấy thành danh sách reviewer"));
        }

        [HttpGet("present-session")]
        public async Task<IActionResult> getPresentSession([FromQuery] string confId)
        {
            var result = await _serviceManager.StatisticsService.GetSessionsWithPresentersByConferenceIdAsync(confId);
            return Ok(ApiResponse<List<ConfRadar.Services.DTOs.Statistics.SessionWithPresentersResponse>>.SuccessResponse(result, "Lấy thành công danh sách session và presenter"));
        }
    }
}
