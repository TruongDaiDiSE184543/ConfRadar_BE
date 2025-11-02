using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Paper
{
    public class AssignAuthorToPaperRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Paper ID is required")]
        public string PaperId { get; set; }
    }

    public class AssignReviewerToPaperRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Paper ID is required")]
        public string PaperId { get; set; }

        public bool IsHeadReviewer { get; set; }
    }
}