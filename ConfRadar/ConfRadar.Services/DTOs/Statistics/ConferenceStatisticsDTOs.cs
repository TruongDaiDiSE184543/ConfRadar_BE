namespace ConfRadar.Services.DTOs.Statistics
{
    public class TicketPhaseStatisticsResponse
    {
        public string ConferencePriceId { get; set; }
        public decimal OriginalPrice { get; set; }
        public string TicketName { get; set; }
        public string PhaseName { get; set; }
        public bool? isAuthor {  get; set; }
        public decimal ApplyPhasePercent { get; set; }
        public int HasCheckin { get; set; }
        public int ExpireCheckin { get; set; }
        public int Pending { get; set; }

        public int TotalNotRefuned { get; set; }
        public int TotalRefunded { get; set; }
        public int TotalSold { get; set; }
        public decimal? TotalAmountNotRefunded { get; set; }
        public decimal? TotalAmountRefunded { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? CommissionPercentage { get; set; } // Only for non-internal hosted conferences
        public decimal? AmountToCollaborator { get; set; } // For non-internal hosted conferences
        public decimal? AmountToConfRadar { get; set; } // For non-internal hosted conferences
    }

    public class ConferenceStatisticsResponse
    {
        public string ConferenceId { get; set; }
        public string ConferenceName { get; set; }
        public bool IsInternalHosted { get; set; }
        public int commision { get; set; }
        public List<TicketPhaseStatisticsResponse> TicketPhaseStatistics { get; set; }
        public int TotalTicketRefunded { get; set; }
        public int TotalNotRefundedTicket { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal? TotalRefundedAmount { get; set; }
        public decimal? TotalRevenueWithoutRefunded { get; set; }
        public decimal? TotalRevenue { get; set; }
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

    //param parameter
    public class TicketHolderSearchParam
    {
        public string ConferenceId { get; set; } = null!; // Bắt buộc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SearchKeyword { get; set; } // Tìm theo tên User, Email hoặc TicketId
        public bool? IsRefunded { get; set; } // Lọc vé đã hoàn hay chưa
        public string? TicketType { get; set; } // Lọc theo tên loại vé (ConferencePrice Name)
        public string? CheckInStatus { get; set; } // Lọc theo trạng thái: "CheckedIn", "Pending", "Expired"
        public DateOnly? FromDate { get; set; } // Ngày mua từ
        public DateOnly? ToDate { get; set; } // Ngày mua đến
    }


    public class TicketHolderDetailResponse
    {
        public string TicketId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        // Thông tin vé
        public string TicketTypeName { get; set; } // Tên loại vé (VIP, Standard...)
        public string PhaseName { get; set; } // Giai đoạn mua (Early Bird...)
        public decimal ActualPrice { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public bool IsRefunded { get; set; }

        // Tổng quan Check-in
        public string OverallStatus { get; set; } // "Đã tham gia", "Chưa đến", "Hết hạn" (Tính dựa trên logic ưu tiên)

        // Chi tiết từng Session đã check-in (List này trả lời cho câu hỏi của bạn)
        public List<SessionCheckInDetail> SessionCheckIns { get; set; } = new List<SessionCheckInDetail>();
    }

    public class SessionCheckInDetail
    {
        public string SessionId { get; set; }
        public string SessionTitle { get; set; }
        public string RoomName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string CheckInStatus { get; set; } // "CheckedIn", "Pending", "Expired"
        public DateTime? CheckInTime { get; set; }
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
        public string? SubmittingAuthorName {  get; set; }
        public string? SubmittingAuthorEmail {  get; set; }
        public string PaperPhase { get; set; }
        public List<Reviewer> AssignedReviewers { get; set; } = new();
        public PaperAbstractPhaseResponse? AbstractPhase { get; set; }
        public PaperFullPaperPhaseResponse? FullPaperPhase { get; set; }
        public PaperRevisionPhaseResponse? RevisionPhase { get; set; }
        public PaperCameraReadyPhaseResponse? CameraReadyPhase { get; set; }
    }

    public class Reviewer
    {
        public string? userId { get; set; }
        public string? name { get; set; }
        public bool? isHeadReviewer { get; set; }
    }

    public class PaperAbstractPhaseResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    public class PaperFullPaperPhaseResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    public class PaperRevisionPhaseResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
    }

    public class PaperCameraReadyPhaseResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    // Danh sách Reviewer được gán
    public class ReviewerAssignmentResponse
    {
        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public int AssignedPaperCount { get; set; }
        public List<string> paperIds { get; set; }
    }



    // Danh sách Session và Presenter
    public class SessionWithPresentersResponse
    {
        public string SessionId { get; set; }
        public string Title { get; set; }
        public DateOnly OnDate { get; set; }
        public List<PresenterDetailResponse> Presenters { get; set; } = new();
    }

    public class PresenterDetailResponse
    {
        public string PresenterName { get; set; }
        public string PaperTitle { get; set; }
    }
}