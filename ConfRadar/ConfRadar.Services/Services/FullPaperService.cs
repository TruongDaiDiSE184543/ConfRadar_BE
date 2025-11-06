using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{

    public interface IFullPaperService
    {
        Task<int> CreateFullPaperAsync(FullPaper fullPaper);
        Task<int> UpdateFullPaperAsync(FullPaper fullPaper);
        Task<bool> DeleteFullPaperAsync(FullPaper fullPaper);
        Task<FullPaper?> GetFullPaperByIdAsync(string fullPaperId);
        Task<List<FullPaper>> GetAllFullPapersAsync();
    }
    public class FullPaperService : IFullPaperService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FullPaperService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateFullPaperAsync(FullPaper fullPaper)
        {
            return await _unitOfWork.FullPaperRepository.CreateFullPaperAsync(fullPaper);
        }

        public async Task<int> UpdateFullPaperAsync(FullPaper fullPaper)
        {
            return await _unitOfWork.FullPaperRepository.UpdateFullPaperAsync(fullPaper);
        }

        public async Task<bool> DeleteFullPaperAsync(FullPaper fullPaper)
        {
            return await _unitOfWork.FullPaperRepository.DeleteFullPaperAsync(fullPaper);
        }

        public async Task<FullPaper?> GetFullPaperByIdAsync(string fullPaperId)
        {
            return await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(fullPaperId);
        }

        public async Task<List<FullPaper>> GetAllFullPapersAsync()
        {
            return await _unitOfWork.FullPaperRepository.GetAllFullPapersAsync();
        }
    }
}