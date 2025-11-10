namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class GetUsersForReviewerContractResponse
    {
        public string UserId { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BioDescription { get; set; }
    }
}
