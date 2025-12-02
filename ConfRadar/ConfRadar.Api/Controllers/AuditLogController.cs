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
        [HttpGet("list-audit-log-categories")]
        public async Task<IActionResult> GetAuditLogCategories()
        {
            var categories = await _serviceManager.AuditLogService.GetAuditLogCategories();
            return Ok(ApiResponse<List<AuditLogCategory>>.SuccessResponse(categories, "Danh sách danh mục audit log "));
        }
    }
}
