using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using ConfRadar.Services.Common;

namespace ConfRadar.Services.DTOs.FullPaperReview
{
    public class CreateFullPaperReviewRequest
    {
        [Required(ErrorMessage = "FullPaperId is required")]
        public string FullPaperId { get; set; }

        [Required(ErrorMessage = "Note is required")]
        public string Note { get; set; }

        public IFormFile? FeedbackMaterialFile { get; set; }

        [Required(ErrorMessage = "FeedbackToAuthor is required")]
        public string FeedbackToAuthor { get; set; }

        [Required(ErrorMessage = "GlobalStatus is required")]
        [EnumDataType(typeof(ReviewStatusEnum), ErrorMessage = "Review status is required")]
        public ReviewStatusEnum reviewStatus { get; set; }
    }

    public class UpdateFullPaperReviewStatusRequest
    {
        [Required(ErrorMessage = "FullPaperReviewId is required")]
        public string FullPaperReviewId { get; set; }

        [Required(ErrorMessage = "GlobalStatus is required")]
        [EnumDataType(typeof(GlobalStatusEnum), ErrorMessage = "Global status is required")]
        public ConfRadar.Repositories.Models.ReviewStatus Statusreview { get; set; }
    }

    public class ListFullPaperReviewRequest
    {
        [Required(ErrorMessage = "FullPaperId is required")]
        public string FullPaperId { get; set; }
    }

    public class FullPaperReviewResponse
    {
        public string FullPaperReviewId { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? FeedbackToAuthor { get; set; }
        public string? FeedbackMaterialUrl { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerAvatarUrl { get; set; }
        public string? FullPaperId { get; set; }
    }
}