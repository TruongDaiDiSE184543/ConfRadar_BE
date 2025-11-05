using ConfRadar.Repositories;
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
    public class ConferenceTimelineService : IConferenceTimelineService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConferenceTimelineService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ConferenceTimeline?> GetConferenceTimelineByIdAsync(string id)
        {
            return await _unitOfWork.ConferenceTimelineRepository.GetConferenceTimelineByIdAsync(id);
        }

        public async Task<int> CreateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline)
        {
            return await _unitOfWork.ConferenceTimelineRepository.CreateConferenceTimelineAsync(conferenceTimeline);
        }

        public async Task<int> UpdateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline)
        {
            return await _unitOfWork.ConferenceTimelineRepository.UpdateConferenceTimelineAsync(conferenceTimeline);
        }

        public async Task<int> DeleteConferenceTimelineAsync(ConferenceTimeline conferenceTimeline)
        {
            return await _unitOfWork.ConferenceTimelineRepository.DeleteConferenceTimelineAsync(conferenceTimeline);
        }

        public async Task<List<ConferenceTimeline>> GetAllConferenceTimelinesAsync()
        {
            return await _unitOfWork.ConferenceTimelineRepository.GetAllConferenceTimelinesAsync();
        }

        public async Task<List<ConferenceTimeline>> GetConferenceTimelinesByConferenceIdAsync(string conferenceId)
        {
            return await _unitOfWork.ConferenceTimelineRepository.GetConferenceTimelineByConfIdAsync(conferenceId);
        }

        public async Task<List<ConferenceTimeline>> GetConferenceTimelineByConfIdAndStatusIdAsync(string confId, string previousId, string afterwardId)
        {
            return await _unitOfWork.ConferenceTimelineRepository.GetConferenceTimelineByConfIdAndStatusIdAsync(confId, previousId, afterwardId);
        }
    }
}