using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRoomRepository
    {
        Task<int> CreateRoomAsync(Room room);
        Task<int> UpdateRoomAsync(Room room);
        Task<int> DeleteRoomAsync(Room room);
        Task<Room?> GetRoomByIdAsync(string roomId);
        Task<List<Room>> GetAllRoomsAsync();
        IQueryable<Room> GetAllRoomsWithoutTracking();
        Task<List<Room>> GetRoomsByDestinationIdAsync(string destinationId);
        Task<Room?> GetRoomWithDetailsAsync(string roomId);
    }

    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateRoomAsync(Room room)
        {
            return await CreateAsync(room);
        }

        public async Task<int> UpdateRoomAsync(Room room)
        {
            return await UpdateAsync(room);
        }

        public async Task<int> DeleteRoomAsync(Room room)
        {
            _context.Rooms.Remove(room);
            return await _context.SaveChangesAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(string roomId)
        {
            return await _context.Rooms.Include(r => r.Destination).ThenInclude(d => d.City)
                .FirstOrDefaultAsync(c => c.RoomId == roomId);
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms.Include(r => r.Destination).ThenInclude(d => d.City).ToListAsync();
        }

        public IQueryable<Room> GetAllRoomsWithoutTracking()
        {
            return _context.Rooms.Include(r => r.Destination).ThenInclude(d => d.City).AsNoTracking();
        }

        public async Task<List<Room>> GetRoomsByDestinationIdAsync(string destinationId)
        {
            return await _context.Rooms
                .Where(r => r.DestinationId == destinationId)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomWithDetailsAsync(string roomId)
        {
            return await _context.Rooms
                .Include(r => r.Destination)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);
        }
    }
}