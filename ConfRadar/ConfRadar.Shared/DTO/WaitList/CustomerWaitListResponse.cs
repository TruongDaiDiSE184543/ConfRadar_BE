namespace ConfRadar.Shared.DTO.WaitList
{
    public class CustomerWaitListResponse
    {
        public string PaperWaitListId { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public string? WaitListStatusId { get; set; }
        public string? WaitListStatusName { get; set; }

        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceDescription { get; set; }
        public DateOnly? ConferenceStartDate { get; set; }
        public DateOnly? ConferenceEndDate { get; set; }
        public int? ConferenceAvailableSlot { get; set; }
        public string? ConferenceAddress { get; set; }
        public string? ConferenceBannerImageUrl { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceCategoryName { get; set; }
        public string? ConferenceStatusId { get; set; }
        public string? ConferenceStatusName { get; set; }

    }
}
