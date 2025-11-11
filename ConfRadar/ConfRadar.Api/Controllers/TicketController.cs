using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.RefundRequest;
using ConfRadar.Shared.DTO.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public TicketController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [Authorize]
        [HttpGet("get-own-paid-ticket")]
        public async Task<IActionResult> GetOwnPaidTicketData([FromQuery] string? keyword, [FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 10, [FromQuery] DateTime? sessionStartTime = null, [FromQuery] DateTime? sessionEndTime = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.TicketService.GetTicketsByUserId(userId, keyword, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResultResponseDto<CustomerPaidTicketResponse>>.SuccessResponse(result, "Danh sách vé đã chi trả"));
        }
        [Authorize]
        [HttpPost("refund-ticket")]
        public async Task<IActionResult> RefundTicket([FromBody] RefundTicketRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.TicketService.CreateRefundTicketRequest(request,userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Hãy check yêu cầu lịch sử refund để biết thêm thông tin chi tiết"));
        }
        [Authorize(Roles ="Conference Organizer")]
        [HttpGet("conferences/{conferenceId}/refunds-request")]
        public async Task<IActionResult> GetRefundRequestByConferenceId([FromRoute] string conferenceId)
        {
            var result = await _serviceManager.TicketService.GetRefundRequestByConferenceId(conferenceId);
            return Ok(ApiResponse<List<RefundRequestResponse>>.SuccessResponse(result, "Danh sách refund request thuộc về hội nghị"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("refunds-request")]
        public async Task<IActionResult> GetRefundRequests()
        {
            var result = await _serviceManager.TicketService.GetAllRefundRequests();
            return Ok(ApiResponse<List<RefundRequestResponse>>.SuccessResponse(result, "Danh sách refund requests"));
        }
    }
}
