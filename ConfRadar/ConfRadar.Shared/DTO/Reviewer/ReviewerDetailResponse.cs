namespace ConfRadar.Shared.DTO.Reviewer
{
    public class GetTotalAssignPapersDetailResponse
    {
        public int? TotalPaperAssignPaper { get; set; }
        public List<PapersDetailResponseForReviewer> PaperDetails { get; set; } = new List<PapersDetailResponseForReviewer>();

    }
    public class PapersDetailResponseForReviewer
    {
        public bool? IsHeadReviewer { get; set; }
        public string? PaperId { get; set; } = null!;
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? PaperPhaseId { get; set; }
        public string? PaperPhaseName { get; set; }
        public string? ResearchConferencePhaseId { get; set; }
        public DateTime? PaperCreatedAt { get; set; }
        public string? PaperTitle { get; set; }
        public string? PaperDescription { get; set; }
        public bool? PaperRefundedStatus { get; set; }
    }



    public class GetTotalReviewedPapersDetailResponse
    {
        public int TotalPaperReviewed { get; set; } = 0;
        public List<PapersDetailResponseForReviewer> PaperDetails { get; set; } = new List<PapersDetailResponseForReviewer>();

    }
    public class GetTotalPendingReviewsDetailResponse
    {
        public int TotalPendingReview { get; set; } = 0;
        public List<PapersDetailResponseForReviewer> PaperDetails { get; set; } = new List<PapersDetailResponseForReviewer>();

    }



}
