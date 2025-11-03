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
    //[Authorize(Roles = "Conference Organizer")]
    public class ConferenceStepController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ConferenceStepController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        #region Step 1: Basic Conference Creation
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateConferenceBasic(string conferenceId, [FromForm] UpdateConferenceBasicRequest request)
        {
            var conference = await _serviceManager.ConferenceStepService.UpdateConferenceBasicAsync(conferenceId, request);
            return Ok(ApiResponse<TechnicalConferenceBasicStepResponse>.SuccessResponse(conference, "Thông tin hội nghị được cập nhật thành công"));
        }

        #endregion

        #region Step 2: Conference Prices


        [HttpPost("{conferenceId}/prices")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> AddConferencePrices(string conferenceId, [FromBody] AddConferencePricesRequest request)
        {
            var prices = await _serviceManager.ConferenceStepService.AddConferencePricesAsync(conferenceId, request);
            return Ok(ApiResponse<ConferencePriceListWithPhasesResponse>.SuccessResponse(prices, "Giá vé được thêm thành công"));
        }

        [HttpGet("{conferenceId}/prices")]
        public async Task<IActionResult> GetConferencePrices(string conferenceId)
        {
            var prices = await _serviceManager.ConferenceStepService.GetConferencePricesAsync(conferenceId);
            return Ok(ApiResponse<List<ConferencePriceWithPhasesResponse>>.SuccessResponse(prices, "Giá vé được lấy thành công"));
        }

        [HttpPut("prices/{priceId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateConferencePrice(string priceId, [FromBody] UpdateConferencePriceRequest request)
        {
            var price = await _serviceManager.ConferenceStepService.UpdateConferencePriceAsync(priceId, request);
            return Ok(ApiResponse<ConferencePriceWithPhasesResponse>.SuccessResponse(price, "Giá vé được cập nhật thành công"));
        }

        [HttpDelete("prices/{priceId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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
        //[Authorize(Roles = "Conference Organizer, Collaborator")]

        [HttpPost("{conferenceId}/sessions")]
        public async Task<IActionResult> AddConferenceSessions(string conferenceId, [FromForm] AddConferenceSessionsRequest request)
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
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateConferenceSession(string sessionId, [FromBody] UpdateConferenceSessionRequest request)
        {
            var session = await _serviceManager.ConferenceStepService.UpdateConferenceSessionAsync(sessionId, request);
            return Ok(ApiResponse<ConferenceSessionWithMediaResponse>.SuccessResponse(session, "Phiên hội nghị được cập nhật thành công"));
        }

        [HttpPut("sessions/{sessionId}/speaker")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateSpeaker(string sessionId, [FromBody] UpdateSpeakerRequest request)
        {
            var speaker = await _serviceManager.ConferenceStepService.UpdateSpeakerAsync(sessionId, request);
            return Ok(ApiResponse<SpeakerResponse>.SuccessResponse(speaker, "Diễn giả được cập nhật thành công"));
        }

        [HttpDelete("sessions/{sessionId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> DeleteConferenceSession(string sessionId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteConferenceSessionAsync(sessionId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Phiên hội nghị được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy phiên hội nghị"));
        }

        //private async Task<bool> CheckIfEachConferenceDateHasSession(Conference conferenece, List<DateOnly> sessionDate)
        //{
        //    List<DateOnly> conferenceDate = new();
        //    for (DateOnly date = conferenece.StartDate.Value; date <= conferenece.EndDate; date = date.AddDays(1)) { 

        //    }
        //}

        #endregion

        #region Step 4: Conference Policies

        [HttpPost("{conferenceId}/policies")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateConferencePolicy(string policyId, [FromBody] UpdateConferencePolicyRequest request)
        {
            var policy = await _serviceManager.ConferenceStepService.UpdateConferencePolicyAsync(policyId, request);
            return Ok(ApiResponse<ConferencePolicyResponse>.SuccessResponse(policy, "Chính sách hội nghị được cập nhật thành công"));
        }

        [HttpDelete("policies/{policyId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateConferenceMedia(string mediaId, [FromForm] UpdateConferenceMediaRequest request)
        {
            var media = await _serviceManager.ConferenceStepService.UpdateConferenceMediaAsync(mediaId, request);
            return Ok(ApiResponse<ConferenceMediaResponse>.SuccessResponse(media, "Phương tiện hội nghị được cập nhật thành công"));
        }

        [HttpDelete("media/{mediaId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateSponsor(string sponsorId, [FromForm] UpdateSponsorRequest request)
        {
            var sponsor = await _serviceManager.ConferenceStepService.UpdateSponsorAsync(sponsorId, request);
            return Ok(ApiResponse<SponsorResponse>.SuccessResponse(sponsor, "Nhà tài trợ hội nghị được cập nhật thành công"));
        }

        [HttpDelete("sponsors/{sponsorId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
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

        #region Step 7: Refund Policies
        [Authorize(Roles = "Conference Organizer, Collaborator")]

        [HttpPost("{conferenceId}/refund-policies")]
        public async Task<IActionResult> AddRefundPolicies(string conferenceId, [FromBody] AddRefundPoliciesRequest request)
        {
            var refundPolicies = await _serviceManager.ConferenceStepService.AddRefundPoliciesAsync(conferenceId, request);
            return Ok(ApiResponse<List<RefundPolicyResponse>>.SuccessResponse(refundPolicies, "Chính sách hoàn trả được thêm thành công"));
        }

        [HttpGet("{conferenceId}/refund-policies")]
        public async Task<IActionResult> GetRefundPolicies(string conferenceId)
        {
            var refundPolicies = await _serviceManager.ConferenceStepService.GetRefundPoliciesAsync(conferenceId);
            return Ok(ApiResponse<List<RefundPolicyResponse>>.SuccessResponse(refundPolicies, "Chính sách hoàn trả được lấy thành công"));
        }

        [HttpPut("refund-policies/{refundPolicyId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateRefundPolicy(string refundPolicyId, [FromBody] UpdateRefundPolicyRequest request)
        {
            var refundPolicy = await _serviceManager.ConferenceStepService.UpdateRefundPolicyAsync(refundPolicyId, request);
            return Ok(ApiResponse<RefundPolicyResponse>.SuccessResponse(refundPolicy, "Chính sách hoàn trả được cập nhật thành công"));
        }

        [HttpDelete("refund-policies/{refundPolicyId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> DeleteRefundPolicy(string refundPolicyId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteRefundPolicyAsync(refundPolicyId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Chính sách hoàn trả được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy chính sách hoàn trả"));
        }
        #endregion


        #region Research Conference Step 2: Research Conference Detail

        [HttpPost("{conferenceId}/research/detail")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CreateResearchConferenceDetail(string conferenceId, [FromBody] CreateResearchConferenceDetailRequest request)
        {
            var detail = await _serviceManager.ConferenceStepService.CreateResearchConferenceDetailAsync(conferenceId, request);
            return Ok(ApiResponse<ResearchConferenceDetailResponse>.SuccessResponse(detail, "Chi tiết hội nghị nghiên cứu được tạo thành công"));
        }

        [HttpGet("{conferenceId}/research/detail")] 

        public async Task<IActionResult> GetResearchConferenceDetail(string conferenceId)
        {
            var detail = await _serviceManager.ConferenceStepService.GetResearchConferenceDetailAsync(conferenceId);
            return Ok(ApiResponse<ResearchConferenceDetailResponse>.SuccessResponse(detail, "Chi tiết hội nghị nghiên cứu được lấy thành công"));
        }

        [HttpPut("{conferenceId}/research/detail")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> UpdateResearchConferenceDetail(string conferenceId, [FromBody] UpdateResearchConferenceDetailRequest request)
        {
            var detail = await _serviceManager.ConferenceStepService.UpdateResearchConferenceDetailAsync(conferenceId, request);
            return Ok(ApiResponse<ResearchConferenceDetailResponse>.SuccessResponse(detail, "Chi tiết hội nghị nghiên cứu được cập nhật thành công"));
        }

        #endregion

        #region Research Conference Step 3: Research Conference Phases

        [HttpPost("{conferenceId}/research/phases")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CreateResearchConferencePhase(string conferenceId, [FromBody] CreateResearchConferencePhaseRequest request)
        {
            var phase = await _serviceManager.ConferenceStepService.CreateResearchConferencePhaseAsync(conferenceId, request);
            return Ok(ApiResponse<ResearchConferencePhaseResponse>.SuccessResponse(phase, "Giai đoạn hội nghị nghiên cứu được tạo thành công"));
        }

        [HttpGet("{conferenceId}/research/phases")]
        public async Task<IActionResult> GetResearchConferencePhase(string conferenceId)
        {
            var phase = await _serviceManager.ConferenceStepService.GetResearchConferencePhaseAsync(conferenceId);
            return Ok(ApiResponse<ResearchConferencePhaseResponse>.SuccessResponse(phase, "Giai đoạn hội nghị nghiên cứu được lấy thành công"));
        }

        [HttpPut("research/phases/{phaseId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> UpdateResearchConferencePhase(string phaseId, [FromBody] UpdateResearchConferencePhaseRequest request)
        {
            var phase = await _serviceManager.ConferenceStepService.UpdateResearchConferencePhaseAsync(phaseId, request);
            return Ok(ApiResponse<ResearchConferencePhaseResponse>.SuccessResponse(phase, "Giai đoạn hội nghị nghiên cứu được cập nhật thành công"));
        }

        #endregion

        #region Research Conference Step 5: Material Downloads

        [HttpPost("{conferenceId}/research/materials")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CreateMaterialDownload(string conferenceId, [FromForm] CreateMaterialDownloadRequest request)
        {
            var material = await _serviceManager.ConferenceStepService.CreateMaterialDownloadAsync(conferenceId, request);
            return Ok(ApiResponse<MaterialDownloadResponse>.SuccessResponse(material, "Tài liệu tải về được tạo thành công"));
        }

        [HttpGet("{conferenceId}/research/materials")]
        public async Task<IActionResult> GetMaterialDownloads(string conferenceId)
        {
            var materials = await _serviceManager.ConferenceStepService.GetMaterialDownloadsByConferenceIdAsync(conferenceId);
            return Ok(ApiResponse<List<MaterialDownloadResponse>>.SuccessResponse(materials, "Tài liệu tải về được lấy thành công"));
        }

        [HttpPut("research/materials/{materialDownloadId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> UpdateMaterialDownload(string materialDownloadId, [FromForm] UpdateMaterialDownloadRequest request)
        {
            var material = await _serviceManager.ConferenceStepService.UpdateMaterialDownloadAsync(materialDownloadId, request);
            return Ok(ApiResponse<MaterialDownloadResponse>.SuccessResponse(material, "Tài liệu tải về được cập nhật thành công"));
        }

        [HttpDelete("research/materials/{materialDownloadId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> DeleteMaterialDownload(string materialDownloadId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteMaterialDownloadAsync(materialDownloadId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Tài liệu tải về được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy tài liệu tải về"));
        }

        #endregion

        #region Research Conference Step 6: Ranking File URLs

        [HttpPost("{conferenceId}/research/ranking-file-urls")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CreateRankingFileUrl(string conferenceId, [FromForm] CreateRankingFileUrlRequest request)
        {
            var fileUrl = await _serviceManager.ConferenceStepService.CreateRankingFileUrlAsync(conferenceId, request);
            return Ok(ApiResponse<RankingFileUrlResponse>.SuccessResponse(fileUrl, "URL tệp xếp hạng được tạo thành công"));
        }

        [HttpGet("{conferenceId}/research/ranking-file-urls")]
        public async Task<IActionResult> GetRankingFileUrls(string conferenceId)
        {
            var fileUrls = await _serviceManager.ConferenceStepService.GetRankingFileUrlsByConferenceIdAsync(conferenceId);
            return Ok(ApiResponse<List<RankingFileUrlResponse>>.SuccessResponse(fileUrls, "URL tệp xếp hạng được lấy thành công"));
        }

        [HttpPut("research/ranking-file-urls/{rankingFileUrlId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> UpdateRankingFileUrl(string rankingFileUrlId, [FromForm] UpdateRankingFileUrlRequest request)
        {
            var fileUrl = await _serviceManager.ConferenceStepService.UpdateRankingFileUrlAsync(rankingFileUrlId, request);
            return Ok(ApiResponse<RankingFileUrlResponse>.SuccessResponse(fileUrl, "URL tệp xếp hạng được cập nhật thành công"));
        }

        [HttpDelete("research/ranking-file-urls/{rankingFileUrlId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> DeleteRankingFileUrl(string rankingFileUrlId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteRankingFileUrlAsync(rankingFileUrlId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "URL tệp xếp hạng được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy URL tệp xếp hạng"));
        }

        #endregion

        #region Research Conference Step 1: Basic Research Conference Creation

        [HttpPost("research/basic")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CreateResearchConferenceBasic([FromForm] CreateResearchConferenceBasicRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.FailResponse("Người dùng chưa xác thực"));
            }

            var conference = await _serviceManager.ConferenceStepService.CreateResearchConferenceBasicAsync(request, userId);
            return Ok(ApiResponse<ResearchConferenceBasicStepResponse>.SuccessResponse(conference, "Hội nghị nghiên cứu được tạo thành công"));
        }

        [HttpGet("{conferenceId}/research/basic")]
        public async Task<IActionResult> GetResearchConferenceBasic(string conferenceId)
        {
            var conference = await _serviceManager.ConferenceStepService.GetResearchConferenceBasicAsync(conferenceId);
            return Ok(ApiResponse<ResearchConferenceBasicStepResponse>.SuccessResponse(conference, "Thông tin hội nghị nghiên cứu được lấy thành công"));
        }

        [HttpPut("{conferenceId}/research/basic")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> UpdateResearchConferenceBasic(string conferenceId, [FromForm] UpdateConferenceBasicRequest request)
        {
            var conference = await _serviceManager.ConferenceStepService.UpdateResearchConferenceBasicAsync(conferenceId, request);
            return Ok(ApiResponse<ResearchConferenceBasicStepResponse>.SuccessResponse(conference, "Thông tin hội nghị nghiên cứu được cập nhật thành công"));
        }

        #endregion

        #region Research Conference Step 4: Research Conference Sessions (without speakers)

        [HttpPost("{conferenceId}/research/sessions")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> AddResearchSessions(string conferenceId, [FromForm] AddResearchSessionsRequest request)
        {
            var sessions = await _serviceManager.ConferenceStepService.AddResearchSessionsAsync(conferenceId, request);
            return Ok(ApiResponse<List<ResearchSessionWithMediaResponse>>.SuccessResponse(sessions, "Phiên hội nghị nghiên cứu được thêm thành công"));
        }

        [HttpGet("{conferenceId}/research/sessions")]
        public async Task<IActionResult> GetResearchSessions(string conferenceId)
        {
            var sessions = await _serviceManager.ConferenceStepService.GetResearchSessionsAsync(conferenceId);
            return Ok(ApiResponse<List<ResearchSessionWithMediaResponse>>.SuccessResponse(sessions, "Phiên hội nghị nghiên cứu được lấy thành công"));
        }

        [HttpPut("research/sessions/{sessionId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> UpdateResearchSession(string sessionId, [FromBody] UpdateConferenceSessionRequest request)
        {
            var session = await _serviceManager.ConferenceStepService.UpdateResearchSessionAsync(sessionId, request);
            return Ok(ApiResponse<ResearchSessionWithMediaResponse>.SuccessResponse(session, "Phiên hội nghị nghiên cứu được cập nhật thành công"));
        }

        [HttpDelete("research/sessions/{sessionId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> DeleteResearchSession(string sessionId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteResearchSessionAsync(sessionId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Phiên hội nghị nghiên cứu được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy phiên hội nghị nghiên cứu"));
        }

        #endregion

        #region Research Conference Step 7: Ranking Reference URLs

        [HttpPost("{conferenceId}/research/ranking-reference-urls")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CreateRankingReferenceUrl(string conferenceId, [FromBody] CreateRankingReferenceUrlRequest request)
        {
            var referenceUrl = await _serviceManager.ConferenceStepService.CreateRankingReferenceUrlAsync(conferenceId, request);
            return Ok(ApiResponse<RankingReferenceUrlResponse>.SuccessResponse(referenceUrl, "URL tham khảo xếp hạng được tạo thành công"));
        }

        [HttpGet("{conferenceId}/research/ranking-reference-urls")]
        public async Task<IActionResult> GetRankingReferenceUrls(string conferenceId)
        {
            var referenceUrls = await _serviceManager.ConferenceStepService.GetRankingReferenceUrlsByConferenceIdAsync(conferenceId);
            return Ok(ApiResponse<List<RankingReferenceUrlResponse>>.SuccessResponse(referenceUrls, "URL tham khảo xếp hạng được lấy thành công"));
        }

        [HttpPut("research/ranking-reference-urls/{referenceUrlId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> UpdateRankingReferenceUrl(string referenceUrlId, [FromBody] UpdateRankingReferenceUrlRequest request)
        {
            var referenceUrl = await _serviceManager.ConferenceStepService.UpdateRankingReferenceUrlAsync(referenceUrlId, request);
            return Ok(ApiResponse<RankingReferenceUrlResponse>.SuccessResponse(referenceUrl, "URL tham khảo xếp hạng được cập nhật thành công"));
        }

        [HttpDelete("research/ranking-reference-urls/{referenceUrlId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> DeleteRankingReferenceUrl(string referenceUrlId)
        {
            var result = await _serviceManager.ConferenceStepService.DeleteRankingReferenceUrlAsync(referenceUrlId);
            if (result)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "URL tham khảo xếp hạng được xóa thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy URL tham khảo xếp hạng"));
        }

        #endregion


    }
}