using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Orcid
{
    public class EducationConfRadarResponse
    {
        public long OrcidPutCode { get; set; }
        public string Degree { get; set; }      // Bằng cấp, ví dụ: "Phd"
        public string Institution { get; set; } // Tên trường/tổ chức
        public string Period { get; set; }      // Thời gian, ví dụ: "2015 - 2023"
        public string Location { get; set; }    // Địa điểm, ví dụ: "Bangkok, TH"
    }
}
