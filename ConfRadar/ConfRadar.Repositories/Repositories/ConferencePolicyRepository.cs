using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferencePolicyRepository
    {
        Task<int> CreateConferencePolicyAsync(Policy policy);
        Task<int> CreateMutipleConferencePoliciesAsync(List<Policy> policy);
        Task<int> UpdateConferencePolicyAsync(Policy policy);
        Task<int> DeleteConferencePolicyAsync(Policy policy);
        Task<Policy?> GetConferencePolicyByIdAsync(string policyId);
        Task<List<Policy>> GetAllConferencePoliciesAsync();
        Task<List<Policy>> GetPoliciesByConferenceIdAsync(string conferenceId);
        Task<Conference> GetConferenceByPolicyId(string policyId);
    }

    public class ConferencePolicyRepository : GenericRepository<Policy>, IConferencePolicyRepository
    {
        public ConferencePolicyRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferencePolicyAsync(Policy policy)
        {
            return await CreateAsync(policy);
        }
        public async Task<int> CreateMutipleConferencePoliciesAsync(List<Policy> policy)
        {
            await _context.Policies.AddRangeAsync(policy);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateConferencePolicyAsync(Policy policy)
        {
            return await UpdateAsync(policy);
        }

        public async Task<int> DeleteConferencePolicyAsync(Policy policy)
        {
            _context.Policies.Remove(policy);
            return await _context.SaveChangesAsync();
        }

        public async Task<Policy?> GetConferencePolicyByIdAsync(string policyId)
        {
            return await _context.Policies
                .FirstOrDefaultAsync(c => c.PolicyId == policyId);
        }

        public async Task<Conference> GetConferenceByPolicyId(string policyId)
        {
            var policy = await _context.Policies.FirstOrDefaultAsync(p => p.PolicyId == policyId);
            return await _context.Conferences.FirstOrDefaultAsync(c => c.ConferenceId == policy.ConferenceId);
        }

        public async Task<List<Policy>> GetAllConferencePoliciesAsync()
        {
            return await _context.Policies.ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByConferenceIdAsync(string conferenceId)
        {
            return await _context.Policies
                .Where(cp => cp.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}