namespace ConfRadar.Services.DTOs.Paper
{
    public class ReviewerWorkItemResponse
    {
        public string PaperId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? ConferenceName { get; set; }
        public string? CurrentPhaseName { get; set; }
        public bool IsHeadReviewer { get; set; }

        public FullPaperWorkItem? FullPaperWork { get; set; }
        public RevisionWorkItem? RevisionWork { get; set; }
        public CameraReadyWorkItem? CameraReadyWork { get; set; }
    }

    public class FullPaperWorkItem
    {
        public string FullPaperId { get; set; } = null!;
        public string? FileUrl { get; set; }
        public string? StatusName { get; set; }

        public bool IsMyReviewSubmitted { get; set; }
        public string? MyReviewResult { get; set; }

        public bool CanReview { get; set; }       // Regular Reviewer
        public bool CanDecide { get; set; }       // Head Reviewer
    }

    public class RevisionWorkItem
    {
        public string RevisionPaperId { get; set; } = null!;
        public int RevisionRound { get; set; }
        public bool IsFeedbackSubmitted { get; set; } // True nếu Head Reviewer đã gửi feedback cho file này
        public string? LatestFileUrl { get; set; }
        public string? StatusName { get; set; }

        public bool IsMyReviewSubmitted { get; set; } // Nếu có chấm điểm Revision

        public bool CanGiveFeedback { get; set; } // Chat/Feedback trong thời gian Round
        public bool CanDecide { get; set; }       // Quyết định khi hết hạn Round
    }

    public class CameraReadyWorkItem
    {
        public string CameraReadyId { get; set; } = null!;
        public string? FileUrl { get; set; }
        public string? StatusName { get; set; }

        public bool CanDecide { get; set; }       // Head Reviewer
    }
}