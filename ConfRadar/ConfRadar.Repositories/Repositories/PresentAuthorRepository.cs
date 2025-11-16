using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPresentAuthorRepository
    {
        Task<int> CreatePresentAuthorAsync(PresentAuthor presentAuthor);
        Task<int> UpdatePresentAuthorAsync(PresentAuthor presentAuthor);
        Task<bool> DeletePresentAuthorAsync(PresentAuthor presentAuthor);
        Task<PresentAuthor?> GetPresentAuthorByIdAsync(string conferenceSessionId, string paperId);
        Task<List<PresentAuthor>> GetAllPresentAuthorsAsync();
        Task<PresentAuthor?> GetPresentAuthorByPaperIdAsync(string paperId);
        Task<List<PresentAuthor>> GetPresentAuthorsBySessionIdAsync(string sessionId);
    }

    public class PresentAuthorRepository : GenericRepository<PresentAuthor>, IPresentAuthorRepository
    {
        private readonly ConfRadarDbContext _context;

        public PresentAuthorRepository(ConfRadarDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CreatePresentAuthorAsync(PresentAuthor presentAuthor)
        {
            _context.PresentAuthors.Add(presentAuthor);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdatePresentAuthorAsync(PresentAuthor presentAuthor)
        {
            var tracker = _context.Attach(presentAuthor);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletePresentAuthorAsync(PresentAuthor presentAuthor)
        {
            _context.PresentAuthors.Remove(presentAuthor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PresentAuthor?> GetPresentAuthorByIdAsync(string conferenceSessionId, string paperId)
        {
            return await _context.PresentAuthors.Include(pa => pa.ConferenceSession)
                .FirstOrDefaultAsync(pa => pa.ConferenceSessionId == conferenceSessionId && pa.PaperId == paperId);
        }

        public async Task<List<PresentAuthor>> GetAllPresentAuthorsAsync()
        {
            return await _context.PresentAuthors.ToListAsync();
        }

        public async Task<PresentAuthor?> GetPresentAuthorByPaperIdAsync(string paperId)
        {
            return await _context.PresentAuthors
                .FirstOrDefaultAsync(pa => pa.PaperId == paperId);
        }

        public async Task<List<PresentAuthor>> GetPresentAuthorsBySessionIdAsync(string sessionId)
        {
            return await _context.PresentAuthors.Where(pa => pa.ConferenceSessionId == sessionId).ToListAsync();
        }
    }
}