using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.DTOs.Transaction;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            //_zaloPayService = zaloPayService;
        }
        [Authorize]
        [HttpPost("pay-tech")]
        public async Task<IActionResult> CreatePaymentForTech([FromBody] CreateTechPaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaymentService.CreatePaymentForTechConference(request, userId);
            return Ok(ApiResponse<GeneralPaymentResultResponse>.SuccessResponse(result, "Thanh toán đã hoạt động"));
        }
        [Authorize]
        [HttpPost("pay-research-paper")]
        public async Task<IActionResult> CreatePaymentForResearchForPaper([FromBody] CreatePaperPaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaymentService.CreatePaymentForAbstract(request, userId);
            return Ok(ApiResponse<GeneralPaymentResultResponse>.SuccessResponse(result, "Thanh toán đã hoạt động"));
        }
        [Authorize]
        [HttpPost("pay-research-as-attendee")]
        public async Task<IActionResult> CreatePaymentForResearchAsAttendee([FromBody] CreateResearchAttendeePaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaymentService.CreatePaymentForResearchAsAttendee(request, userId);
            return Ok(ApiResponse<GeneralPaymentResultResponse>.SuccessResponse(result, "Thanh toán đã hoạt động"));

        }






        [Authorize]
        [HttpGet("get-own-transaction")]
        public async Task<IActionResult> GetOwnTransaction()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaymentService.GetOwnTransactionByUserId(userId);
            return Ok(ApiResponse<List<TransactionDetailResponse>>.SuccessResponse(result, "danh sách toàn bộ giao dịch"));
        }

       
       


        [HttpPost("verify-payos")]
        public async Task<IActionResult> VerifyPayOs([FromBody] PayOS.Models.Webhooks.Webhook data)
        {
            await _serviceManager.PaymentService.VerifyPayOsDataForConference(data);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã thanh toán thành công"));
        }
        [HttpPost("verify-momo")]
        public async Task<IActionResult> VerifyMomo([FromBody] MomoPaymentCallBackResponse data)
        {
            //api cb lên production ko call dc (sandbox)
            await _serviceManager.PaymentService.VerifyMomoDataForConference(data);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã thanh toán thành công"));
        }
      


        [HttpGet("success-payos")]
        public IActionResult SuccessPayOs()
        {
            string message = "Đã thanh toán thành công payos";
            return Ok(ApiResponse<object>.SuccessResponse(message, "Đã thanh toán thành công"));
        }
        [HttpGet("success-momo")]
        public async Task<IActionResult> SuccessMomo([FromQuery] MomoPaymentCallBackResponse data)
        {
            //method post trên deploy k call đc
            await _serviceManager.PaymentService.VerifyMomoDataForConference(data);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã thanh toán thành công"));
        }
        [HttpGet("success-vnpay")]
        public IActionResult SuccessVnPay()
        {
            string message = "Đã thanh toán thành công vnpay";
            return Ok(ApiResponse<object>.SuccessResponse(message, "Đã thanh toán thành công"));
        }
        [HttpGet("verify-vnpay")]
        public async Task<IActionResult> VerifyVnPay([FromQuery]VnPayResponse data)
        {
             await _serviceManager.PaymentService.VerifyVnPayDataForConference(data);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã thanh toán thành công"));
        }
    }
}
