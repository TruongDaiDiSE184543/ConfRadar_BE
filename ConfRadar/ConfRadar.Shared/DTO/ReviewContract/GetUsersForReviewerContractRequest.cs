using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class GetUsersForReviewerContractRequest
    {
        [Required(ErrorMessage = "Mã hội nghĩ là bắt buộc")]
        public string ConferenceId { get; set; }
    }
}
