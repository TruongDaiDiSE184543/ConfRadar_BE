using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public PaymentMethodController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [HttpGet("list-all-payment-methods")]
        public async Task<IActionResult> GetListAllPaymentMethod()
        {
            var result = await _serviceManager.PaymentService.GetListPaymentMethod();
            return Ok(ApiResponse<List<PaymentMethod>>.SuccessResponse(result, "danh sách các phương thức thanh toán"));
        }
    }
}
