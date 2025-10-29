using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public PaymentController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [Authorize]
        [HttpPost("pay-tech-with-momo")]
        public async Task<IActionResult> CreatePaymentForTech([FromBody] CreateTechPaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.MomoService.HandleMomoPaymentWithTechConf(request, userId!);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Đã thanh toán thành công với momo. Hãy truy cập link để thực hiện thanh toán"));
        }
        [Authorize]
        [HttpPost("pay-research-paper-with-momo")]
        public async Task<IActionResult> CreateResearchPaperWithMomo([FromBody] CreatePaperPaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.MomoService.CreatePaymentForAbstract(request, userId!);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Đã thanh toán thành công với momo. Hãy truy cập link để thực hiện thanh toán"));
        }
        [HttpPost("verify-momo-for-tech")]
        public async Task<IActionResult> VerifyPaymentForTech([FromBody] MomoPaymentCallBackResponse response)
        {
            //await _serviceManager.MomoService.VerifyMomoPaymentDataForTechConference(response);
            await Task.CompletedTask;
            return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        }
        [HttpPost("verify-momo-for-research")]
        public async Task<IActionResult> VerifyPaymentForResearch([FromBody] MomoPaymentCallBackResponse response)
        {
            //await _serviceManager.MomoService.VerifyMomoPaymentDataForResearchConferenceAbstractSubmission(response);
            await Task.CompletedTask;
            return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        }
        [HttpGet("momo-success-for-tech")]
        public async Task<IActionResult> MomoSucessForTech([FromQuery] MomoPaymentCallBackResponse response)
        {
            await _serviceManager.MomoService.VerifyMomoPaymentDataForTechConference(response);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        }
        [HttpGet("momo-success-for-research")]
        public async Task<IActionResult> MomoSucessForResearch([FromQuery] MomoPaymentCallBackResponse response)
        {
            await _serviceManager.MomoService.VerifyMomoPaymentDataForResearchConferenceAbstractSubmission(response);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        }
        [Authorize]
        [HttpGet("get-own-transaction")]
        public async Task<IActionResult> GetOwnTransaction()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaymentService.GetOwnTransactionByUserId(userId);
            return Ok(ApiResponse<List<TransactionDetailResponse>>.SuccessResponse(result, "data retrieved!"));
        }

    }
}
