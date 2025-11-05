using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceFeedbackRepository
    {
        Task<int> CreateFeedbackAsync(ConferenceFeedback feedback);
        Task<int> UpdateFeedbackAsync(ConferenceFeedback feedback);
        Task<bool> DeleteFeedbackAsync(ConferenceFeedback feedback);
    }
    public class ConferenceFeedbackRepository : GenericRepository<ConferenceFeedback>, IConferenceFeedbackRepository
    {
        public ConferenceFeedbackRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateFeedbackAsync(ConferenceFeedback feedback)
        {
            return await CreateAsync(feedback);
        }

        public async Task<int> UpdateFeedbackAsync(ConferenceFeedback feedback)
        {
            return await UpdateAsync(feedback);
        }

        public async Task<bool> DeleteFeedbackAsync(ConferenceFeedback feedback)
        {
            return await RemoveAsync(feedback);
        }


    }
}
