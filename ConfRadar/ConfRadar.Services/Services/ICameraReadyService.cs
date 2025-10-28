using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface ICameraReadyService
    {
        Task<int> CreateCameraReadyAsync(CameraReady cameraReady);
        Task<int> UpdateCameraReadyAsync(CameraReady cameraReady);
        Task<bool> DeleteCameraReadyAsync(CameraReady cameraReady);
        Task<CameraReady?> GetCameraReadyByIdAsync(string cameraReadyId);
        Task<List<CameraReady>> GetAllCameraReadysAsync();
    }
}