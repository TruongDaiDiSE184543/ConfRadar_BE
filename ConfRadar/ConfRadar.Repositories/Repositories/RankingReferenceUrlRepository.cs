using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRankingReferenceUrlRepository
    {
        Task<int> CreateRankingReferenceUrlAsync(RankingReferenceUrl rankingReferenceUrl);
        Task<int> UpdateRankingReferenceUrlAsync(RankingReferenceUrl rankingReferenceUrl);
        Task<int> DeleteRankingReferenceUrlAsync(RankingReferenceUrl rankingReferenceUrl);
        Task<RankingReferenceUrl?> GetRankingReferenceUrlByIdAsync(string referenceUrlId);
        Task<List<RankingReferenceUrl>> GetRankingReferenceUrlsByConferenceIdAsync(string conferenceId);
    }

    public class RankingReferenceUrlRepository
        : GenericRepository<RankingReferenceUrl>, IRankingReferenceUrlRepository
    {
        public RankingReferenceUrlRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateRankingReferenceUrlAsync(RankingReferenceUrl rankingReferenceUrl)
        {
            return await CreateAsync(rankingReferenceUrl);
        }

        public async Task<int> UpdateRankingReferenceUrlAsync(RankingReferenceUrl rankingReferenceUrl)
        {
            return await UpdateAsync(rankingReferenceUrl);
        }

        public async Task<int> DeleteRankingReferenceUrlAsync(RankingReferenceUrl rankingReferenceUrl)
        {
            _context.RankingReferenceUrls.Remove(rankingReferenceUrl);
            return await _context.SaveChangesAsync();
        }

        public async Task<RankingReferenceUrl?> GetRankingReferenceUrlByIdAsync(string referenceUrlId)
        {
            return await _context.RankingReferenceUrls
                .FirstOrDefaultAsync(r => r.ReferenceUrlId == referenceUrlId);
        }

        public async Task<List<RankingReferenceUrl>> GetRankingReferenceUrlsByConferenceIdAsync(string conferenceId)
        {
            return await _context.RankingReferenceUrls
                .Where(r => r.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}