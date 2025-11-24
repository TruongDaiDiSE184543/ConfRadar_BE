using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Paper
{
    public class MarkCompleteReviseRequest
    {
        [Required(ErrorMessage = "Mã revision round deadline là bắt buộc")]
        public string RevisionRoundDeadlineId { get; set; }
        [Required(ErrorMessage = "Mã revision paper là bắt buộc")]
        public string RevisionPaperId { get; set; } = null!;
        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
        public string PaperId { get; set; } = null!;

    }
}
