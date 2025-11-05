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

    public class AddWaitListRequest
    {
        [Required(ErrorMessage = "Mã hội nghị là bắt buộc")]
        public string ConferenceId { get; set; }
    }


    public class LeaveWaitListResponse
    {
        public string? ConferenceId { get; set; }
        public bool IsLeaved { get; set; }
    }
    public class AddWaitListResponse
    {
        public string? ConferenceId { get; set; }
        public bool? IsAdded { get; set; }
    }
}
