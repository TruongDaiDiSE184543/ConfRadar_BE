using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Configuration
{
    public class SessionConfigurationRequest
    {
        [Range(0.1, 24, ErrorMessage = "Minimum session duration must be between 0.1 and 24 hours")]
        public double? MinimumSessionDurationHours { get; set; } = 1.0; // Default to 1 hour

        [Range(0, 24, ErrorMessage = "Session interval must be between 0 and 24 hours")]
        public double? SessionIntervalHours { get; set; } = 0.5; // Default to 30 minutes
        public int? IntervalDateFromTicketOpenSaleEndDateToConferenceStart {  get; set; }
    }

    public class SessionConfigurationResponse
    {
        public double MinimumSessionDurationHours { get; set; }
        public double SessionIntervalHours { get; set; }
    }
}