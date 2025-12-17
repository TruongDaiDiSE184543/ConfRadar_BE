using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.Collaborator;
using ConfRadar.Shared.DTO.Contract;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.ReviewContract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public ContractController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [Authorize]
        [HttpGet("conferences-for-outsourced-reviewer")]
        public async Task<IActionResult> GetConferencesForReviewerOutSourced()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.GetListConferenceBelongToReviewContractByUserId(userId);
            return Ok(ApiResponse<List<ConferenceBelongToReviewContractResponse>>.SuccessResponse(result, "Danh sách các conference trong hợp đồng"));
        }
        [Authorize]
        [HttpGet("papers-belong-to-conference")]
        public async Task<IActionResult> GetPapersBelongToAConference([FromQuery] string conferenceId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.GetPapersBelongToAConferenceByConferenceIdAndUserId(conferenceId, userId);
            return Ok(ApiResponse<List<PaperDetailBelongToConferenceInReviewContractResposne>>.SuccessResponse(result, "Danh sách các bài báo thuộc 1 hội nghị"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpPost("create-review-contract")]
        public async Task<IActionResult> CreateReviewerContract([FromForm] CreateReviewerContractRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.CreateReviewerContract(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Contract, $"đã tạo hợp đồng thành công cho reviewer với id {request.ReviewerId} thuộc hội nghị {request.ConferenceId}");

            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã tạo hợp đồng thành công"));
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPost("create-review-contract-for-new-user")]
        public async Task<IActionResult> CreateReviewerContractForNewUser([FromForm] CreateReviewerContractForNewUserRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.CreateReviewerContractForNewUser(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Contract, $"đã tạo hợp đồng thành công cho người dùng {request.FullName}");

            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã tạo hợp đồng thành công cho người dùng {request.FullName}"));
        }
        [Authorize]
        [HttpGet("list-own-review-contract")]
        public async Task<IActionResult> GetListOwnReviewContract()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.GetListOwnContract(userId);
            return Ok(ApiResponse<List<OwnContractDetailResponse>>.SuccessResponse(result, "Danh sách review contract"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("list-review-contract-by-reviewer")]
        public async Task<IActionResult> GetListReviewContractByReviewerId([FromQuery] string reviewerId)
        {
            var result = await _serviceManager.ContractService.GetListContractByReviewerId(reviewerId);
            return Ok(ApiResponse<List<ContractDetailResponseForOrganizer>>.SuccessResponse(result, "Danh sách review contract"));
        }

        //[Authorize(Roles ="Conference Organizer")]
        [HttpGet("users-for-reviewer-contract")]
        public async Task<IActionResult> GetUsersForReviewerContract([FromQuery] GetUsersForReviewerContractRequest request)
        {
            var result = await _serviceManager.ContractService.GetUsersForReviewerContract(request);
            return Ok(ApiResponse<List<GetUsersForReviewerContractResponse>>.SuccessResponse(result, "Danh sách người dùng"));
        }
        // số hợp đồng đã kí
        [HttpGet("external-contracts-count")]
        [Authorize]
        public async Task<IActionResult> GetUserExternalContractCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.GetOwnContractCount(userId);
            return Ok(ApiResponse<UserExternalContractCount>.SuccessResponse(result, "Tổng hợp đồng review của bạn"));

        }
        [HttpGet("external-contracts-active")]
        [Authorize]
        public async Task<IActionResult> GetUserActiveExternalContract()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.GetUserActiveExternalContract(userId);
            return Ok(ApiResponse<OwnActiveContractDetailResponse>.SuccessResponse(result, "Tổng hợp đồng review đang hoạt động của bạn"));

        }
        [HttpGet("external-contracts-wage-total")]
        [Authorize]
        public async Task<IActionResult> GetUserExternalWageTotal()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.GetUserExternalWageTotal(userId);
            return Ok(ApiResponse<UserExternalWageTotal>.SuccessResponse(result, "Lương của bạn"));

        }
        [HttpPost("create-collaborator-contract")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> CreateCollaboratorContract([FromForm] CollaboratorContractRequest request)
        {
            var result = await _serviceManager.ContractService.CreateCollaboratorContract(request);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã tạo hợp đồng"));

        }

        [HttpGet("list-collaborator-contract")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> GetListCollaboratorContract([FromQuery] CollaboratorContractSearchParam request)
        {
            var result = await _serviceManager.ContractService.GetListCollaboratorContract(request);
            return Ok(ApiResponse<PagedResultResponseDto<CollaboratorContractResponse>>.SuccessResponse(result, ""));

        }
        [HttpGet("own-collaborator-contract")]
        [Authorize(Roles = "Collaborator")]
        public async Task<IActionResult> GetListOwnCollaboratorContract()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.GetListOwnCollaboratorContract(userId);
            return Ok(ApiResponse<List<OwnCollaboratorContractDetailResponse>>.SuccessResponse(result, "Danh sách hợp đồng collaborator"));

        }

        [HttpPut("collaborator-contract")]
        [Authorize(Roles = "Conference Organizer")]

        public async Task<IActionResult> UpdateCollabContract([FromBody] UpdateCollabContractRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ContractService.UpdateCollabContract(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Contract, $"cập nhật hợp đồng collaborator với mã {request.CollaboratorContractId} thành công cho ");
            return Ok(ApiResponse<int>.SuccessResponse(result, "Cập nhật  hợp đồng collaborator  thành công"));

        }

    }
}
