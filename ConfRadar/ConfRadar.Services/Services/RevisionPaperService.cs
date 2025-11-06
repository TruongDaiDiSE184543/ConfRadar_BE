using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface IRevisionPaperService
    {
        Task<int> CreateRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<int> UpdateRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<bool> DeleteRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<RevisionPaper?> GetRevisionPaperByIdAsync(string revisionPaperId);
        Task<List<RevisionPaper>> GetAllRevisionPapersAsync();
    }
    public class RevisionPaperService : IRevisionPaperService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevisionPaperService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateRevisionPaperAsync(RevisionPaper revisionPaper)
        {
            return await _unitOfWork.RevisionPaperRepository.CreateRevisionPaperAsync(revisionPaper);
        }

        public async Task<int> UpdateRevisionPaperAsync(RevisionPaper revisionPaper)
        {
            return await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
        }

        public async Task<bool> DeleteRevisionPaperAsync(RevisionPaper revisionPaper)
        {
            return await _unitOfWork.RevisionPaperRepository.DeleteRevisionPaperAsync(revisionPaper);
        }

        public async Task<RevisionPaper?> GetRevisionPaperByIdAsync(string revisionPaperId)
        {
            return await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(revisionPaperId);
        }

        public async Task<List<RevisionPaper>> GetAllRevisionPapersAsync()
        {
            return await _unitOfWork.RevisionPaperRepository.GetAllRevisionPapersAsync();
        }
    }
}