using ConfRadar.Api.Responses;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.QrCode;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCoderController : ControllerBase
    {
        private readonly IQRCoderService _qRCoderService;
        private readonly ITokenService _tokenService;
        public QrCoderController(IQRCoderService qRCoderService, ITokenService tokenService)
        {
            _qRCoderService = qRCoderService;
            _tokenService = tokenService;
        }
        [HttpGet("create-qrcode")]
        public async Task<IActionResult> GetQrCode()
        {
            var json = new QrDataPayload()
            {
                ConferenceSessionId = "1234",
                CreatedAt = ExtensionHelper.GetVietnamTime(),
                Signature = "12313",
                TicketId = "1313",
                UserCheckinId = "123",
                UserId = "123"
            };
            string uniqueFileName = _tokenService.GenerateSecureRandomToken();
            var qrLink = await _qRCoderService.GenerateQrCodeAsync(json, uniqueFileName, "image/png");
            return Ok(ApiResponse<string>.SuccessResponse(qrLink, "qrLink"));
        }
        [HttpPost("verify-qrcode")]
        public IActionResult VerifyQrCode([FromBody] VerifyQrDataRequest request)
        {
            _qRCoderService.ProcessScanQr(request.Content);
            return Ok(ApiResponse<object>.SuccessResponse(null, "qrLink"));
        }
    }
}
