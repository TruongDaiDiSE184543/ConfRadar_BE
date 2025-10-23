using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Google.Apis.Requests.BatchRequest;

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
            return Ok(ApiResponse<string>.SuccessResponse(result, "Pay with momo"));
        }
        [HttpPost("verify-momo-for-tech")]
        public async Task<IActionResult> VerifyPaymentForTech([FromBody]MomoPaymentCallBackResponse response)
        {
            await _serviceManager.MomoService.VerifyMomoPaymentDataWithTechConf(response);
            return Ok(ApiResponse<object>.SuccessResponse(null, "verified successfully"));
        }
        [HttpGet("momo-success")]
        public async Task<IActionResult> MomoSucess(/*[FromQuery]MomoPaymentCallBackResponse response*/) 
        {
            //await _serviceManager.MomoService.VerifyMomoPaymentDataWithTechConf(response);
            return Ok(ApiResponse<object>.SuccessResponse(null, "verified successfully"));
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
