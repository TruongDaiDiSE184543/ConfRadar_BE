using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.Common;
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
            var result = await _serviceManager.AuthService.VerifyRegistration(token);
            var redirectUrl = result.Success
                ? $"{FrontEndDomain.Url}{ConfRadarApiEndPoint.EmailConfirmSuccess_FE}?code={result.ErrorCode}"
                : $"{FrontEndDomain.Url}{ConfRadarApiEndPoint.EmailConfirmFail_FE}?code={result.ErrorCode}";
            return Redirect(redirectUrl);
        }




        [HttpPost("register")]
        public async Task<IActionResult> RegisterAccount([FromForm] CreateUserRequest request)
        {
            var result = await _serviceManager.AuthService.RegisterAccount(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Hãy kiểm tra email"));
        }
        [HttpPost("login")]
        public async Task<IActionResult> LocalLogin([FromBody] LocalLoginUserRequest request)
        {
            var loginResponse = await _serviceManager.AuthService.LocalLogin(request);

            return Ok(ApiResponse<LoginUserResponse>.SuccessResponse(loginResponse, "Đăng nhập thành công"));
        }
        [HttpPost("firebase-login")]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
        {
            var loginResponse = await _serviceManager.AuthService.FirebaseLogin(request);
            return Ok(ApiResponse<LoginUserResponse>.SuccessResponse(loginResponse, "Đăng nhập thành công"));
        }
        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            await _serviceManager.AuthService.ForgetPassword(email);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã gửi thư qua email"));
        }
        [HttpPost("verify-forget-password")]
        public async Task<IActionResult> VerifyForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            await _serviceManager.AuthService.VerifyForgetPassword(request.Token, request.Password);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Password đổi thành công"));
        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _serviceManager.AuthService.ChangePassword(request.OldPassword, request.NewPassword, userId);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Authentication, $"đổi mật khẩu");
            return Ok(ApiResponse<object>.SuccessResponse(null, "Password đổi thành công"));
        }
        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var refreshTokenResponse = await _serviceManager.AuthService.RefreshToken(userId!, request.Token);
            return Ok(ApiResponse<LoginUserResponse>.SuccessResponse(refreshTokenResponse, "token tạo mới thành công"));
        }
        [Authorize(Roles = "Conference Organizer,Admin")]
        [HttpPut("suspend-account")]
        public async Task<IActionResult> SuspendAccount([FromBody] UserSuspendRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.SuspendAccount(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.User, $"đã đình chỉ tài khoản với người dùng id {request.UserId}");

            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã đình chỉ tài khoản này!"));
        }
        [Authorize(Roles = "Conference Organizer,Admin")]
        [HttpPut("activate-account")]
        public async Task<IActionResult> ActivateAccount([FromBody] UserActiveAccountRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.ActivateAccount(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.User, $"đã kích hoạt tài khoản cho người dùng với id {request.UserId}");
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã kích hoạt tài khoản của người dùng"));
        }
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileUpdateRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.UpdateProfile(request, userId);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Authentication, $"đã cập nhật lại hồ sơ cá nhân");
            return Ok(ApiResponse<int>.SuccessResponse(result, "Đã cập nhật profile"));

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.CreateCollaboratorAccount(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.User, $"tạo tài khoản cho collaborator");
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
        [HttpGet("list-all-reviewers-by-conference")]
        public async Task<IActionResult> ListAllReviewerByConference([FromQuery] string conferenceId)
        {
            var result = await _serviceManager.AuthService.ListAllReviewerByConferenceId(conferenceId);
            return Ok(ApiResponse<List<ReviewerDetailResponse>>.SuccessResponse(result, $"Danh sách reviewer theo conference"));
        }
        [Authorize(Roles = "Conference Organizer,Admin")]
        [HttpPut("suspend-external-reviewer")]
        public async Task<IActionResult> SuspendExternalReviewer([FromBody] UserSuspendRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.SuspendExternalReviewerAccount(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.User, $"Đã đình chỉ người reviewer outsource với id {request.UserId}");

            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã suspend người reviewer outsource với id {request.UserId}"));
        }
        [Authorize(Roles = "Conference Organizer,Admin")]
        [HttpPut("activate-external-reviewer")]
        public async Task<IActionResult> ActivateExternalReviewer([FromBody] UserActiveAccountRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.ActivateExternalReviewerAccount(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.User, $"Đã kích hoạt tài khoản người reviewer outsource với id {request.UserId}");
            return Ok(ApiResponse<int>.SuccessResponse(result, $"Đã activate người reviewer outsource với id {request.UserId}"));
        }
        [Authorize(Roles = "Conference Organizer")]
        [HttpPost("create-local-reviewer-account")]
        public async Task<IActionResult> CreateLocalReviewerAccount([FromBody] CreateLocalReviewerAccountRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.CreateLocalReviewerAccount(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.User, $"đã tạo tài khoản cho local reviewer với email {request.Email}");

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.AuthService.UpdateOrganization(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.User, $"đã cập nhật thông tin cho tổ chức mã {request.OrganizationId}");

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
