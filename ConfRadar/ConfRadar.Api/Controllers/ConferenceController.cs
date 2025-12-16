using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Conference;
using ConfRadar.Services.DTOs.General;
using ConfRadar.Services.DTOs.Ticket;
using ConfRadar.Shared.DTO.Conference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
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
            return Ok(ApiResponse<PagedResult<ConferenceResponseDTO>>.SuccessResponse(conferences, "Conferences retrieved successfully"));
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
            [FromQuery] DateOnly? endDate = null,
            [FromQuery] bool? isResearch = null,
            [FromQuery] string? rankingCategoryId = null,
            [FromQuery] bool? allowListener = null,
            [FromQuery] bool? noSubmitFee = null,
            [FromQuery] int? totalRevisionRound = null,
            [FromQuery] string? containTargetAudience = null
            )
        {
            var conferences = await _serviceManager.ConferenceService.GetConferencesWithPricesAsync(page, pageSize, searchKeyword, cityId, startDate, endDate, isResearch, rankingCategoryId, allowListener, noSubmitFee, totalRevisionRound, containTargetAudience);
            return Ok(ApiResponse<PagedResult<ConferenceWithPricesResponse>>.SuccessResponse(conferences, "Conferences with prices retrieved successfully"));
        }

        [HttpGet("technical-detail-for-anon/{conferenceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTechnicalConferenceDetail(string conferenceId)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var conferenceDetail = await _serviceManager.ConferenceService.GetTechnicalConferenceDetailAsync(conferenceId, userId);
            return Ok(ApiResponse<TechnicalConferenceDetailResponse>.SuccessResponse(conferenceDetail, "Technical conference detail retrieved successfully"));
        }

        [HttpGet("research-detail-for-anon/{conferenceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetResearchConferenceDetail(string conferenceId)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var conferenceDetail = await _serviceManager.ConferenceService.GetResearchConferenceDetailAsync(conferenceId, userId);
            return Ok(ApiResponse<ResearchConferenceDetailResponse>.SuccessResponse(conferenceDetail, "Research conference detail retrieved successfully"));
        }

        [Authorize]
        [HttpPost("submit-conference-feedback")]
        public async Task<IActionResult> SubmitConferenceFeedback(CreateConferenceFeedbackRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.SubmitConferenceFeedback(request, userId);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"đã gửi thành công đánh giá cho hội nghị với phiên {request.ConferenceSessionId} với đánh giá {request.Rating} sao");
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công đánh giá"));
        }

        // NEW ENDPOINT 3: Get conferences by status ID with filtering
        [HttpGet("by-status/{conferenceStatusId}")]
        [Authorize(Roles = "Conference Organizer")]
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
            return Ok(ApiResponse<PagedResult<ConferenceResponseDTO>>.SuccessResponse(conferences, "Conferences retrieved successfully"));
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
            var conferences = await _serviceManager.ConferenceService.GetTechnicalConferencesStepCompletionStatusAsync(page, pageSize, searchKeyword, cityId, startDate, endDate);
            return Ok(ApiResponse<PagedResult<ConferenceStepCompletionStatusResponse>>.SuccessResponse(conferences, "Conference step completion status retrieved successfully"));
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
        [HttpGet("technical-conferences-by-Organizer")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> GetTechnicalConferencesListByOrganizer(
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

            var conferences = await _serviceManager.ConferenceService.GetTechnicalConferencesListByOrganizerAsync(
                page, pageSize, conferenceStatusId, searchKeyword, cityId, startDate, endDate, userId, isOrganizer);
            return Ok(ApiResponse<PagedResult<Services.DTOs.Conference.TechnicalConferenceDetailResponse>>.SuccessResponse(conferences, "Technical conferences retrieved successfully"));
        }

        [HttpGet("technical-conferences-by-collaborator-no-draft")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> GetTechnicalConferencesByCollaboratorListNoDraft(
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10,
         [FromQuery] string? conferenceStatusId = null,
         [FromQuery] string? searchKeyword = null,
         [FromQuery] string? cityId = null,
         [FromQuery] DateOnly? startDate = null,
         [FromQuery] DateOnly? endDate = null,
         [FromQuery] string? collaboratorId = null, // Đã sửa tên
         [FromQuery] string? organizationName = null) // Đã sửa tên
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isOrganizer = User.IsInRole("Conference Organizer");

            var conferences = await _serviceManager.ConferenceService.GetTechnicalConferencesListByCollaboratorNoDraftAsync(
                page, pageSize, conferenceStatusId, searchKeyword, cityId, startDate, endDate, userId, isOrganizer, collaboratorId, organizationName);
            return Ok(ApiResponse<PagedResult<Services.DTOs.Conference.TechnicalConferenceDetailResponse>>.SuccessResponse(conferences, "Technical conferences retrieved successfully"));
        }

        [HttpGet("technical-conferences-by-collaborator-only-draft")]
        [Authorize(Roles = " Collaborator")]
        public async Task<IActionResult> GetTechnicalConferencesByCollaboratorListOnlyDraft(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchKeyword = null,
        [FromQuery] string? cityId = null,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] string? collaboratorId = null, // Đã sửa tên
        [FromQuery] string? organizationName = null) // Đã sửa tên
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isOrganizer = User.IsInRole("Conference Organizer");

            var conferences = await _serviceManager.ConferenceService.GetTechnicalConferencesListByCollaboratorOnlyDraftAsync(
                page, pageSize, searchKeyword, cityId, startDate, endDate, userId, isOrganizer, collaboratorId, organizationName);
            return Ok(ApiResponse<PagedResult<Services.DTOs.Conference.TechnicalConferenceDetailResponse>>.SuccessResponse(conferences, "Technical conferences retrieved successfully"));
        }



        // NEW ENDPOINT 14: Get detailed research conference data for organizer with timeline
        [HttpGet("detail-research-organizer-for-organizer/{conferenceId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> GetDetailResearchForOrganizer(string conferenceId)
        {
            var conferenceDetail = await _serviceManager.ConferenceService.GetDetailResearchForOrganizerAsync(conferenceId);
            return Ok(ApiResponse<ResearchConferenceDetailResponse>.SuccessResponse(conferenceDetail, "Research conference detail retrieved successfully with timeline"));
        }

        // NEW ENDPOINT 15: Get detailed technical conference data for organizer and collaborator with timeline
        [HttpGet("detail-technical-for-organizer-collaborator/{conferenceId}")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> GetDetailTechnical(string conferenceId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isOrganizer = User.IsInRole("Conference Organizer");

            var conferenceDetail = await _serviceManager.ConferenceService.GetDetailTechnicalAsync(conferenceId, userId, isOrganizer);
            return Ok(ApiResponse<TechnicalConferenceDetailResponse>.SuccessResponse(conferenceDetail, "Technical conference detail retrieved successfully with timeline"));
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("get-skeleton-tech-conf-created-for-collaborator")]
        public async Task<IActionResult> GetSkeletonConferenceBasicForCollaboratorToBuildOn([FromQuery] string collaboratorId)
        {
            var conferenceList = await _serviceManager.ConferenceService.getSkeletonTechConf(collaboratorId);
            return Ok(ApiResponse<List<SkeletonTechConfResponse>>.SuccessResponse(conferenceList, $"Láy thành công những conference tạo cho collaborator với ID {collaboratorId}"));
        }



        [HttpPut("request-a-conference-to-be-approved")]
        [Authorize(Roles = "Collaborator")]
        public async Task<IActionResult> RequestPendingConference([FromQuery] string confId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.RequestOrganizerApproval(confId, userId);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"gửi yêu cầu duyệt cho sự kiện với id {confId}");
            if (result) return Ok(ApiResponse<bool>.SuccessResponse(result, "Gửi yêu cầu duyệt cho conference thành công"));
            return Ok(ApiResponse<bool>.FailResponse("Gửi yêu cầu duyệt cho conference thất bại"));
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
            return Ok(ApiResponse<PagedResult<ConferenceResponseDTO>>.SuccessResponse(conferences, "Pending conferences retrieved successfully"));
        }


        // NEW ENDPOINT 6: Approve conference (change status from pending to preparing)
        [HttpPut("approve-conference/{conferenceId}")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> ApproveConference(string conferenceId, [FromBody] ApproveConferenceRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.ApproveConferenceAsync(conferenceId, request);
            if (result)
            {
                await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"duyệt hội nghị {conferenceId} thành công");
                return Ok(ApiResponse<object>.SuccessResponse(null, "Duyệt hội nghị thành công"));
            }
            return NotFound(ApiResponse<object>.FailResponse("Conference không tìm thấy hay được xét duyệt"));
        }

        [HttpPut("disable-conference")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> DisablingConference([FromQuery] string conferenceId, [FromQuery] string? reason = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.DisableContractedConference(conferenceId, reason);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"vô hiệu hóa hội nghị {conferenceId} ");
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Disable Hội nghị thành công"));
        }


        [HttpPut("transition-conference-from-disable-to-ready")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> ActivateConference([FromQuery] string conferenceId, [FromQuery] string? reason = null)
        {
            var result = await _serviceManager.ConferenceService.ToReadyFromDisabledContractedConference(conferenceId, reason);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Cập nhật trạng thái từ disabled về ready thành công"));
        }


        [HttpGet("get-own-conferences")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> GetOwnConference(string? statusId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.GetAllConferenceWithStatusByUserId(userId, statusId);
            return Ok(ApiResponse<List<ConferenceWithStatusNameResponse>>.SuccessResponse(result, "User conferences retrieved successfully"));
        }


        //NEW ENDPOINT 13: update conf status
        [HttpPut("Update-own-conference-Status")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> UpdateConferenceStatus(string confid, string newStatus, string? reason = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.ChangeConferenceStatus(userId, confid, newStatus, reason);
            if (result)
            {
                await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"cập nhật trạng thái hội nghị {confid} thành công");
                return Ok(ApiResponse<bool>.SuccessResponse(result, "cập nhật trạng thái hội nghị thành công"));

            }
            else
            {
                return Ok(ApiResponse<bool>.FailResponse("cập nhật Hội nghị thất bại"));
            }
        }

        [Authorize]
        [HttpGet("own-conferences-for-schedule")]
        public async Task<IActionResult> GetListConferencesForSchedule()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.GetListConferencesForScheduleByUserId(userId);
            return Ok(ApiResponse<List<ConferenceDetailForScheduleResponse>>.SuccessResponse(result, "Danh sách hội nghị"));
        }


        [Authorize(Roles = "Local Reviewer")]
        [HttpGet("get-conferences-assigned-papers-belong-to")]
        public async Task<IActionResult> GetAssignConferenceList()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.GetConferenceByAssignedPapers(userId);
            return Ok(ApiResponse<List<ConferenceResponseDTO>>.SuccessResponse(result, "Lấy thành công danh sách conference có papers được assigned cho local reviewer"));
        }


        [HttpPut("activate-next-phase")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> WaitListBegin([FromQuery] string confId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.ActivateNextPhase(confId, userId);
            if (result)
            {
                await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"kích hoạt giai đoạn kế tiếp thành công cho hội nghị {confId}");
                return Ok(ApiResponse<bool>.SuccessResponse(result, "kích hoạt giai đoạn kế tiếp thành công"));
            }
            return Ok(ApiResponse<bool>.FailResponse("kích hoạt giai đoạn kế tiếp thất bại"));
        }

        [HttpPut("add-days-since-last-onhold")]
        [Authorize(Roles = "Conference Organizer, Collaborator")]
        public async Task<IActionResult> AddDaysFromLastOnHold([FromQuery] string confId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceService.AutoAdjustTimelineForOnHoldAsync(confId, userId);
            if (result) return Ok(ApiResponse<bool>.SuccessResponse(result, "Thêm ngày dựa trên ngày onhold thành công"));
            else return Ok(ApiResponse<bool>.FailResponse("Thêm ngày dựa trên ngày onhold thất bại"));
        }
    }
}