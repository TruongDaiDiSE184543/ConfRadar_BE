using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.AuditLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public AuditLogController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [Authorize]
        [HttpGet("list-audit-log")]
        public async Task<IActionResult> GetAuditLogList()
        {
            var auditLogs = await _serviceManager.AuditLogService.GetListAuditLogDetail();
            return Ok(ApiResponse<List<AuditLogDetailResponse>>.SuccessResponse(auditLogs, "Danh sách audit log chi tiết"));
        }



        [Authorize]
        [HttpGet("total-users")]
        public async Task<IActionResult> GetTotalUser()
        {
            var user = await _serviceManager.AuditLogService.GetUserCount();
            return Ok(ApiResponse<int>.SuccessResponse(user, "Tổng số người "));
        }
        [Authorize]
        [HttpGet("list-recent-audit")]
        public async Task<IActionResult> GetRecentAudits([FromQuery] int? row)
        {
            var audits = await _serviceManager.AuditLogService.GetRecentAuditActivity(row);
            return Ok(ApiResponse<List<AuditLogDetailResponse>>.SuccessResponse(audits, "Những hoạt động logs gần đây "));
        }


        [Authorize]
        [HttpGet("list-recent-report")]
        public async Task<IActionResult> GetRecentReports([FromQuery] int? row)
        {
            var reports = await _serviceManager.AuditLogService.GetRecentReport(row);
            return Ok(ApiResponse<List<AuditReportDetailResponse>>.SuccessResponse(reports, "Những báo cáo gần đây"));
        }


        [Authorize]
        [HttpGet("internal-event-count")]
        public async Task<IActionResult> GetInternalEventCount()
        {
            var result = await _serviceManager.AuditLogService.CountActiveInternalEvents();
            return Ok(ApiResponse<AuditInternalConferenceCountDetailResponse>.SuccessResponse(result, "Tổng hội nghị confradar"));
        }
        [Authorize]
        [HttpGet("external-event-count")]
        public async Task<IActionResult> GetExternalEventCount()
        {
            var result = await _serviceManager.AuditLogService.CountActiveExternalEvents();
            return Ok(ApiResponse<AuditExternalConferenceCountDetailResponse>.SuccessResponse(result, "Tổng hội nghị bên ngoài"));
        }

        [AllowAnonymous]
        [HttpGet("list-general-faq")]
        public async Task<IActionResult> GetListGeneralFAQ()
        {
            var result = await _serviceManager.AuditLogService.GetListGeneralFAQ();
            return Ok(ApiResponse<List<GeneralFaq>>.SuccessResponse(result, "General FAQs"));
        }

    }
}
