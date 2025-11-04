using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IReportFeedbackRepository
    {
        Task<ReportFeedback?> GetReportFeedbackByIdAsync(string reportId);
        Task<int> CreateReportFeedbackAsync(ReportFeedback reportFeedback);
        Task<int> UpdateReportFeedbackAsync(ReportFeedback reportFeedback);
        Task<int> DeleteReportFeedbackAsync(ReportFeedback reportFeedback);
        Task<List<ReportFeedback>> GetAllReportFeedbacksAsync();
        Task<ReportFeedback?> GetReportFeedbackByReportIdAsync(string reportId);
    }

    public class ReportFeedbackRepository : GenericRepository<ReportFeedback>, IReportFeedbackRepository
    {
        public ReportFeedbackRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<ReportFeedback?> GetReportFeedbackByIdAsync(string reportId)
        {
            return await _context.ReportFeedbacks
                .Include(rf => rf.Admin)
                .Include(rf => rf.Report)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(rf => rf.ReportId == reportId);
        }

        public async Task<int> CreateReportFeedbackAsync(ReportFeedback reportFeedback)
        {
            return await CreateAsync(reportFeedback);
        }

        public async Task<int> UpdateReportFeedbackAsync(ReportFeedback reportFeedback)
        {
            return await UpdateAsync(reportFeedback);
        }

        public async Task<int> DeleteReportFeedbackAsync(ReportFeedback reportFeedback)
        {
            _context.ReportFeedbacks.Remove(reportFeedback);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<ReportFeedback>> GetAllReportFeedbacksAsync()
        {
            return await _context.ReportFeedbacks
                .Include(rf => rf.Admin)
                .Include(rf => rf.Report)
                    .ThenInclude(r => r.User)
                .ToListAsync();
        }

        public async Task<ReportFeedback?> GetReportFeedbackByReportIdAsync(string reportId)
        {
            return await _context.ReportFeedbacks
                .Include(rf => rf.Admin)
                .Include(rf => rf.Report)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(rf => rf.ReportId == reportId);
        }
    }
}