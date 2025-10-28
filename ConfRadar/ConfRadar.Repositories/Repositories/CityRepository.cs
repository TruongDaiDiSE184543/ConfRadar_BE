using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ICityRepository
    {
        Task<int> CreateCityAsync(City city);
        Task<int> UpdateCityAsync(City city);
        Task<int> DeleteCityAsync(City city);
        Task<City?> GetCityByIdAsync(string cityId);
        Task<List<City>> GetAllCitiesAsync();
    }
    public class CityRepository : GenericRepository<City>, ICityRepository
    {
        public CityRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateCityAsync(City city)
        {
            return await CreateAsync(city);
        }

        public async Task<int> UpdateCityAsync(City city)
        {
            return await UpdateAsync(city);
        }

        public async Task<int> DeleteCityAsync(City city)
        {
            _context.Cities.Remove(city);
            return await _context.SaveChangesAsync();
        }

        public async Task<City?> GetCityByIdAsync(string cityId)
        {
            return await _context.Cities
                .FirstOrDefaultAsync(c => c.CityId == cityId);
        }

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities.ToListAsync();
        }
    }
}
