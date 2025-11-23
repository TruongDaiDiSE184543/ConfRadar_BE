using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Dashboard
{
    public class ConferenceStatsResponse
    {
        // Tổng số conference của user
        public int Total { get; set; }

        // Danh sách nhóm theo Status
        public List<ConferenceGroup> GroupByStatus { get; set; } = new List<ConferenceGroup>();

        // Danh sách nhóm theo Type (Category)
        //public List<ConferenceGroup> GroupByType { get; set; } = new List<ConferenceGroup>();
    }

    public class ConferenceGroup
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public int Count { get; set; }
        public List<ConfRadar.Services.DTOs.Conference.ConferenceResponseDTO> Conferences { get; set; } // Hoặc dùng ConferenceDTO nếu muốn gọn nhẹ hơn
    }
}