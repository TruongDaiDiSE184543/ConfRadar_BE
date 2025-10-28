using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface ICameraReadyRepository
    {
        Task<int> CreateCameraReadyAsync(CameraReady cameraReady);
        Task<int> UpdateCameraReadyAsync(CameraReady cameraReady);
        Task<bool> DeleteCameraReadyAsync(CameraReady cameraReady);
        Task<CameraReady?> GetCameraReadyByIdAsync(string cameraReadyId);
        Task<List<CameraReady>> GetAllCameraReadysAsync();
    }
}