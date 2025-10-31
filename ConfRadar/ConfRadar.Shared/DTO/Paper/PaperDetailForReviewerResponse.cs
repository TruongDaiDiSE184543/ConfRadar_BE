using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.Paper
{
    public class PaperDetailForReviewerResponse
    {
        public bool IsHeadReviewer { get; set; }
        public FullPaperDetailForReviewerResponse FullPaper { get; set; } = new();
        public RevisonPaperForReviewerResponse RevisionPaper { get; set; } = new();

    }
    public class FullPaperDetailForReviewerResponse
    {
        public string? FullPaperId { get; set; } = null!;
        public string? ReviewStatusId { get; set; }
        public string? ReviewStatusName { get; set; }
        public string? FullPaperUrl { get; set; }
        public bool? IsAllSubmittedFullPaperReview { get; set; }
    }
    public class RevisonPaperForReviewerResponse
    {
        public string RevisionPaperId { get; set; } = null!;

        public int? RevisionRound { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public bool? IsAllSubmittedRevisionPaperReview { get; set; }
        public bool? IsAnsweredAllDiscussion { get; set; }
        public List<RevisionPaperSubmissionForReviewerResponse> RevisionPaperSubmissions { get; set; } = new();
    }
    public class RevisionPaperSubmissionForReviewerResponse
    {
        public string RevisionPaperSubmissionId { get; set; } = null!;

        public string? RevisionPaperUrl { get; set; }

        public string? RevisionPaperId { get; set; }

        public string? RevisionDeadlineRoundId { get; set; }
        public DateOnly? EndDate { get; set; }

        public int? RoundNumber { get; set; }

        public List<RevisionPaperSubmissionFeedBackForReviewerResponse> RevisionSubmissionFeedbacks { get; set; } = new();
        public List<RevisionPaperReviewForReviewerResponse> RevisionPaperReviews { get; set; } = new();
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
}
