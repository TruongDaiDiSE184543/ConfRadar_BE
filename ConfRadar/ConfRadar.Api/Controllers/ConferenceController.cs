using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Conference;
using ConfRadar.Services.DTOs.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferenceController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ConferenceController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateConference([FromForm] CreateConferenceRequest request)
        {
            try
            {
                var conferenceId = await _serviceManager.ConferenceService.CreateConferenceAsync(request);
                return Ok(ApiResponse<string>.SuccessResponse(conferenceId, "Conference created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConference(string id, [FromBody] UpdateConferenceRequest request)
        {
            try
            {
                var result = await _serviceManager.ConferenceService.UpdateConferenceAsync(request, id);
                if (result > 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference updated successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConference(string id)
        {
            try
            {
                var result = await _serviceManager.ConferenceService.DeleteConferenceAsync(id);
                if (result > 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConference(string id)
        {
            try
            {
                var conference = await _serviceManager.ConferenceService.GetConferenceByIdAsync(id);
                if (conference != null)
                {
                    return Ok(ApiResponse<ConferenceResponse>.SuccessResponse(conference, "Conference retrieved successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllConferences()
        {
            try
            {
                var conferences = await _serviceManager.ConferenceService.GetAllConferencesAsync();
                return Ok(ApiResponse<List<ConferenceResponse>>.SuccessResponse(conferences, "Conferences retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
        
        [HttpGet("view-registered-users-for-conference")]
        public async Task<IActionResult> ViewRegisteredUsersInAConference(string conferenceId)
        {
            var userList = await _serviceManager.TicketService.GetTicketListByConferenceId(conferenceId);
            return Ok(ApiResponse<List<PaidTicketResponse>>.SuccessResponse(userList, "data retrieved"));
        }
        
        [HttpGet("paginated-conferences")]
        public async Task<IActionResult> GetAllConferencesWithPagination([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var conferences = await _serviceManager.ConferenceService.GetAllConferencesPaginatedAsync(page, pageSize);
            return Ok(ApiResponse<PagedResult<ConferenceResponse>>.SuccessResponse(conferences, "Conferences retrieved successfully"));
        }
        
        // NEW ENDPOINT 1: Get all conferences with their price phases (with pagination/filtering)
        [HttpGet("conferences-with-prices")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConferencesWithPrices(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? cityId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            var conferences = await _serviceManager.ConferenceService.GetConferencesWithPricesAsync(page, pageSize, searchKeyword, cityId, startDate, endDate);
            return Ok(ApiResponse<PagedResult<ConferenceWithPricesResponse>>.SuccessResponse(conferences, "Conferences with prices retrieved successfully"));
        }
        
        // NEW ENDPOINT 2: Get detailed technical conference data
        [HttpGet("technical-detail/{conferenceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTechnicalConferenceDetail(string conferenceId)
        {
            var conferenceDetail = await _serviceManager.ConferenceService.GetTechnicalConferenceDetailAsync(conferenceId);
            return Ok(ApiResponse<TechnicalConferenceDetailResponse>.SuccessResponse(conferenceDetail, "Technical conference detail retrieved successfully"));
        }
        
        // NEW ENDPOINT 3: Get conferences by status ID with filtering
        [HttpGet("by-status/{conferenceStatusId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConferencesByStatus(
            string conferenceStatusId,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? cityId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            var conferences = await _serviceManager.ConferenceService.GetConferencesByStatusAsync(conferenceStatusId, page, pageSize, searchKeyword, cityId, startDate, endDate);
            return Ok(ApiResponse<PagedResult<ConferenceResponse>>.SuccessResponse(conferences, "Conferences retrieved successfully"));
        }
        
        // NEW ENDPOINT 4: Get conferences with step completion status
        [HttpGet("step-completion-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConferencesStepCompletionStatus(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? cityId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            var conferences = await _serviceManager.ConferenceService.GetConferencesStepCompletionStatusAsync(page, pageSize, searchKeyword, cityId, startDate, endDate);
            return Ok(ApiResponse<PagedResult<ConferenceStepCompletionStatusResponse>>.SuccessResponse(conferences, "Conference step completion status retrieved successfully"));
        }
    }
}