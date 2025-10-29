using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.DTOs.FullPaperReview;
using Microsoft.AspNetCore.Http;
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

        [Authorize]
        [HttpPost("submit-abstract")]
        public async Task<IActionResult> SubmitAbstract([FromForm]CreateAbstractRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.SubmitAbstract(request,userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã nộp abstract thành công"));
        }
        [Authorize]
        [HttpPut("decide-abstract-paper-status")]
        public async Task<IActionResult> DecideAbstractPaperStatus([FromBody] UpdateAbstractPaperStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.DecideAbstractPaperStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã quyết định abstract thành công"));
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
        [HttpPost("submit-paper-revision")]
        public async Task<IActionResult> SubmitPaperRevision([FromForm] CreateRevisionPaperSubmissionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.CreateRevisionPaperSubmission(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công revision paper"));

        }
        [Authorize]
        [HttpPost("submit-paper-revision-feedback")]
        public async Task<IActionResult> SubmitPaperRevisionFeedback([FromForm] CreateRevisionPaperSubmissionFeedback request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.CreateRevisionSubmissionFeedBack(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã gửi thành công revision feedback"));

        }
        [Authorize]
        [HttpPost("submit-paper-revision-response")]
        public async Task<IActionResult> SubmitPaperRevisionFeedback([FromForm] CreateRevisionPaperSubmissionResponse request)
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
        public async Task<IActionResult> ListRevisionPaperReview([FromQuery]ListRevisionPaperReviewRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.ListRevisionPaperReview(request, userId);
            return Ok(ApiResponse<List<RevisionPaperReviewResponse>>.SuccessResponse(result, "Danh sách paper reviewer trong revise phase"));
        }

        [HttpPost("create-camera-ready")]
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

        [HttpPost("submit-review-for-full-paper")]
        [Authorize(Roles = "Local Reviewer,External Reviewer")]
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

        //[HttpPut("decide-fullpaper-review-status")]
        //[Authorize(Roles = "Conference Organizer")]
        //public async Task<IActionResult> DecideFullPaperReviewStatus([FromBody] UpdateFullPaperReviewStatusRequest request)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _serviceManager.PaperService.DecideFullPaperFinalStatus(request, userId);
        //    return Ok(ApiResponse<int>.SuccessResponse(result, "Full paper review status decided successfully"));
        //}

        [HttpPut("decide-camera-ready-status")]
        [Authorize(Roles = "Conference Organizer")]
        public async Task<IActionResult> DecideCameraReadyStatus([FromBody] UpdateCameraReadyStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.DecideCameraReadyStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Camera ready status decided successfully"));
        }
    }
}
