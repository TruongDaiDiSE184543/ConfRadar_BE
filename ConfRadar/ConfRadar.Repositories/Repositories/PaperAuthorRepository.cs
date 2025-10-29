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
            return await _context.Set<PaperAuthor>()
                .Include(pa => pa.Paper)
                .Where(pa => pa.UserId == userId)
                .Select(pa => pa.Paper)
                .ToListAsync();
        }
    }
}