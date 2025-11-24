using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Dashboard;
using ConfRadar.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Services.Services
{
    public interface IDashboardService
    {
        Task<RevenueAnalyticsResponse> GetRevenueAnalyticsAsync(string userId, int monthsBack);
        Task<ConferenceStatsResponse> GetConferenceStatsByUserIdAsync(string userId);
        Task<List<ConferenceReminderDto>> GetUpcomingConferencesAsync(string userId, int nextMonths);
        Task<RegisterConferenceResponse> GetTopRegisteredConferencesAsync(string userId, int topN = 5);
        Task<List<ConferenceContractResponse>> GetCollaboratorContractsAsync(string userId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;

        public DashboardService(IUnitOfWork unitOfWork, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;
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

        public async Task<RevenueAnalyticsResponse> GetRevenueAnalyticsAsync(string userId, int monthsBack)
        {
            // 1. Validate Input
            if (monthsBack <= 0) monthsBack = 6;
            if (monthsBack > 60) monthsBack = 60; // Max 5 năm

            // Tính ngày bắt đầu (DateOnly)
            var today = await _timeProviderService.GetVietnamDate();
            // Lấy ngày mùng 1 của n tháng trước
            var startDate = today.AddMonths(-monthsBack);

            // 2. Query Data (Chỉ dùng bảng Ticket và các bảng thông tin Conference)
            var query = _unitOfWork.TicketRepository.GetIncludedQueryable();

            // Lọc theo người tạo Conference
            query = query.Where(t => t.PricePhase.ConferencePrice.Conference.CreatedBy == userId)
                // Lọc vé đã hoàn tiền (dùng trực tiếp flag IsRefunded)
                .Where(t => t.IsRefunded != true)
                // Lọc theo thời gian đăng ký vé
                .Where(t => t.RegisteredDate >= startDate);

            // 3. Projection: Lấy các trường cần thiết lên RAM để xử lý GroupBy
            var rawTickets = await query.Select(t => new
            {
                // Lấy ngày tháng để group
                RegDate = t.RegisteredDate,

                // Lấy thông tin Conference để group con
                ConfId = t.PricePhase.ConferencePrice.ConferenceId,
                ConfName = t.PricePhase.ConferencePrice.Conference.ConferenceName,

                // LOGIC TÍNH GIÁ QUAN TRỌNG:
                // Ưu tiên 1: Lấy ActualPrice (Giá thực tế đã lưu khi mua)
                // Ưu tiên 2: Nếu null, tính lại: Giá gốc * (Phần trăm áp dụng / 100)
                Revenue = t.ActualPrice ??
                         ((t.PricePhase.ConferencePrice.TicketPrice ?? 0) *
                          ((t.PricePhase.ApplyPercent ?? 100) / 100m))
            }).ToListAsync();

            // 4. Xử lý Grouping (In-Memory)
            var response = new RevenueAnalyticsResponse();

            // Group cấp 1: Theo Tháng/Năm (Dựa vào RegisteredDate)
            var groupedByMonth = rawTickets
                .GroupBy(t => new { t.RegDate.Value.Year, t.RegDate.Value.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .ToList();

            foreach (var monthGroup in groupedByMonth)
            {
                // Group cấp 2: Theo Conference trong tháng đó
                var confsInMonth = monthGroup
                    .GroupBy(t => new { t.ConfId, t.ConfName })
                    .Select(cg => new ConferenceRevenueStats
                    {
                        ConferenceId = cg.Key.ConfId,
                        ConferenceName = cg.Key.ConfName,
                        TicketsSold = cg.Count(),
                        Revenue = cg.Sum(x => x.Revenue)
                    })
                    .OrderByDescending(c => c.Revenue) // Conference nào doanh thu cao xếp trên
                    .ToList();

                // Tạo object thống kê tháng
                var monthStats = new MonthlyRevenueStats
                {
                    Year = monthGroup.Key.Year,
                    Month = monthGroup.Key.Month,
                    MonthLabel = $"{monthGroup.Key.Month}/{monthGroup.Key.Year}",
                    MonthlyTotal = confsInMonth.Sum(c => c.Revenue),
                    MonthlyTickets = confsInMonth.Sum(c => c.TicketsSold),
                    Conferences = confsInMonth
                };

                response.MonthlyStats.Add(monthStats);
            }

            // Tính tổng toàn bộ thời gian
            response.TotalRevenue = rawTickets.Sum(t => t.Revenue);
            response.TotalTicketsSold = rawTickets.Count;

            return response;
        }


        public async Task<List<ConferenceReminderDto>> GetUpcomingConferencesAsync(string userId, int nextMonths)
        {
            // 1. Xử lý thời gian đầu vào
            if (nextMonths <= 0) nextMonths = 1; // Mặc định nhắc trong 1 tháng tới nếu input sai

            var today = ExtensionHelper.GetVietnamDate();
            var maxDate = today.AddMonths(nextMonths);

            // 2. Query
            // Lưu ý: Cần thêm hàm GetQueryable() trong ConferenceRepository giống như TicketRepository đã làm
            var query = _unitOfWork.ConferenceRepository.GetAllConferences();
            query = query.Where(c => c.CreatedBy == userId)
                .Where(c => c.StartDate != null) // Bắt buộc phải có ngày bắt đầu mới nhắc được
                                                 // Logic nhắc nhở: Lấy từ hôm nay đến n tháng tới
                .Where(c => c.StartDate >= today && c.StartDate <= maxDate)
                // (Tùy chọn) Loại bỏ các hội nghị đã hủy hoặc bản nháp
                .Where(c => c.ConferenceStatus.ConferenceStatusName == "Ready");

            // 3. Sắp xếp: Cái nào sắp đến thì hiện trước
            query = query.OrderBy(c => c.StartDate);

            // 4. Projection (Map sang DTO)
            var result = await query.Select(c => new ConferenceReminderDto
            {
                ConferenceId = c.ConferenceId,
                ConferenceName = c.ConferenceName,
                BannerImageUrl = c.BannerImageUrl,
                StartDate = c.StartDate.Value, // Đã check null ở Where
                EndDate = c.EndDate,
                StatusName = c.ConferenceStatus.ConferenceStatusName,



                // Tính số ngày còn lại (Logic tính toán trong SQL/LINQ)
                // DayNumber là property có sẵn của DateOnly để tính khoảng cách ngày
                DaysUntilStart = c.StartDate.Value.DayNumber - today.DayNumber
            })
            // Giới hạn lấy 5 cái gần nhất (Reminder chỉ nên hiện ít)
            .Take(5)
            .ToListAsync();

            return result;
        }

        public async Task<RegisterConferenceResponse> GetTopRegisteredConferencesAsync(string userId, int topN = 5)
        {

            // 1. Query từ Conference
            var query = _unitOfWork.ConferenceRepository.GetAllConferences()
                .AsNoTracking()
                .Where(c => c.CreatedBy == userId)
                // Chỉ lấy các hội nghị không bị hủy/nháp (tuỳ logic của bạn)
                .Where(c => c.ConferenceStatus.ConferenceStatusName == "Ready");

            var resultList = await query.Select(c => new ConferenceRegisterDto
            {
                ConferenceId = c.ConferenceId,
                Name = c.ConferenceName,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,


                TotalSlot = c.TotalSlot,

                // Đếm số vé đã bán (Logic: Đi xuyên qua các bảng con để đếm Ticket)
                // Điều kiện: Vé thuộc các PricePhase của Conference này VÀ chưa hoàn tiền
                PurchaseSlot = c.ConferencePrices
                                .SelectMany(cp => cp.PricePhases)
                                .SelectMany(pp => pp.Tickets)
                                .Count(t => t.IsRefunded != true),

                // Percent sẽ tính sau ở Client (RAM) để tránh lỗi chia cho 0 dưới DB
                OccupancyRate = 0
            })
            // Sắp xếp giảm dần theo số lượng vé bán được
            .OrderByDescending(dto => dto.PurchaseSlot)
            .Take(topN)
            .ToListAsync();

            // 3. Tính phần trăm (OccupancyRate) trong bộ nhớ
            foreach (var item in resultList)
            {
                if (item.TotalSlot > 0)
                {
                    // Ép kiểu decimal để chia lấy thập phân
                    item.OccupancyRate = Math.Round(((decimal)item.PurchaseSlot / (decimal)item.TotalSlot) * 100, 2);
                }
                else
                {
                    // Nếu TotalSlot = 0 hoặc null (không giới hạn chỗ), có thể set logic khác hoặc để 100%
                    item.OccupancyRate = 0;
                }
            }

            return new RegisterConferenceResponse
            {
                ConferenceRegisters = resultList
            };
        }

        public async Task<List<ConferenceContractResponse>> GetCollaboratorContractsAsync(string userId)
        {
            // BƯỚC 1: Tạo Query và Lọc (Vẫn là IQueryable để chạy dưới DB)
            var query = _unitOfWork.ConferenceRepository.GetAllConferences()
                .AsNoTracking()
                .Include(c => c.TechnicalConferenceDetail) // Bắt buộc include để lấy Commission
                .Include(c => c.ConferenceStatus)
                .Where(c => c.CreatedBy == userId)
                .Where(c => c.IsInternalHosted == false)
                .Where(c => c.TechnicalConferenceDetail != null);

            // BƯỚC 2: Sắp xếp ngay trên Entity (SQL hiểu được cột StartDate)
            // Thay vì sort trên DTO, hãy sort trên Entity gốc
            query = query.OrderByDescending(c => c.StartDate);

            // BƯỚC 3: Thực thi SQL và lấy dữ liệu thô về RAM
            // Lúc này biến entities là List<Conference> thật sự trong bộ nhớ
            var entities = await query.ToListAsync();

            // BƯỚC 4: Map sang DTO bằng C# (Client Evaluation)
            // Bây giờ hàm toConferenceResponse() sẽ chạy bình thường vì dữ liệu đã ở trên RAM
            var result = entities.Select(c => new ConferenceContractResponse
            {
                // Gọi hàm mapper của bạn thoải mái
                ConferenceResponse = c.toConferenceResponse(),

                // Map các trường từ bảng con
                //Commission = c.TechnicalConferenceDetail?.Commission,
                //ContractUrl = c.TechnicalConferenceDetail?.ContractUrl,
                TargetAudience = c.TechnicalConferenceDetail?.TargetAudience
            }).ToList();

            return result;
        }
    }
}
