using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ICameraReadyRepository
    {
        Task<int> CreateCameraReadyAsync(CameraReady cameraReady);
        Task<int> UpdateCameraReadyAsync(CameraReady cameraReady);
        Task<int> UpdateMutipleCameraReadiesAsync(List<CameraReady> cameraReadies);
        Task<bool> DeleteCameraReadyAsync(CameraReady cameraReady);
        Task<CameraReady?> GetCameraReadyByIdAsync(string cameraReadyId);
        Task<List<CameraReady>> GetAllCameraReadysAsync();
        Task<List<CameraReady>> GetCameraBystatusName(string status);
        Task<List<CameraReady>> GetExpiredCameraReadies(DateOnly dateNow, GlobalStatus status, List<ConferenceStatus> confStatuses);
    }
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
            return await _context.CameraReadies
                .Include(cr => cr.GlobalStatus)
                .FirstOrDefaultAsync(x => x.CameraReadyId == cameraReadyId);
        }

        public async Task<List<CameraReady>> GetAllCameraReadysAsync()
        {
            return await GetAllAsync();
        }
        public async Task<List<CameraReady>> GetCameraBystatusName(string status)
        {
            return await _context.CameraReadies.Include(c => c.GlobalStatus).Where(c => c.GlobalStatus.Name == status).ToListAsync();
        }

        public async Task<List<CameraReady>> GetExpiredCameraReadies(DateOnly dateNow, GlobalStatus status, List<ConferenceStatus> confStatuses)
        {
            var confStatusIds = confStatuses
            .Select(c => c.ConferenceStatusId)
            .ToList();
            return await _context.CameraReadies
                .Where(c => c.GlobalStatusId == status.GlobalStatusId
                && c.Papers.Any(p => p.ResearchConferencePhase != null
                && p.ResearchConferencePhase.RegistrationEndDate < dateNow
                && p.Conference != null 
                && p.Conference.ConferenceStatus !=null
                && confStatusIds.Contains(p.Conference.ConferenceStatusId))).ToListAsync();
        }

        public async Task<int> UpdateMutipleCameraReadiesAsync(List<CameraReady> cameraReadies)
        {
           _context.CameraReadies.UpdateRange(cameraReadies);
            return await _context.SaveChangesAsync();
        }
    }
}