using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IResearchConferenceDetailRepository
    {
        Task<int> CreateResearchConferenceDetailAsync(ResearchConferenceDetail researchConferenceDetail);
        Task<int> UpdateResearchConferenceDetailAsync(ResearchConferenceDetail researchConferenceDetail);
        Task<int> DeleteResearchConferenceDetailAsync(ResearchConferenceDetail researchConferenceDetail);
        Task<ResearchConferenceDetail?> GetResearchConferenceDetailByConferenceIdAsync(string conferenceId);
    }

    public class ResearchConferenceDetailRepository
        : GenericRepository<ResearchConferenceDetail>, IResearchConferenceDetailRepository
    {
        public ResearchConferenceDetailRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateResearchConferenceDetailAsync(ResearchConferenceDetail researchConferenceDetail)
        {
            return await CreateAsync(researchConferenceDetail);
        }

        public async Task<int> UpdateResearchConferenceDetailAsync(ResearchConferenceDetail researchConferenceDetail)
        {
            return await UpdateAsync(researchConferenceDetail);
        }

        public async Task<int> DeleteResearchConferenceDetailAsync(ResearchConferenceDetail researchConferenceDetail)
        {
            _context.ResearchConferenceDetails.Remove(researchConferenceDetail);
            return await _context.SaveChangesAsync();
        }

        public async Task<ResearchConferenceDetail?> GetResearchConferenceDetailByConferenceIdAsync(string conferenceId)
        {
            return await _context.ResearchConferenceDetails
                .Include(r => r.RankingCategory) // Include the RankingCategory for related data
                .FirstOrDefaultAsync(r => r.ConferenceId == conferenceId);
        }
    }
}