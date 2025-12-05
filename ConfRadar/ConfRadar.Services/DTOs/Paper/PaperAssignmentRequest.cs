using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Paper
{
    public class AssignCoAuthorsToPaperRequest
    {
        [Required(ErrorMessage = "Danh sách User ID là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một User ID trong danh sách")]
        public List<string> UserIds { get; set; }

        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
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