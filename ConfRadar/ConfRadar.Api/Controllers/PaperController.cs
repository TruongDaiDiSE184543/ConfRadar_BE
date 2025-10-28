using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.Paper;
using Microsoft.AspNetCore.Http;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.DTOs.RevisionPaper;
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
            return Ok(ApiResponse<string>.SuccessResponse(result, "hãy truy cập link để thực hiện thanh toán"));
        }
        [HttpPost("submit-fullpaper")]
        public async Task<IActionResult> SubmitFullPaper([FromForm] CreateFullPaperRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.SubmitFullPaper(request, userId);
            return Ok(ApiResponse<FullPaperResponse>.SuccessResponse(result, "nộp full paper "));
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
        [HttpPost("submit-fullpaper")]
        public async Task<IActionResult> SubmitFullPaper([FromForm] UpdateFullPaperRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.UpdateFullPaper(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã cập nhật thành công full paper"));
        }
        [Authorize]
        [HttpPut("decide-fullpaper-status")]
        public async Task<IActionResult> DecideFullPaperStatus([FromBody] UpdateFullPaperStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.DecideFullPaperStatus(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã cập nhật thành công full paper status"));

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
    }
}
