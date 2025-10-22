using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.ConferencePriceTicket;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferencePriceTicketController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ConferencePriceTicketController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetConferencePriceTicketList([FromQuery] ConferencePriceTicketSearchRequest request)
        {
            try
            {
                var tickets = await _serviceManager.ConferencePriceTicketService.GetConferencePriceTicketListAsync(request);
                var totalCount = await _serviceManager.ConferencePriceTicketService.GetTotalConferencePriceTicketCountAsync(request);

                // Calculate pagination info
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                var response = new
                {
                    Data = tickets,
                    Pagination = new
                    {
                        Page = request.Page,
                        PageSize = request.PageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(response, "Conference price tickets retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConferencePriceTicketDetail(string id)
        {
            try
            {
                var ticket = await _serviceManager.ConferencePriceTicketService.GetConferencePriceTicketDetailAsync(id);
                if (ticket == null)
                {
                    return NotFound(ApiResponse<object>.FailResponse("Conference price ticket not found"));
                }

                return Ok(ApiResponse<ConferencePriceTicketDetailResponse>.SuccessResponse(ticket, "Conference price ticket detail retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
    }
}