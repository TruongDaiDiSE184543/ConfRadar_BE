using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.WaitList
{
    public class CustomerWaitListRequest
    {
    }
    public class LeaveWaitListRequest
    {
        [Required(ErrorMessage ="Mã hội nghị là bắt buộc")]
        public string ConferenceId { get; set; }
    }
}
