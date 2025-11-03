namespace ConfRadar.Shared.DTO.Paper
{
    public class PaperDetailForReviewerResponse
    {
        public bool IsHeadReviewer { get; set; }
        public FullPaperDetailForReviewerResponse FullPaper { get; set; } = new();
        public RevisonPaperForReviewerResponse RevisionPaper { get; set; } = new();
        public CameraReadyPaperForReviewerResponse CameraReady { get; set; } = new();
        public PaperPhaseForReviewerResponse CurrentPhase { get; set; } = new();

    }
    public class PaperPhaseForReviewerResponse
    {
        public string? PaperPhaseId { get; set; }
        public string? PhaseName { get; set; }
    }
    public class FullPaperDetailForReviewerResponse
    {
        public string? FullPaperId { get; set; } = null!;
        public string? ReviewStatusId { get; set; }
        public string? Title { get; set; }

        public string? Description { get; set; }
        public string? ReviewStatusName { get; set; }
        public string? FullPaperUrl { get; set; }
        public List<FullPaperReviewForReviewerResponse> FullPaperReviews { get; set; } = new();
        public bool? IsAllSubmittedFullPaperReview { get; set; }
        public DateOnly? FullPaperStartDate { get; set; }

        public DateOnly? FullPaperEndDate { get; set; }
    }
    public class FullPaperReviewForReviewerResponse
    {
        public string FullPaperReviewId { get; set; } = null!;
        public string? ReviewStatusId { get; set; }
        public string? ReviewStatusName { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? FeedbackToAuthor { get; set; }
        public string? FeedbackMaterialUrl { get; set; }
        public string? FullPaperId { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerAvatarUrl { get; set; }

    }
    public class RevisonPaperForReviewerResponse
    {
        public string RevisionPaperId { get; set; } = null!;

        public int? RevisionRound { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public bool? IsAllSubmittedRevisionPaperReview { get; set; }
        public bool? IsAnsweredAllDiscussion { get; set; }
        public DateOnly? ReviewStartDate { get; set; }

        public DateOnly? ReviewEndDate { get; set; }
        public List<RevisionPaperReviewForReviewerResponse> RevisionPaperReviews { get; set; } = new(); // cho head reviewer
        public List<RevisionPaperSubmissionForReviewerResponse> RevisionPaperSubmissions { get; set; } = new();
    }
    public class RevisionPaperSubmissionForReviewerResponse
    {
        public string RevisionPaperSubmissionId { get; set; } = null!;

        public string? RevisionPaperUrl { get; set; }

        public string? RevisionPaperId { get; set; }
        public string? Title { get; set; }

        public string? Description { get; set; }
        public string? RevisionDeadlineRoundId { get; set; }
        public DateOnly? RevisionDeadlineStartSubmissionDate { get; set; }
        public DateOnly? RevisionDeadlineEndSubmissionDate { get; set; }
        public int? RevisionDeadlineRoundNumber { get; set; }
        public List<RevisionPaperSubmissionFeedBackForReviewerResponse> RevisionSubmissionFeedbacks { get; set; } = new(); // cho head reviewer

    }
    public class RevisionPaperSubmissionFeedBackForReviewerResponse
    {
        public string RevisionSubmissionFeedbackId { get; set; } = null!;

        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Feedback { get; set; }

        public string? Response { get; set; }

        public int? SortOrder { get; set; }

        public DateTime? CreatedAt { get; set; }



    }
    public class RevisionPaperReviewForReviewerResponse
    {
        public string RevisionPaperReviewId { get; set; } = null!;
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? FeedbackToAuthor { get; set; }
        public string? FeedbackMaterialUrl { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerAvatarUrl { get; set; }
        public string? RevisionPaperId { get; set; }

    }
    public class CameraReadyPaperForReviewerResponse
    {
        public string PaperId { get; set; }

        public string? CameraReadyId { get; set; } = null!;

        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? CameraReadyUrl { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ReviewAt { get; set; }

        public DateOnly? CameraReadyStartDate { get; set; }

        public DateOnly? CameraReadyEndDate { get; set; }



    }
}
