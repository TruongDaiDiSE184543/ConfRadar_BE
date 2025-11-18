using ConfRadar.Repositories.Models;
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
            int totalTicketsSold,
            decimal totalRevenue)
        {
            return new ConferenceStatisticsResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                IsInternalHosted = conference.IsInternalHosted ?? false,
                TicketPhaseStatistics = ticketPhaseStats,
                TotalTicketsSold = totalTicketsSold,
                TotalRevenue = totalRevenue
            };
        }
    }
}