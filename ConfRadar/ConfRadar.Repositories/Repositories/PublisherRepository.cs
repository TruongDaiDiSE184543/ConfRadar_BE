using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPublisherRepository
    {
        Task<Publisher?> GetPublisherByNameAsync(string publisherName);
        Task<int> CreateMultiplePublishersAsync(IEnumerable<Publisher> publishers);
        Task<int> CreatePublisher(Publisher publisher);
        Task<Publisher?> GetPublisherByIdAsync(string publisherId);
        Task<int> UpdatePublisherAsync(Publisher publisher);
        Task<bool> DeletePublisherAsync(Publisher publisher);
        Task<List<Publisher>> GetAllPublishersAsync();
        Task<bool> IsPublisherBeingUsedAsync(string publisherId);
    }
    
    public class PublisherRepository : GenericRepository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<Publisher?> GetPublisherByNameAsync(string publisherName)
        {
            return await _context.Publishers.FirstOrDefaultAsync(x => x.Name == publisherName);
        }
        
        public async Task<int> CreateMultiplePublishersAsync(IEnumerable<Publisher> publishers)
        {
            await _context.Publishers.AddRangeAsync(publishers);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreatePublisher(Publisher publisher)
        {
            return await CreateAsync(publisher);
        }
        
        public async Task<Publisher?> GetPublisherByIdAsync(string publisherId)
        {
            return await _context.Publishers.FirstOrDefaultAsync(x => x.PublisherId == publisherId);
        }
        
        public async Task<int> UpdatePublisherAsync(Publisher publisher)
        {
            return await UpdateAsync(publisher);
        }
        
        public async Task<bool> DeletePublisherAsync(Publisher publisher)
        {
            return await RemoveAsync(publisher);
        }
        
        public async Task<List<Publisher>> GetAllPublishersAsync()
        {
            return await GetAllAsync();
        }
        public async Task<bool> IsPublisherBeingUsedAsync(string publisherId)
        {
            return await _context.ConferencePrices.AnyAsync(cp => cp.PublisherId == publisherId);
        }
    }
}