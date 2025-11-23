using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Orcid
{
    public class OrcidBiographyResponse
    {
        [JsonPropertyName("last-modified-date")]
        public DateValue LastModifiedDate { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("visibility")]
        public string Visibility { get; set; }
    }

    // Lớp phụ trợ để lấy giá trị timestamp (kiểu long)
    public class DateValue
    {
        [JsonPropertyName("value")]
        public long Value { get; set; }
    }
}
