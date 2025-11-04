using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IReportRepository
    {
        Task<Report?> GetReportByIdAsync(string reportId);
        Task<int> CreateReportAsync(Report report);
        Task<int> UpdateReportAsync(Report report);
        Task<int> DeleteReportAsync(Report report);
        Task<List<Report>> GetAllReportsAsync();
        Task<List<Report>> GetUnresolvedReportsAsync();
        Task<List<Report>> GetReportsByUserIdAsync(string userId);
    }

    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<Report?> GetReportByIdAsync(string reportId)
        {
            return await _context.Reports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        public async Task<int> CreateReportAsync(Report report)
        {
            // Set default HasResolve to true when creating a new report
            if (!report.HasResolve.HasValue)
            {
                report.HasResolve = true;
            }

            if (!report.CreatedAt.HasValue)
            {
                report.CreatedAt = DateTime.UtcNow;
            }

            return await CreateAsync(report);
        }

        public async Task<int> UpdateReportAsync(Report report)
        {
            return await UpdateAsync(report);
        }

        public async Task<int> DeleteReportAsync(Report report)
        {
            _context.Reports.Remove(report);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Report>> GetAllReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<List<Report>> GetUnresolvedReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.User)
                .Where(r => r.HasResolve == false)
                .ToListAsync();
        }

        public async Task<List<Report>> GetReportsByUserIdAsync(string userId)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}