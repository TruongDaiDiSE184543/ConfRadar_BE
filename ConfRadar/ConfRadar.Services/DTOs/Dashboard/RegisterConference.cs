namespace ConfRadar.Services.DTOs.Dashboard
{
    public class RegisterConferenceResponse
    {

        public List<ConferenceRegisterDto> ConferenceRegisters { get; set; } = new List<ConferenceRegisterDto>();
    }

    public class ConferenceRegisterDto
    {
        public string ConferenceId { get; set; }
        public string Name { get; set; }

        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public int? TotalSlot { get; set; }
        public int PurchaseSlot { get; set; }


        public decimal OccupancyRate { get; set; }
    }
}