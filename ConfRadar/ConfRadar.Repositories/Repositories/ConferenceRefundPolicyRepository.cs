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
    public  interface IConferenceRefundPolicyRepository
    {
        Task<int> CreateConferenceRefundPolicyAsync(RefundPolicy refundPolicy);
        Task<int> CreateMultipleConferenceRefundPoliciesAsync(List<RefundPolicy> refundPolicies);
        Task<int> UpdateConferenceRefundPolicyAsync(RefundPolicy refundPolicy);
        Task<int> DeleteConferenceRefundPolicyAsync(RefundPolicy refundPolicy);
        Task<RefundPolicy?> GetConferenceRefundPolicyByIdAsync(string refundPolicyId);
        Task<List<RefundPolicy>> GetAllConferenceRefundPoliciesAsync();
        Task<List<RefundPolicy>> GetRefundPoliciesByConferenceIdAsync(string conferenceId);
    }
    public class ConferenceRefundPolicyRepository
    : GenericRepository<RefundPolicy>, IConferenceRefundPolicyRepository
    {
        public ConferenceRefundPolicyRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceRefundPolicyAsync(RefundPolicy refundPolicy)
        {
            return await CreateAsync(refundPolicy);
        }

        public async Task<int> CreateMultipleConferenceRefundPoliciesAsync(List<RefundPolicy> refundPolicies)
        {
            await _context.RefundPolicies.AddRangeAsync(refundPolicies);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateConferenceRefundPolicyAsync(RefundPolicy refundPolicy)
        {
            return await UpdateAsync(refundPolicy);
        }

        public async Task<int> DeleteConferenceRefundPolicyAsync(RefundPolicy refundPolicy)
        {
            _context.RefundPolicies.Remove(refundPolicy);
            return await _context.SaveChangesAsync();
        }

        public async Task<RefundPolicy?> GetConferenceRefundPolicyByIdAsync(string refundPolicyId)
        {
            return await _context.RefundPolicies
                .FirstOrDefaultAsync(rp => rp.RefundPolicyId == refundPolicyId);
        }

        public async Task<List<RefundPolicy>> GetAllConferenceRefundPoliciesAsync()
        {
            return await _context.RefundPolicies.ToListAsync();
        }

        public async Task<List<RefundPolicy>> GetRefundPoliciesByConferenceIdAsync(string conferenceId)
        {
            return await _context.RefundPolicies
                .Where(rp => rp.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}
