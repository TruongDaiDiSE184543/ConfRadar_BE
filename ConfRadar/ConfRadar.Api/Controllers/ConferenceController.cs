using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Conference;
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.FailResponse("User not authenticated"));
                }

                var conferenceId = await _serviceManager.ConferenceService.CreateConferenceAsync(request, userId);
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
    }
}