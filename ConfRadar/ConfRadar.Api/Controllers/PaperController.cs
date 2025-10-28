using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.Paper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    }
}
