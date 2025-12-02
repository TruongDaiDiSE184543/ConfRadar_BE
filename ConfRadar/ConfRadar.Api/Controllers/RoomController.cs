using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [Authorize(Roles = "Conference Organizer, Admin")]
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

        [Authorize(Roles = "Conference Organizer, Admin")]
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

        [Authorize(Roles = "Conference Organizer, Admin")]
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

        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
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

        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
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
        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("{roomId}/session-between-2-dateonlys")]
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

        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("{roomId}/occupation/check-if-room-is-available-in-a-date-between-2-timeonly")]
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

        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("{roomId}/occupation/is-occupied-in-a-given-time")]
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

        // NEW ENDPOINT: Get all sessions in a room for a specific date
        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("{roomId}/sessions-on-date")]
        public async Task<IActionResult> GetSessionsInRoomOnDate(string roomId, [FromQuery] DateOnly date)
        {
            try
            {
                var sessions = await _serviceManager.RoomService.GetSessionsInRoomOnDateAsync(roomId, date);
                return Ok(ApiResponse<List<RoomOccupationSlotResponse>>.SuccessResponse(sessions, "Sessions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        // NEW ENDPOINT: Get free available time spans in a room for a specific date
        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("{roomId}/available-times")]
        public async Task<IActionResult> GetUnoccupiedTimeSpansInRoomOnDate(string roomId, [FromQuery] DateOnly date)
        {
            try
            {
                var unoccupiedTimeSpans = await _serviceManager.RoomService.GetUnoccupiedTimeSpansInRoomOnDateAsync(roomId, date);
                return Ok(ApiResponse<List<TimeSpanResponse>>.SuccessResponse(unoccupiedTimeSpans, "Unoccupied time spans retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        //NEW ENDPOINT: Get busy time in a room for a specific date
        [HttpGet("{roomId}/busy-time")]
        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        public async Task<IActionResult> GetBusyTimeSpansInRoomOnDate(string roomId, [FromQuery] DateOnly date)
        {
            try
            {
                var unoccupiedTimeSpans = await _serviceManager.RoomService.GetBusyTimeSpansInRoomOnDateAsync(roomId, date);
                return Ok(ApiResponse<List<TimeSpanResponse>>.SuccessResponse(unoccupiedTimeSpans, "Unoccupied time spans retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        // NEW ENDPOINT: Get rooms with their sessions (paginated with filtering)
        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("rooms-with-sessions")]
        public async Task<IActionResult> GetRoomsWithSessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? destinationId = null,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] DateOnly? date = null)
        {
            try
            {
                var roomsWithSessions = await _serviceManager.RoomService.GetRoomsWithSessionsAsync(page, pageSize, destinationId, searchKeyword, date);
                return Ok(ApiResponse<Services.DTOs.General.PagedResult<Services.DTOs.Room.RoomWithSessionsResponse>>.SuccessResponse(roomsWithSessions, "Rooms with sessions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("room-availablility-between-dates")]
        public async Task<IActionResult> Roomavailability(string roomId, [FromQuery] DateOnly dayStart, [FromQuery] DateOnly dayEnd)
        {
            var result = await _serviceManager.RoomService.RoomAvailableBetweenDate(roomId, dayStart, dayEnd);
            return Ok(ApiResponse<List<RoomAvailablity>>.SuccessResponse(result, "lấy danh sách ngày trống thành công"));
        }

        [Authorize(Roles = "Conference Organizer, Admin, Collaborator")]
        [HttpGet("available-rooms-between-dates")]
        public async Task<IActionResult> RoomsAvailableBetweenDate([FromQuery] DateOnly startdate, [FromQuery] DateOnly endate, [FromQuery] string cityId = null, [FromQuery] string destinationId = null)
        {
            var result = await _serviceManager.RoomService.GetAvailableRoomsBetweenDates(startdate, endate, cityId, destinationId);
            return Ok(ApiResponse<List<AvailableRoomResponse>>.SuccessResponse(result, "Lấy thành công danh sách phòng trống"));
        }
    }
}