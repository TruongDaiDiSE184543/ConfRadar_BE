using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public RoomController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            try
            {
                var roomId = await _serviceManager.RoomService.CreateRoomAsync(request);
                return Ok(ApiResponse<string>.SuccessResponse(roomId, "Room created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(string id, [FromBody] UpdateRoomRequest request)
        {
            try
            {
                var result = await _serviceManager.RoomService.UpdateRoomAsync(request, id);
                if (result > 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Room updated successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Room not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            try
            {
                var result = await _serviceManager.RoomService.DeleteRoomAsync(id);
                if (result > 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Room deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Room not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(string id)
        {
            try
            {
                var room = await _serviceManager.RoomService.GetRoomByIdAsync(id);
                return Ok(ApiResponse<RoomResponse>.SuccessResponse(room, "Room retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            try
            {
                var rooms = await _serviceManager.RoomService.GetAllRoomsAsync();
                return Ok(ApiResponse<List<RoomResponse>>.SuccessResponse(rooms, "Rooms retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        // Room occupation checking endpoints
        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("{roomId}/occupation")]
        public async Task<IActionResult> GetRoomOccupation(string roomId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var occupationSlots = await _serviceManager.RoomService.GetRoomOccupationSlots(roomId, startDate, endDate);
                return Ok(ApiResponse<List<RoomOccupationSlotResponse>>.SuccessResponse(occupationSlots, "Room occupation slots retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("{roomId}/occupation/check-availability")]
        public async Task<IActionResult> CheckRoomAvailability(string roomId, [FromQuery] DateOnly date, [FromQuery] TimeOnly startTime, [FromQuery] TimeOnly endTime)
        {
            try
            {
                var isAvailable = await _serviceManager.RoomService.IsRoomAvailable(roomId, date, startTime, endTime);
                return Ok(ApiResponse<bool>.SuccessResponse(isAvailable, "Room availability checked successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer")]
        [HttpGet("{roomId}/occupation/is-occupied")]
        public async Task<IActionResult> IsRoomOccupiedAtTime(string roomId, [FromQuery] DateOnly date, [FromQuery] TimeOnly time)
        {
            try
            {
                var isOccupied = await _serviceManager.RoomService.IsRoomOccupiedAtTime(roomId, date, time);
                return Ok(ApiResponse<bool>.SuccessResponse(isOccupied, "Room occupation checked successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
    }
}