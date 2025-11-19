using System;
using System.Collections.Generic;

namespace ConfRadar.Services.DTOs.Statistics
{
    public class TicketPhaseStatisticsResponse
    {
        public string ConferencePriceId { get; set; }
        public decimal OriginalPrice { get; set; }
        public string TicketName { get; set; }
        public string PhaseName { get; set; }
        public decimal ApplyPhasePercent { get; set; }

        public int TotalSold { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? CommissionPercentage { get; set; } // Only for non-internal hosted conferences
        public decimal? AmountToCollaborator { get; set; } // For non-internal hosted conferences
        public decimal? AmountToConfRadar { get; set; } // For non-internal hosted conferences
    }

    public class ConferenceStatisticsResponse
    {
        public string ConferenceId { get; set; }
        public string ConferenceName { get; set; }
        public bool IsInternalHosted { get; set; }
        public List<TicketPhaseStatisticsResponse> TicketPhaseStatistics { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ExportStatisticsRequest
    {
        public string ConferenceId { get; set; }
        public string ExportFormat { get; set; } // "pdf", "excel", "csv"
    }

    public class ExportStatisticsResponse
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ExportFormat { get; set; }
        public DateTime ExportedAt { get; set; }
    }
    
    // Thống kê Doanh thu vé
    public class TicketSalesSummaryResponse
    {
        public string TicketTypeId { get; set; } // ConferencePriceId
        public string TicketTypeName { get; set; }
        public List<PhaseSalesSummaryResponse> PhaseSummaries { get; set; } = new();
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? TotalCommission { get; set; } // Dành cho Collab
    }

    public class PhaseSalesSummaryResponse
    {
        public string PhaseId { get; set; } // PricePhaseId
        public string PhaseName { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal? Commission { get; set; } // Dành cho Collab
    }

    // Chi tiết người mua vé
    public class TicketHolderDetailResponse
    {
        public string TicketId { get; set; }
        public string CustomerName { get; set; }
        public string TicketTypeName { get; set; }
        public string PhaseName { get; set; }
        public decimal ActualPrice { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public string Status { get; set; } // "Đã thanh toán", "Đã hoàn tiền"
    }

    // Thống kê Bài báo
    public class PaperStatisticsResponse
    {
        public int TotalSubmissions { get; set; }
        public List<PaperDetailResponse> PaperDetails { get; set; } = new();
    }

    public class PaperDetailResponse
    {
        public string PaperId { get; set; }
        public string Title { get; set; }
        public string SubmittingAuthorId { get; set; }
        public string PaperPhase { get; set; }
        public List<string> AssignedReviewers { get; set; } = new();
    }

    // Danh sách Reviewer được gán
    public class ReviewerAssignmentResponse
    {
        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public int AssignedPaperCount { get; set; }
    }

    // Danh sách Session và Presenter
    public class SessionWithPresentersResponse
    {
        public string SessionId { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public List<PresenterDetailResponse> Presenters { get; set; } = new();
    }

    public class PresenterDetailResponse
    {
        public string PresenterName { get; set; }
        public string PaperTitle { get; set; }
    }
}