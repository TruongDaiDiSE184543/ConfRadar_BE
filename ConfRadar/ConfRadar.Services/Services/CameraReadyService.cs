using ConfRadar.Repositories;
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
    public class CameraReadyService : ICameraReadyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CameraReadyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateCameraReadyAsync(CameraReady cameraReady)
        {
            return await _unitOfWork.CameraReadyRepository.CreateCameraReadyAsync(cameraReady);
        }

        public async Task<int> UpdateCameraReadyAsync(CameraReady cameraReady)
        {
            return await _unitOfWork.CameraReadyRepository.UpdateCameraReadyAsync(cameraReady);
        }

        public async Task<bool> DeleteCameraReadyAsync(CameraReady cameraReady)
        {
            return await _unitOfWork.CameraReadyRepository.DeleteCameraReadyAsync(cameraReady);
        }

        public async Task<CameraReady?> GetCameraReadyByIdAsync(string cameraReadyId)
        {
            return await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(cameraReadyId);
        }

        public async Task<List<CameraReady>> GetAllCameraReadysAsync()
        {
            return await _unitOfWork.CameraReadyRepository.GetAllCameraReadysAsync();
        }
    }
}