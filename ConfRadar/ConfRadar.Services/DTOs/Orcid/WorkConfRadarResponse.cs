using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Orcid
{
    public class WorkConfRadarResponse
    {
        public long OrcidPutCode { get; set; }
        public string Title { get; set; }
        public string WorkType { get; set; }
        public string PublicationYear { get; set; }
        public string Doi { get; set; }
        public string Link { get; set; }
    }
}
