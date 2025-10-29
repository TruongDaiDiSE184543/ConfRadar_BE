using ConfRadar.Services.Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.RevisionPaper
{
    public class CreateRevisionPaperSubmissionRequest
    {
        [Required(ErrorMessage = "File revision là bắt buộc")]
        public IFormFile RevisionPaperFile { get; set; }

        //[Required(ErrorMessage = "RevisionPaperId là bắt buộc")]
        //public string? RevisionPaperId { get; set; }

        [Required(ErrorMessage = "RevisionDeadlineRoundId là bắt buộc")]
        public string? RevisionDeadlineRoundId { get; set; }
        [Required(ErrorMessage = "Paper Id là bắt buộc")]
        public string PaperId { get; set; }
    }
    public class CreateRevisionPaperSubmissionFeedback
    {
        [Required(ErrorMessage = "Feedback là bắt buộc")]
        public List<RevisionPaperSubmissionFeedbackRequest> Feedbacks { get; set; }
        [Required(ErrorMessage = "RevisionPaperSubmissionId là bắt buộc")]

        public string RevisionPaperSubmissionId { get; set; }
        [Required(ErrorMessage = "PaperId là bắt buộc")]

        public string PaperId { get; set; }
    }
    public class RevisionPaperSubmissionFeedbackRequest
    {
        [Required(ErrorMessage = "Feedback là bắt buộc")]

        public string? Feedback { get; set; }
        [Required(ErrorMessage = "SortOrder là bắt buộc")]

        public int? SortOrder { get; set; }
    }
    public class RevisionPaperSubmissionFeedbackResponse
    {
        [Required(ErrorMessage = "RevisionSubmissionFeedbackId là bắt buộc")]

        public string RevisionSubmissionFeedbackId { get; set; } = null!;
        [Required(ErrorMessage = "Response là bắt buộc")]
        public string? Response { get; set; }

    }
    public class CreateRevisionPaperSubmissionResponse
    {
        [Required(ErrorMessage = "Responses là bắt buộc")]

        public List<RevisionPaperSubmissionFeedbackResponse> Responses { get; set; }
        [Required(ErrorMessage = "RevisionPaperSubmissionId là bắt buộc")]

        public string? RevisionPaperSubmissionId { get; set; }
        [Required(ErrorMessage = "PaperId là bắt buộc")]

        public string PaperId { get; set; }

    }
    public class CreateRevisionPaperReviewRequest
    {
        [Required(ErrorMessage = "GlobalStatusEnum là bắt buộc")]
        [EnumDataType(typeof(GlobalStatusEnum), ErrorMessage = "Global status là bắt buộc")]
        public GlobalStatusEnum GlobalStatus { get; set; }
        [Required(ErrorMessage = "Note là bắt buộc")]
        public string? Note { get; set; }
        [Required(ErrorMessage = "FeedbackToAuthor là bắt buộc")]
        public string? FeedbackToAuthor { get; set; }

        public IFormFile? FeedbackMaterialFile { get; set; }
        [Required(ErrorMessage = "RevisionPaperId là bắt buộc")]
        public string? RevisionPaperId { get; set; }

        [Required(ErrorMessage = "PaperId là bắt buộc")]

        public string PaperId { get; set; }
    }
}
