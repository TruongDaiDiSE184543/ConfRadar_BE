using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.DTOs.PresenterSession;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssigningPresenterSessionController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public AssigningPresenterSessionController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("Get-accepted-papers")]
        public async Task<IActionResult> GetAllAcceptedPaper()
        {
            var result = await _serviceManager.AssigningPresenterSessionService.GetAllAcceptedPaper();
            return Ok(ApiResponse<List<PaperDetailResponseDtoDetail>>.SuccessResponse(result, "Lấy thành công"));
        }

        //[Authorize(Roles = "Conference Organizer,Admin")]
        [HttpPost("assign-presenter-to-session")]
        public async Task<IActionResult> AssignPresenterToSession([FromBody] AssignPresenterToSessionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AssigningPresenterSessionService.AssignPresenterToSession(request.PaperId, request.SessionId);
            return Ok(ApiResponse<PresenterSessionResponse>.SuccessResponse(result, "Gán người trình bày vào phiên thành công"));
        }

        //[Authorize(Roles = "Customer,Collaborator,Local Reviewer,Conference Organizer,Admin")]
        [HttpPost("request-change-presenter")]
        public async Task<IActionResult> RequestChangePresenter([FromBody] CreatePresenterChangeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AssigningPresenterSessionService.ChangePresenterSession(userId,  request);
            return Ok(ApiResponse<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>.SuccessResponse(result, "Yêu cầu thay đổi người trình bày đã được gửi"));
        }

        //[Authorize(Roles = "Conference Organizer,Admin")]
        [HttpPost("approve-change-presenter")]
        public async Task<IActionResult> ApproveChangePresenter([FromBody] ApprovePresenterChangeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AssigningPresenterSessionService.ApprovePresenterChangeRequest(request, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xử lý yêu cầu thay đổi người trình bày thành công"));
        }

        //[Authorize(Roles = "Conference Organizer,Admin")]
        [HttpGet("get-pending-presenter-change-requests")]
        public async Task<IActionResult> GetPendingPresenterChangeRequests()
        {
            var result = await _serviceManager.AssigningPresenterSessionService.GetPendingPresenterChangeRequests();
            return Ok(ApiResponse<List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>>.SuccessResponse(result, "Lấy danh sách yêu cầu thay đổi người trình bày đang chờ thành công"));
        }

        //[Authorize(Roles = "Customer,Collaborator,Local Reviewer,Conference Organizer,Admin")]
        //[HttpPost("request-change-session")]
        //public async Task<IActionResult> RequestChangeSession([FromBody] CreateSessionChangeRequest request)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _serviceManager.AssigningPresenterSessionService.CreateSessionChangeRequest(request, userId);
        //    return Ok(ApiResponse<string>.SuccessResponse(result, "Yêu cầu thay đổi phiên đã được gửi"));
        //}

        //[Authorize(Roles = "Conference Organizer,Admin")]
        //[HttpPost("approve-change-session")]
        //public async Task<IActionResult> ApproveChangeSession([FromBody] ApproveSessionChangeRequest request)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _serviceManager.AssigningPresenterSessionService.ApproveSessionChangeRequest(request, userId);
        //    return Ok(ApiResponse<string>.SuccessResponse(result, "Xử lý yêu cầu thay đổi phiên thành công"));
        //}

        //[Authorize(Roles = "Conference Organizer,Admin")]
        //[HttpGet("get-pending-session-change-requests")]
        //public async Task<IActionResult> GetPendingSessionChangeRequests()
        //{
        //    var result = await _serviceManager.AssigningPresenterSessionService.GetPendingSessionChangeRequests();
        //    return Ok(ApiResponse<List<SessionChangeRequestResponse>>.SuccessResponse(result, "Lấy danh sách yêu cầu thay đổi phiên đang chờ thành công"));
        //}

        //[Authorize(Roles = "Conference Organizer,Admin")]
        //[HttpGet("get-all-presenter-sessions")]
        //public async Task<IActionResult> GetAllPresenterSessions()
        //{
        //    var result = await _serviceManager.AssigningPresenterSessionService.GetAllPresenterResponse();
        //    return Ok(ApiResponse<List<PresenterSessionResponse>>.SuccessResponse(result, "Lấy danh sách người trình bày và phiên thành công"));
        //}

        //[Authorize(Roles = "Conference Organizer,Admin")]
        //[HttpGet("get-presenter-session-by-session-paper")]
        //public async Task<IActionResult> GetPresenterSessionBySessionAndPaper(string sessionId, string paperId)
        //{
        //    var result = await _serviceManager.AssigningPresenterSessionService.GetPresentSessionbySessionAndPaperid(sessionId, paperId);
        //    return Ok(ApiResponse<PresenterSessionResponse>.SuccessResponse(result, "Lấy thông tin người trình bày cho phiên và bài báo thành công"));
        //}
    }

    public class AssignPresenterToSessionRequest
    {
        public string? PaperId { get; set; }
        public string? SessionId { get; set; }
    }
}