using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Ticket
{
    public class PaidTicketResponse
    {
        public string TicketId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string AvatarUrl { get; set; } 
        public string Email { get; set; } = default!;
        public DateTime RegisteredDate { get; set; }
        public string ConferenceId { get; set; } = default!;
        public string ConferenceName { get; set; } = default!;
      
    }
}
