using ConfRadar.Services.DTOs.Conference;

namespace ConfRadar.Services.DTOs.Dashboard
{
    public class ConferenceContractResponse
    {

        public ConferenceResponse ConferenceResponse { get; set; }

        public int? Commission { get; set; }
        public string? ContractUrl { get; set; }
        public string? TargetAudience { get; set; }
    }
}
