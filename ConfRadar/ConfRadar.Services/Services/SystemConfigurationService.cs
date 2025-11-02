using ConfRadar.Services.DTOs.Configuration;

namespace ConfRadar.Services.Services
{
    public interface ISystemConfigurationService
    {
        Task<SessionConfigurationResponse> GetAllSessionConfigurationAsync();
        Task<int> UpdateSessionConfigurationAsync(SessionConfigurationRequest request);
        int GetSaleEndAndStartDateInterval();
    }

    public class SystemConfigurationService : ISystemConfigurationService
    {
        // These are the configurable values that can be changed via the API
        private static double _minimumSessionDurationHours = 1.0;  // Minimum session duration in hours
        private static double _sessionIntervalHours = 0.5;        // Interval between sessions in hours
        public static int _intervalDateFromTicketOpenSaleEndDateToConferenceStart { get; set; } = 30; //interval of ticketsaleEnd to startdate in conference table

        public async Task<SessionConfigurationResponse> GetAllSessionConfigurationAsync()
        {
            return new SessionConfigurationResponse
            {
                MinimumSessionDurationHours = _minimumSessionDurationHours,
                SessionIntervalHours = _sessionIntervalHours
            };
        }

        public int GetSaleEndAndStartDateInterval()
        {
            return _intervalDateFromTicketOpenSaleEndDateToConferenceStart;
        }

        public async Task<int> UpdateSessionConfigurationAsync(SessionConfigurationRequest request)
        {
            if (request.MinimumSessionDurationHours.HasValue)
            {
                _minimumSessionDurationHours = request.MinimumSessionDurationHours.Value;
            }

            if (request.SessionIntervalHours.HasValue)
            {
                _sessionIntervalHours = request.SessionIntervalHours.Value;
            }

            // Return 1 to indicate successful update
            return 1;
        }


    }
}