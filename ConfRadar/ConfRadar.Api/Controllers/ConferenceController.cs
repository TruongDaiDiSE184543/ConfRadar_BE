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



        //[HttpGet("view-registered-users-for-conference")]
        //public async Task<IActionResult> ViewRegisteredUsersInAConference(string conferenceId)
        //{
        //    var userList = await _serviceManager.TicketService.GetTicketListByConferenceId(conferenceId);
        //    return Ok(ApiResponse<List<PaidTicketResponse>>.SuccessResponse(userList, "data retrieved"));
        //}

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
        [Authorize(Roles = "Conference Organizer")]
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

        // NEW ENDPOINT 5: Get all pending conferences
        [HttpGet("pending-conferences")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> GetPendingConferences(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchKeyword = null)
        {
            var conferences = await _serviceManager.ConferenceService.GetPendingConferencesAsync(page, pageSize, searchKeyword);
            return Ok(ApiResponse<PagedResult<ConferenceResponse>>.SuccessResponse(conferences, "Pending conferences retrieved successfully"));
        }

        // NEW ENDPOINT 6: Approve conference (change status from pending to preparing)
        [HttpPut("approve-conference/{conferenceId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> ApproveConference(string conferenceId, [FromBody] ApproveConferenceRequest request)
        {
            var result = await _serviceManager.ConferenceService.ApproveConferenceAsync(conferenceId, request);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Conference approved successfully"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Conference not found or could not be approved"));
        }

        // NEW ENDPOINT 7: Get detailed research conference data
        [HttpGet("research-detail/{conferenceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetResearchConferenceDetail(string conferenceId)
        {
            var conferenceDetail = await _serviceManager.ConferenceService.GetResearchConferenceDetailAsync(conferenceId);
            return Ok(ApiResponse<ResearchConferenceDetailResponse>.SuccessResponse(conferenceDetail, "Research conference detail retrieved successfully"));
        }

        // NEW ENDPOINT 8: Get research conferences with step completion status
        [HttpGet("research-step-completion-status")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> GetResearchConferencesStepCompletionStatus(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? cityId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            var conferences = await _serviceManager.ConferenceService.GetResearchConferencesStepCompletionStatusAsync(page, pageSize, searchKeyword, cityId, startDate, endDate);
            return Ok(ApiResponse<PagedResult<ResearchConferenceStepCompletionStatusResponse>>.SuccessResponse(conferences, "Research conference step completion status retrieved successfully"));
        }

        // NEW ENDPOINT 9: Check if technical conference has completed a specific step
        [HttpGet("check-technical-step-completion")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> CheckTechnicalConferenceStepCompletion([FromQuery] string conferenceId, [FromQuery] string step)
        {
            var result = await _serviceManager.ConferenceService.CheckTechnicalConferenceStepCompletionAsync(conferenceId, step);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Technical conference step completion status retrieved successfully"));
        }

        // NEW ENDPOINT 10: Check if research conference has completed a specific step
        [HttpGet("check-research-step-completion")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CheckResearchConferenceStepCompletion([FromQuery] string conferenceId, [FromQuery] string step)
        {
            var result = await _serviceManager.ConferenceService.CheckResearchConferenceStepCompletionAsync(conferenceId, step);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Research conference step completion status retrieved successfully"));
        }

        // NEW ENDPOINT 11: Get list of research conferences with pagination and filtering (for organizers and collaborators)
        [HttpGet("research-conferences-for-Organizer")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> GetResearchConferencesList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? conferenceStatusId = null,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? cityId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isOrganizer = User.IsInRole("Conference Organizer");

            var conferences = await _serviceManager.ConferenceService.GetResearchConferencesListAsync(
                page, pageSize, conferenceStatusId, searchKeyword, cityId, startDate, endDate, userId, isOrganizer);
            return Ok(ApiResponse<PagedResult<Services.DTOs.Conference.ResearchConferenceDetailResponse>>.SuccessResponse(conferences, "Research conferences retrieved successfully"));
        }

        // NEW ENDPOINT 12: Get list of technical conferences with pagination and filtering (for organizers and collaborators)
        [HttpGet("technical-conferences-for-collaborator-and-Organizer")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> GetTechnicalConferencesList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? conferenceStatusId = null,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? cityId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isOrganizer = User.IsInRole("Conference Organizer");

            var conferences = await _serviceManager.ConferenceService.GetTechnicalConferencesListAsync(
                page, pageSize, conferenceStatusId, searchKeyword, cityId, startDate, endDate, userId, isOrganizer);
            return Ok(ApiResponse<PagedResult<Services.DTOs.Conference.TechnicalConferenceDetailResponse>>.SuccessResponse(conferences, "Technical conferences retrieved successfully"));
        }

        [HttpPost("Update-own-conference-Status")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateConferenceStatus(string confid, string newStatus,string? reason = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.UpdateConferenceStatusAsync(confid, newStatus, reason);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Update trạng thái hội nghị thành công"));
        }

        [HttpGet("get-own-conferences")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> GetOwnConference(string? statusId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.GetAllConferenceWithStatusByUserId(userId, statusId);
            return Ok(ApiResponse<List<ConferenceWithStatusNameResponse>>.SuccessResponse(result, "User conferences retrieved successfully"));
        }
    }
}