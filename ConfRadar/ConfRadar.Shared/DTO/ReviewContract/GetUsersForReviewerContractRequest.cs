using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class GetUsersForReviewerContractRequest
    {
        [Required(ErrorMessage ="Mã hội nghĩ là bắt buộc")]
        public string ConferenceId { get; set; }
    }
}
