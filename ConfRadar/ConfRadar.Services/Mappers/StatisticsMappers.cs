using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Statistics;

namespace ConfRadar.Services.Mappers
{
    public static class StatisticsMappers
    {
        // Extension methods to convert models to responses
        public static ConferenceStatisticsResponse ToResponse(this ConferenceStatisticsResponse model)
        {
            return new ConferenceStatisticsResponse
            {
                ConferenceId = model.ConferenceId,
                ConferenceName = model.ConferenceName,
                IsInternalHosted = model.IsInternalHosted,
                TicketPhaseStatistics = model.TicketPhaseStatistics,
                TotalTicketsSold = model.TotalTicketsSold,
                TotalRevenue = model.TotalRevenue
            };
        }

        public static TicketPhaseStatisticsResponse ToResponse(this TicketPhaseStatisticsResponse model)
        {
            return new TicketPhaseStatisticsResponse
            {
                ConferencePriceId = model.ConferencePriceId,
                TicketName = model.TicketName,
                PhaseName = model.PhaseName,
                TotalSold = model.TotalSold,
                TotalAmount = model.TotalAmount,
                CommissionPercentage = model.CommissionPercentage ?? 0,
                AmountToCollaborator = model.AmountToCollaborator ?? 0,
                AmountToConfRadar = model.AmountToConfRadar ?? 0,
            };
        }

        public static ExportStatisticsResponse ToResponse(this ExportStatisticsResponse model)
        {
            return new ExportStatisticsResponse
            {
                FileName = model.FileName,
                FileUrl = model.FileUrl,
                ExportFormat = model.ExportFormat,
                ExportedAt = model.ExportedAt
            };
        }

        // Map from Conference entity to ConferenceStatisticsResponse
        public static ConferenceStatisticsResponse ToConferenceStatisticsResponse(
            this Conference conference,
            List<TicketPhaseStatisticsResponse> ticketPhaseStats,
            int grandTotalSold,
            int grandTotalRefundedCount,
            int grandTotalNotRefundedCount,
            decimal grandTotalRefundedAmountToCustomer,
            decimal grandTotalRevenueWithoutRefunded,
            decimal grandTotalRealRevenue)
        {
            return new ConferenceStatisticsResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                IsInternalHosted = conference.IsInternalHosted ?? false,
                TicketPhaseStatistics = ticketPhaseStats,

                TotalTicketsSold = grandTotalSold,
                TotalTicketRefunded = grandTotalRefundedCount,
                TotalNotRefundedTicket = grandTotalNotRefundedCount,

                // Map các field tổng theo yêu cầu
                // TotalRefundedAmount: Tổng tiền trả khách (số âm)
                TotalRefundedAmount = -grandTotalRefundedAmountToCustomer,

                // TotalRevenueWithoutRefunded: Doanh thu từ vé active
                TotalRevenueWithoutRefunded = grandTotalRevenueWithoutRefunded,

                // TotalRevenue: Tổng doanh thu thực nhận (đã trừ tiền trả khách)
                TotalRevenue = grandTotalRealRevenue
            };
        }

        public static SessionCheckInDetail ToSessionCheckInDetail(this UserCheckIn uc)
        {
            return new SessionCheckInDetail
            {
                SessionId = uc.ConferenceSessionId,
                SessionTitle = uc.ConferenceSession?.Title ?? "Unknown Session",
                // Logic lấy tên phòng ưu tiên DisplayName -> Number -> N/A
                RoomName = uc.ConferenceSession?.Room?.DisplayName
                         ?? uc.ConferenceSession?.Room?.Number
                         ?? "N/A",
                StartTime = uc.ConferenceSession?.StartTime,
                EndTime = uc.ConferenceSession?.EndTime,
                CheckInStatus = uc.CheckinStatus?.CheckinStatusName ?? "Unknown",
                CheckInTime = uc.CheckInTime
            };
        }

        public static TicketHolderDetailResponse ToTicketHolderDetailResponse(this Ticket ticket)
        {
            // Logic tính toán Overall Status

            var checkedInStr = CheckInStatusEnum.CheckedIn.GetDescription();
            var expiredStr = CheckInStatusEnum.Expired.GetDescription();
            var pendingStr = CheckInStatusEnum.Pending.GetDescription();

            string overallStatus = "Chưa tham gia";
            if (ticket.IsRefunded == true) overallStatus = "Đã hoàn tiền";
            else if (ticket.UserCheckIns.Any(uc => uc.CheckinStatus?.CheckinStatusName == checkedInStr)) overallStatus = "Đã tham gia";
            else if (ticket.UserCheckIns.Any(uc => uc.CheckinStatus?.CheckinStatusName == expiredStr)) overallStatus = "Vắng mặt (Hết hạn)";

            return new TicketHolderDetailResponse
            {
                TicketId = ticket.TicketId,
                CustomerId = ticket.UserId,
                CustomerName = ticket.User?.FullName ?? "Unknown",
                CustomerEmail = ticket.User?.Email ?? "N/A",
                CustomerPhone = ticket.User?.PhoneNumber ?? "N/A",

                TicketTypeName = ticket.PricePhase?.ConferencePrice?.TicketName ?? "Unknown",
                PhaseName = ticket.PricePhase?.PhaseName ?? "N/A",
                ActualPrice = ticket.ActualPrice ?? 0,
                PurchaseDate = ticket.RegisteredDate ?? DateOnly.MinValue,
                IsRefunded = ticket.IsRefunded ?? false,

                OverallStatus = overallStatus,
                CheckedInCount = ticket.UserCheckIns.Count(uc => uc.CheckinStatus?.CheckinStatusName == checkedInStr),
                ExpiredCount = ticket.UserCheckIns.Count(uc => uc.CheckinStatus?.CheckinStatusName == expiredStr),
                PendingCount = ticket.UserCheckIns.Count(uc => uc.CheckinStatus?.CheckinStatusName == pendingStr),



                // Gọi lại hàm map nhỏ ở trên
                SessionCheckIns = ticket.UserCheckIns
                                    .Select(uc => uc.ToSessionCheckInDetail())
                                    .OrderBy(s => s.StartTime)
                                    .ToList()
            };
        }
    }
}