using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.DTOs.Transaction;
using ConfRadar.Shared.DTO.Payment;
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
        public async Task<IActionResult> CreatePaymentForResearch([FromBody] CreatePaperPaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaymentService.CreatePaymentForAbstract(request, userId);
            return Ok(ApiResponse<GeneralPaymentResultResponse>.SuccessResponse(result, "Thanh toán đã hoạt động"));
        }





        //[HttpPost("verify-momo-for-tech")]
        //public async Task<IActionResult> VerifyPaymentForTech([FromBody] MomoPaymentCallBackResponse response)
        //{
        //    //await _serviceManager.MomoService.VerifyMomoPaymentDataForTechConference(response);
        //    await Task.CompletedTask;
        //    return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        //}
        //[HttpPost("verify-momo-for-research")]
        //public async Task<IActionResult> VerifyPaymentForResearch([FromBody] MomoPaymentCallBackResponse response)
        //{
        //    await _serviceManager.MomoService.VerifyMomoPaymentDataForResearchConferenceAbstractSubmission(response);
        //    await Task.CompletedTask;
        //    return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        //}
        //[HttpGet("momo-success-for-tech")]
        //public async Task<IActionResult> MomoSucessForTech([FromQuery] MomoPaymentCallBackResponse response)
        //{
        //    await _serviceManager.MomoService.VerifyMomoPaymentDataForTechConference(response);
        //    return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        //}
        //[HttpGet("momo-success-for-research")]
        //public async Task<IActionResult> MomoSucessForResearch([FromQuery] MomoPaymentCallBackResponse response)
        //{
        //    await _serviceManager.MomoService.VerifyMomoPaymentDataForResearchConferenceAbstractSubmission(response);
        //    return Ok(ApiResponse<object>.SuccessResponse(null, "Thanh toán thành công"));
        //}

        [Authorize]
        [HttpGet("get-own-transaction")]
        public async Task<IActionResult> GetOwnTransaction()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.PaymentService.GetOwnTransactionByUserId(userId);
            return Ok(ApiResponse<List<TransactionDetailResponse>>.SuccessResponse(result, "danh sách toàn bộ giao dịch"));
        }

        #region test zalopay
        //[HttpPost("test-zalo-pay")]
        //public IActionResult TestZaloPayCallback([FromBody] dynamic callbackData)
        //{
        //    var result = new Dictionary<string, object>();

        //    try
        //    {
        //        string dataStr = Convert.ToString(callbackData["data"]);
        //        string reqMac = Convert.ToString(callbackData["mac"]);

        //        string computedMac = ComputeHmacSha256(dataStr, _key2);


        //        if (reqMac != computedMac)
        //        {
        //            result["return_code"] = -1;
        //            result["return_message"] = "Invalid MAC";
        //        }
        //        else
        //        {

        //            var dataJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
        //            string appTransId = dataJson["app_trans_id"]?.ToString();



        //            result["return_code"] = 1;
        //            result["return_message"] = "success";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result["return_code"] = 0;
        //        result["return_message"] = ex.Message;
        //    }


        //    return Ok(result);
        //}


        //[HttpGet("create-zalopay")]
        //public async Task<IActionResult> CreateZaloPayPayment()
        //{
        //    try
        //    {
        //        var response = await _zaloPayService.CreateMomoPayment();

        //        var json = JsonConvert.DeserializeObject<object>(response);
        //        Console.WriteLine(json);
        //        return Ok(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Tạo thanh toán ZaloPay thất bại",
        //            error = ex.Message
        //        });
        //    }
        //}
        //[HttpGet("payment/success")]
        //public IActionResult ZaloPaySuccess([FromQuery] string returncode, [FromQuery] string zptransid)
        //{
        //    return Ok($"Thanh toán thành công");
        //}
        #endregion


        [HttpPost("verify-payos")]
        public async Task<IActionResult> VerifyPayOs([FromBody] PayOS.Models.Webhooks.Webhook data)
        {
            await _serviceManager.PaymentService.VerifyPayOsDataForConference(data);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã thanh toán thành công"));
        }

        //[HttpPost("cancel-payos")]
        //public async Task<IActionResult> CancelPayOs()
        //{
        //    var link = await _serviceManager.PaymentService.CreatePayOsPayment();
        //    return Ok(link);
        //}


        [HttpGet("success-payos")]
        public IActionResult Success()
        {
            string message = "Đã thanh toán thành công payos";
            return Ok(ApiResponse<object>.SuccessResponse(message, "Đã thanh toán thành công"));
        }


    }
}
