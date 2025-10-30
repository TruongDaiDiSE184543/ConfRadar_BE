using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.DTO.Abstract;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IAbstractRepository
    {
        Task<int> CreateAbstractAsync(Abstract abstractEntity);
        Task<int> UpdateAbstractAsync(Abstract abstractEntity);
        Task<bool> DeleteAbstractAsync(Abstract abstractEntity);
        Task<Abstract?> GetAbstractByIdAsync(string abstractId);
        Task<List<Abstract>> GetAllAbstractsAsync();
        Task<List<PendingAbstractResponse>> GetAllPendingAbstractsAsync(string pendingGlobalStatusId);
    }
    public class AbstractRepository : GenericRepository<Abstract>, IAbstractRepository
    {
        public AbstractRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateAbstractAsync(Abstract abstractEntity)
        {
            return await CreateAsync(abstractEntity);
        }

        public async Task<int> UpdateAbstractAsync(Abstract abstractEntity)
        {
            return await UpdateAsync(abstractEntity);
        }

        public async Task<bool> DeleteAbstractAsync(Abstract abstractEntity)
        {
            return await RemoveAsync(abstractEntity);
        }

        public async Task<Abstract?> GetAbstractByIdAsync(string abstractId)
        {
            return await _context.Abstracts.Include(a => a.GlobalStatus).Where(a => a.AbstractId == abstractId).FirstOrDefaultAsync();
        }

        public async Task<List<Abstract>> GetAllAbstractsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<PendingAbstractResponse>> GetAllPendingAbstractsAsync(string pendingGlobalStatusId)
        {
            var result = await (
         from a in _context.Abstracts
         join p in _context.Papers on a.AbstractId equals p.AbstractId
         join u in _context.Users on p.PresenterId equals u.UserId
         join c in _context.Conferences on p.ConferenceId equals c.ConferenceId
         join gs in _context.GlobalStatuses on a.GlobalStatusId equals gs.GlobalStatusId
         where a.GlobalStatusId == pendingGlobalStatusId && p.AbstractId !=null
         select new PendingAbstractResponse
         {
             AbstractId = a.AbstractId,
             AbstractUrl = a.AbstractUrl,
             PaperId = p.PaperId,
             PresenterId = p.PresenterId,
             PresenterName = u.FullName,
             AvatarUrl = u.AvatarUrl,
             ConferenceId = p.ConferenceId, 
             ConferenceName = c.ConferenceName,
             GlobalStatusId = a.GlobalStatusId,
             GlobalStatusName = gs.Name,
             CreatedAt = p.CreatedAt,
             
         }).ToListAsync();

            return result;
        }
    }

}
