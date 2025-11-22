using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Dashboard
{
    public class ConferenceReminderDto
    {
        public string ConferenceId { get; set; }
        public string ConferenceName { get; set; }
        public string BannerImageUrl { get; set; }

        // Thời gian
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        // Con số quan trọng nhất cho Reminder
        public int DaysUntilStart { get; set; }


        // Trạng thái hiện tại (VD: Published, Open for Ticket...)
        public string StatusName { get; set; }
    }
}
