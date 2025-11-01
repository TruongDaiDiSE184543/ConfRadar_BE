using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IReviewerContractRepository
    {
        Task<int> CreateReviewerContractAsync(ReviewerContract contract);
        Task<int> CreateMultipleReviewerContractsAsync(List<ReviewerContract> contracts);
        Task<int> UpdateReviewerContractAsync(ReviewerContract contract);
        Task<bool> DeleteReviewerContractAsync(ReviewerContract contract);
        Task<ReviewerContract?> GetReviewerContractByIdAsync(string contractId);
        Task<List<ReviewerContract>> GetReviewerContractsByUserIdAsync(string userId);
        Task<List<ReviewerContract>> GetReviewerContractsByConferenceIdAsync(string conferenceId);
        Task<ReviewerContract?> GetContractByUserAndConferenceAsync(string userId, string conferenceId);
        Task<List<ReviewerContract>> GetAllReviewerContractsAsync();
    }
    public class ReviewerContractRepository : GenericRepository<ReviewerContract>, IReviewerContractRepository
    {
        public ReviewerContractRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateReviewerContractAsync(ReviewerContract contract)
        {
            return await CreateAsync(contract);
        }

        public async Task<int> CreateMultipleReviewerContractsAsync(List<ReviewerContract> contracts)
        {
            await _context.ReviewerContracts.AddRangeAsync(contracts);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateReviewerContractAsync(ReviewerContract contract)
        {
            return await UpdateAsync(contract);
        }

        public async Task<bool> DeleteReviewerContractAsync(ReviewerContract contract)
        {
            return await RemoveAsync(contract);
        }

        public async Task<ReviewerContract?> GetReviewerContractByIdAsync(string contractId)
        {
            return await _context.ReviewerContracts
                .FirstOrDefaultAsync(c => c.ReviewerContractId == contractId);
        }

        public async Task<List<ReviewerContract>> GetReviewerContractsByUserIdAsync(string userId)
        {
            return await _context.ReviewerContracts
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<ReviewerContract>> GetReviewerContractsByConferenceIdAsync(string conferenceId)
        {
            return await _context.ReviewerContracts
                .Where(c => c.ConferenceId == conferenceId)
                .ToListAsync();
        }

        public async Task<ReviewerContract?> GetContractByUserAndConferenceAsync(string userId, string conferenceId)
        {
            return await _context.ReviewerContracts
                .FirstOrDefaultAsync(rc => rc.UserId == userId && rc.ConferenceId == conferenceId);
        }

        public async Task<List<ReviewerContract>> GetAllReviewerContractsAsync()
        {
            return await _context.ReviewerContracts.ToListAsync();
        }
    }
}
