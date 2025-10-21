using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IMomoService _momoService;
        public PaymentController(IMomoService momoService)
        {
            _momoService = momoService;
        }
        [Authorize]
        [HttpPost("pay-tech-with-momo")]
        public async Task<IActionResult> CreatePayment([FromBody] CreateTechPaymentRequest request)
        {
            await _momoService.CreateMomoPayment();
            return Ok(new { Message = "Payment created successfully." });
        }
        [HttpPost("test-momo")]
        public async Task<IActionResult> TestMomo(MomoPaymentRequestResponse response)
        {
           
            Console.WriteLine("Hello, đã thành công!");
            Console.WriteLine(response);
            string json = JsonSerializer.Serialize(response, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
            Console.WriteLine(json);
            _momoService.VerifyMomoPaymentData(response);
            return Ok("Test MoMo đã chạy thành công!");
        }
        [HttpGet("test-momo-success")]
        public async Task<IActionResult> TestMomoSucess()
        {

            Console.WriteLine("Hello, đã thành công!");
            return Ok("Test MoMo đã chạy thành công!");
        }
    }
}
