using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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


        [HttpGet("confirm-registration-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            await _serviceManager.AuthService.VerifyRegistration(token);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Email confirmed successfully"));
        }




        [HttpPost("register")]
        public async Task<IActionResult> RegisterAccount([FromForm] CreateUserRequest request)
        {
            var result = await _serviceManager.AuthService.RegisterAccount(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Please check your email"));
        }
        [HttpPost("login")]
        public async Task<IActionResult> LocalLogin([FromBody] LocalLoginUserRequest request)
        {
            var loginResponse = await _serviceManager.AuthService.LocalLogin(request);
            return Ok(ApiResponse<LoginUserResponse>.SuccessResponse(loginResponse, "Login successful"));
        }
        [HttpPost("firebase-login")]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
        {
            var loginResponse = await _serviceManager.AuthService.FirebaseLogin(request);
            return Ok(ApiResponse<LoginUserResponse>.SuccessResponse(loginResponse, "Login successful"));
        }
        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            await _serviceManager.AuthService.ForgetPassword(email);
            return Ok(ApiResponse<object>.SuccessResponse(null, "An email has been sent to your mailbox"));
        }
        [HttpPost("verify-forget-password")]
        public async Task<IActionResult> VerifyForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            await _serviceManager.AuthService.VerifyForgetPassword(request.Token, request.Password);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _serviceManager.AuthService.ChangePassword(request.OldPassword, request.NewPassword, userId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Your password has been changed"));
        }
        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var refreshTokenResponse = await _serviceManager.AuthService.RefreshToken(userId!, request.Token);
            return Ok(ApiResponse<LoginUserResponse>.SuccessResponse(refreshTokenResponse, "Token refreshed successfully"));
        }
        [Authorize]
        [HttpPut("suspend-account")]
        public async Task<IActionResult> SuspendAccount(string userId)
        {
            var result = await _serviceManager.AuthService.SuspendAccount(userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Suspended user!"));
        }
        [Authorize]
        [HttpPut("activate-account")]
        public async Task<IActionResult> ActivateAccount(string userId)
        {
            var result = await _serviceManager.AuthService.ActivateAccount(userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Activated user!"));
        }
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm]ProfileUpdateRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.UpdateProfile(request, userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã update profile"));

        }
        [HttpGet("view-profile-by-id")]
        public async Task<IActionResult> ViewProfileDetail(string userId)
        {
            var result = await _serviceManager.AuthService.ViewUserDetail(userId);
            return Ok(ApiResponse<UserDetailResponse>.SuccessResponse(result, $"Thông tin user với id {userId}"));
        }

    }
}
