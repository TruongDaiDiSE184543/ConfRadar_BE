using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.User;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public AuthController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAccount([FromForm] CreateUserRequest request)
        {
            var result = await _serviceManager.AuthService.RegisterAccount(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Please check your email"));
        }

        [HttpGet("confirm-registration-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery]string token)
        {
            await _serviceManager.AuthService.VerifyRegistration(token);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Email confirmed successfully"));
        }

    }
}
