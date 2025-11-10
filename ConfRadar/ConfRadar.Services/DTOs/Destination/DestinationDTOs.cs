using ConfRadar.Services.DTOs.Room;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Destination
{
    public class CreateDestinationRequest
    {
        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string? CityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? District { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Street { get; set; }
    }

    public class UpdateDestinationRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? CityId { get; set; }

        [MaxLength(50)]
        public string? District { get; set; }

        [MaxLength(50)]
        public string? Street { get; set; }
    }

    public class DestinationResponse
    {
        public string DestinationId { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Street { get; set; }
    }

    public class DestinationWithRoomsResponse
    {
        public string DestinationId { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Street { get; set; }
        public List<RoomResponse> Rooms { get; set; } = new List<RoomResponse>();
    }
}