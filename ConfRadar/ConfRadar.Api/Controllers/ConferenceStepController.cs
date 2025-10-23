using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.ConferenceStep;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Conference Organizer")]
    public class ConferenceStepController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ConferenceStepController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        #region Step 1: Basic Conference Creation

        [HttpPost("basic")]
        public async Task<IActionResult> CreateConferenceBasic([FromForm] CreateConferenceBasicRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.FailResponse("User not authenticated"));
                }

                var conference = await _serviceManager.ConferenceStepService.CreateConferenceBasicAsync(request, userId);
                return Ok(ApiResponse<ConferenceStepResponse>.SuccessResponse(conference, "Conference basic information created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{conferenceId}/basic")]
        public async Task<IActionResult> GetConferenceBasic(string conferenceId)
        {
            try
            {
                var conference = await _serviceManager.ConferenceStepService.GetConferenceBasicAsync(conferenceId);
                return Ok(ApiResponse<ConferenceStepResponse>.SuccessResponse(conference, "Conference basic information retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpPut("{conferenceId}/basic")]
        public async Task<IActionResult> UpdateConferenceBasic(string conferenceId, [FromForm] UpdateConferenceBasicRequest request)
        {
            try
            {
                var conference = await _serviceManager.ConferenceStepService.UpdateConferenceBasicAsync(conferenceId, request);
                return Ok(ApiResponse<ConferenceStepResponse>.SuccessResponse(conference, "Conference basic information updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        #endregion

        #region Step 2: Conference Prices

        [HttpPost("{conferenceId}/prices")]
        public async Task<IActionResult> AddConferencePrices(string conferenceId, [FromBody] AddConferencePricesRequest request)
        {
            try
            {
                var prices = await _serviceManager.ConferenceStepService.AddConferencePricesAsync(conferenceId, request);
                return Ok(ApiResponse<List<ConferencePriceStepResponse>>.SuccessResponse(prices, "Conference prices added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{conferenceId}/prices")]
        public async Task<IActionResult> GetConferencePrices(string conferenceId)
        {
            try
            {
                var prices = await _serviceManager.ConferenceStepService.GetConferencePricesAsync(conferenceId);
                return Ok(ApiResponse<List<ConferencePriceStepResponse>>.SuccessResponse(prices, "Conference prices retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpPut("prices/{priceId}")]
        public async Task<IActionResult> UpdateConferencePrice(string priceId, [FromBody] UpdateConferencePriceRequest request)
        {
            try
            {
                var price = await _serviceManager.ConferenceStepService.UpdateConferencePriceAsync(priceId, request);
                return Ok(ApiResponse<ConferencePriceStepResponse>.SuccessResponse(price, "Conference price updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("prices/{priceId}")]
        public async Task<IActionResult> DeleteConferencePrice(string priceId)
        {
            try
            {
                var result = await _serviceManager.ConferenceStepService.DeleteConferencePriceAsync(priceId);
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference price deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference price not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        #endregion

        #region Step 3: Conference Sessions

        [HttpPost("{conferenceId}/sessions")]
        public async Task<IActionResult> AddConferenceSessions(string conferenceId, [FromBody] AddConferenceSessionsRequest request)
        {
            try
            {
                var sessions = await _serviceManager.ConferenceStepService.AddConferenceSessionsAsync(conferenceId, request);
                return Ok(ApiResponse<List<ConferenceSessionStepResponse>>.SuccessResponse(sessions, "Conference sessions added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{conferenceId}/sessions")]
        public async Task<IActionResult> GetConferenceSessions(string conferenceId)
        {
            try
            {
                var sessions = await _serviceManager.ConferenceStepService.GetConferenceSessionsAsync(conferenceId);
                return Ok(ApiResponse<List<ConferenceSessionStepResponse>>.SuccessResponse(sessions, "Conference sessions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpPut("sessions/{sessionId}")]
        public async Task<IActionResult> UpdateConferenceSession(string sessionId, [FromBody] UpdateConferenceSessionRequest request)
        {
            try
            {
                var session = await _serviceManager.ConferenceStepService.UpdateConferenceSessionAsync(sessionId, request);
                return Ok(ApiResponse<ConferenceSessionStepResponse>.SuccessResponse(session, "Conference session updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpPut("sessions/{sessionId}/speaker")]
        public async Task<IActionResult> UpdateSpeaker(string sessionId, [FromBody] UpdateSpeakerRequest request)
        {
            try
            {
                var speaker = await _serviceManager.ConferenceStepService.UpdateSpeakerAsync(sessionId, request);
                return Ok(ApiResponse<SpeakerResponse>.SuccessResponse(speaker, "Speaker updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteConferenceSession(string sessionId)
        {
            try
            {
                var result = await _serviceManager.ConferenceStepService.DeleteConferenceSessionAsync(sessionId);
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference session deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference session not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        #endregion

        #region Step 4: Conference Policies

        [HttpPost("{conferenceId}/policies")]
        public async Task<IActionResult> AddConferencePolicies(string conferenceId, [FromBody] AddConferencePoliciesRequest request)
        {
            try
            {
                var policies = await _serviceManager.ConferenceStepService.AddConferencePoliciesAsync(conferenceId, request);
                return Ok(ApiResponse<List<ConferencePolicyResponse>>.SuccessResponse(policies, "Conference policies added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{conferenceId}/policies")]
        public async Task<IActionResult> GetConferencePolicies(string conferenceId)
        {
            try
            {
                var policies = await _serviceManager.ConferenceStepService.GetConferencePoliciesAsync(conferenceId);
                return Ok(ApiResponse<List<ConferencePolicyResponse>>.SuccessResponse(policies, "Conference policies retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpPut("policies/{policyId}")]
        public async Task<IActionResult> UpdateConferencePolicy(string policyId, [FromBody] UpdateConferencePolicyRequest request)
        {
            try
            {
                var policy = await _serviceManager.ConferenceStepService.UpdateConferencePolicyAsync(policyId, request);
                return Ok(ApiResponse<ConferencePolicyResponse>.SuccessResponse(policy, "Conference policy updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("policies/{policyId}")]
        public async Task<IActionResult> DeleteConferencePolicy(string policyId)
        {
            try
            {
                var result = await _serviceManager.ConferenceStepService.DeleteConferencePolicyAsync(policyId);
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference policy deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference policy not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        #endregion

        #region Step 5: Conference Media

        [HttpPost("{conferenceId}/media")]
        public async Task<IActionResult> AddConferenceMedia(string conferenceId, [FromForm] AddConferenceMediaRequest request)
        {
            try
            {
                var media = await _serviceManager.ConferenceStepService.AddConferenceMediaAsync(conferenceId, request);
                return Ok(ApiResponse<List<ConferenceMediaResponse>>.SuccessResponse(media, "Conference media added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{conferenceId}/media")]
        public async Task<IActionResult> GetConferenceMedia(string conferenceId)
        {
            try
            {
                var media = await _serviceManager.ConferenceStepService.GetConferenceMediaAsync(conferenceId);
                return Ok(ApiResponse<List<ConferenceMediaResponse>>.SuccessResponse(media, "Conference media retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpPut("media/{mediaId}")]
        public async Task<IActionResult> UpdateConferenceMedia(string mediaId, [FromForm] UpdateConferenceMediaRequest request)
        {
            try
            {
                var media = await _serviceManager.ConferenceStepService.UpdateConferenceMediaAsync(mediaId, request);
                return Ok(ApiResponse<ConferenceMediaResponse>.SuccessResponse(media, "Conference media updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("media/{mediaId}")]
        public async Task<IActionResult> DeleteConferenceMedia(string mediaId)
        {
            try
            {
                var result = await _serviceManager.ConferenceStepService.DeleteConferenceMediaAsync(mediaId);
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference media deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference media not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        #endregion

        #region Step 6: Conference Sponsors

        [HttpPost("{conferenceId}/sponsors")]
        public async Task<IActionResult> AddConferenceSponsors(string conferenceId, [FromForm] AddConferenceSponsorsRequest request)
        {
            try
            {
                var sponsors = await _serviceManager.ConferenceStepService.AddConferenceSponsorsAsync(conferenceId, request);
                return Ok(ApiResponse<List<SponsorResponse>>.SuccessResponse(sponsors, "Conference sponsors added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{conferenceId}/sponsors")]
        public async Task<IActionResult> GetConferenceSponsors(string conferenceId)
        {
            try
            {
                var sponsors = await _serviceManager.ConferenceStepService.GetConferenceSponsorsAsync(conferenceId);
                return Ok(ApiResponse<List<SponsorResponse>>.SuccessResponse(sponsors, "Conference sponsors retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpPut("sponsors/{sponsorId}")]
        public async Task<IActionResult> UpdateSponsor(string sponsorId, [FromForm] UpdateSponsorRequest request)
        {
            try
            {
                var sponsor = await _serviceManager.ConferenceStepService.UpdateSponsorAsync(sponsorId, request);
                return Ok(ApiResponse<SponsorResponse>.SuccessResponse(sponsor, "Conference sponsor updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("sponsors/{sponsorId}")]
        public async Task<IActionResult> DeleteSponsor(string sponsorId)
        {
            try
            {
                var result = await _serviceManager.ConferenceStepService.DeleteSponsorAsync(sponsorId);
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference sponsor deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference sponsor not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        #endregion
    }
}