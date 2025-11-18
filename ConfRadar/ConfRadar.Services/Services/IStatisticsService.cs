using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Statistics;

namespace ConfRadar.Services.Services
{
    public interface IStatisticsService
    {
        Task<ConferenceStatisticsResponse> GetConferenceStatisticsAsync(string conferenceId);
        Task<ExportStatisticsResponse> ExportConferenceStatisticsAsync(string conferenceId, string exportFormat);
    }
}