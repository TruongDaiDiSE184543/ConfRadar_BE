using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRankingFileUrlRepository
    {
        Task<int> CreateRankingFileUrlAsync(RankingFileUrl rankingFileUrl);
        Task<int> UpdateRankingFileUrlAsync(RankingFileUrl rankingFileUrl);
        Task<int> DeleteRankingFileUrlAsync(RankingFileUrl rankingFileUrl);
        Task<RankingFileUrl?> GetRankingFileUrlByIdAsync(string rankingFileUrlId);
        Task<List<RankingFileUrl>> GetRankingFileUrlsByConferenceIdAsync(string conferenceId);
    }

    public class RankingFileUrlRepository
        : GenericRepository<RankingFileUrl>, IRankingFileUrlRepository
    {
        public RankingFileUrlRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateRankingFileUrlAsync(RankingFileUrl rankingFileUrl)
        {
            return await CreateAsync(rankingFileUrl);
        }

        public async Task<int> UpdateRankingFileUrlAsync(RankingFileUrl rankingFileUrl)
        {
            return await UpdateAsync(rankingFileUrl);
        }

        public async Task<int> DeleteRankingFileUrlAsync(RankingFileUrl rankingFileUrl)
        {
            _context.RankingFileUrls.Remove(rankingFileUrl);
            return await _context.SaveChangesAsync();
        }

        public async Task<RankingFileUrl?> GetRankingFileUrlByIdAsync(string rankingFileUrlId)
        {
            return await _context.RankingFileUrls
                .FirstOrDefaultAsync(r => r.RankingFileUrlId == rankingFileUrlId);
        }

        public async Task<List<RankingFileUrl>> GetRankingFileUrlsByConferenceIdAsync(string conferenceId)
        {
            return await _context.RankingFileUrls
                .Where(r => r.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}