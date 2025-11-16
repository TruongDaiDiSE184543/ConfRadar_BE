using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.ConferenceStatusService.GetAllConferenceStatusesAsync(userId);
            return Ok(ApiResponse<List<ConferenceStatus>>.SuccessResponse(result, "danh sach conference status"));
        }

        //[HttpGet("get-status-for-customer")]
        //[Authorize(Roles = "Customer")]
        //public async Task<IActionResult> ConferenceStatusForCustomer()
        //{
        //    var result = await _serviceManager.ConferenceStatusService.GetAllConferenceStatusesAsync();
        //    return Ok(ApiResponse<List<ConferenceStatus>>.SuccessResponse(result, "danh sach conference status"));
        //}
    }
}