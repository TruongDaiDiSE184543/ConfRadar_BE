using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.FavouriteConference
{
    public class FavouriteConferenceRequest
    {
        [Required(ErrorMessage ="Mã sự kiện là bắt buộc")]
        public string ConferenceId { get; set; }
    }
}
