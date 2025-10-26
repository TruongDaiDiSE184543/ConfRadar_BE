//using ConfRadar.Repositories.Base;
//using ConfRadar.Repositories.Data;
//using ConfRadar.Repositories.Models;
//using Microsoft.EntityFrameworkCore;

//namespace ConfRadar.Repositories.Repositories
//{
//    public interface IMediaTypeRepository
//    {
//        Task<int> CreateMediaTypeAsync(MediaType mediaType);
//        Task<int> UpdateMediaTypeAsync(MediaType mediaType);
//        Task<int> DeleteMediaTypeAsync(MediaType mediaType);
//        Task<MediaType?> GetMediaTypeByIdAsync(string mediaTypeId);
//        Task<MediaType?> GetMediaTypeByNameAsync(string mediaTypeName);
//        Task<List<MediaType>> GetAllMediaTypesAsync();
//    }

//    public class MediaTypeRepository : GenericRepository<MediaType>, IMediaTypeRepository
//    {
//        public MediaTypeRepository(ConfRadarDbContext context) : base(context) { }

//        public async Task<int> CreateMediaTypeAsync(MediaType mediaType)
//        {
//            return await CreateAsync(mediaType);
//        }

//        public async Task<int> UpdateMediaTypeAsync(MediaType mediaType)
//        {
//            return await UpdateAsync(mediaType);
//        }

//        public async Task<int> DeleteMediaTypeAsync(MediaType mediaType)
//        {
//            _context.MediaTypes.Remove(mediaType);
//            return await _context.SaveChangesAsync();
//        }

//        public async Task<MediaType?> GetMediaTypeByIdAsync(string mediaTypeId)
//        {
//            return await _context.MediaTypes
//                .FirstOrDefaultAsync(m => m.MediaTypeId == mediaTypeId);
//        }

//        public async Task<MediaType?> GetMediaTypeByNameAsync(string mediaTypeName)
//        {
//            return await _context.MediaTypes
//                .FirstOrDefaultAsync(m => m.MediaTypeName == mediaTypeName);
//        }

//        public async Task<List<MediaType>> GetAllMediaTypesAsync()
//        {
//            return await _context.MediaTypes.ToListAsync();
//        }
//    }
//}