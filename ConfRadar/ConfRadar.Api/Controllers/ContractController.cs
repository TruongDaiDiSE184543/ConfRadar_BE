using ConfRadar.Api.Responses;
using ConfRadar.Services;
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
        public async Task<IActionResult> CreateReviewerContract(CreateReviewerContractRequest request)
        {
            var result = await _serviceManager.ContractService.CreateReviewerContract(request);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã tạo hợp đồng thành công"));
        }

        [HttpGet("users-for-reviewer-contract")]
        public async Task<IActionResult> GetUsersForReviewerContract([FromQuery] GetUsersForReviewerContractRequest request)
        {
            var result = await _serviceManager.ContractService.GetUsersForReviewerContract(request);
            return Ok(ApiResponse<List<GetUsersForReviewerContractResponse>>.SuccessResponse(result, "Danh sách người dùng"));
        }

    }
}
