using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.GlobalStatus;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalStatusController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public GlobalStatusController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;

        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateGlobalStatus([FromBody] GlobalStatusRequest request)
        {
            var result = await _serviceManager.GlobalStatusService.CreateGlobalStatus(request);
            return Ok(ApiResponse<int>.SuccessResponse(result, "global status created successfully"));
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateGlobalStatus(string id, [FromBody] GlobalStatusRequest request)
        {
            var result = await _serviceManager.GlobalStatusService.UpdateGlobalStatusAsync(id, request);
            return Ok(ApiResponse<int>.SuccessResponse(result, "data updated successfully"));
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteGlobalStatus(string id)
        {
            var result = await _serviceManager.GlobalStatusService.DeleteGlobalStatusAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "data deleted successfully"));
        }


        [HttpGet("detail")]
        public async Task<IActionResult> GetGlobalStatus(string id)
        {
            var result = await _serviceManager.GlobalStatusService.GetGlobalStatusByIdAsync(id);
            return Ok(ApiResponse<GlobalStatus>.SuccessResponse(result, "data retrieved successfully"));

        }
    }
}
