using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Destination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DestinationController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public DestinationController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateDestination([FromBody] CreateDestinationRequest request)
        {
            try
            {
                var destinationId = await _serviceManager.DestinationService.CreateDestinationAsync(request);
                return Ok(ApiResponse<string>.SuccessResponse(destinationId, "Destination created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDestination(string id, [FromBody] UpdateDestinationRequest request)
        {
            try
            {
                var result = await _serviceManager.DestinationService.UpdateDestinationAsync(request, id);
                if (result > 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Destination updated successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Destination not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
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

        [Authorize(Roles = "Conference Organizer")]
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

        [Authorize(Roles = "Conference Organizer")]
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

        [Authorize(Roles = "Conference Organizer")]
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