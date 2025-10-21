using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Room
{
    public class CreateRoomRequest
    {
        [MaxLength(255)]
        public string? Number { get; set; }

        [MaxLength(50)]
        public string? DisplayName { get; set; }

        public string? DestinationId { get; set; }
    }

    public class UpdateRoomRequest
    {
        [MaxLength(255)]
        public string? Number { get; set; }

        [MaxLength(50)]
        public string? DisplayName { get; set; }

        public string? DestinationId { get; set; }
    }

    public class RoomResponse
    {
        public string RoomId { get; set; }
        public string? Number { get; set; }
        public string? DisplayName { get; set; }
        public string? DestinationId { get; set; }
    }

    public class RoomOccupationSlotResponse
    {
        public string SessionId { get; set; }
        public string SessionTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ConferenceId { get; set; }
        public string ConferenceName { get; set; }
    }
}