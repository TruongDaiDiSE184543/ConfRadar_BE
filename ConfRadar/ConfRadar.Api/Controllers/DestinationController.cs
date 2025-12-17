using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Destination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public DestinationController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [Authorize(Roles = "Conference Organizer, Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDestination([FromBody] CreateDestinationRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var destinationId = await _serviceManager.DestinationService.CreateDestinationAsync(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"tạo điểm đến {request.Name} đến thành công");
            return Ok(ApiResponse<string>.SuccessResponse(destinationId, "Tạo điểm đến thành công"));


        }

        [Authorize(Roles = "Conference Organizer,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDestination(string id, [FromBody] UpdateDestinationRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _serviceManager.DestinationService.UpdateDestinationAsync(request, id);
                if (result > 0)
                {
                    await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"cập nhật điểm đến với id {id} thành công");
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật điểm đến thành công"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Không tìm thấy điểm đến"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer,Admin ")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDestination(string id)
        {
            try
            {
                var result = await _serviceManager.DestinationService.DeleteDestinationAsync(id);
                if (result > 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Destination deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Destination not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDestination(string id)
        {
            try
            {
                var destination = await _serviceManager.DestinationService.GetDestinationByIdAsync(id);
                return Ok(ApiResponse<DestinationResponse>.SuccessResponse(destination, "Destination retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllDestinations()
        {
            try
            {
                var destinations = await _serviceManager.DestinationService.GetAllDestinationsAsync();
                return Ok(ApiResponse<List<DestinationResponse>>.SuccessResponse(destinations, "Destinations retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer, Admin")]
        [HttpGet("{id}/rooms")]
        public async Task<IActionResult> GetDestinationWithRooms(string id)
        {
            try
            {
                var destination = await _serviceManager.DestinationService.GetDestinationWithRoomsAsync(id);
                return Ok(ApiResponse<DestinationWithRoomsResponse>.SuccessResponse(destination, "Destination with rooms retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
    }
}