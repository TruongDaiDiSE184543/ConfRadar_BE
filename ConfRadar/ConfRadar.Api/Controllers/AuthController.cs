using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.User;
using ConfRadar.Shared.DTO.Collaborator;
using ConfRadar.Shared.DTO.Organization;
using ConfRadar.Shared.DTO.User;
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
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã đình chỉ account này!"));
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
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileUpdateRequest request)
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
        [Authorize(Roles = "Conference Organizer,Admin")]
        [HttpGet("list-users")]
        public async Task<IActionResult> GetUserListForAdminAndOrganizer()
        {
            var result = await _serviceManager.AuthService.ListUserForAdminAndOrganizer();
            return Ok(ApiResponse<List<ListUserDetailForAdminAndOrganizerResponse>>.SuccessResponse(result, $"Danh sách người dùng:"));
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPost("create-collaborator-account")]
        public async Task<IActionResult> CreateCollaboratorAccount([FromBody] CreateCollaboratorAccountRequest request)
        {
            var result = await _serviceManager.AuthService.CreateCollaboratorAccount(request);
            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã tạo account cho collaborator"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("users-for-collaborator-create")]
        public async Task<IActionResult> GetUsersForCollaboratorCreate()
        {
            var result = await _serviceManager.AuthService.GetUsersForCollaboratorCreate();
            return Ok(ApiResponse<List<GetUsersForCollaboratorCreateResponse>>.SuccessResponse(result, $"Danh sách người dùng cho việc tạo collaborator account"));
        }

        [HttpGet("list-all-reviewers")]
        public async Task<IActionResult> ListAllReviewer()
        {
            var result = await _serviceManager.AuthService.ListAllReviewer();
            return Ok(ApiResponse<List<ReviewerDetailResponse>>.SuccessResponse(result, $"Danh sách reviewer"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("suspend-external-reviewer/{userId}")]
        public async Task<IActionResult> SuspendExternalReviewer([FromRoute] string userId)
        {
            var result = await _serviceManager.AuthService.SuspendExternalReviewerAccount(userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã suspend người reviewer outsource với id {userId}"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("activate-external-reviewer/{userId}")]
        public async Task<IActionResult> ActivateExternalReviewer([FromRoute] string userId)
        {
            var result = await _serviceManager.AuthService.ActivateExternalReviewerAccount(userId);
            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã activate người reviewer outsource với id {userId}"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpPost("create-local-reviewer-account")]
        public async Task<IActionResult> CreateLocalReviewerAccount([FromBody] CreateLocalReviewerAccountRequest request)
        {
            var result = await _serviceManager.AuthService.CreateLocalReviewerAccount(request);
            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã tạo account cho local reviewer"));
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("list-organization")]
        public async Task<IActionResult> GetListOrganization()
        {
            var result = await _serviceManager.AuthService.GetListOrganization();
            return Ok(ApiResponse<List<OrganizationDetailResponse>>.SuccessResponse(result, $"Danh sách organization"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("update-organization")]
        public async Task<IActionResult> UpdateOrganization([FromBody] OrganizationUpdateRequest request)
        {
            var result = await _serviceManager.AuthService.UpdateOrganization(request);
            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã update"));
        }
        [HttpGet("list-collaborator-accounts")]
        public async Task<IActionResult> GetListCollaboratorAccounts()
        {
            var result = await _serviceManager.AuthService.GetListCollaboratorAccounts();
            return Ok(ApiResponse<List<CollaboratorDetailResponse>>.SuccessResponse(result, $"Danh sách các collaborator"));
        }
    }
}
