using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IDestinationRepository
    {
        Task<int> CreateDestinationAsync(Destination destination);
        Task<int> UpdateDestinationAsync(Destination destination);
        Task<int> DeleteDestinationAsync(Destination destination);
        Task<Destination?> GetDestinationByIdAsync(string destinationId);
        Task<List<Destination>> GetAllDestinationsAsync();
        Task<List<Destination>> GetDestinationsWithRoomsAsync();
    }

    public class DestinationRepository : GenericRepository<Destination>, IDestinationRepository
    {
        public DestinationRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateDestinationAsync(Destination destination)
        {
            return await CreateAsync(destination);
        }

        public async Task<int> UpdateDestinationAsync(Destination destination)
        {
            return await UpdateAsync(destination);
        }

        public async Task<int> DeleteDestinationAsync(Destination destination)
        {
            _context.Destinations.Remove(destination);
            return await _context.SaveChangesAsync();
        }

        public async Task<Destination?> GetDestinationByIdAsync(string destinationId)
        {
            return await _context.Destinations.Include(d => d.City)
                .FirstOrDefaultAsync(c => c.DestinationId == destinationId);
        }

        public async Task<List<Destination>> GetAllDestinationsAsync()
        {
            return await _context.Destinations.Include(d => d.City).ToListAsync();
        }

        public async Task<List<Destination>> GetDestinationsWithRoomsAsync()
        {
            return await _context.Destinations
                .Include(d => d.Rooms)
                .ToListAsync();
        }
    }
}