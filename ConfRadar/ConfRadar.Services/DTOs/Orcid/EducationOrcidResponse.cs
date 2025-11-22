using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Orcid
{
    internal class EducationOrcidResponse
    {
    }
    public class OrcidEducationsResponse
    {
        [JsonPropertyName("affiliation-group")]
        public List<EducationAffiliationGroup> AffiliationGroup { get; set; }
    }

    public class EducationAffiliationGroup
    {
        [JsonPropertyName("summaries")]
        public List<EducationSummaryContainer> Summaries { get; set; }
    }

    public class EducationSummaryContainer
    {
        [JsonPropertyName("education-summary")]
        public EducationSummary EducationSummary { get; set; }
    }

    public class EducationSummary
    {
        [JsonPropertyName("put-code")]
        public long PutCode { get; set; }

        [JsonPropertyName("department-name")]
        public string DepartmentName { get; set; }

        [JsonPropertyName("role-title")]
        public string RoleTitle { get; set; } // Đây là "Bằng cấp" (Degree)

        [JsonPropertyName("start-date")]
        public OrcidDate StartDate { get; set; }

        [JsonPropertyName("end-date")]
        public OrcidDate EndDate { get; set; }

        [JsonPropertyName("organization")]
        public Organization Organization { get; set; }
    }

    public class OrcidDate
    {
        [JsonPropertyName("year")]
        public ValueObject Year { get; set; }
        // Bạn có thể thêm Month, Day nếu cần
    }

    public class Organization
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public Address Address { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } // Ví dụ: "VN", "TH"
    }

}
