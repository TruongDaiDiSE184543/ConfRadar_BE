using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.QrCode;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCoderController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public QrCoderController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [HttpPost("verify-qrcode")]
        public async Task<IActionResult> VerifyQrCode([FromBody] VerifyQrDataRequest request)
        {
            var message = await _serviceManager.QRCoderService.ProceedQrCode(request);
            return Ok(ApiResponse<object>.SuccessResponse(message, "đã check in thành công"));
        }
    }
}
