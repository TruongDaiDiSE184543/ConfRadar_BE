using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public class CameraReadyRepository : GenericRepository<CameraReady>, ICameraReadyRepository
    {
        public CameraReadyRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateCameraReadyAsync(CameraReady cameraReady)
        {
            return await CreateAsync(cameraReady);
        }

        public async Task<int> UpdateCameraReadyAsync(CameraReady cameraReady)
        {
            return await UpdateAsync(cameraReady);
        }

        public async Task<bool> DeleteCameraReadyAsync(CameraReady cameraReady)
        {
            return await RemoveAsync(cameraReady);
        }

        public async Task<CameraReady?> GetCameraReadyByIdAsync(string cameraReadyId)
        {
            return await GetByIdAsync(cameraReadyId);
        }

        public async Task<List<CameraReady>> GetAllCameraReadysAsync()
        {
            return await GetAllAsync();
        }
        public async Task<List<CameraReady>> GetCameraBystatusName(string status)
        {
            return await _context.CameraReadies.Include(c => c.GlobalStatus).Where(c => c.GlobalStatus.Name == status).ToListAsync();
        }
    }
}