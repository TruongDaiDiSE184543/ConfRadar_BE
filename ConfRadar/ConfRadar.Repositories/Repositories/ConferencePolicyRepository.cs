using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferencePolicyRepository
    {
        Task<int> CreateConferencePolicyAsync(ConferencePolicy policy);
        Task<int> UpdateConferencePolicyAsync(ConferencePolicy policy);
        Task<int> DeleteConferencePolicyAsync(ConferencePolicy policy);
        Task<ConferencePolicy?> GetConferencePolicyByIdAsync(string policyId);
        Task<List<ConferencePolicy>> GetAllConferencePoliciesAsync();
        Task<List<ConferencePolicy>> GetPoliciesByConferenceIdAsync(string conferenceId);
    }

    public class ConferencePolicyRepository : GenericRepository<ConferencePolicy>, IConferencePolicyRepository
    {
        public ConferencePolicyRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferencePolicyAsync(ConferencePolicy policy)
        {
            return await CreateAsync(policy);
        }

        public async Task<int> UpdateConferencePolicyAsync(ConferencePolicy policy)
        {
            return await UpdateAsync(policy);
        }

        public async Task<int> DeleteConferencePolicyAsync(ConferencePolicy policy)
        {
            _context.ConferencePolicies.Remove(policy);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferencePolicy?> GetConferencePolicyByIdAsync(string policyId)
        {
            return await _context.ConferencePolicies
                .FirstOrDefaultAsync(c => c.PolicyId == policyId);
        }

        public async Task<List<ConferencePolicy>> GetAllConferencePoliciesAsync()
        {
            return await _context.ConferencePolicies.ToListAsync();
        }

        public async Task<List<ConferencePolicy>> GetPoliciesByConferenceIdAsync(string conferenceId)
        {
            return await _context.ConferencePolicies
                .Where(cp => cp.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}