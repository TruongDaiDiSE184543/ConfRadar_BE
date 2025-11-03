namespace ConfRadar.Shared.DTO.FavouriteConference
{
    public class FavouriteConferenceDetailResponse
    {
        public string ConferenceId { get; set; }
        public DateTime? FavouriteCreatedAt { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceDescription { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateOnly? ConferenceStartDate { get; set; }

        public DateOnly? ConferenceEndDate { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public int? AvailableSlot { get; set; }
    }
    public class DeletedFavouriteConfereceResponse
    {
        public string ConferenceId { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class AddedFavouriteConfereceResponse
    {
        public string ConferenceId { get; set; }
        public bool IsAdded { get; set; }
    }
}
