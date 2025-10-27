using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
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
        public async Task<IActionResult> CreateConferenceBasic([FromForm] CreateTechnicalConferenceBasicRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.FailResponse("Người dùng chưa xác thực"));
            }

            var conference = await _serviceManager.ConferenceStepService.CreateTechnicalConferenceBasicAsync(request, userId);
            return Ok(ApiResponse<TechnicalConferenceBasicStepResponse>.SuccessResponse(conference, "Hội nghị được tạo thành công"));
        }

        [HttpGet("{conferenceId}/basic")]
        public async Task<IActionResult> GetConferenceBasic(string conferenceId)
        {
            var conference = await _serviceManager.ConferenceStepService.GetConferenceBasicAsync(conferenceId);
            return Ok(ApiResponse<TechnicalConferenceBasicStepResponse>.SuccessResponse(conference, "Thông tin hội nghị được lấy thành công"));
        }

        [HttpPut("{conferenceId}/basic")]
        public async Task<IActionResult> UpdateConferenceBasic(string conferenceId, [FromForm] UpdateConferenceBasicRequest request)
        {
            var conference = await _serviceManager.ConferenceStepService.UpdateConferenceBasicAsync(conferenceId, request);
            return Ok(ApiResponse<TechnicalConferenceBasicStepResponse>.SuccessResponse(conference, "Thông tin hội nghị được cập nhật thành công"));
        }

        #endregion

        #region Step 2: Conference Prices

        [HttpPost("{conferenceId}/prices")]
        public async Task<IActionResult> AddConferencePrices(string conferenceId, [FromBody] AddConferencePricesRequest request)
        {
            var prices = await _serviceManager.ConferenceStepService.AddConferencePricesAsync(conferenceId, request);
            return Ok(ApiResponse<List<ConferencePriceWithPhasesResponse>>.SuccessResponse(prices, "Giá vé được thêm thành công"));
        }

        [HttpGet("{conferenceId}/prices")]
        public async Task<IActionResult> GetConferencePrices(string conferenceId)
        {
            var prices = await _serviceManager.ConferenceStepService.GetConferencePricesAsync(conferenceId);
            return Ok(ApiResponse<List<ConferencePriceWithPhasesResponse>>.SuccessResponse(prices, "Giá vé được lấy thành công"));
        }

        [HttpPut("prices/{priceId}")]
        public async Task<IActionResult> UpdateConferencePrice(string priceId, [FromBody] UpdateConferencePriceRequest request)
        {
            var price = await _serviceManager.ConferenceStepService.UpdateConferencePriceAsync(priceId, request);
            return Ok(ApiResponse<ConferencePriceWithPhasesResponse>.SuccessResponse(price, "Giá vé được cập nhật thành công"));
        }

        [HttpDelete("prices/{priceId}")]
        public async Task<IActionResult> DeleteConferencePrice(string priceId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteConferencePriceAsync(priceId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Giá vé được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy giá vé"));
        }

        #endregion

        #region Step 3: Conference Sessions

        [HttpPost("{conferenceId}/sessions")]
        public async Task<IActionResult> AddConferenceSessions(string conferenceId, [FromBody] AddConferenceSessionsRequest request)
        {
            var sessions = await _serviceManager.ConferenceStepService.AddConferenceSessionsAsync(conferenceId, request);
            return Ok(ApiResponse<List<ConferenceSessionWithMediaResponse>>.SuccessResponse(sessions, "Phiên hội nghị được thêm thành công"));
        }

        [HttpGet("{conferenceId}/sessions")]
        public async Task<IActionResult> GetConferenceSessions(string conferenceId)
        {
            var sessions = await _serviceManager.ConferenceStepService.GetConferenceSessionsAsync(conferenceId);
            return Ok(ApiResponse<List<ConferenceSessionWithMediaResponse>>.SuccessResponse(sessions, "Phiên hội nghị được lấy thành công"));
        }

        [HttpPut("sessions/{sessionId}")]
        public async Task<IActionResult> UpdateConferenceSession(string sessionId, [FromBody] UpdateConferenceSessionRequest request)
        {
            var session = await _serviceManager.ConferenceStepService.UpdateConferenceSessionAsync(sessionId, request);
            return Ok(ApiResponse<ConferenceSessionWithMediaResponse>.SuccessResponse(session, "Phiên hội nghị được cập nhật thành công"));
        }

        [HttpPut("sessions/{sessionId}/speaker")]
        public async Task<IActionResult> UpdateSpeaker(string sessionId, [FromBody] UpdateSpeakerRequest request)
        {
            var speaker = await _serviceManager.ConferenceStepService.UpdateSpeakerAsync(sessionId, request);
            return Ok(ApiResponse<SpeakerResponse>.SuccessResponse(speaker, "Diễn giả được cập nhật thành công"));
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteConferenceSession(string sessionId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteConferenceSessionAsync(sessionId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Phiên hội nghị được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy phiên hội nghị"));
        }

        #endregion

        #region Step 4: Conference Policies

        [HttpPost("{conferenceId}/policies")]
        public async Task<IActionResult> AddConferencePolicies(string conferenceId, [FromBody] AddConferencePoliciesRequest request)
        {
            var policies = await _serviceManager.ConferenceStepService.AddConferencePoliciesAsync(conferenceId, request);
            return Ok(ApiResponse<List<ConferencePolicyResponse>>.SuccessResponse(policies, "Chính sách hội nghị được thêm thành công"));
        }

        [HttpGet("{conferenceId}/policies")]
        public async Task<IActionResult> GetConferencePolicies(string conferenceId)
        {
            var policies = await _serviceManager.ConferenceStepService.GetConferencePoliciesAsync(conferenceId);
            return Ok(ApiResponse<List<ConferencePolicyResponse>>.SuccessResponse(policies, "Chính sách hội nghị được lấy thành công"));
        }

        [HttpPut("policies/{policyId}")]
        public async Task<IActionResult> UpdateConferencePolicy(string policyId, [FromBody] UpdateConferencePolicyRequest request)
        {
            var policy = await _serviceManager.ConferenceStepService.UpdateConferencePolicyAsync(policyId, request);
            return Ok(ApiResponse<ConferencePolicyResponse>.SuccessResponse(policy, "Chính sách hội nghị được cập nhật thành công"));
        }

        [HttpDelete("policies/{policyId}")]
        public async Task<IActionResult> DeleteConferencePolicy(string policyId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteConferencePolicyAsync(policyId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Chính sách hội nghị được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy chính sách hội nghị"));
        }

        #endregion

        #region Step 5: Conference Media

        [HttpPost("{conferenceId}/media")]
        public async Task<IActionResult> AddConferenceMedia(string conferenceId, [FromForm] AddConferenceMediaRequest request)
        {
            var media = await _serviceManager.ConferenceStepService.AddConferenceMediaAsync(conferenceId, request);
            return Ok(ApiResponse<List<ConferenceMediaResponse>>.SuccessResponse(media, "Phương tiện hội nghị được thêm thành công"));
        }

        [HttpGet("{conferenceId}/media")]
        public async Task<IActionResult> GetConferenceMedia(string conferenceId)
        {
            var media = await _serviceManager.ConferenceStepService.GetConferenceMediaAsync(conferenceId);
            return Ok(ApiResponse<List<ConferenceMediaResponse>>.SuccessResponse(media, "Phương tiện hội nghị được lấy thành công"));
        }

        [HttpPut("media/{mediaId}")]
        public async Task<IActionResult> UpdateConferenceMedia(string mediaId, [FromForm] UpdateConferenceMediaRequest request)
        {
            var media = await _serviceManager.ConferenceStepService.UpdateConferenceMediaAsync(mediaId, request);
            return Ok(ApiResponse<ConferenceMediaResponse>.SuccessResponse(media, "Phương tiện hội nghị được cập nhật thành công"));
        }

        [HttpDelete("media/{mediaId}")]
        public async Task<IActionResult> DeleteConferenceMedia(string mediaId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteConferenceMediaAsync(mediaId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Phương tiện hội nghị được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy phương tiện hội nghị"));
        }

        #endregion

        #region Step 6: Conference Sponsors

        [HttpPost("{conferenceId}/sponsors")]
        public async Task<IActionResult> AddConferenceSponsors(string conferenceId, [FromForm] AddConferenceSponsorsRequest request)
        {
            var sponsors = await _serviceManager.ConferenceStepService.AddConferenceSponsorsAsync(conferenceId, request);
            return Ok(ApiResponse<List<SponsorResponse>>.SuccessResponse(sponsors, "Nhà tài trợ hội nghị được thêm thành công"));
        }

        [HttpGet("{conferenceId}/sponsors")]
        public async Task<IActionResult> GetConferenceSponsors(string conferenceId)
        {
            var sponsors = await _serviceManager.ConferenceStepService.GetConferenceSponsorsAsync(conferenceId);
            return Ok(ApiResponse<List<SponsorResponse>>.SuccessResponse(sponsors, "Nhà tài trợ hội nghị được lấy thành công"));
        }

        [HttpPut("sponsors/{sponsorId}")]
        public async Task<IActionResult> UpdateSponsor(string sponsorId, [FromForm] UpdateSponsorRequest request)
        {
            var sponsor = await _serviceManager.ConferenceStepService.UpdateSponsorAsync(sponsorId, request);
            return Ok(ApiResponse<SponsorResponse>.SuccessResponse(sponsor, "Nhà tài trợ hội nghị được cập nhật thành công"));
        }

        [HttpDelete("sponsors/{sponsorId}")]
        public async Task<IActionResult> DeleteSponsor(string sponsorId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteSponsorAsync(sponsorId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Nhà tài trợ hội nghị được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy nhà tài trợ hội nghị"));
        }

        #endregion
    }
}