namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class OwnContractDetailResponse
    {
        public string? ReviewerContractId { get; set; }
        public bool? IsActive { get; set; }
        public DateOnly? SignDay { get; set; }
        public DateOnly? ExpireDay { get; set; }
        public decimal? Wage { get; set; }
        public string? ContractUrl { get; set; }
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceDescription { get; set; }
        public string? ConferenceBannerImageUrl { get; set; }
    }
    public class ContractDetailResponseForOrganizer
    {
        public string? ReviewerContractId { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }
        public DateOnly? SignDay { get; set; }
        public DateOnly? ExpireDay { get; set; }
        public decimal? Wage { get; set; }
        public string? ContractUrl { get; set; }
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceDescription { get; set; }
        public string? ConferenceBannerImageUrl { get; set; }
    }

    public class OwnActiveContractDetailResponse
    {
        public int ActiveContractCount { get; set; } = 0;
        public List<OwnContractDetailResponse> ContractDetail { get; set; } = new List<OwnContractDetailResponse>();
    }

}
