using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ITechnicalConferenceDetailRepository
    {
        Task<TechnicalConferenceDetail?> GetByConferenceIdAsync(string conferenceId);
        Task<int> CreateTechnicalAsync(TechnicalConferenceDetail detail);
        Task<int> UpdateTechnicalAsync(TechnicalConferenceDetail detail);
        Task<int> DeleteTechnicalAsync(TechnicalConferenceDetail detail);
        Task<List<TechnicalConferenceDetail>> GetAllAsync();
    }

    public class TechnicalConferenceDetailRepository : GenericRepository<TechnicalConferenceDetail>, ITechnicalConferenceDetailRepository
    {
        public TechnicalConferenceDetailRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<TechnicalConferenceDetail?> GetByConferenceIdAsync(string conferenceId)
        {
            return await _context.TechnicalConferenceDetails
                .FirstOrDefaultAsync(t => t.ConferenceId == conferenceId);
        }

        public async Task<int> CreateTechnicalAsync(TechnicalConferenceDetail detail)
        {
            return await CreateAsync(detail);
        }

        public async Task<int> UpdateTechnicalAsync(TechnicalConferenceDetail detail)
        {
            return await UpdateAsync(detail);
        }

        public async Task<int> DeleteTechnicalAsync(TechnicalConferenceDetail detail)
        {
            _context.TechnicalConferenceDetails.Remove(detail);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<TechnicalConferenceDetail>> GetAllAsync()
        {
            return await _context.TechnicalConferenceDetails.ToListAsync();
        }
    }
}