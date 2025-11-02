using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferenceStatusController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public ConferenceStatusController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        
        [HttpGet("get-all-conference-statuses")]
        public async Task<IActionResult> ConferenceStatus()
        {
            var result = await _serviceManager.ConferenceStatusService.GetAllConferenceStatusesAsync();
            return Ok(ApiResponse<List<ConferenceStatus>>.SuccessResponse(result, "danh sach conference status"));
        }
        
      
    }
}