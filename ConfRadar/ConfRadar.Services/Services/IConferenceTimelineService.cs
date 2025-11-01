using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface IConferenceTimelineService
    {
        Task<ConferenceTimeline?> GetConferenceTimelineByIdAsync(string id);
        Task<int> CreateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline);
        Task<int> UpdateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline);
        Task<int> DeleteConferenceTimelineAsync(ConferenceTimeline conferenceTimeline);
        Task<List<ConferenceTimeline>> GetAllConferenceTimelinesAsync();
        Task<List<ConferenceTimeline>> GetConferenceTimelinesByConferenceIdAsync(string conferenceId);
        Task<List<ConferenceTimeline>> GetConferenceTimelineByConfIdAndStatusIdAsync(string confId, string previousId, string afterwardId);
    }
}