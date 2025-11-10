using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.DTOs.Transaction;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly IZaloPayService _zaloPayService;
        private readonly ITokenService _tokenService;
        public PaymentController(IServiceManager serviceManager, IZaloPayService zaloPayService, ITokenService tokenService)
        {
            _serviceManager = serviceManager;
            _zaloPayService = zaloPayService;
            _tokenService = tokenService;
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
        public async Task<IActionResult> VerifyVnPay([FromQuery] VnPayResponse data)
        {
            await _serviceManager.PaymentService.VerifyVnPayDataForConference(data);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã thanh toán thành công"));
        }
        [HttpPost("verify-zalopay")]
        public IActionResult VerifyZaloPay([FromBody] dynamic cbdata)
        {
            var result = new Dictionary<string, object>();

            try
            {
                string key2 = "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz";
                var dataStr = Convert.ToString(cbdata["data"]);
                var reqMac = Convert.ToString(cbdata["mac"]);


                Console.WriteLine("mac = {0}", reqMac);
                Console.WriteLine("cbdata:" + cbdata);
                var mac = _tokenService.CreateSignature(dataStr, key2);

                Console.WriteLine("mac = {0}", mac);
                // kiểm tra callback hợp lệ (đến từ ZaloPay server)
                if (!reqMac.Equals(mac))
                {

                    // callback không hợp lệ
                    result["returncode"] = -1;
                    result["returnmessage"] = "mac not equal";
                }
                else
                {
                    // thanh toán thành công
                    // merchant cập nhật trạng thái cho đơn hàng
                    var dataJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
                    Console.WriteLine("update order's status = success where apptransid = {0}", dataJson["apptransid"]);

                    result["returncode"] = 1;
                    result["returnmessage"] = "success";
                }
            }
            catch (Exception ex)
            {
                result["returncode"] = 0; // ZaloPay server sẽ callback lại (tối đa 3 lần)
                result["returnmessage"] = ex.Message;
            }

            // thông báo kết quả cho ZaloPay server
            return Ok(result);
        }
        [HttpPost("create-zalopay")]
        public async Task<IActionResult> CreateZaloPay()
        {
            var result = await _zaloPayService.CreateZaloPayment();
            return Ok(result);
        }






    }
}
