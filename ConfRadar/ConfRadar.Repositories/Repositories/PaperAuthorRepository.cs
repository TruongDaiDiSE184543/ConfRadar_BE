using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperAuthorRepository
    {
        Task<int> CreatePaperAuthorAsync(PaperAuthor paperAuthor);
        Task<int> UpdatePaperAuthorAsync(PaperAuthor paperAuthor);
        Task<bool> DeletePaperAuthorAsync(PaperAuthor paperAuthor);
        Task<int> DeleteMutiplePaperAuthorAsync(List<PaperAuthor> paperAuthors);

        Task<PaperAuthor?> GetPaperAuthorByIdAsync(string? userId, string? paperId);
        Task<List<PaperAuthor>> GetAllPaperAuthorsAsync();
        Task<List<PaperAuthor>> GetPaperAuthorsByPaperIdAsync(string paperId);
        Task<List<PaperAuthor>> GetPaperAuthorsByUserIdAsync(string userId);
        Task<int> CreateMutiplePaperAuthorAsync(List<PaperAuthor> paperAuthor);
        Task<List<Paper>> GetPapersByUserIdAsync(string userId);
        Task<List<Paper>> GetPapersByUserIdAndConfIdAsync(string userId, string confId);
        Task<PaperAuthor> GetRootAuthor(string paperId);

    }
    public class PaperAuthorRepository : GenericRepository<PaperAuthor>, IPaperAuthorRepository
    {
        public PaperAuthorRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreatePaperAuthorAsync(PaperAuthor paperAuthor)
        {
            return await CreateAsync(paperAuthor);
        }
        public async Task<int> CreateMutiplePaperAuthorAsync(List<PaperAuthor> paperAuthor)
        {
            await _context.PaperAuthors.AddRangeAsync(paperAuthor);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdatePaperAuthorAsync(PaperAuthor paperAuthor)
        {
            return await UpdateAsync(paperAuthor);
        }

        public async Task<bool> DeletePaperAuthorAsync(PaperAuthor paperAuthor)
        {
            return await RemoveAsync(paperAuthor);
        }

        public async Task<PaperAuthor?> GetPaperAuthorByIdAsync(string? userId, string? paperId)
        {
            return await _context.Set<PaperAuthor>()
                .FirstOrDefaultAsync(pa => pa.UserId == userId && pa.PaperId == paperId);
        }

        public async Task<List<PaperAuthor>> GetAllPaperAuthorsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<PaperAuthor>> GetPaperAuthorsByPaperIdAsync(string paperId)
        {
            return await _context.Set<PaperAuthor>()
                .Where(pa => pa.PaperId == paperId)
                .ToListAsync();
        }

        public async Task<List<PaperAuthor>> GetPaperAuthorsByUserIdAsync(string userId)
        {
            return await _context.Set<PaperAuthor>()
                .Where(pa => pa.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Paper>> GetPapersByUserIdAsync(string userId)
        {
            return await _context.PaperAuthors.Where(pa => pa.IsRootAuthor == true && pa.UserId == userId).Include(pa => pa.Paper)
                 .Select(pa => pa.Paper).ToListAsync();
        }

        public async Task<int> DeleteMutiplePaperAuthorAsync(List<PaperAuthor> paperAuthors)
        {
            _context.PaperAuthors.RemoveRange(paperAuthors);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Paper>> GetPapersByUserIdAndConfIdAsync(string userId, string confId)
        {
            return await _context.Papers.Include(p => p.PaperAuthors).Where(p => p.ConferenceId == confId && p.PaperAuthors.Any(pa => pa.UserId == userId)).ToListAsync();
        }

        public async Task<PaperAuthor> GetRootAuthor(string paperId)
        {
            return await _context.PaperAuthors.FirstOrDefaultAsync(pa => pa.PaperId == paperId && pa.IsRootAuthor == true);
        }
    }
}