using ConfRadar.Api.Responses;
using ConfRadar.Services.DTOs.Orcid;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
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
            string orcidOauth = _orcidService.GenerateAuthorizationLink();
            return Ok(ApiResponse<string>.SuccessResponse(orcidOauth,"Lấy link oauth thành công"));
        }

        [HttpGet("callback")]
        public async Task<IActionResult> ExchangeForAccessToken([FromQuery]string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new BadRequestException($"Không tìm thấy code");
            var tokenResponse = await _orcidService.ExchangeCodeForTokenAsync(code);
            return Ok(ApiResponse<OrcidAuthorizationResponse>.SuccessResponse(tokenResponse,""));
        }

    }
}
