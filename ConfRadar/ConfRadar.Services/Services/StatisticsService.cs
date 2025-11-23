using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Statistics;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace ConfRadar.Services.Services
{
    public interface IStatisticsService
    {
        //Task<ExportStatisticsResponse> ExportConferenceStatisticsAsync(string conferenceId, string exportFormat);
        #region getForJson
        Task<ConferenceStatisticsResponse> GetConferenceStatisticsAsync(string conferenceId);
        Task<List<TicketHolderDetailResponse>> GetTicketHoldersByConferenceIdAsync(string conferenceId);
        
        Task<DTOs.Statistics.PaperStatisticsResponse> GetPaperStatisticsByConferenceIdAsync(string conferenceId);
        Task<List<DTOs.Statistics.ReviewerAssignmentResponse>> GetReviewersByConferenceIdAsync(string conferenceId);
        Task<List<DTOs.Statistics.SessionWithPresentersResponse>> GetSessionsWithPresentersByConferenceIdAsync(string conferenceId);
        #endregion
        #region export to excel
        Task<byte[]> ExportTicketHoldersListAsync(string conferenceId);
        Task<byte[]> ExportDetailedConferenceStatisticsAsync(string conferenceId);
        #endregion
    }
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelExportService _excelExportService;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public StatisticsService(IUnitOfWork unitOfWork,
            IExcelExportService excelExportService,
            IObjectStorageFileService objectStorageFileService,
            IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings)
        {
            _unitOfWork = unitOfWork;
            _excelExportService = excelExportService;
            _objectStorageFileService = objectStorageFileService;
            _objectStorageSettings = objectStorageSettings.Value;
        }

        #region get for json
        public async Task<ConferenceStatisticsResponse> GetConferenceStatisticsAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            // Get all tickets for the conference that have been paid
            var paidTickets = await _unitOfWork.TicketRepository.GetPaidTicketsByConferenceIdAsync(conferenceId);

            // Get conference prices and phases with details
            var conferencePrices = await _unitOfWork.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(conferenceId);

            // Calculate statistics for each ticket type and phase
            var ticketPhaseStats = new List<TicketPhaseStatisticsResponse>();
            int totalTicketsSold = 0;
            decimal totalRevenue = 0;

            foreach (var price in conferencePrices)
            {
                foreach (var phase in price.PricePhases)
                {
                    // Filter tickets for this specific price and phase
                    var ticketsForPhase = paidTickets
                        .Where(t => t.PricePhaseId == phase.PricePhaseId)
                        .ToList();

                    var totalSold = ticketsForPhase.Count;
                    var totalAmount = ticketsForPhase.Sum(t => ((t.PricePhase.ConferencePrice?.TicketPrice ?? 0)) * (t.PricePhase.ApplyPercent / 100m));

                    var ticketPhaseStat = new TicketPhaseStatisticsResponse
                    {
                        ConferencePriceId = price.ConferencePriceId,
                        TicketName = price.TicketName,
                        OriginalPrice = price.TicketPrice.Value,
                        PhaseName = phase.PhaseName,
                        ApplyPhasePercent = phase.ApplyPercent.Value,
                        TotalSold = totalSold,
                        TotalAmount = totalAmount.Value
                    };

                    // If conference is not internal hosted, calculate commission
                    if (!conference.IsInternalHosted.Value)
                    {
                        var technicalDetail = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conferenceId);
                        if (technicalDetail != null && technicalDetail.Commission.HasValue)
                        {
                            var commissionPercentage = technicalDetail.Commission.Value;
                            var commissionAmount = totalAmount * (commissionPercentage / 100m);
                            var amountToConfRadar = commissionAmount;
                            var amountToCollaborator = totalAmount - commissionAmount;

                            ticketPhaseStat.CommissionPercentage = commissionPercentage;

                            ticketPhaseStat.AmountToConfRadar = amountToConfRadar;
                            ticketPhaseStat.AmountToCollaborator = amountToCollaborator;
                        }
                    }

                    ticketPhaseStats.Add(ticketPhaseStat);
                    totalTicketsSold += totalSold;
                    totalRevenue += totalAmount.Value;
                }
            }

            // Create response
            var response = conference.ToConferenceStatisticsResponse(ticketPhaseStats, totalTicketsSold, totalRevenue);
            return response;
        }


        public async Task<List<TicketHolderDetailResponse>> GetTicketHoldersByConferenceIdAsync(string conferenceId)
        {
            // Get all tickets associated with the conference, including related entities
            var tickets = await _unitOfWork.TicketRepository.GetTicketsWithDetailsByConferenceIdAsync(conferenceId);

            var ticketHolders = new List<TicketHolderDetailResponse>();

            foreach (var ticket in tickets)
            {
                // Get the associated user who purchased the ticket
                var user = await _unitOfWork.UserRepository.GetUserByUserId(ticket.UserId);

                // Get the conference price details for the ticket
                var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(ticket.PricePhase.ConferencePrice.ConferencePriceId);

                // Get the price phase for the ticket
                var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(ticket.PricePhaseId);


                var ticketHolder = new TicketHolderDetailResponse
                {
                    TicketId = ticket.TicketId,
                    CustomerName = user?.FullName ?? "Unknown Customer", // Use user's full name
                    TicketTypeName = conferencePrice?.TicketName ?? "Unknown Ticket Type", // Use conference price name as ticket type
                    PhaseName = pricePhase?.PhaseName ?? "N/A", // Get the phase name
                    ActualPrice = (conferencePrice?.TicketPrice * pricePhase.ApplyPercent / 100) ?? 0, // Price based on the phase
                    PurchaseDate = ticket.RegisteredDate.Value, // Register date from ticket
                    Status = ticket.IsRefunded == true ? "Đã hoàn tiền" : "Đã thanh toán", // Status based on IsRefunded flag
                    isRefunded = ticket.IsRefunded.Value
                };

                ticketHolders.Add(ticketHolder);
            }

            return ticketHolders;
        }
        public async Task<DTOs.Statistics.PaperStatisticsResponse> GetPaperStatisticsByConferenceIdAsync(string conferenceId)
        {
            // Kiểm tra conference tồn tại
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Không tìm thấy hội nghị với ID {conferenceId}");
            }

            // Get all papers with phases for the conference
            var papers = await _unitOfWork.PaperRepository.GetPapersWithPhasesForStatisticsByConferenceIdAsync(conferenceId);

            var paperDetails = new List<DTOs.Statistics.PaperDetailResponse>();

            foreach (var paper in papers)
            {
                // Get paper reviewers assigned to this paper
                var paperReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(paper.PaperId);
                var rootAuthor = await _unitOfWork.PaperAuthorRepository.GetRootAuthor(paper.PaperId);

                var rootUser = await _unitOfWork.UserRepository.GetUserByUserId(rootAuthor.UserId);
                var assignedReviewers = new List<string>();
                if (paperReviewers != null && paperReviewers.Any())
                {
                    foreach (var paperReviewer in paperReviewers)
                    {
                        // Get reviewer user details
                        var reviewer = await _unitOfWork.UserRepository.GetUserByUserId(paperReviewer.UserId);
                        if (reviewer != null)
                        {
                            assignedReviewers.Add(reviewer.FullName + " (" + reviewer.UserId + ")");
                        }
                    }
                }

                // Get the paper phase information
                string paperPhaseName = "N/A";
                if (paper.PaperPhaseId != null)
                {
                    var paperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByIdAsync(paper.PaperPhaseId);
                    if (paperPhase != null)
                    {
                        paperPhaseName = paperPhase.PhaseName;
                    }
                }

                var paperDetail = new DTOs.Statistics.PaperDetailResponse
                {
                    PaperId = paper.PaperId,
                    Title = paper.Title,
                    SubmittingAuthorId = rootUser?.UserId ?? "N/A",
                    PaperPhase = paperPhaseName,
                    AssignedReviewers = assignedReviewers
                };

                // Populate Abstract Phase
                if (paper.Abstract != null)
                {
                    paperDetail.AbstractPhase = new DTOs.Statistics.PaperAbstractPhaseResponse
                    {
                        Id = paper.Abstract.AbstractId,
                        Status = paper.Abstract.GlobalStatus?.Name ?? "Chưa xác định",
                        Title = paper.Abstract.Title,
                        Description = paper.Abstract.Description
                    };
                }

                // Populate FullPaper Phase
                if (paper.FullPaper != null)
                {
                    paperDetail.FullPaperPhase = new DTOs.Statistics.PaperFullPaperPhaseResponse
                    {
                        Id = paper.FullPaper.FullPaperId,
                        Status = paper.FullPaper.ReviewStatus?.Name ?? "Chưa xác định",
                        Title = paper.FullPaper.Title,
                        Description = paper.FullPaper.Description
                    };
                }

                // Populate Revision Phase
                if (paper.RevisionPaper != null)
                {
                    paperDetail.RevisionPhase = new DTOs.Statistics.PaperRevisionPhaseResponse
                    {
                        Id = paper.RevisionPaper.RevisionPaperId,
                        Status = paper.RevisionPaper.GlobalStatus?.Name ?? "Chưa xác định"
                    };
                }

                // Populate Camera Ready Phase
                if (paper.CameraReady != null)
                {
                    paperDetail.CameraReadyPhase = new DTOs.Statistics.PaperCameraReadyPhaseResponse
                    {
                        Id = paper.CameraReady.CameraReadyId,
                        Status = paper.CameraReady.GlobalStatus?.Name ?? "Chưa xác định",
                        Title = paper.CameraReady.Title,
                        Description = paper.CameraReady.Description
                    };
                }

                paperDetails.Add(paperDetail);
            }

            var response = new DTOs.Statistics.PaperStatisticsResponse
            {
                TotalSubmissions = papers.Count,
                PaperDetails = paperDetails
            };

            return response;
        }

        public async Task<List<DTOs.Statistics.ReviewerAssignmentResponse>> GetReviewersByConferenceIdAsync(string conferenceId)
        {
            // Get all paper reviewers for the conference
            var paperReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync(conferenceId);

            var reviewerAssignments = new List<DTOs.Statistics.ReviewerAssignmentResponse>();

            // Group paper reviewers by UserId (which represents the reviewer)
            var reviewerGrouping = paperReviewers.GroupBy(pr => pr.UserId);

            foreach (var group in reviewerGrouping)
            {
                var reviewerId = group.Key;
                var user = await _unitOfWork.UserRepository.GetUserByUserId(reviewerId);
                if (user != null)
                {
                    var paperIds = group.Select(pr => pr.PaperId).ToList();

                    var reviewerAssignment = new DTOs.Statistics.ReviewerAssignmentResponse
                    {
                        ReviewerId = user.UserId,
                        ReviewerName = user.FullName,
                        AssignedPaperCount = group.Count(),
                        paperIds = paperIds
                    };

                    reviewerAssignments.Add(reviewerAssignment);
                }
            }

            return reviewerAssignments;
        }

        public async Task<List<DTOs.Statistics.SessionWithPresentersResponse>> GetSessionsWithPresentersByConferenceIdAsync(string conferenceId)
        {
            // Get all conference sessions for the conference
            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);

            var sessionWithPresentersList = new List<DTOs.Statistics.SessionWithPresentersResponse>();

            foreach (var session in sessions)
            {
                // Get presenters for this session - for research conferences, these are from PresentAuthor table
                var presentAuthors = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorsBySessionIdAsync(session.ConferenceSessionId);

                var presenters = new List<DTOs.Statistics.PresenterDetailResponse>();
                foreach (var presentAuthor in presentAuthors)
                {
                    // Get paper details for the presenter
                    var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(presentAuthor.PaperId);
                    var presenter = await _unitOfWork.PaperAuthorRepository.GetPresenter(paper.PaperId);
                    var presenterUser = await _unitOfWork.UserRepository.GetUserByUserId(presenter.UserId);
                    if (paper != null)
                    {
                        presenters.Add(new DTOs.Statistics.PresenterDetailResponse
                        {
                            PresenterName = presenterUser.FullName, // Use submitting author as presenter
                            PaperTitle = paper.Title
                        });
                    }
                }

                sessionWithPresentersList.Add(new DTOs.Statistics.SessionWithPresentersResponse
                {
                    SessionId = session.ConferenceSessionId,
                    Title = session.Title,
                    OnDate = session.SessionDate ?? DateOnly.MinValue,
                    Presenters = presenters
                });
            }

            return sessionWithPresentersList;
        }



        #endregion

        #region Unnecessary

        public async Task<ExportStatisticsResponse> ExportConferenceStatisticsAsync(string conferenceId, string exportFormat)
        {
            // Get the conference statistics data
            var statistics = await GetConferenceStatisticsAsync(conferenceId);

            // Validate export format
            var validFormats = new[] { "pdf", "excel", "csv" };
            if (!validFormats.Contains(exportFormat.ToLower()))
            {
                throw new BadRequestException($"Invalid export format. Valid formats are: {string.Join(", ", validFormats)}");
            }

            // Generate the file based on the format
            string fileName, fileUrl;
            var fileNameWithoutExt = $"conference_statistics_{conferenceId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            switch (exportFormat.ToLower())
            {
                case "pdf":
                    fileName = fileNameWithoutExt + ".pdf";
                    // Export to PDF logic would go here
                    // For now, simulate generating a PDF by saving some basic data
                    // In a real implementation, you would use a PDF generation library
                    fileUrl = await GeneratePdfReport(statistics, fileName);
                    break;
                case "excel":
                    fileName = fileNameWithoutExt + ".xlsx";
                    // Export to Excel logic would go here
                    fileUrl = await GenerateExcelReport(statistics, fileName);
                    break;
                case "csv":
                    fileName = fileNameWithoutExt + ".csv";
                    // Export to CSV logic would go here
                    fileUrl = await GenerateCsvReport(statistics, fileName);
                    break;
                default:
                    throw new BadRequestException($"Unsupported export format: {exportFormat}");
            }

            return new ExportStatisticsResponse
            {
                FileName = fileName,
                FileUrl = fileUrl,
                ExportFormat = exportFormat.ToLower(),
                ExportedAt = DateTime.UtcNow
            };
        }

        // Helper methods to generate different report formats
        private async Task<string> GeneratePdfReport(ConferenceStatisticsResponse statistics, string fileName)
        {
            // In a real implementation, you would use a PDF generation library like iTextSharp or DinkToPdf
            // For now, just return a mock file URL for demonstration
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Mock PDF content"));
            var fileUrl = await _objectStorageFileService.UploadFileAsync(
                ObjectStorageBucketEnum.report.ToString(),
                fileName,
                stream,
                "application/pdf");

            return _objectStorageSettings.EndPoint + fileUrl;
        }

        private async Task<string> GenerateExcelReport(ConferenceStatisticsResponse statistics, string fileName)
        {
            // Create a flat list of ticket phase statistics for Excel export
            var exportData = new List<object>();
            foreach (var stat in statistics.TicketPhaseStatistics)
            {
                exportData.Add(new
                {
                    TicketName = stat.TicketName,
                    PhaseName = stat.PhaseName,
                    TotalSold = stat.TotalSold,
                    TotalAmount = stat.TotalAmount,
                    CommissionPercentage = stat.CommissionPercentage ?? 0,
                    AmountToCollaborator = stat.AmountToCollaborator ?? 0,
                    AmountToConfRadar = stat.AmountToConfRadar ?? 0,
                });
            }

            // Use the ExcelExportService to generate the Excel file
            var excelBytes = await _excelExportService.ExportToExcelAsync(exportData, "Ticket Statistics");

            // Convert to stream and upload the Excel file to object storage
            using var stream = new MemoryStream(excelBytes);
            var fileUrl = await _objectStorageFileService.UploadFileAsync(
                ObjectStorageBucketEnum.report.ToString(),
                fileName,
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            return _objectStorageSettings.EndPoint + fileUrl;
        }

        private async Task<string> GenerateCsvReport(ConferenceStatisticsResponse statistics, string fileName)
        {
            // Create a flat list of ticket phase statistics for CSV export
            var exportData = new List<object>();
            foreach (var stat in statistics.TicketPhaseStatistics)
            {
                exportData.Add(new
                {
                    TicketName = stat.TicketName,
                    PhaseName = stat.PhaseName,
                    TotalSold = stat.TotalSold,
                    TotalAmount = stat.TotalAmount,
                    CommissionPercentage = stat.CommissionPercentage ?? 0,
                    AmountToCollaborator = stat.AmountToCollaborator ?? 0,
                    AmountToConfRadar = stat.AmountToConfRadar ?? 0,

                });
            }

            // Note: we're not using the excel export here for CSV, just creating CSV directly
            // Use the ExcelExportService for actual Excel export functionality

            // For CSV, we'll create the content directly
            var csvContent = new System.Text.StringBuilder();
            csvContent.AppendLine("TicketName,PhaseName,TotalSold,TotalAmount,CommissionPercentage,AmountToCollaborator,AmountToConfRadar,CommissionAmount");

            foreach (var stat in statistics.TicketPhaseStatistics)
            {
                csvContent.AppendLine($"{EscapeCsvField(stat.TicketName)},{EscapeCsvField(stat.PhaseName)},{stat.TotalSold},{stat.TotalAmount},{stat.CommissionPercentage ?? 0},{stat.AmountToCollaborator ?? 0},{stat.AmountToConfRadar ?? 0}");
            }

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent.ToString()));
            var fileUrl = await _objectStorageFileService.UploadFileAsync(
                ObjectStorageBucketEnum.report.ToString(),
                fileName,
                stream,
                "text/csv");

            return _objectStorageSettings.EndPoint + fileUrl;
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Escape commas, quotes, and newlines in CSV fields
            field = field.Replace("\"", "\"\"");
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                field = $"\"{field}\"";
            }
            return field;
        }
        #endregion

       
     

        


        #region export
        public async Task<byte[]> ExportDetailedConferenceStatisticsAsync(string conferenceId)
        {
            // Bước 1: Lấy dữ liệu thống kê đầy đủ
            var statistics = await GetConferenceStatisticsAsync(conferenceId);

            ExcelPackage.License.SetNonCommercialPersonal("<My Name>");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Thống Kê Doanh Thu");

                // === PHẦN 1: TRÌNH BÀY THÔNG TIN TỔNG QUAN ===

                // Dùng Merge và Style để làm tiêu đề báo cáo
                worksheet.Cells["A1:H1"].Merge = true;
                worksheet.Cells["A1"].Value = $"BÁO CÁO DOANH THU - {statistics.ConferenceName}";
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells["A3"].Value = "Tổng số vé đã bán:";
                worksheet.Cells["B3"].Value = statistics.TotalTicketsSold;
                worksheet.Cells["B3"].Style.Font.Bold = true;

                worksheet.Cells["A4"].Value = "Tổng doanh thu:";
                worksheet.Cells["B4"].Value = statistics.TotalRevenue;
                worksheet.Cells["B4"].Style.Numberformat.Format = "#,##0"; // Định dạng số cho dễ đọc
                worksheet.Cells["B4"].Style.Font.Bold = true;

                // Tính toán và hiển thị tổng hoa hồng nếu có
                if (!statistics.IsInternalHosted)
                {
                    var totalCommission = statistics.TicketPhaseStatistics.Sum(s => s.AmountToCollaborator ?? 0);
                    var totalToConfRadar = statistics.TicketPhaseStatistics.Sum(s => s.AmountToConfRadar ?? 0);

                    worksheet.Cells["A5"].Value = "Tổng tiền cho Cộng tác viên:";
                    worksheet.Cells["B5"].Value = totalCommission;
                    worksheet.Cells["B5"].Style.Numberformat.Format = "#,##0";

                    worksheet.Cells["A6"].Value = "Tổng tiền cho ConfRadar:";
                    worksheet.Cells["B6"].Value = totalToConfRadar;
                    worksheet.Cells["B6"].Style.Numberformat.Format = "#,##0";
                }

                // === PHẦN 2: BẢNG CHI TIẾT DOANH THU THEO PHASE ===

                int startRowForTable = 8;

                // Tạo header cho bảng chi tiết
                worksheet.Cells[startRowForTable, 1].Value = "ID Loại Vé";
                worksheet.Cells[startRowForTable, 2].Value = "Tên Vé";
                worksheet.Cells[startRowForTable, 3].Value = "Tên Giai Đoạn";
                worksheet.Cells[startRowForTable, 4].Value = "Số Lượng Bán";
                worksheet.Cells[startRowForTable, 5].Value = "Tổng Doanh Thu";

                int currentColumn = 6;
                // Chỉ thêm các cột hoa hồng nếu cần
                if (!statistics.IsInternalHosted)
                {
                    worksheet.Cells[startRowForTable, currentColumn++].Value = "% Hoa Hồng";
                    worksheet.Cells[startRowForTable, currentColumn++].Value = "Tiền cho CTV";
                    worksheet.Cells[startRowForTable, currentColumn++].Value = "Tiền cho ConfRadar";
                }

                // Làm đậm header
                worksheet.Cells[startRowForTable, 1, startRowForTable, currentColumn - 1].Style.Font.Bold = true;

                // Đổ dữ liệu chi tiết vào bảng
                int currentRow = startRowForTable + 1;
                foreach (var stat in statistics.TicketPhaseStatistics)
                {
                    worksheet.Cells[currentRow, 1].Value = stat.ConferencePriceId;
                    worksheet.Cells[currentRow, 2].Value = stat.TicketName;
                    worksheet.Cells[currentRow, 3].Value = stat.PhaseName;
                    worksheet.Cells[currentRow, 4].Value = stat.TotalSold;
                    worksheet.Cells[currentRow, 5].Value = stat.TotalAmount;

                    if (!statistics.IsInternalHosted)
                    {
                        worksheet.Cells[currentRow, 6].Value = stat.CommissionPercentage;
                        worksheet.Cells[currentRow, 7].Value = stat.AmountToCollaborator;
                        worksheet.Cells[currentRow, 8].Value = stat.AmountToConfRadar;
                    }
                    currentRow++;
                }

                // Định dạng số cho các cột tiền tệ trong bảng
                worksheet.Cells[startRowForTable + 1, 5, currentRow - 1, 5].Style.Numberformat.Format = "#,##0";
                if (!statistics.IsInternalHosted)
                {
                    worksheet.Cells[startRowForTable + 1, 7, currentRow - 1, 8].Style.Numberformat.Format = "#,##0";
                }

                // Tự động điều chỉnh độ rộng cột
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<byte[]> ExportTicketHoldersListAsync(string conferenceId)
        {
            // Get the list of ticket holders for the conference
            var ticketHolders = await GetTicketHoldersByConferenceIdAsync(conferenceId);

            // Prepare the data for export - flatten it appropriately
            var exportData = ticketHolders.Select(holder => new
            {
                TicketId = holder.TicketId,
                CustomerName = holder.CustomerName,
                TicketTypeName = holder.TicketTypeName,
                PhaseName = holder.PhaseName,
                ActualPrice = holder.ActualPrice,
                PurchaseDate = holder.PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = holder.Status // Already in Vietnamese text format
            }).ToList();

            // Call the Excel export service
            return await _excelExportService.ExportToExcelAsync(exportData, "Danh Sách Người Mua Vé");
        }

        #endregion
    }
}