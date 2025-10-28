using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Abstract;
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
        public async Task<IActionResult> SubmitFullPaper([FromForm] CreateAbstractRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaperService.SubmitAbstract(request, userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, "nộp full paper "));
        }
    }
}
