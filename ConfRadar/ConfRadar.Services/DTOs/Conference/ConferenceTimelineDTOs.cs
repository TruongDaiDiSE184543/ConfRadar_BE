namespace ConfRadar.Services.DTOs.Conference
{
    public class ConferenceTimelineResponse
    {
        public string? ConferenceTimelineId { get; set; }
        public string? ConferenceId { get; set; }
        public DateOnly? ChangeDate { get; set; }
        public string? PreviousStatusId { get; set; }
        public string? AfterwardStatusId { get; set; }
        public string? Reason { get; set; }
        public string? PreviousStatusName { get; set; }
        public string? AfterwardStatusName { get; set; }
        public string? ConferenceName { get; set; }
    }
}