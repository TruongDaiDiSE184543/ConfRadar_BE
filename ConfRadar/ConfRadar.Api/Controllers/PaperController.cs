using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.DTOs.FullPaperReview;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Shared.DTO.Abstract;
using ConfRadar.Shared.DTO.Paper;
using ConfRadar.Shared.DTO.User;
using ConfRadar.Shared.DTO.WaitList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaperController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public PaperController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }



        [HttpPost("assign-author-to-paper")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AssignAuthorToPaper([FromBody] AssignAuthorToPaperRequest request)
        {
            var result = await _serviceManager.PaperAssignmentService.AssignAuthorToPaper(request);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Author assigned to paper successfully"));
        }

        [HttpPost("assign-reviewer-to-paper")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> AssignReviewerToPaper([FromBody] AssignReviewerToPaperRequest request)
        {
            var result = await _serviceManager.PaperAssignmentService.AssignReviewerToPaper(request);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Reviewer assigned to paper successfully"));
        }

        [Authorize]
        [HttpPost("submit-abstract")]
        public async Task<IActionResult> SubmitAbstract([FromForm] CreateAbstractRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.SubmitAbstract(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã nộp abstract thành công"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("decide-abstract-paper-status")]
        public async Task<IActionResult> DecideAbstractPaperStatus([FromBody] UpdateAbstractPaperStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.DecideAbstractPaperStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã quyết định abstract thành công"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("list-pending-abstract")]
        public async Task<IActionResult> ListPendingAbstract([FromQuery]string? confId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.GetListPendingAbstract(confId);
            return Ok(ApiResponse<List<PendingAbstractResponse>>.SuccessResponse(result, "danh sách pending abstract"));
        }
        [HttpGet("list-available-customers")]
        public async Task<IActionResult> ListAvailableCustomer()
        {
            var result = await _serviceManager.AuthService.GetAvailableCustomer();
            return Ok(ApiResponse<List<AvailableCustomerResponse>>.SuccessResponse(result, "danh sách các người dùng"));
        }


        [Authorize]
        [HttpPost("submit-fullpaper")]
        public async Task<IActionResult> SubmitFullPaper([FromForm] CreateFullPaperRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.SubmitFullPaper(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công full paper"));
        }
        [Authorize]
        [HttpPut("decide-fullpaper-status")]
        public async Task<IActionResult> DecideFullPaperStatus([FromBody] UpdateFullPaperStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.DecideFullPaperFinalStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã cập nhật thành công full paper status"));
        }


        [HttpGet("get-pending-fullpaper")]
        public async Task<IActionResult> GetPendingfullpaper()
        {
            var result = await _serviceManager.PaperService.ListPendingfullpaper();
            return Ok(ApiResponse<List<FullPaperDtoDetail>>.SuccessResponse(result, "Lấy thành công pending fullpaper"));
        }


        [HttpPost("submit-fullpaper-review")]
        //[Authorize(Roles = "Local Reviewer,External Reviewer")]
        public async Task<IActionResult> SubmitReviewForFullPaper([FromForm] CreateFullPaperReviewRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.SubmitReviewForFullPaper(request, userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Review submitted successfully for full paper"));
        }


        [HttpGet("get-fullpaper-reviews/{fullPaperId}")]
        [Authorize]
        public async Task<IActionResult> GetFullPaperReviewsByFullPaperId(string fullPaperId)
        {
            var result = await _serviceManager.PaperService.GetFullPaperReviewsByFullPaperId(fullPaperId);
            return Ok(ApiResponse<List<FullPaperReviewResponse>>.SuccessResponse(result, "Full paper reviews retrieved successfully"));
        }






        [Authorize]
        [HttpPost("submit-paper-revision")]
        public async Task<IActionResult> SubmitPaperRevision([FromForm] CreateRevisionPaperSubmissionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.CreateRevisionPaperSubmission(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công revision paper"));

        }
        [Authorize]
        [HttpPost("submit-paper-revision-feedback")]
        public async Task<IActionResult> SubmitPaperRevisionFeedback([FromBody] CreateRevisionPaperSubmissionFeedback request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.CreateRevisionSubmissionFeedBack(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công revision feedback"));

        }
        [Authorize]
        [HttpPut("submit-paper-revision-response")]
        public async Task<IActionResult> SubmitPaperRevisionFeedback([FromBody] CreateRevisionPaperSubmissionResponse request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.CreateRevisionSubmissionResponse(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công revision response"));
        }
        [Authorize]
        [HttpPost("submit-paper-revision-review")]
        public async Task<IActionResult> SubmitPaperRevisionReview([FromForm] CreateRevisionPaperReviewRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.CreateRevisionReview(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công revision review"));
        }
        [Authorize]
        [HttpPut("decide-revise-status")]
        public async Task<IActionResult> DecideReviseStatus([FromBody] UpdateRevisionStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.DecideReviseStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã cập nhật thành công status thành công cho giai đoạn revise"));
        }
        [Authorize]
        [HttpGet("list-revision-paper-review")]
        public async Task<IActionResult> ListRevisionPaperReview([FromQuery] ListRevisionPaperReviewRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.ListRevisionPaperReview(request, userId);
            return Ok(ApiResponse<List<RevisionPaperReviewResponse>>.SuccessResponse(result, "Danh sách paper reviewer trong revise phase"));
        }

        [HttpPost("submit-camera-ready")]
        [Authorize]
        public async Task<IActionResult> CreateCameraReady([FromForm] CreateCameraReadyRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.CreateCameraReady(request, userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Camera ready created successfully"));
        }

        [HttpPut("update-camera-ready")]
        [Authorize]
        public async Task<IActionResult> UpdateCameraReady([FromForm] UpdateCameraReadyRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.UpdateCameraReady(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Camera ready updated successfully"));
        }




        //[HttpPut("decide-fullpaper-review-status")]
        //[Authorize(Roles = "Conference Organizer")]
        //public async Task<IActionResult> DecideFullPaperReviewStatus([FromBody] UpdateFullPaperReviewStatusRequest request)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _serviceManager.PaperService.DecideFullPaperFinalStatus(request, userId);
        //    return Ok(ApiResponse<int>.SuccessResponse(result, "Full paper review status decided successfully"));
        //}

        [HttpPut("decide-camera-ready-status")]

        public async Task<IActionResult> DecideCameraReadyStatus([FromBody] UpdateCameraReadyStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.DecideCameraReadyStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã update status của camera ready thành công"));
        }

        [HttpGet("get-pending-cameraready")]
        public async Task<IActionResult> GetPendingCameraReady()
        {
            var result = await _serviceManager.PaperService.ListPendingCameraReady();
            return Ok(ApiResponse<List<CameraReadyDtoDetail>>.SuccessResponse(result, "Lấy thành công pending cameraready"));
        }


        //[HttpGet("get-assigned-papers-by-conferenceId")]
        //public async Task<IActionResult> GetAssignedPaperToReviewer( string conferenceId)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _serviceManager.PaperService.GetAllAssignedPapersToAReviewer(userId,conferenceId);
        //    return Ok(ApiResponse<List<PapersAssignedToReviewerResponse>>.SuccessResponse(result, "Lấy thành công papers đã assigned cho reviewer"));
        //}

        [HttpGet("get-assigned-papers")]
        public async Task<IActionResult> GetAssignedPapersByReviewerId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.GetAssignedPapersByReviewerId(userId);
            return Ok(ApiResponse<List<Paper>>.SuccessResponse(result, "Lấy thành công những paper được assigned theo reviewerId"));
        }



        [HttpGet("get-all-submitted-papers-for-customer")]
        public async Task<IActionResult> getSubmittedPapers()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.GetSubmittedPaper(userId);
            return Ok(ApiResponse<List<Paper>>.SuccessResponse(result, "Lấy thành công paper mà user đã nộp"));
        }

        [HttpGet("get-paper-detail-customer")]
        public async Task<IActionResult> getPaperDetail(string paperId)
        {

            var result = await _serviceManager.PaperService.getPaperDetail(paperId);
            return Ok(ApiResponse<PaperDetailResponseDtoDetail>.SuccessResponse(result, "Lấy detail paper thành công"));
        }

        [HttpGet("list-paper-phases")]
        public async Task<IActionResult> GetListPaperPhase()
        {
            var result = await _serviceManager.PaperService.GetListPaperPhases();
            return Ok(ApiResponse<List<PaperPhase>>.SuccessResponse(result, "Danh sách các paper phase"));
        }




        [HttpGet("list-all-papers")]
        public async Task<IActionResult> GetListAllPaper()
        {
            var result = await _serviceManager.PaperService.GetListAllPaper();
            return Ok(ApiResponse<List<PaperDetailResponseDTO>>.SuccessResponse(result, "Danh sách các paper"));
        }

        [HttpGet("list-unassign-abstract")]
        public async Task<IActionResult> GetUnassignAbstractList()
        {
            var result = await _serviceManager.PaperService.GetUnassignAbstractList();
            return Ok(ApiResponse<List<UnAssignAbstractResponse>>.SuccessResponse(result, "Danh sách các paper chưa được phân reviewer"));
        }



        [Authorize]
        [HttpGet("paper-detail-for-reviewer")]
        public async Task<IActionResult> GetPaperDetailForReviewer(string paperId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.GetPaperDetailForReviewer(paperId, userId);
            return Ok(ApiResponse<PaperDetailForReviewerResponse>.SuccessResponse(result, "Danh sách các paper"));
        }
        [Authorize]
        [HttpGet("list-customer-waitlist")]
        public async Task<IActionResult> GetCustomerWaitList()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.GetCustomerWaitList(userId);
            return Ok(ApiResponse<List<CustomerWaitListResponse>>.SuccessResponse(result, "Danh sách các hàng đợi của bạn"));
        }

        [Authorize]
        [HttpDelete("leave-waitlist")]
        public async Task<IActionResult> GetCustomerWaitList([FromBody] LeaveWaitListRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.LeaveWaitList(userId, request.ConferenceId);
            return Ok(ApiResponse<LeaveWaitListResponse>.SuccessResponse(result, "Đã thoát khỏi hàng đợi"));
        }
        [Authorize]
        [HttpPost("add-waitlist")]
        public async Task<IActionResult> AddCustomerToWaitList([FromBody] AddWaitListRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.AddWaitList(userId, request.ConferenceId);
            return Ok(ApiResponse<AddWaitListResponse>.SuccessResponse(result, "Đã thêm vào  hàng đợi"));
        }
    }
}
