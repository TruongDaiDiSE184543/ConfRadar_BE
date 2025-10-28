using ConfRadar.Services.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.RevisionPaper
{
    public class ListRevisionPaperReviewRequest
    {
        public string? RevisionPaperId { get; set; }
        public string? PaperId { get; set; }
        
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
