using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.FavouriteConference
{
    public class FavouriteConferenceRequest
    {
        [Required(ErrorMessage = "Mã sự kiện là bắt buộc")]
        public string ConferenceId { get; set; }
    }
}
