using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ISponsorRepository
    {
        Task<int> CreateSponsorAsync(Sponsor sponsor);
        Task<int> UpdateSponsorAsync(Sponsor sponsor);
        Task<int> DeleteSponsorAsync(Sponsor sponsor);
        Task<Sponsor?> GetSponsorByIdAsync(string sponsorId);
        Task<List<Sponsor>> GetAllSponsorsAsync();
        Task<List<Sponsor>> GetSponsorsByConferenceIdAsync(string conferenceId);
    }

    public class SponsorRepository : GenericRepository<Sponsor>, ISponsorRepository
    {
        public SponsorRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateSponsorAsync(Sponsor sponsor)
        {
            return await CreateAsync(sponsor);
        }

        public async Task<int> UpdateSponsorAsync(Sponsor sponsor)
        {
            return await UpdateAsync(sponsor);
        }

        public async Task<int> DeleteSponsorAsync(Sponsor sponsor)
        {
            _context.Sponsors.Remove(sponsor);
            return await _context.SaveChangesAsync();
        }

        public async Task<Sponsor?> GetSponsorByIdAsync(string sponsorId)
        {
            return await _context.Sponsors
                .FirstOrDefaultAsync(c => c.SponsorId == sponsorId);
        }

        public async Task<List<Sponsor>> GetAllSponsorsAsync()
        {
            return await _context.Sponsors.ToListAsync();
        }

        public async Task<List<Sponsor>> GetSponsorsByConferenceIdAsync(string conferenceId)
        {
            return await _context.Sponsors
                .Where(s => s.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}