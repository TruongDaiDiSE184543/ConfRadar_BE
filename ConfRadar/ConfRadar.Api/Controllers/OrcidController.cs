using ConfRadar.Api.Responses;
using ConfRadar.Services.DTOs.Orcid;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace ConfRadar.Api.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class OrcidController : ControllerBase
    {
        private readonly IOrcidService _orcidService;

        public OrcidController(IOrcidService orcidService)
        {
            _orcidService = orcidService;
        }

        [HttpGet("authorize-orcid")]
        public async Task<IActionResult> AuthorizeOrcid()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new BadRequestException("Người dùng chưa đăng nhập");

            string orcidOauth = _orcidService.GenerateAuthorizationLink("read-limited", userId);
            return Ok(ApiResponse<string>.SuccessResponse(orcidOauth, "Lấy link oauth thành công"));
        }

        //[HttpGet("callback")]
        [HttpGet("signin-orcid")]
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

            var tokenResponse = await _orcidService.ExchangeCodeForTokenAsync(code, userId);
            return Ok(ApiResponse<OrcidAuthorizationResponse>.SuccessResponse(tokenResponse, ""));
        }

    }
}
