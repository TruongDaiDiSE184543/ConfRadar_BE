using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface IReviewStatusService
    {
        Task<bool> IsTransitionValid(string currentStatus, string toStatus);
        Task<ReviewStatus> GetReviewStatusById(string id);
        Task<ReviewStatus> GetReviewStatusByName(string name);
        Task<List<ReviewStatus>> GetAllReviewStatuses();

    }
    public class ReviewStatusService : IReviewStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReviewStatusService(IUnitOfWork unitOfWork)  => _unitOfWork = unitOfWork;

        public async Task<List<ReviewStatus>> GetAllReviewStatuses()
        {
            return await _unitOfWork.ReviewStatusRepository.GetAllReviewStatusAsync();
        }

        public async Task<ReviewStatus> GetReviewStatusById(string id)
        {
            return await _unitOfWork.ReviewStatusRepository.GetReviewStatusByIdAsync(id);
        }

        public async Task<ReviewStatus> GetReviewStatusByName(string name)
        {
            return await _unitOfWork.ReviewStatusRepository.GetReviewStatusByName(name);
        }

        public async Task<bool> IsTransitionValid(string currentStatus, string toStatus)
        {
            var validTransition = new Dictionary<string, List<String>>
            {
               { "Pending" , new List<string> { "Revise", "Rejected", "Accepted" }},
            };
            if (!validTransition.ContainsKey(currentStatus)) throw new Exception("Phải bắt đầu từ status pending");
            var validStatus = validTransition[currentStatus];
            return validStatus.Contains(toStatus);
        }
    }
}
