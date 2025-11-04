using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.ConferenceStep;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ReportController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// Customer creates a report (HasResolve defaults to true)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.FailResponse("User not authenticated"));
            }

            var report = await _serviceManager.ReportService.CreateReportAsync(userId, request);
            return Ok(ApiResponse<ReportResponse>.SuccessResponse(report, "Report created successfully"));
        }

        /// <summary>
        /// Admin retrieves list of unresolved reports (HasResolve = false)
        /// </summary>
        [HttpGet("unresolved")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUnresolvedReports()
        {
            var reports = await _serviceManager.ReportService.GetUnresolvedReportsAsync();
            return Ok(ApiResponse<List<UnresolvedReportResponse>>.SuccessResponse(reports, "Unresolved reports retrieved successfully"));
        }

        /// <summary>
        /// Admin responds to report (inserts to ReportFeedback table and marks report as resolved)
        /// </summary>
        [HttpPost("{reportId}/response")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateReportFeedback(string reportId, [FromBody] CreateReportFeedbackRequest request)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(ApiResponse<object>.FailResponse("Admin not authenticated"));
            }

            var feedback = await _serviceManager.ReportService.CreateReportFeedbackAsync(reportId, adminId, request);
            return Ok(ApiResponse<ReportFeedbackResponse>.SuccessResponse(feedback, "Report feedback created successfully"));
        }
    }
}