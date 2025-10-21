using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> GetOwnPaidTicketData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.TicketService.GetTicketsByUserId(userId);
            return Ok(ApiResponse<List<Ticket>>.SuccessResponse(result, "Retrieve data successfully"));
        }
    }
}
