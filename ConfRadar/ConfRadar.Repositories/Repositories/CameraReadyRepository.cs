using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface ICameraReadyRepository
    {
        Task<CameraReady?> GetCameraReadyByIdAsync(string cameraReadyId);
        Task<int> CreateAsync(CameraReady cameraReady);
        Task<int> UpdateAsync(CameraReady cameraReady);
        Task<bool> DeleteAsync(CameraReady cameraReady);
    }
    public class CameraReadyRepository : GenericRepository<CameraReady>, ICameraReadyRepository
    {

        public CameraReadyRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<CameraReady?> GetCameraReadyByIdAsync(string cameraReadyId)
        {
            return await GetByIdAsync(cameraReadyId);
        }

        public async Task<int> CreateAsync(CameraReady cameraReady)
        {
            return await CreateAsync(cameraReady);
        }

        public async Task<int> UpdateAsync(CameraReady cameraReady)
        {
            return await UpdateAsync(cameraReady);
        }

        public async Task<bool> DeleteAsync(CameraReady cameraReady)
        {
            return await RemoveAsync(cameraReady);
        }
    }
}
