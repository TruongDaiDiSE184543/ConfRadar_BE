using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceTimelineRepository
    {
        Task<ConferenceTimeline?> GetConferenceTimelineByIdAsync(string id);
        Task<int> UpdateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline);
        Task<int> DeleteConferenceTimelineAsync(ConferenceTimeline conferenceTimeline);
        Task<int> CreateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline);
        Task<List<ConferenceTimeline>> GetAllConferenceTimelinesAsync();
        Task<List<ConferenceTimeline>> GetConferenceTimelineByConfIdAndStatusIdAsync(string confId, string previousId, string afterwardId);
        Task<ConferenceTimeline> GetLastTransitionConferenceTimelineByConfIdAndStatusIdAsync(string confId, string readyId, string onHoldId);
        Task<List<ConferenceTimeline>> GetConferenceTimelineByConfIdAsync(string confId);
    }

    public class ConferenceTimelineRepository : GenericRepository<ConferenceTimeline>, IConferenceTimelineRepository
    {
        public ConferenceTimelineRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline)
        {
            return await base.CreateAsync(conferenceTimeline);
        }

        public async Task<int> DeleteConferenceTimelineAsync(ConferenceTimeline conferenceTimeline)
        {
            _context.ConferenceTimelines.Remove(conferenceTimeline);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferenceTimeline?> GetConferenceTimelineByIdAsync(string id)
        {
            return await _context.ConferenceTimelines.FirstOrDefaultAsync(ct => ct.ConferenceTimelineId == id);
        }

        public async Task<List<ConferenceTimeline>> GetConferenceTimelineByConfIdAsync(string confId)
        {
            return await _context.ConferenceTimelines.Where(ct => ct.ConferenceId == confId).ToListAsync();
        }

        public async Task<List<ConferenceTimeline>> GetConferenceTimelineByConfIdAndStatusIdAsync(string confId, string previousId, string afterwardId)
        {
            return await _context.ConferenceTimelines.Where(ct => ct.ConferenceId == confId &&
            ct.PreviousStatusId == previousId &&
            ct.AfterwardStatusId == afterwardId
            ).ToListAsync();
        }

        public async Task<List<ConferenceTimeline>> GetAllConferenceTimelinesAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task<int> UpdateConferenceTimelineAsync(ConferenceTimeline conferenceTimeline)
        {
            return await base.UpdateAsync(conferenceTimeline);
        }

        public async Task<ConferenceTimeline> GetLastTransitionConferenceTimelineByConfIdAndStatusIdAsync(string confId, string beforeStatusId, string AfterStatusId)
        {
            return await _context.ConferenceTimelines.Where(c => c.ConferenceId == confId &&
            c.PreviousStatusId == beforeStatusId &&
            c.AfterwardStatusId == AfterStatusId).OrderByDescending(c => c.ChangeDate).FirstAsync();
        }
    }


}
