namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class PaperDetailBelongToConferenceInReviewContractResposne
    {
        public string PaperId { get; set; } = null!;
        public string? ConferenceId { get; set; }
        public string? PaperPhaseId { get; set; }
        public string? PhaseName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
