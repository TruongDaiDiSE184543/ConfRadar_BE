using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Orcid;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrcidController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public OrcidController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("authorize-orcid")]
        public async Task<IActionResult> AuthorizeOrcid()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new BadRequestException("Người dùng chưa đăng nhập");

            string orcidOauth = _serviceManager.OrcidService.GenerateAuthorizationLink("read-limited", userId);
            return Ok(ApiResponse<string>.SuccessResponse(orcidOauth, "Lấy link oauth thành công"));
        }

        [HttpGet("callback")]
        //[HttpGet("signin-orcid")]
        public async Task<IActionResult> ExchangeForAccessToken([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code))
                throw new BadRequestException($"Không tìm thấy code");

            if (string.IsNullOrEmpty(state))
                throw new BadRequestException($"Không tìm thấy state parameter");

            // Decode the state parameter to get the userId
            string userId;
            try
            {
                byte[] data = Convert.FromBase64String(state);
                userId = System.Text.Encoding.UTF8.GetString(data);
            }
            catch (Exception)
            {
                throw new BadRequestException($"State parameter không hợp lệ");
            }

            var tokenResponse = await _serviceManager.OrcidService.ExchangeCodeForTokenAsync(code, userId);
            return Ok(ApiResponse<OrcidAuthorizationResponse>.SuccessResponse(tokenResponse, ""));
        }

        [HttpGet("Get-works-from-orcid")]
        public async Task<IActionResult> getWork([FromQuery] string userId)
        {
            var result = await _serviceManager.OrcidService.SyncWorksAsync(userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, ""));
        }

        [HttpGet("Get-biography-from-orcid")]
        public async Task<IActionResult> getBiography([FromQuery] string userId)
        {
            var result = await _serviceManager.OrcidService.SyncBiographyAsync(userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, ""));
        }

        [HttpGet("Get-Educations-from-orcid")]
        public async Task<IActionResult> getEducations([FromQuery] string userId)
        {
            var result = await _serviceManager.OrcidService.SyncEducationAsync(userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, ""));
        }

        [HttpGet("Get-section-from-db")]
        public async Task<IActionResult> getSectionByUserId([FromQuery]string section)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.OrcidService.GetSectionByUserIdFromDb(userId,section);
            return Ok(ApiResponse<object>.SuccessResponse(result, ""));
        }
    }
}
