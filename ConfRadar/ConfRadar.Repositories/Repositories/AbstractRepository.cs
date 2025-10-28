using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IAbstractRepository
    {
        Task<int> CreateAbstractAsync(Abstract abstractEntity);
        Task<int> UpdateAbstractAsync(Abstract abstractEntity);
        Task<bool> DeleteAbstractAsync(Abstract abstractEntity);
        Task<Abstract?> GetAbstractByIdAsync(string abstractId);
        Task<List<Abstract>> GetAllAbstractsAsync();
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
            return await GetByIdAsync(abstractId);
        }

        public async Task<List<Abstract>> GetAllAbstractsAsync()
        {
            return await GetAllAsync();
        }
    }

}
