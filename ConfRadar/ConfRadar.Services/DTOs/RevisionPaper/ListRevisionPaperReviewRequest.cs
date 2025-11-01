using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.RevisionPaper
{
    public class ListRevisionPaperReviewRequest
    {
        [Required(ErrorMessage = "RevisionPaperId là bắt buộc")]
        public string RevisionPaperId { get; set; }
        [Required(ErrorMessage = "PaperId là bắt buộc")]
        public string PaperId { get; set; }

    }
    public class RevisionPaperReviewResponse
    {
        public string? RevisionPaperReviewId { get; set; } = null!;
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
