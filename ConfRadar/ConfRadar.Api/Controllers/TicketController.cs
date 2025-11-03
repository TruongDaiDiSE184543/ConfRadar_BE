using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.General;
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
    }
}
