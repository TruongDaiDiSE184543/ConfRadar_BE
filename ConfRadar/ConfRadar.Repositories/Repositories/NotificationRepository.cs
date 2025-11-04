using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface INotificationRepository
    {
        Task<int> CreateNotificationAsync(Notification notification);
        Task<int> CreateMutipleNotificationAsync(List<Notification> notification);

        Task<List<Notification>> GetNotificationsByUserIdAsync(string userId);
        Task<Notification?> GetNotificationByIdAsync(string notificationId);
        Task<int> UpdateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(Notification notification);
    }
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {

        public NotificationRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateNotificationAsync(Notification notification)
        {
            return await CreateAsync(notification);
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetNotificationByIdAsync(string notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }

        public async Task<int> UpdateNotificationAsync(Notification notification)
        {
            return await UpdateAsync(notification);
        }

        public async Task<bool> DeleteNotificationAsync(Notification notification)
        {
            return await RemoveAsync(notification);
        }

        public async Task<int> CreateMutipleNotificationAsync(List<Notification> notification)
        {
            await _context.Notifications.AddRangeAsync(notification);
            return await _context.SaveChangesAsync();
        }
    }
}
