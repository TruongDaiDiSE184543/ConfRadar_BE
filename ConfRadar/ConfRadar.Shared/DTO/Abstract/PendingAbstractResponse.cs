using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.Abstract
{
    public class PendingAbstractResponse
    {
        public string AbstractId { get; set; } = null!;
        public string? AbstractUrl { get; set; }
        public string PaperId { get; set; } = null!;
        public string? PresenterId { get; set; }
        public string? PresenterName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
