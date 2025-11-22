using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.Dashboard;
using ConfRadar.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Services.Services
{
    public interface IDashboardService
    {
        Task<ConferenceStatsResponse> GetConferenceStatsByUserIdAsync(string userId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ConferenceStatsResponse> GetConferenceStatsByUserIdAsync(string userId)
        {
            // 1. Lấy tất cả conference của user (tham số statusId là null để lấy tất cả)
            var conferences = await _unitOfWork.ConferenceRepository
                .GetConferencesByUserIdAndStatusAsync(userId, null);

            var response = new ConferenceStatsResponse
            {
                Total = conferences.Count
            };

            // 2. Nhóm theo Conference Status
            response.GroupByStatus = conferences
                .GroupBy(c => c.ConferenceStatus) // Nhóm theo object Status
                .Select(g => new ConferenceGroup
                {
                    GroupId = g.Key?.ConferenceStatusId ?? "Unknown",
                    GroupName = g.Key?.ConferenceStatusName ?? "Chưa xác định",
                    Count = g.Count(),
                    Conferences = g.Select(c => c.toConferenceResponse()).ToList()
                })
                .ToList();


            return response;
        }
    }
}
