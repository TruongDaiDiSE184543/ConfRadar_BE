using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var report = await _serviceManager.ReportService.CreateReportAsync(userId, request);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Report được tạo thành công "));
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
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var feedback = await _serviceManager.ReportService.CreateReportFeedbackAsync(reportId, adminId, request);
            return Ok(ApiResponse<ReportFeedbackResponse>.SuccessResponse(feedback, "Report feedback created successfully"));
        }

        [HttpGet("{reportId}/get-response")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReportFeedback(string reportId)
        {
            var result = await _serviceManager.ReportService.GetReportFeedBackByReportId(reportId);
            return Ok(ApiResponse<ReportFeedbackResponse>.SuccessResponse(result, "Lấy report feedback thành công"));
        }
    }
}