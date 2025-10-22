using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.GlobalStatus;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.Services.Services
{
    public interface IGlobalStatusService
    {
        Task<int> CreateGlobalStatus(GlobalStatusRequest globalStatus);
        Task<GlobalStatus> GetGlobalStatusByIdAsync(string globalStatusId);
        Task<int> UpdateGlobalStatusAsync(string globalStatusId, GlobalStatusRequest globalStatus);
        Task<bool> DeleteGlobalStatusAsync(string globalStatusId);
    }
    public class GlobalStatusService : IGlobalStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        public GlobalStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<int> CreateGlobalStatus(GlobalStatusRequest globalStatus)
        {
            var globalStatusObj = new GlobalStatus()
            {
                Description = globalStatus.Description,
                GlobalStatusId = Guid.NewGuid().ToString(),
                Name = globalStatus.Name,
            };
            return await _unitOfWork.GlobalStatusRepository.CreateGlobalStatus(globalStatusObj);
        }

        public async Task<bool> DeleteGlobalStatusAsync(string globalStatusId)
        {
            var globalStatusFound = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByIdAsync(globalStatusId);
            if (globalStatusFound == null)
            {
                throw new NotFoundException("global status not found");
            }
            return await _unitOfWork.GlobalStatusRepository.DeleteGlobalStatusAsync(globalStatusFound);
        }

        public async Task<GlobalStatus> GetGlobalStatusByIdAsync(string globalStatusId)
        {
            var result = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByIdAsync(globalStatusId);
            if (result == null)
            {
                throw new NotFoundException("global status not found");
            }
            return result;
        }

        public async Task<int> UpdateGlobalStatusAsync(string globalStatusId, GlobalStatusRequest globalStatus)
        {
            var globalStatusFound = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByIdAsync(globalStatusId);
            if (globalStatusFound == null)
            {
                throw new NotFoundException("global status not found");
            }
            globalStatusFound.Description = globalStatus.Description;
            globalStatusFound.Name = globalStatus.Name;
            return await _unitOfWork.GlobalStatusRepository.UpdateGlobalStatusAsync(globalStatusFound);
        }
    }
}
