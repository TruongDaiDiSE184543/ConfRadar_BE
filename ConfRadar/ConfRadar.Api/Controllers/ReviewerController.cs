using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.Reviewer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewerController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public ReviewerController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        // tổng số bài dc assign
        [HttpGet("stats/assigned")]
        [Authorize]
        public async Task<IActionResult> GetTotalAssignPapers()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ReviewerService.GetTotalAssignPapers(userId);
            return Ok(ApiResponse<GetTotalAssignPapersDetailResponse>.SuccessResponse(result, "Thông tin tổng số bài đã làm"));
        }
        [HttpGet("stats/reviewed")]
        [Authorize]
        public async Task<IActionResult> GetTotalReviewedPapers()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ReviewerService.GetTotalReviewedPapers(userId);
            return Ok(ApiResponse<GetTotalReviewedPapersDetailResponse>.SuccessResponse(result, "Thông tin số bài đã review"));
        }


        [HttpGet("stats/pending-reviews")]
        [Authorize]
        public async Task<IActionResult> GetTotalPendingReviews()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ReviewerService.GetTotalPendingReviews(userId);
            return Ok(ApiResponse<GetTotalPendingReviewsDetailResponse>.SuccessResponse(result, "Thông tin số bài đang pending"));
        }
    }
}
