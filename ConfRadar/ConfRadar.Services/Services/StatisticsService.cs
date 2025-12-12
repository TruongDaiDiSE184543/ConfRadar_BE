using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Statistics;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using ConfRadar.Shared.DTO.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace ConfRadar.Services.Services
{
    public interface IStatisticsService
    {
        //Task<ExportStatisticsResponse> ExportConferenceStatisticsAsync(string conferenceId, string exportFormat);
        #region getForJson
        Task<ConferenceStatisticsResponse> GetSoldTicketStatisticsAsync(string conferenceId);
        Task<PagedResultResponseDto<TicketHolderDetailResponse>> GetTicketHoldersByConferenceIdAsync(TicketHolderSearchParam request);

        Task<DTOs.Statistics.PaperStatisticsResponse> GetPaperStatisticsByConferenceIdAsync(string conferenceId);
        Task<List<DTOs.Statistics.ReviewerAssignmentResponse>> GetReviewersByConferenceIdAsync(string conferenceId);
        Task<List<DTOs.Statistics.SessionWithPresentersResponse>> GetSessionsWithPresentersByConferenceIdAsync(string conferenceId);
        Task<ConferenceTransactionHistoryResponse> GetTransactionHistoryAsync(string conferenceId);
        #endregion
        #region export to excel
        //Task<byte[]> ExportTicketHoldersListAsync(string conferenceId);
        Task<byte[]> ExportDetailedConferenceStatisticsAsync(string conferenceId);
        //Task<byte[]> ExportTicketHoldersToExcelAsync(TicketHolderSearchParam request);
        //Task<byte[]> ExportPaperStatisticsToExcelAsync(string conferenceId);
        //Task<byte[]> ExportReviewersToExcelAsync(string conferenceId);
        //Task<byte[]> ExportSessionsToExcelAsync(string conferenceId);
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
        public async Task<ConferenceStatisticsResponse> GetSoldTicketStatisticsAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            // Get all tickets for the conference that have been paid
            var allTickets = await _unitOfWork.TicketRepository.GetPaidTicketIncludeRefunded(conferenceId);

            // Get conference prices and phases with details
            var conferencePrices = await _unitOfWork.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(conferenceId);
            int commissionRate = 0;

            //Get commission if the conference is contracted with collaborator
            if (conference.IsInternalHosted != true)
            {
                var contract = await _unitOfWork.CollaboratorContractRepository.GetCollaboratorContractByConferenceId(conferenceId);
                if (contract == null)
                    throw new Exception($"Không tìm thấy hợp đồng cho conference được hợp đống với đối tác (không phải hội nghị nội bộ) với ID {conferenceId}");
                if (contract.IsTicketSelling == true)
                {
                    if (!contract.Commission.HasValue)
                    {
                        throw new Exception($"Không tìm thấy khoản hoa hồng trong hợp đồng của hội nghị với ID {conferenceId}");
                    }

                    if (contract.Commission.Value <= 0)
                    {
                        throw new Exception("Khoản hoa hồng của hội nghị không thể bé hơn hoặc bằng 0");
                    }
                    commissionRate = contract.Commission.Value;

                }
                else
                {
                    throw new Exception($"Hội nghị với ID {conferenceId} không được kí bán vé hộ trong hợp đồng với ID {contract.CollaboratorContractId}");
                }
            }


            var ticketPhaseStats = new List<TicketPhaseStatisticsResponse>();

            int grandTotalSold = 0;
            int grandTotalRefundedCount = 0;
            int grandTotalNotRefundedCount = 0;

            decimal grandTotalRevenueWithoutRefunded = 0; // Doanh thu từ vé chưa hoàn
            decimal grandTotalRefundedAmountToCustomer = 0; // Tiền đã trả lại khách (số âm hoặc dương tùy DB, ở đây lấy Abs)
            decimal grandTotalRetainedFromRefund = 0; // Tiền giữ lại từ vé hoàn (phí phạt)

            foreach (var price in conferencePrices)
            {
                foreach (var phase in price.PricePhases)
                {
                    // Lọc vé thuộc phase này
                    var phaseTickets = allTickets.Where(t => t.PricePhaseId == phase.PricePhaseId).ToList();

                    // --- Counters cho Phase này ---
                    int phaseSold = phaseTickets.Count;
                    int phaseRefundedCount = 0;
                    int phaseNotRefundedCount = 0;

                    decimal phaseRevenueWithoutRefunded = 0;
                    decimal phaseRefundedAmountToCustomer = 0;
                    decimal phaseRetainedFromRefund = 0;

                    // Checkin Counters
                    int countCheckedIn = 0;
                    int countPending = 0;
                    int countExpired = 0;

                    foreach (var ticket in phaseTickets)
                    {
                        // A. Tính toán tài chính
                        if (ticket.IsRefunded == true)
                        {
                            phaseRefundedCount++;

                            // Lấy giao dịch hoàn tiền từ list đã Include (không query DB)
                            var refundTx = ticket.Transactions.FirstOrDefault(tx => tx.IsRefunded == true);
                            decimal refundAmt = refundTx?.Amount ?? 0; // Số tiền trả khách

                            // Lấy giao dịch mua ban đầu (để biết giá mua thực tế)
                            // Hoặc dùng ticket.ActualPrice nếu nó lưu giá lúc mua
                            decimal originalPaid = ticket.ActualPrice ?? 0;

                            phaseRefundedAmountToCustomer += refundAmt;

                            // Doanh thu giữ lại = Giá mua - Tiền trả khách
                            // Lưu ý: refundAmt trong DB có thể là số âm. Nếu là số âm, hãy dùng Math.Abs().
                            // Giả sử refundAmt trong DB là số dương:
                            phaseRetainedFromRefund += (originalPaid - refundAmt);
                        }
                        else
                        {
                            phaseNotRefundedCount++;
                            phaseRevenueWithoutRefunded += ticket.ActualPrice ?? 0;
                        }

                        // B. Tính toán Check-in
                        // Mỗi vé có thể có nhiều UserCheckIn (ví dụ checkin nhiều session), hoặc 1 checkin tổng.
                        // Logic dưới đây giả định đếm trạng thái check-in mới nhất hoặc check-in quan trọng nhất.
                        // Nếu 1 vé checkin nhiều lần, cần logic cụ thể. Ở đây đếm theo UserCheckIn entity.
                        //foreach (var checkin in ticket.UserCheckIns)
                        //{
                        //    if (checkin.CheckinStatus?.CheckinStatusName == CheckInStatusEnum.CheckedIn.GetDescription()) countCheckedIn++;
                        //    else if (checkin.CheckinStatus?.CheckinStatusName == CheckInStatusEnum.Pending.GetDescription()) countPending++;
                        //    else if (checkin.CheckinStatus?.CheckinStatusName == CheckInStatusEnum.Expired.GetDescription()) countExpired++;
                        //}


                        // B. Tính toán Check-in (Logic 1 vé chỉ tính 1 trạng thái)
                        // Định nghĩa độ ưu tiên: CheckedIn > Expired > Pending
                        bool isTicketCheckedIn = false;
                        bool isTicketExpired = false;

                        if (ticket.UserCheckIns != null && ticket.UserCheckIns.Any())
                        {
                            // Kiểm tra xem có bất kỳ session nào đã check-in chưa
                            if (ticket.UserCheckIns.Any(uc => uc.CheckinStatus?.CheckinStatusName == CheckInStatusEnum.CheckedIn.GetDescription()))
                            {
                                isTicketCheckedIn = true;
                            }
                            // Nếu chưa check-in cái nào, xem có cái nào bị expired không
                            else if (ticket.UserCheckIns.Any(uc => uc.CheckinStatus?.CheckinStatusName == CheckInStatusEnum.Expired.GetDescription()))
                            {
                                isTicketExpired = true;
                            }
                            // Nếu không thì mặc định là Pending (đã mua vé nhưng chưa làm gì)
                        }
                        else
                        {
                            // Trường hợp vé chưa có record checkin nào (thường mặc định là Pending hoặc New)
                            // Tùy logic tạo data của bạn, nếu tạo vé là tạo luôn checkin record status Pending thì code trên đã cover.
                        }

                        // Cộng vào biến tổng của Phase
                        if (isTicketCheckedIn) countCheckedIn++;
                        else if (isTicketExpired) countExpired++;
                        else countPending++; // Bao gồm cả Pending và trường hợp chưa có record nào
                    }

                    // Tổng doanh thu thực tế của Phase = (Tiền vé chưa hoàn) + (Tiền giữ lại từ vé hoàn)
                    decimal phaseTotalRealRevenue = phaseRevenueWithoutRefunded + phaseRetainedFromRefund;

                    // C. Tạo DTO chi tiết
                    var stat = new TicketPhaseStatisticsResponse
                    {
                        ConferencePriceId = price.ConferencePriceId,
                        TicketName = price.TicketName,
                        PhaseName = phase.PhaseName,
                        isAuthor = price.IsAuthor ?? null,
                        OriginalPrice = price.TicketPrice ?? 0,
                        ApplyPhasePercent = phase.ApplyPercent ?? 0,

                        // Checkin
                        HasCheckin = countCheckedIn,
                        Pending = countPending,
                        ExpireCheckin = countExpired,

                        // Ticket Counts
                        TotalSold = phaseSold,
                        TotalRefunded = phaseRefundedCount,
                        TotalNotRefuned = phaseNotRefundedCount,

                        // Financials
                        // TotalAmountNotRefunded: Doanh thu từ vé active
                        TotalAmountNotRefunded = phaseRevenueWithoutRefunded,

                        // TotalAmountRefunded: Ở đây hiểu là TIỀN TRẢ KHÁCH (hiển thị số âm để clear?)
                        // Hoặc bạn muốn hiển thị Doanh thu giữ lại từ vé hoàn?
                        // Theo yêu cầu: "tổng tiền bị hoàn, hiện số âm" -> Hiển thị số tiền trả khách
                        TotalAmountRefunded = -phaseRefundedAmountToCustomer,

                        // TotalAmount: Tổng doanh thu thực tế của BTC
                        TotalAmount = phaseTotalRealRevenue
                    };

                    // D. Tính hoa hồng
                    if (!conference.IsInternalHosted.Value && commissionRate > 0)
                    {
                        decimal commissionAmt = phaseTotalRealRevenue * (commissionRate / 100m);
                        stat.CommissionPercentage = commissionRate;
                        stat.AmountToConfRadar = commissionAmt;
                        stat.AmountToCollaborator = phaseTotalRealRevenue - commissionAmt;
                    }
                    ticketPhaseStats.Add(stat);

                    // E. Cộng dồn tổng
                    grandTotalSold += phaseSold;
                    grandTotalRefundedCount += phaseRefundedCount;
                    grandTotalNotRefundedCount += phaseNotRefundedCount;

                    grandTotalRevenueWithoutRefunded += phaseRevenueWithoutRefunded;
                    grandTotalRefundedAmountToCustomer += phaseRefundedAmountToCustomer;
                    grandTotalRetainedFromRefund += phaseRetainedFromRefund;
                }
            }

            // 5. Final Response
            // Tổng doanh thu toàn hội nghị
            decimal grandTotalRealRevenue = grandTotalRevenueWithoutRefunded + grandTotalRetainedFromRefund;

            // Create response
            var response = conference.ToConferenceStatisticsResponse(ticketPhaseStats, grandTotalSold, grandTotalRefundedCount, grandTotalNotRefundedCount, grandTotalRefundedAmountToCustomer, grandTotalRevenueWithoutRefunded, grandTotalRealRevenue);
            return response;
        }


        public async Task<PagedResultResponseDto<TicketHolderDetailResponse>> GetTicketHoldersByConferenceIdAsync(TicketHolderSearchParam request)
        {
            // Get all tickets associated with the conference, including related entities
            var query = _unitOfWork.TicketRepository.GetTicketHolderInfo(request.ConferenceId);

            var responses = new List<TicketHolderDetailResponse>();

            // Filter: Refund Status
            if (request.IsRefunded.HasValue)
            {
                query = query.Where(t => t.IsRefunded == request.IsRefunded.Value);
            }

            // Filter: Date Range (Ngày mua)
            if (request.FromDate.HasValue)
            {
                query = query.Where(t => t.RegisteredDate >= request.FromDate.Value);
            }
            if (request.ToDate.HasValue)
            {
                query = query.Where(t => t.RegisteredDate <= request.ToDate.Value);
            }

            // Filter: Keyword (Tên, Email, TicketId)
            if (!string.IsNullOrEmpty(request.SearchKeyword))
            {
                var keyword = request.SearchKeyword.ToLower();
                query = query.Where(t =>
                    t.TicketId.ToLower().Contains(keyword) ||
                    (t.User != null && (t.User.FullName.ToLower().Contains(keyword) || t.User.Email.ToLower().Contains(keyword)))
                );
            }

            // Filter: Loại vé
            if (!string.IsNullOrEmpty(request.TicketType))
            {
                query = query.Where(t => t.PricePhase.ConferencePrice.TicketName.Contains(request.TicketType));
            }

            // Filter: Check-in Status (Phức tạp hơn xíu)
            // Nếu muốn tìm ai "Đã check-in" (bất kể session nào)
            if (!string.IsNullOrEmpty(request.CheckInStatus))
            {
                if (request.CheckInStatus == CheckInStatusEnum.CheckedIn.GetDescription())
                {
                    query = query.Where(t => t.UserCheckIns.Any(uc => uc.CheckinStatus.CheckinStatusName == CheckInStatusEnum.CheckedIn.GetDescription()));
                }
                else if (request.CheckInStatus == CheckInStatusEnum.Pending.GetDescription())
                {
                    // Pending nghĩa là chưa check-in cái nào và chưa hết hạn
                    query = query.Where(t => !t.UserCheckIns.Any() || t.UserCheckIns.All(uc => uc.CheckinStatus.CheckinStatusName == CheckInStatusEnum.Pending.GetDescription()));
                }
            }

            // 3. Pagination Execution
            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(t => t.RegisteredDate) // Mặc định sắp xếp người mua mới nhất lên đầu
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // 4. Mapping Data to DTO (In-memory)
            var items = data.Select(ticket => ticket.ToTicketHolderDetailResponse()).ToList();

            return new PagedResultResponseDto<TicketHolderDetailResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
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
                var assignedReviewers = new List<Reviewer>();
                if (paperReviewers != null && paperReviewers.Any())
                {
                    foreach (var paperReviewer in paperReviewers)
                    {
                        // Get reviewer user details
                        var reviewer = await _unitOfWork.UserRepository.GetUserByUserId(paperReviewer.UserId);
                        if (reviewer != null)
                        {
                            assignedReviewers.Add(new Reviewer
                            {
                                userId = reviewer.UserId,
                                name = reviewer.FullName,
                                isHeadReviewer = paperReviewer.IsHeadReviewer
                            });
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
                    SubmittingAuthorName = rootUser?.FullName ?? "N/A",
                    SubmittingAuthorEmail = rootUser?.Email ?? "N/A",
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

                paperDetail.paymentDetail = null;

                if (paper.TicketId != null)
                {
                    //find ticket
                    var ticket = await _unitOfWork.TicketRepository.GetTicketById(paper.TicketId);
                    if (ticket != null)
                    {
                        //find price phase
                        var pp = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(ticket.PricePhaseId);
                        if (pp != null)
                        {
                            //find conferenceprice
                            var cp = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(pp.ConferencePriceId);
                            if (cp != null)
                            {
                                PurchasedPhaseInfo purchasedPhaseInfo = new PurchasedPhaseInfo
                                {
                                    PricePhaseId = pp.PricePhaseId,
                                    PhaseName = pp.PhaseName,
                                    StartDate = pp.StartDate,
                                    EndDate = pp.EndDate,
                                    ApplyPercent = pp.ApplyPercent,
                                };
                                PaymentDetail paymentDetail = new PaymentDetail
                                {
                                    ConferencePriceId = cp.ConferencePriceId,
                                    TicketPrice = cp.TicketPrice,
                                    TicketName = cp.TicketName,
                                    TicketDescription = cp.TicketDescription,
                                    IsAuthor = cp.IsAuthor,
                                    purchasedPhaseInfo = purchasedPhaseInfo
                                };
                                paperDetail.paymentDetail = paymentDetail;
                            }
                        }
                    }

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


        public async Task<ConferenceTransactionHistoryResponse> GetTransactionHistoryAsync(string conferenceId)
        {
            // 1. Query phẳng (Flat data)
            var query = _unitOfWork.TransactionRepository.TransactionHistory(conferenceId);

            // 2. Lấy về RAM và Sắp xếp theo thời gian giảm dần (Mới nhất lên đầu)
            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // 3. Group by User
            var groupedHistories = transactions
                .GroupBy(t => t.UserId)
                .Select(g => new UserTransactionHistoryDto
                {
                    UserId = g.Key ?? "Guest", // Xử lý khách vãng lai
                    FullName = g.First().User?.FullName ?? "Unknown User",
                    Email = g.First().User?.Email ?? "N/A",

                    // Map chi tiết từng dòng giao dịch
                    Transactions = g.Select(t => new TransactionItemDto
                    {
                        TransactionId = t.TransactionId,
                        TransactionCode = t.TransactionCode,
                        Amount = t.Amount ?? 0,
                        Time = t.CreatedAt ?? DateTime.MinValue,

                        // Logic hiển thị trạng thái đơn giản
                        Status = (t.IsRefunded == true) ? "Đã hoàn tiền" : "Thành công",
                        Type = (t.IsRefunded == true) ? "Hoàn tiền" : "Thanh toán",

                        TicketType = t.Ticket?.PricePhase?.ConferencePrice?.TicketName ?? "Unknown Ticket",
                        PaymentMethod = t.PaymentMethod?.MethodName ?? "Unknown"
                    })
                    // Trong mỗi User, giao dịch mới nhất cũng phải nằm đầu
                    .OrderByDescending(x => x.Time)
                    .ToList()
                })
                // Sắp xếp danh sách User: User nào vừa giao dịch xong thì đẩy lên đầu list
                .OrderByDescending(u => u.Transactions.FirstOrDefault()?.Time)
                .ToList();

            return new ConferenceTransactionHistoryResponse
            {
                UserHistories = groupedHistories
            };
        }



        #endregion

        #region Unnecessary

        public async Task<ExportStatisticsResponse> ExportConferenceStatisticsAsync(string conferenceId, string exportFormat)
        {
            // Get the conference statistics data
            var statistics = await GetSoldTicketStatisticsAsync(conferenceId);

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
            // 1. Lấy dữ liệu thống kê chi tiết (đã bao gồm logic check-in, refund, commission)
            var statistics = await GetSoldTicketStatisticsAsync(conferenceId);

            ExcelPackage.License.SetNonCommercialPersonal("NonCommercial");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Báo Cáo Chi Tiết");

                // ======================================================
                // PHẦN 1: HEADER & THÔNG TIN CHUNG (DÒNG 1-6)
                // ======================================================

                // Tiêu đề lớn
                worksheet.Cells["A1:L1"].Merge = true;
                worksheet.Cells["A1"].Value = $"BÁO CÁO DOANH THU & VẬN HÀNH - {statistics.ConferenceName.ToUpper()}";
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Row(1).Height = 30;

                // Ngày xuất báo cáo
                worksheet.Cells["A2:L2"].Merge = true;
                worksheet.Cells["A2"].Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A2"].Style.Font.Italic = true;

                // --- Bảng tóm tắt nhỏ (Dashboard) ---
                worksheet.Cells["B4"].Value = "TỔNG VÉ ĐÃ BÁN";
                worksheet.Cells["C4"].Value = statistics.TotalTicketsSold;
                worksheet.Cells["B4:C4"].Style.Font.Bold = true;

                worksheet.Cells["B5"].Value = "TỔNG DOANH THU THỰC TẾ";
                worksheet.Cells["C5"].Value = statistics.TotalRevenue;
                worksheet.Cells["C5"].Style.Numberformat.Format = "#,##0";
                worksheet.Cells["B5:C5"].Style.Font.Bold = true;
                worksheet.Cells["B5:C5"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["B5:C5"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

                // Nếu có chia sẻ doanh thu
                int summaryRow = 6;
                if (!statistics.IsInternalHosted)
                {
                    var totalCommission = statistics.TicketPhaseStatistics.Sum(s => s.AmountToCollaborator ?? 0);
                    var totalConfRadar = statistics.TicketPhaseStatistics.Sum(s => s.AmountToConfRadar ?? 0);

                    worksheet.Cells[$"B{summaryRow}"].Value = "Doanh thu Đối tác (Sau hoa hồng)";
                    worksheet.Cells[$"C{summaryRow}"].Value = totalCommission;
                    worksheet.Cells[$"C{summaryRow}"].Style.Numberformat.Format = "#,##0";
                    summaryRow++;

                    worksheet.Cells[$"B{summaryRow}"].Value = "Phí nền tảng ConfRadar";
                    worksheet.Cells[$"C{summaryRow}"].Value = totalConfRadar;
                    worksheet.Cells[$"C{summaryRow}"].Style.Numberformat.Format = "#,##0";
                }

                // ======================================================
                // PHẦN 2: BẢNG CHI TIẾT (TABLE)
                // ======================================================

                int tableStartRow = summaryRow + 3; // Cách bảng tóm tắt 3 dòng
                int col = 1;

                // --- 2.1 Tạo Header cột ---

                // Nhóm: Thông tin vé
                worksheet.Cells[tableStartRow, col++].Value = "Tên Vé";
                worksheet.Cells[tableStartRow, col++].Value = "Giai Đoạn (Phase)";
                worksheet.Cells[tableStartRow, col++].Value = "Giá Gốc";

                // Nhóm: Số lượng (Vận hành)
                worksheet.Cells[tableStartRow, col++].Value = "Tổng Bán";
                worksheet.Cells[tableStartRow, col++].Value = "Đang Active"; // Chưa hoàn
                worksheet.Cells[tableStartRow, col++].Value = "Đã Hoàn";

                // Nhóm: Check-in (Vận hành)
                worksheet.Cells[tableStartRow, col++].Value = "Đã Check-in";
                worksheet.Cells[tableStartRow, col++].Value = "Chưa Check-in";

                // Nhóm: Tài chính
                worksheet.Cells[tableStartRow, col++].Value = "DT Từ Vé Active"; // TotalAmountNotRefunded
                worksheet.Cells[tableStartRow, col++].Value = "Tiền Trả Khách (Refund)"; // TotalAmountRefunded
                worksheet.Cells[tableStartRow, col++].Value = "Doanh Thu Thực (Net)"; // TotalAmount

                // Cột hoa hồng (nếu cần)
                bool showCommission = !statistics.IsInternalHosted;
                if (showCommission)
                {
                    worksheet.Cells[tableStartRow, col++].Value = "Hoa hồng (%)";
                    worksheet.Cells[tableStartRow, col++].Value = "Thực Nhận (Partner)";
                }

                // Style cho Header bảng
                using (var range = worksheet.Cells[tableStartRow, 1, tableStartRow, col - 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(52, 152, 219)); // Màu xanh dương đậm
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
                }

                // --- 2.2 Đổ dữ liệu ---
                int currentRow = tableStartRow + 1;
                foreach (var stat in statistics.TicketPhaseStatistics)
                {
                    int c = 1;
                    // Info
                    worksheet.Cells[currentRow, c++].Value = stat.TicketName;
                    worksheet.Cells[currentRow, c++].Value = stat.PhaseName;
                    worksheet.Cells[currentRow, c++].Value = stat.OriginalPrice;
                    worksheet.Cells[currentRow, c - 1].Style.Numberformat.Format = "#,##0";

                    // Counts
                    worksheet.Cells[currentRow, c++].Value = stat.TotalSold;
                    worksheet.Cells[currentRow, c++].Value = stat.TotalNotRefuned;
                    worksheet.Cells[currentRow, c++].Value = stat.TotalRefunded;

                    // Check-in Stats (Tô màu nhẹ để phân biệt)
                    worksheet.Cells[currentRow, c].Value = stat.HasCheckin;
                    worksheet.Cells[currentRow, c].Style.Font.Color.SetColor(System.Drawing.Color.Green); // Checkin xanh lá
                    c++;

                    worksheet.Cells[currentRow, c].Value = stat.Pending + stat.ExpireCheckin; // Gom pending và expired vào "Chưa checkin" hoặc tách ra tùy ý
                    c++;

                    // Financials
                    worksheet.Cells[currentRow, c].Value = stat.TotalAmountNotRefunded;
                    worksheet.Cells[currentRow, c++].Style.Numberformat.Format = "#,##0";

                    worksheet.Cells[currentRow, c].Value = stat.TotalAmountRefunded; // Số âm
                    worksheet.Cells[currentRow, c].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    worksheet.Cells[currentRow, c++].Style.Numberformat.Format = "#,##0";

                    worksheet.Cells[currentRow, c].Value = stat.TotalAmount; // Net Revenue
                    worksheet.Cells[currentRow, c].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, c++].Style.Numberformat.Format = "#,##0";

                    // Commission Columns
                    if (showCommission)
                    {
                        worksheet.Cells[currentRow, c++].Value = (stat.CommissionPercentage ?? 0) + "%";
                        worksheet.Cells[currentRow, c].Value = stat.AmountToCollaborator;
                        worksheet.Cells[currentRow, c++].Style.Numberformat.Format = "#,##0";
                    }

                    currentRow++;
                }

                // --- 2.3 Style đường viền cho bảng dữ liệu ---
                var tableRange = worksheet.Cells[tableStartRow, 1, currentRow - 1, col - 1];
                tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Tự động giãn cột cho đẹp
                worksheet.Cells.AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }



        #endregion
    }
}