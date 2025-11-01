using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Room;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.Services.Services
{
    public interface IRoomService
    {
        Task<string> CreateRoomAsync(CreateRoomRequest request);
        Task<int> UpdateRoomAsync(UpdateRoomRequest request, string roomId);
        Task<int> DeleteRoomAsync(string roomId);
        Task<RoomResponse> GetRoomByIdAsync(string roomId);
        Task<List<RoomResponse>> GetAllRoomsAsync();

        // Room occupation checking methods
        Task<List<RoomOccupationSlotResponse>> GetRoomOccupationSlots(string roomId, DateOnly startDate, DateOnly endDate);
        Task<bool> IsRoomAvailable(string roomId, DateOnly date, TimeOnly startTime, TimeOnly endTime);
        Task<bool> IsRoomOccupiedAtTime(string roomId, DateOnly date, TimeOnly time);
        Task<List<RoomOccupationSlotResponse>> GetSessionsInRoomOnDateAsync(string roomId, DateOnly date);
        Task<List<TimeSpanResponse>> GetUnoccupiedTimeSpansInRoomOnDateAsync(string roomId, DateOnly date);
        Task<List<TimeSpanResponse>> GetBusyTimeSpansInRoomOnDateAsync(string roomId, DateOnly date);

    }

    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreateRoomAsync(CreateRoomRequest request)
        {
            var room = new Room
            {
                RoomId = Guid.NewGuid().ToString(),
                Number = request.Number,
                DisplayName = request.DisplayName,
                DestinationId = request.DestinationId
            };

            var result = await _unitOfWork.RoomRepository.CreateRoomAsync(room);
            if (result <= 0)
            {
                throw new BadRequestException("Failed to create room");
            }

            return room.RoomId;
        }

        public async Task<int> UpdateRoomAsync(UpdateRoomRequest request, string roomId)
        {
            var existingRoom = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (existingRoom == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            existingRoom.Number = request.Number ?? existingRoom.Number;
            existingRoom.DisplayName = request.DisplayName ?? existingRoom.DisplayName;
            existingRoom.DestinationId = request.DestinationId ?? existingRoom.DestinationId;

            var result = await _unitOfWork.RoomRepository.UpdateRoomAsync(existingRoom);
            return result;
        }

        public async Task<int> DeleteRoomAsync(string roomId)
        {
            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            var result = await _unitOfWork.RoomRepository.DeleteRoomAsync(room);
            return result;
        }

        public async Task<RoomResponse> GetRoomByIdAsync(string roomId)
        {
            var room = await _unitOfWork.RoomRepository.GetRoomWithDetailsAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            var response = new RoomResponse
            {
                RoomId = room.RoomId,
                Number = room.Number,
                DisplayName = room.DisplayName,
                DestinationId = room.DestinationId
            };

            return response;
        }

        public async Task<List<RoomResponse>> GetAllRoomsAsync()
        {
            var rooms = await _unitOfWork.RoomRepository.GetAllRoomsAsync();
            var responses = rooms.Select(room => new RoomResponse
            {
                RoomId = room.RoomId,
                Number = room.Number,
                DisplayName = room.DisplayName,
                DestinationId = room.DestinationId
            }).ToList();

            return responses;
        }

        // Room occupation checking methods

        /// <summary>
        /// Get all occupation slots for a room within a date range
        /// Performance optimization: Uses efficient date/time queries with indexes
        /// PostgreSQL performance tip: To optimize further, create these indexes:
        /// - CREATE INDEX idx_conference_session_room_date ON ConferenceSession(RoomId, Date);
        /// - CREATE INDEX idx_conference_session_starttime_endtime ON ConferenceSession(StartTime, EndTime);
        /// </summary>
        public async Task<List<RoomOccupationSlotResponse>> GetRoomOccupationSlots(string roomId, DateOnly startDate, DateOnly endDate)
        {
            // Validate room exists
            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            // Check if dates are valid (endDate should not be before startDate)
            if (endDate < startDate)
            {
                throw new BadRequestException("End date cannot be before start date");
            }

          
            var conferenceSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdAndDateRangeAsync(roomId, startDate, endDate);

            // Get all associated conferences at once to reduce database calls
            var conferenceIds = conferenceSessions.Select(cs => cs.ConferenceId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            var conferences = new Dictionary<string, Conference>();
            if (conferenceIds.Any())    
            {
                conferences = await _unitOfWork.ConferenceRepository.GetConferencesByIdsAsync(conferenceIds);
            }

            // Convert the database times (which are effectively in local timezone due to Unspecified kind) to local time for the response
            var occupationSlots = conferenceSessions.Select(session => new RoomOccupationSlotResponse
            {
                SessionId = session.ConferenceSessionId,
                SessionTitle = session.Title,
                StartTime = session.StartTime.GetValueOrDefault(),
                EndTime = session.EndTime.GetValueOrDefault(),
                ConferenceId = session.ConferenceId, // Added this line back
                ConferenceName = conferences.ContainsKey(session.ConferenceId!)
            ? conferences[session.ConferenceId!].ConferenceName
            : "Unknown Conference"
            }).ToList();


            return occupationSlots;
        }

        /// <summary>
        /// Check if a room is available for a specific time slot on a given date
        /// Performance optimization: Uses efficient range overlap checking
        /// PostgreSQL performance tip: This uses the range overlap logic which is optimized in PostgreSQL
        /// The query will efficiently use indexes on StartTime/EndTime and RoomId
        /// </summary>
        public async Task<bool> IsRoomAvailable(string roomId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            // Validate room exists
            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            // Convert DateOnly + TimeOnly to DateTime
            var startDateTime = date.ToDateTime(startTime);
            var endDateTime = date.ToDateTime(endTime);
            
            // Validate time range
            if (endTime <= startTime)
            {
                throw new BadRequestException("End time must be after start time");
            }

          
            // Check for overlapping sessions in the same room on the same date
            // PostgreSQL optimization: This query efficiently checks for time overlaps
            // Overlap condition: (new_start < existing_end) AND (new_end > existing_start)
            var overlappingSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOverlappingTimeAsync(roomId, date, startDateTime, endDateTime);
            var overlappingSession = overlappingSessions.Any();

            // If there's an overlapping session, the room is NOT available
            return !overlappingSession;
        }

        /// <summary>
        /// Check if a room is occupied at a specific date and time
        /// Performance optimization: Direct time range check
        /// PostgreSQL Performance tip: Efficient time range query utilizing indexes
        /// </summary>
        public async Task<bool> IsRoomOccupiedAtTime(string roomId, DateOnly date, TimeOnly time)
        {
            // Validate room exists
            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            // Convert DateOnly + TimeOnly to DateTime
            var checkDateTime = date.ToDateTime(time);


            // Check if there's a session running in this room at the specified time
            // PostgreSQL optimization: Efficient time range check
            var sessionsAtTime = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdAtTimeAsync(roomId, date, checkDateTime);
            var isOccupied = sessionsAtTime.Any();

            return isOccupied;
        }

        /// <summary>
        /// Get all sessions in a room for a specific date
        /// </summary>
        public async Task<List<RoomOccupationSlotResponse>> GetSessionsInRoomOnDateAsync(string roomId, DateOnly date)
        {
            // Validate room exists
            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }


            // Get all sessions in the room for the specific date
            var conferenceSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(roomId, date);

            // Get all associated conferences at once to reduce database calls
            var conferenceIds = conferenceSessions.Select(cs => cs.ConferenceId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            var conferences = new Dictionary<string, Conference>();
            if (conferenceIds.Any())
            {
                conferences = await _unitOfWork.ConferenceRepository.GetConferencesByIdsAsync(conferenceIds);
            }

            // Convert the database times (which are effectively in local timezone due to Unspecified kind) to local time for the response
            var occupationSlots = conferenceSessions.Select(session => new RoomOccupationSlotResponse
            {
                SessionId = session.ConferenceSessionId,
                SessionTitle = session.Title,
                StartTime = session.StartTime.Value,
                EndTime = session.EndTime.Value,
                ConferenceId = session.ConferenceId!,
                ConferenceName = conferences.ContainsKey(session.ConferenceId!)
                    ? conferences[session.ConferenceId!].ConferenceName
                    : "Unknown Conference"
            }).ToList();

            return occupationSlots;
        }

        /// <summary>
        /// Get all unoccupied time spans in a room for a specific date
        /// Returns a list of available time slots between 00:00 and 23:59, excluding occupied sessions
        /// </summary>
        public async Task<List<TimeSpanResponse>> GetUnoccupiedTimeSpansInRoomOnDateAsync(string roomId, DateOnly date)
        {
            // Validate room exists
            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            // Get all sessions in the room for the specific date
            var occupiedSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(roomId, date);

            // Process the occupied sessions, sorting by start time
            var occupiedTimeSpans = occupiedSessions
                .Where(s => s.StartTime.HasValue && s.EndTime.HasValue)
                .Select(s => new
                {
                    // Since database values come as Unspecified kind, treat as local time
                    Start = s.StartTime.Value,
                    End = s.EndTime.Value
                })
                .OrderBy(s => s.Start)
                .ToList();

            // Define the full day range (00:00 to 23:59) in local time
            var dayStart = date.ToDateTime(new TimeOnly(0, 0, 0));
            var dayEnd = date.ToDateTime(new TimeOnly(23, 59, 59));

            var unoccupiedSpans = new List<TimeSpanResponse>();
            
            // If no sessions exist for the day, the entire day is unoccupied
            if (!occupiedTimeSpans.Any())
            {
                unoccupiedSpans.Add(new TimeSpanResponse
                {
                    StartTime = TimeOnly.FromDateTime(dayStart),
                    EndTime = TimeOnly.FromDateTime(dayEnd)
                });
                return unoccupiedSpans;
            }

            // Check for unoccupied time before the first session
            var firstSessionStart = occupiedTimeSpans.First().Start;
            if (dayStart < firstSessionStart)
            {
                unoccupiedSpans.Add(new TimeSpanResponse
                {
                    StartTime = TimeOnly.FromDateTime(dayStart),
                    EndTime = TimeOnly.FromDateTime(firstSessionStart)
                });
            }

            // Check for unoccupied time between sessions
            for (int i = 0; i < occupiedTimeSpans.Count - 1; i++)
            {
                var currentEnd = occupiedTimeSpans[i].End;
                var nextStart = occupiedTimeSpans[i + 1].Start;

                if (currentEnd < nextStart)
                {
                    unoccupiedSpans.Add(new TimeSpanResponse
                    {
                        StartTime = TimeOnly.FromDateTime(currentEnd),
                        EndTime = TimeOnly.FromDateTime(nextStart)
                    });
                }
            }

            // Check for unoccupied time after the last session
            var lastSessionEnd = occupiedTimeSpans.Last().End;
            if (lastSessionEnd < dayEnd)
            {
                unoccupiedSpans.Add(new TimeSpanResponse
                {
                    StartTime = TimeOnly.FromDateTime(lastSessionEnd),
                    EndTime = TimeOnly.FromDateTime(dayEnd)
                });
            }

            return unoccupiedSpans;
        }

        public async Task<List<TimeSpanResponse>> GetBusyTimeSpansInRoomOnDateAsync(string roomId, DateOnly date)
        {
            // Validate room exists
            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                throw new NotFoundException($"Room with ID {roomId} not found");
            }

            // Get all sessions in the room for the specific date
            var occupiedSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(roomId, date);

            // Process the occupied sessions, sorting by start time
            var occupiedTimeSpans = occupiedSessions
                .Where(s => s.StartTime.HasValue && s.EndTime.HasValue)
                .Select(s => new TimeSpanResponse
                {
                    // Since database values come as Unspecified kind, treat as local time
                    StartTime = TimeOnly.FromDateTime(s.StartTime.Value),
                    EndTime = TimeOnly.FromDateTime(s.EndTime.Value)
                })
                .OrderBy(s => s.StartTime)
                .ToList();
 
            return occupiedTimeSpans;
        }
    }
}