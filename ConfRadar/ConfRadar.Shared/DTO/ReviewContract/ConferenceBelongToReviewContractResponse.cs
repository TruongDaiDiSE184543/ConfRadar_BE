namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class ConferenceBelongToReviewContractResponse
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
        public DateOnly? CreatedAt { get; set; }
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
        public ResearchConferenceDetailForReviewContract? ResearchConferenceDetail { get; set; }
    }
    public class ResearchConferenceDetailForReviewContract
    {
        public string ConferenceId { get; set; } = null!;
        public string? Name { get; set; }
        public string? PaperFormat { get; set; }
        public int? NumberPaperAccept { get; set; }
        public int? RevisionAttemptAllowed { get; set; }
        public string? RankingDescription { get; set; }
        public bool? AllowListener { get; set; }
        public string? RankValue { get; set; }
        public int? RankYear { get; set; }
        public decimal? ReviewFee { get; set; }
        public string? RankingCategoryId { get; set; }
        public string? RankCategoryName { get; set; }
        public string? RankCategoryDescription { get; set; }

    }
}
