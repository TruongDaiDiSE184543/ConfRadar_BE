using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.WaitList
{
    public class CustomerWaitListRequest
    {
    }
    public class LeaveWaitListRequest
    {
        [Required(ErrorMessage = "Mã hội nghị là bắt buộc")]
        public string ConferenceId { get; set; }
    }
}
