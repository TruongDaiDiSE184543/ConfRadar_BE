namespace ConfRadar.Shared.DTO.Conference
{
    public class ConferenceDetailForScheduleResponse
    {
        public string ConferenceId { get; set; } = null!;

        public string? ConferenceName { get; set; }

        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public string? Address { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? CityName { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceCategoryName { get; set; }
        public string? ConferenceStatusId { get; set; }
        public string? ConferenceStatusName { get; set; }
        public List<SessionDetailForScheduleResponse> Sessions { get; set; }




    }
    public class SessionDetailForScheduleResponse
    {
        public string ConferenceSessionId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateOnly? SessionDate { get; set; }
        public string? ConferenceId { get; set; }
        public string? RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomDisplayName { get; set; }
        public string? DestinationId { get; set; }
        public string? DestinationName { get; set; }
        public string? DestinationDistrict { get; set; }
        public string? DestinationStreet { get; set; }
        public string? CityId { get; set; }
        public string? CityName { get; set; }
        public List<PresenterAuthorDetailForScheduleResponse> PresenterAuthor { get; set; } = new List<PresenterAuthorDetailForScheduleResponse>();


    }
    public class PresenterAuthorDetailForScheduleResponse
    {
        public string ConferenceSessionId { get; set; } = null!;
        public string PaperId { get; set; } = null!;
        public DateTime? AssignedAt { get; set; }
        public string? ConferenceId { get; set; }
        public string? PaperPhaseId { get; set; }
        public string? PaperPhaseName { get; set; }
        public string? ResearchConferencePhaseId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? PaperTitle { get; set; }
        public string? PaperDescription { get; set; }
        public List<PaperAuthorDetailForScheduleResponse> PaperAuthor { get; set; } = new List<PaperAuthorDetailForScheduleResponse>();
    }
    public class PaperAuthorDetailForScheduleResponse
    {
        public string UserId { get; set; } = null!;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string PaperId { get; set; } = null!;
        public bool? IsPresenter { get; set; }
        public bool? IsRootAuthor { get; set; }
    }
}
