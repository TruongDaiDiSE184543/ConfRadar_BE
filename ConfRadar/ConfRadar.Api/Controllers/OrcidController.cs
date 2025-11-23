using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Orcid;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Authorization;
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

        //[Authorize]
        [HttpGet("authorize-orcid")]
        public async Task<IActionResult> AuthorizeOrcid([FromQuery] string userId)
        {
            //var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
          

            string orcidOauth = _serviceManager.OrcidService.GenerateAuthorizationLink(userId);
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
            string redirectType = "link-orcid";

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
            try
            {
                var tokenResponse = await _serviceManager.OrcidService.ExchangeCodeForTokenAsync(code, userId);
                //return Ok(ApiResponse<OrcidAuthorizationResponse>.SuccessResponse(tokenResponse, ""));

                string URL = $"https://confradar.vercel.app/{redirectType}/success";
                return Redirect(URL);
            }catch (Exception e)
            {
                string URL = $"https://confradar.vercel.app/{redirectType}/fail";
                return Redirect(URL);
            }
            
        }

        [HttpGet("Get-works-from-orcid")]
        public async Task<IActionResult> getWork(/*[FromQuery] string userId*/)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.OrcidService.SyncWorksAsync(userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, ""));
        }

        [HttpGet("Get-biography-from-orcid")]
        public async Task<IActionResult> getBiography()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.OrcidService.SyncBiographyAsync(userId);
            return Ok(ApiResponse<string>.SuccessResponse(result, ""));
        }

        [HttpGet("Get-Educations-from-orcid")]
        public async Task<IActionResult> getEducations()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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

        [HttpGet("status")]
        public async Task<IActionResult> GetOrcidStatus([FromQuery] string userId)
        {

            var status = await _serviceManager.OrcidService.CheckOrcidStatusAsync(userId);

            return Ok(ApiResponse<OrcidStatusResponse>.SuccessResponse(status, "Kiểm tra trạng thái ORCID thành công."));
        }
    }
}
