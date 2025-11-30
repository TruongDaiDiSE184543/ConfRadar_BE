using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio.Exceptions;

namespace ConfRadar.Services.Services
{
    public interface IConferenceStepService
    {
        // Step 1: Basic Conference Creation
        Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid);
        Task<string> CreateSkeletonTechnicalConferenceBasicForCollaboratorAsync(string name, string collabId);
        Task<TechnicalConferenceBasicStepResponse> GetConferenceBasicAsync(string conferenceId);
        Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request, string userId);

        // Step 2: Add Conference Prices
        Task<ConferencePriceListWithPhasesResponse> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request, string userId);
        Task<List<ConferencePriceWithPhasesResponse>> GetConferencePricesAsync(string conferenceId);
        Task<ConferencePriceWithPhasesResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request, string user);
        Task<bool> DeleteConferencePriceAsync(string priceId, string userId);

        // Step 3: Add Conference Sessions
        Task<List<ConferenceSessionWithMediaResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request, string userId);
        Task<List<ConferenceSessionWithMediaResponse>> GetConferenceSessionsAsync(string conferenceId);
        Task<ConferenceSessionWithMediaResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request, string userId);
        Task<SpeakerResponse> UpdateSpeakerAsync(string sessionId, UpdateSpeakerRequest request);
        Task<bool> DeleteConferenceSessionAsync(string sessionId);

        // Step 4: Add Conference Policies
        Task<List<ConferencePolicyResponse>> AddConferencePoliciesAsync(string conferenceId, AddConferencePoliciesRequest request, string userId);
        Task<List<ConferencePolicyResponse>> GetConferencePoliciesAsync(string conferenceId);
        Task<ConferencePolicyResponse> UpdateConferencePolicyAsync(string policyId, UpdateConferencePolicyRequest request, string userId);
        Task<bool> DeleteConferencePolicyAsync(string policyId, string userId);

        // Step 5: Add Conference Media
        Task<List<ConferenceMediaResponse>> AddConferenceMediaAsync(string conferenceId, AddConferenceMediaRequest request, string userId);
        Task<List<ConferenceMediaResponse>> GetConferenceMediaAsync(string conferenceId);
        Task<ConferenceMediaResponse> UpdateConferenceMediaAsync(string mediaId, UpdateConferenceMediaRequest request, string userId);
        Task<bool> DeleteConferenceMediaAsync(string mediaId, string userId);

        // Step 6: Add Conference Sponsors
        Task<List<SponsorResponse>> AddConferenceSponsorsAsync(string conferenceId, AddConferenceSponsorsRequest request, string userId);
        Task<List<SponsorResponse>> GetConferenceSponsorsAsync(string conferenceId);
        Task<SponsorResponse> UpdateSponsorAsync(string sponsorId, UpdateSponsorRequest request, string userId);
        Task<bool> DeleteSponsorAsync(string sponsorId, string userId);

        // Step 7: Add Refund Policies
        Task<List<RefundPolicyResponse>> AddRefundPoliciesAsync(string conferenceId, string pricephaseId, AddRefundPoliciesRequest request, string userId);
        Task<List<RefundPolicyResponse>> GetRefundPoliciesAsync(string conferenceId);
        Task<RefundPolicyResponse> UpdateRefundPolicyAsync(string refundPolicyId, UpdateRefundPolicyRequest request, string userId);
        Task<bool> DeleteRefundPolicyAsync(string refundPolicyId, string userId);

        // Research Conference Step 1: Basic Research Conference Creation
        Task<ResearchConferenceBasicStepResponse> CreateResearchConferenceBasicAsync(CreateResearchConferenceBasicRequest request, string userid);
        Task<ResearchConferenceBasicStepResponse> GetResearchConferenceBasicAsync(string conferenceId);
        Task<ResearchConferenceBasicStepResponse> UpdateResearchConferenceBasicAsync(string conferenceId, UpdateResearchConferenceBasicRequest request,string userId);

        // Research Conference Step 2: Research Conference Detail
        Task<ResearchConferenceDetailResponse> CreateResearchConferenceDetailAsync(string conferenceId, CreateResearchConferenceDetailRequest request, string userId);
        Task<ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId);
        Task<ResearchConferenceDetailResponse> UpdateResearchConferenceDetailAsync(string conferenceId, UpdateResearchConferenceDetailRequest request, string userId);

        // Research Conference Step 3: Research Conference Phases
        Task<CreatePhasesResponse> CreateResearchConferencePhaseAsync(string conferenceId, CreateResearchConferencePhasesRequest request, string userId);
        Task<ResearchConferencePhaseResponse> GetResearchConferencePhaseAsync(string conferenceId);
        Task<ResearchConferencePhaseResponse> UpdateResearchConferencePhaseAsync(string phaseId, UpdateResearchConferencePhaseRequest request, string userId);

        // Research Conference Step 4: Research Conference Sessions (without speakers)
        Task<List<ResearchSessionWithMediaResponse>> AddResearchSessionsAsync(string conferenceId, AddResearchSessionsRequest request, string userId);
        Task<List<ResearchSessionWithMediaResponse>> GetResearchSessionsAsync(string conferenceId);
        Task<List<ResearchSessionWithMediaResponse>> GetResearchSessionsWithoutRoomAsync(string conferenceId);
        Task<ResearchSessionWithMediaResponse> UpdateResearchSessionAsync(string sessionId, UpdateConferenceSessionRequest request, string userId);
        Task<bool> DeleteResearchSessionAsync(string sessionId, string userId);

        // Research Conference Step 5: Material Downloads
        Task<MaterialDownloadResponse> CreateMaterialDownloadAsync(string conferenceId, CreateMaterialDownloadRequest request, string userId);
        Task<List<MaterialDownloadResponse>> GetMaterialDownloadsByConferenceIdAsync(string conferenceId);
        Task<MaterialDownloadResponse> UpdateMaterialDownloadAsync(string materialDownloadId, UpdateMaterialDownloadRequest request, string userId);
        Task<bool> DeleteMaterialDownloadAsync(string materialDownloadId, string userId);

        // Research Conference Step 6: Ranking File URLs
        Task<RankingFileUrlResponse> CreateRankingFileUrlAsync(string conferenceId, CreateRankingFileUrlRequest request, string userId);
        Task<List<RankingFileUrlResponse>> GetRankingFileUrlsByConferenceIdAsync(string conferenceId);
        Task<RankingFileUrlResponse> UpdateRankingFileUrlAsync(string rankingFileUrlId, UpdateRankingFileUrlRequest request, string userId);
        Task<bool> DeleteRankingFileUrlAsync(string rankingFileUrlId, string userId);

        // Research Conference Step 7: Ranking Reference URLs
        Task<RankingReferenceUrlResponse> CreateRankingReferenceUrlAsync(string conferenceId, CreateRankingReferenceUrlRequest request, string userId);
        Task<List<RankingReferenceUrlResponse>> GetRankingReferenceUrlsByConferenceIdAsync(string conferenceId);
        Task<RankingReferenceUrlResponse> UpdateRankingReferenceUrlAsync(string referenceUrlId, UpdateRankingReferenceUrlRequest request, string userId);
        Task<bool> DeleteRankingReferenceUrlAsync(string referenceUrlId, string userId);

        // PricePhase CRUD operations - Create with conferencePriceId, RUD with its own id
        Task<List<PricePhaseResponse>> AddPricePhasesAsync(string conferencePriceId, AddPricePhasesRequest request, string userId);
        Task<List<PricePhaseResponse>> AddPricePhaseForWaitList(string conferencePriceId, PhaseForWaitList request, string userId);
        Task<List<PricePhaseResponse>> GetPricePhasesByConferencePriceIdAsync(string conferencePriceId);
        Task<PricePhaseResponse> UpdatePricePhaseAsync(string pricePhaseId, UpdatePricePhaseRequest request, string userId);
        Task<bool> DeletePricePhaseAsync(string pricePhaseId);

        // Speaker CRUD operations - Create with conferenceSessionId, RUD with its own id
        Task<List<SpeakerResponse>> AddSpeakersAsync(string conferenceSessionId, AddSpeakersRequest request, string userId);
        Task<List<SpeakerResponse>> GetSpeakersByConferenceSessionIdAsync(string conferenceSessionId);
        Task<SpeakerResponse> UpdateSpeakerBySpeakerIdAsync(string speakerId, UpdateSpeakerRequestForConferenceSession request, string userId);
        Task<bool> DeleteSpeakerAsync(string speakerId, string userId);

        // Revision Round Deadline CRUD operations - Create with researchConferencePhaseId, RUD with its own id
        Task<List<RevisionRoundDeadlineResponse>> AddRevisionRoundDeadlinesAsync(string researchConferencePhaseId, addRevisionRequest request, string userId);
        Task<List<RevisionRoundDeadlineResponse>> GetRevisionRoundDeadlinesByResearchPhaseIdAsync(string researchConferencePhaseId);
        Task<RevisionRoundDeadlineResponse> UpdateRevisionRoundDeadlineAsync(string revisionRoundDeadlineId, UpdateRevisionRoundDeadlineRequest request, string userId);
        Task<bool> DeleteRevisionRoundDeadlineAsync(string revisionRoundDeadlineId, string userId);
        
    }

    public class ConferenceStepService : IConferenceStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITokenService _tokenService;
        private readonly IConferenceService _conferenceService;
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        private static readonly HashSet<string> AllowedPaperFormats = new HashSet<string>
    {
        "ieee",
        "acm",
        "apa",
        "springer",
        "mla",       // Modern Language Association
        "chicago",   // Chicago Manual of Style
        "elsevier",  // Elsevier format
        "lncs"       // Lecture Notes in Computer Science (m?t d?ng c?a Springer)
    };

        private readonly ITimeProviderService _timeProviderService;

        public ConferenceStepService(
            IUnitOfWork unitOfWork,
            IObjectStorageFileService objectStorageFileService,
            ITokenService tokenService,
            IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings,
            IConferenceService conferenceService,
            ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _objectStorageFileService = objectStorageFileService;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings.Value;
            _conferenceService = conferenceService;
            _timeProviderService = timeProviderService;
        }

        #region Helper Methods



        private string? AddBaseUrlToUrl(string? url)
        {
            if (string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;

            return _objectStorageSettings.EndPoint?.TrimEnd('/') + "/" + url.TrimStart('/');
        }

        private async Task ValidateSessionTimeAvailability(DateTime startTime, DateTime endTime, string roomId, string? sessionIdToExclude = null)
        {
            if ((endTime - startTime).TotalMinutes < 30)
            {
                throw new BadRequestException("Session duration must be at least 30 minutes.");
            }

            // The date is simply the date part of the local start time. No time zone math.
            var sessionDate = DateOnly.FromDateTime(startTime);
            var existingSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(roomId, sessionDate);

            foreach (var existingSession in existingSessions)
            {
                if (existingSession.ConferenceSessionId == sessionIdToExclude) continue;
                if (!existingSession.StartTime.HasValue || !existingSession.EndTime.HasValue) continue;

                var existingStart = existingSession.StartTime.Value;
                var existingEnd = existingSession.EndTime.Value;

                // Direct, simple comparison of local times.
                if (startTime < existingEnd && endTime > existingStart)
                {
                    throw new BadRequestException($"Session conflicts with an existing session in room {roomId} from {existingStart:HH:mm} to {existingEnd:HH:mm}.");
                }
            }
        }

        private async Task<bool> IsValidConferenceAndTicketSaleDates(DateOnly startDate, DateOnly endDate, DateOnly ticketSaleStart, DateOnly ticketSaleEnd)
        {
            // S? d?ng ngày hôm nay theo múi gi? c?a máy ch?.
            var today = DateOnly.FromDateTime(DateTime.Now);

            // 1. Không có ngày nào du?c n?m trong quá kh?.
            if (startDate < today || ticketSaleStart < today)
            {
                return false;
            }

            // 2. Ngày b?t d?u ph?i tru?c ho?c b?ng ngày k?t thúc.
            if (startDate > endDate)
            {
                return false;
            }

            // 3. Ngày b?t d?u bán vé ph?i tru?c ho?c b?ng ngày k?t thúc bán vé.
            if (ticketSaleStart > ticketSaleEnd)
            {
                return false;
            }

            // 4. Vi?c bán vé ph?i k?t thúc tru?c ho?c trong ngày h?i ngh? b?t d?u.
            if (ticketSaleEnd > startDate)
            {
                return false;
            }

            // T?t c? ki?m tra d?u h?p l?
            return true;
        }
        private async Task<bool> checkConferenceResearchPhase(string confId, DateOnly registrationStart, DateOnly registrationEnd,
            DateOnly FullPaperStart, DateOnly FullPaperEnd,
            DateOnly ReviewStart, DateOnly ReviewEnd,
            DateOnly ReviseStart, DateOnly ReviseEnd,
            DateOnly CameraReadyStart, DateOnly CameraReadyEnd)
        {
            //get conference
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            DateOnly TicketSaleStart = conference.TicketSaleStart.Value, TicketSaleEnd = conference.TicketSaleEnd.Value;
            return true;
        }

        //price check for research conf
        private async Task<ResearchConferencePhase> ValidateAndGetResearchConferencePrerequisitesAsync(string conferenceId, AddConferencePricesRequest request)
        {
            var hasResearchDetail = await _conferenceService.CheckResearchConferenceStepCompletionAsync(conferenceId, "researchconferencedetail");
            var hasResearchPhase = await _conferenceService.CheckResearchConferenceStepCompletionAsync(conferenceId, "researchphase");
            if (!hasResearchDetail || !hasResearchPhase)
            {
                throw new BadRequestException("H?i ngh? nghiên c?u c?n hoàn thành bu?c 'chi ti?t' và 'giai do?n' tru?c khi thêm giá vé.");
            }

            if (!request.TypeOfTicket.Any(tot => tot.isAuthor == true))
            {
                throw new BadRequestException("H?i ngh? nghiên c?u c?n có ít nh?t m?t lo?i vé dành cho tác gi?.");
            }




            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (researchDetail == null)
                throw new InvalidOperationException("Không tìm th?y chi ti?t nghiên c?u cho h?i ngh? này.");

            //price must be larger than review fee
            if (request.TypeOfTicket.Any(tot => tot.TicketPrice < researchDetail.ReviewFee && tot.isAuthor == true))
                throw new Exception($"Không thể để giá của vé isauthor bé hơn review fee {researchDetail.ReviewFee} của conference {conferenceId}");
            var IsAuthorConferencePrice = await _unitOfWork.ConferencePriceRepository.GetNumberOfIsAuthorByConferenceId(conferenceId);
            var sumOfExistingIssAuthor = IsAuthorConferencePrice.Sum(cp => cp.TotalSlot);
            var sumOfRequestIsAuthor = request.TypeOfTicket.Where(cp => cp.isAuthor == true).Sum(cp => cp.TotalSlot);

            //existing isAuthor + request sum of isAuthor must net exceed numberOfAcceptedPaper in researchDetail
            if (sumOfExistingIssAuthor + sumOfRequestIsAuthor > researchDetail.NumberPaperAccept)
                throw new Exception($"T?ng vé is author dã có {sumOfExistingIssAuthor} và các vé trong request lo?i isauthor{sumOfRequestIsAuthor} không th? vu?t numberOfAccepted in researchDetail{researchDetail.NumberPaperAccept}");

            if (researchDetail.AllowListener == true)
            {
                if (!request.TypeOfTicket.Any(tot => tot.isAuthor == false))
                {
                    throw new BadRequestException("H?i ngh? nghiên c?u này cho phép thính gi?, do dó c?n có ít nh?t m?t lo?i vé không dành cho tác gi?.");
                }
            }
            var activePhase = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(conferenceId);
            if (activePhase == null)
            {
                throw new InvalidOperationException("Không tìm th?y phase nào dang ho?t d?ng cho h?i ngh? này.");
            }

            return activePhase;
        }


        // DÁN TOÀN B? PHIÊN B?N NÀY Ð? THAY TH? PHIÊN B?N CU C?A B?N

        private async Task checkEachDateHasConferenceSession(Conference conference, List<DateOnly> newSessionDates, bool checkOnlyBoundaries = false)
        {
            // --- BU?C 1: T?NG H?P T?T C? CÁC NGÀY CÓ SESSION (CU + M?I) ---

            // L?y các ngày có session dã t?n t?i trong database
            var existingSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conference.ConferenceId);
            var existingSessionDates = existingSessions.Select(s => s.SessionDate.Value).Distinct();

            // H?p nh?t danh sách ngày t? request m?i và ngày dã có trong DB.
            // Dùng ToHashSet() d? t?i uu hóa vi?c tra c?u ? các bu?c sau.
            var allUniqueSessionDates = newSessionDates.Union(existingSessionDates).ToHashSet();


            // --- BU?C 2: TH?C HI?N VALIDATION D?A TRÊN `checkOnlyBoundaries` ---

            if (checkOnlyBoundaries)
            {
                // LOGIC M?I: Ch? ki?m tra ngày d?u và cu?i
                if (!conference.StartDate.HasValue || !conference.EndDate.HasValue) return;

                var startDate = conference.StartDate.Value;
                var endDate = conference.EndDate.Value;
                var missingBoundaryDates = new List<DateOnly>();

                // Ki?m tra trên danh sách ÐÃ ÐU?C H?P NH?T
                if (!allUniqueSessionDates.Contains(startDate))
                {
                    missingBoundaryDates.Add(startDate);
                }
                if (startDate != endDate && !allUniqueSessionDates.Contains(endDate))
                {
                    missingBoundaryDates.Add(endDate);
                }

                if (missingBoundaryDates.Any())
                {
                    var missingDatesString = string.Join(" và ", missingBoundaryDates.Select(d => d.ToString("dd/MM/yyyy")));
                    throw new BadRequestException($"Ngày b?t d?u và ngày k?t thúc c?a h?i ngh? ph?i có ít nh?t m?t phiên. Các ngày sau dang b? thi?u phiên: {missingDatesString}");
                }
            }
            else // Tru?ng h?p checkOnlyBoundaries = false
            {
                // LOGIC CU ÐU?C C?P NH?T: Ki?m tra t?t c? các ngày

                // 1. T?o danh sách t?t c? các ngày mà h?i ngh? di?n ra
                List<DateOnly> allConferenceDates = new();
                if (conference.StartDate.HasValue && conference.EndDate.HasValue)
                {
                    for (var date = conference.StartDate.Value; date <= conference.EndDate.Value; date = date.AddDays(1))
                    {
                        allConferenceDates.Add(date);
                    }
                }
                else // N?u h?i ngh? không có ngày, không c?n ki?m tra
                {
                    return;
                }

                // 2. Tìm nh?ng ngày b? thi?u b?ng cách so sánh v?i danh sách ÐÃ ÐU?C H?P NH?T
                var missingDates = allConferenceDates.Except(allUniqueSessionDates);

                if (missingDates.Any())
                {
                    var missingDatesString = string.Join(", ", missingDates.Select(d => d.ToString("dd/MM/yyyy")));
                    throw new BadRequestException($"T?t c? các ngày trong h?i ngh? ph?i có ít nh?t m?t phiên. Các ngày sau dang b? thi?u phiên: {missingDatesString}");
                }
            }
        }

        private async Task EnsureConferenceIsEditable(Conference conference,bool restrictExternalHostedAtPreaparingStatus = false)
        {
            var conferenceStatusId = conference.ConferenceStatusId;

            var pending = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription());
            var preparing = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Preparing.GetDescription());
            var currentStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByIdAsync(conferenceStatusId);
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Draft.GetDescription());
            var onHoldStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.OnHold.GetDescription());

            if (restrictExternalHostedAtPreaparingStatus)
            {
                if (conference.IsInternalHosted != true && conference.ConferenceStatusId == preparing.ConferenceStatusId)
                    throw new BadRequestException("Bạn không thể cập nhật các thông tin cốt lõi (Tên, Vé, Phiên) sau khi hội nghị đã được duyệt lên trạng thái Preparing. Vui lòng liên hệ Organizer nếu cần thay đổi lớn.");
            }
            if (conferenceStatusId != preparing.ConferenceStatusId && conferenceStatusId != draftStatus.ConferenceStatusId && conferenceStatusId != onHoldStatus.ConferenceStatusId )
            {
                throw new BadRequestException($"Thao tác không được phép. Hội nghị đang ở trạng thái '{currentStatus.ConferenceStatusName}' và không thể chỉnh sửa.");
            }
        }

        private Task ValidatePaperFormat(string paperFormat)
        {

            if (!AllowedPaperFormats.Contains(paperFormat.Trim().ToLower()))
            {
                var allowedFormatsString = string.Join(", ", AllowedPaperFormats.OrderBy(f => f));
                throw new BadRequestException($"Ð?nh d?ng bài báo không h?p l?. Các d?nh d?ng du?c ch?p nh?n là: {allowedFormatsString}.");
            }

            return Task.CompletedTask; // Hoàn thành thành công n?u validation pass
        }



        private async Task ValidateRankValueAsync(string rankingCategoryId, string rankValue)
        {
            // N?u RankValue không du?c cung c?p thì không c?n ki?m tra
            if (string.IsNullOrWhiteSpace(rankValue))
            {
                return;
            }

            var rankingCategory = await _unitOfWork.RankingCategoryRepository.GetRankingCategoryByIdAsync(rankingCategoryId);
            if (rankingCategory == null)
            {
                // L?i này dã du?c x? lý ? các phuong th?c g?i, nhung d? an toàn v?n ki?m tra
                throw new NotFoundException($"Lo?i x?p h?ng v?i ID '{rankingCategoryId}' không t?n t?i.");
            }

            // S? d?ng switch d? áp d?ng quy t?c validation cho t?ng lo?i RankName
            switch (rankingCategory.RankName)
            {
                case "Core":
                case "CoreRanking": // G?p chung 2 tru?ng h?p n?u logic gi?ng nhau
                    var validQValues = new HashSet<string> { "Q1", "Q2", "Q3", "Q4" };
                    if (!validQValues.Contains(rankValue.ToUpper()))
                    {
                        throw new BadRequestException($"Giá tr? x?p h?ng cho '{rankingCategory.RankName}' ph?i là Q1, Q2, Q3, ho?c Q4.");
                    }
                    break;

                case "IF": // Impact Factor
                case "CiteScore":
                    if (!decimal.TryParse(rankValue, out var decimalValue) || decimalValue < 0)
                    {
                        throw new BadRequestException($"Giá tr? x?p h?ng cho '{rankingCategory.RankName}' ph?i là m?t s? th?p phân không âm (ví d?: 1.25).");
                    }
                    break;

                case "H5": // H5-Index
                    if (!int.TryParse(rankValue, out var intValue) || intValue < 0)
                    {
                        throw new BadRequestException($"Giá tr? x?p h?ng cho '{rankingCategory.RankName}' ph?i là m?t s? nguyên không âm (ví d?: 15).");
                    }
                    break;

                    // Thêm các tru?ng h?p khác n?u có
                    // default:
                    //     // M?c d?nh không làm gì, cho phép các lo?i rank khác có giá tr? t? do
                    //     break;
            }
        }


        private async Task<ConferenceSession> UpdateSessionInternalAsync(string sessionId, UpdateConferenceSessionRequest request, string userId)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            if (session == null) throw new NotFoundException($"Không tìm thấy phiên với ID {sessionId}");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(session.ConferenceId);

            #region === 1. VALIDATION ===
            // 1.1. Phân quyền và trạng thái
            if (conference.CreatedBy != userId)
                throw new Exception("Bạn không có quyền cập nhật phiên này.");
            await EnsureConferenceIsEditable(conference);

            // 1.2. VALIDATION NGHIỆP VỤ QUAN TRỌNG: Kiểm tra dữ liệu phụ thuộc
            //var assignedPapers = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorsBySessionIdAsync(sessionId);
            //if (assignedPapers.Any())
            //{
            //    // Nếu đã có bài báo được gán, cấm thay đổi các thông tin quan trọng về lịch trình
            //    if (request.Date.HasValue || request.StartTime.HasValue || request.EndTime.HasValue || request.RoomId != null)
            //    {
            //        throw new BadRequestException("Không thể thay đổi thời gian hoặc địa điểm của phiên này vì đã có bài báo được gán để trình bày.");
            //    }
            //}

            // 1.3. Xác định các giá trị cuối cùng
            var finalDate = request.Date ?? session.SessionDate.Value;
            var finalStartTime = request.StartTime ?? TimeOnly.FromDateTime(session.StartTime.Value);
            var finalEndTime = request.EndTime ?? TimeOnly.FromDateTime(session.EndTime.Value);

            string? finalRoomId;

            if (conference.IsResearchConference == true)
            {
                // 1. Research: Linh hoạt (Optional)
                // Lấy cái mới nếu có, không thì giữ cái cũ. Cho phép null.
                if (request.RoomId != null) // Có gửi field roomId trong JSON
                {
                    // Nếu gửi "" -> gán null. Nếu gửi "ID" -> gán "ID".
                    finalRoomId = string.IsNullOrWhiteSpace(request.RoomId) ? null : request.RoomId;
                }
                else
                {
                    // Không gửi field roomId -> Giữ nguyên cũ
                    finalRoomId = session.RoomId;
                }
            }
            else if (conference.IsInternalHosted == true)
            {
                // 2. Technical Internal: Bắt buộc (Strict)
                finalRoomId = request.RoomId ?? session.RoomId;
                if (string.IsNullOrEmpty(finalRoomId))
                    throw new BadRequestException("Hội nghị Technical nội bộ bắt buộc phiên phải có phòng (RoomId).");
            }
            else
            {
                // 3. Technical External / Collaborator: Bắt buộc Null (Force Null)
                finalRoomId = null;
            }

            // --- CHECK TỒN TẠI VÀ OVERLAP (CHỈ KHI CÓ ROOMID) ---
            if (!string.IsNullOrEmpty(finalRoomId))
            {
                // Nếu RoomId thay đổi (hoặc mới gán cho Research), check xem Room có tồn tại không
                if (finalRoomId != session.RoomId)
                {
                    if (await _unitOfWork.RoomRepository.GetRoomByIdAsync(finalRoomId) == null)
                        throw new NotFoundException($"Phòng với ID {finalRoomId} không tồn tại.");
                }

                // Validate ngày giờ
                if (finalDate < conference.StartDate || finalDate > conference.EndDate)
                    throw new BadRequestException($"Ngày của phiên nằm ngoài thời gian hội nghị.");

                var finalStartDateTime = finalDate.ToDateTime(finalStartTime);
                var finalEndDateTime = finalDate.ToDateTime(finalEndTime);

                // Check trùng giờ
                await ValidateSessionTimeAvailability(finalStartDateTime, finalEndDateTime, finalRoomId, sessionId);
            }
            #endregion

            #region === 2. THỰC THI ===
            session.Title = request.Title ?? session.Title;
            session.Description = request.Description ?? session.Description;

            // Chỉ cập nhật các trường này nếu an toàn (chưa có bài báo gán vào)
            //if (!assignedPapers.Any())
            //{
            //    session.StartTime = finalStartDateTime;
            //    session.EndTime = finalEndDateTime;
            //    session.SessionDate = finalDate;
            //    session.RoomId = finalRoomId;
            //}
            session.StartTime = finalDate.ToDateTime(finalStartTime);
            session.EndTime = finalDate.ToDateTime(finalEndTime);
            session.SessionDate = finalDate;
            session.RoomId = finalRoomId;

            await _unitOfWork.ConferenceSessionRepository.UpdateConferenceSessionAsync(session);
            #endregion

            return await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
        }
        #endregion

        #region Step 1: Basic Conference

        public async Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid)
        {

            if (string.IsNullOrWhiteSpace(request.ConferenceName))
                throw new BadRequestException("Tên hội nghị là bắt buộc.");

            if (request.IsResearchConference == true)
                throw new BadRequestException("Chức năng này dùng để tạo hội nghị kỹ thuật, 'IsResearchConference' phải là false.");
            if (request.TotalSlot <= 0)
                throw new BadRequestException("Tổng số vé phải là một số dương.");


            if (!IsValidConferenceAndTicketSaleDates(request.StartDate, request.EndDate, request.TicketSaleStart, request.TicketSaleEnd).Result)
                throw new BadRequestException("Ngày tháng cung cấp không hợp lệ. Vui lòng đảm bảo các ngày không nằm trong quá khứ, ngày bắt đầu/kết thúc theo đúng thứ tự, và ngày bán vé phải kết thúc trước khi hội nghị bắt đầu.");

            if (await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId) == null)
                throw new NotFoundException($"Danh mục hội nghị với ID '{request.ConferenceCategoryId}' không tồn tại.");
            if (await _unitOfWork.CityRepository.GetCityByIdAsync(request.CityId) == null)
                throw new NotFoundException($"Thành phố với ID '{request.CityId}' không tồn tại.");

            // 1.5. Validation file banner
            if (request.BannerImageFile == null)
                throw new BadRequestException("Ảnh bìa (banner) là bắt buộc.");
            if (!_objectStorageFileService.IsValidImageFile(request.BannerImageFile))
                throw new BadRequestException($"Loại ảnh bìa không được hỗ trợ: '{request.BannerImageFile.ContentType}'.");
            const long maxBannerSize = 5 * 1024 * 1024; // 5 MB
            if (request.BannerImageFile.Length > maxBannerSize)
                throw new BadRequestException("Kích thước tệp ảnh bìa không được vượt quá 5 MB.");


            await _unitOfWork.BeginTransactionAsync();
            try
            {


                // 2.1. T?i file banner
                using var bannerStream = request.BannerImageFile.OpenReadStream();
                var bannerFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                request.bannerImageFileUrl = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), bannerFileName, bannerStream, request.BannerImageFile.ContentType);



                // 2.3. Xác d?nh tr?ng thái ban d?u
                string initialStatusName = ConferenceStatusEnum.Preparing.GetDescription();
                var initialStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(initialStatusName);

                // 2.4. T?o Conference
                var conference = ConferenceStepBasicCreateToModel.creatBasicConference(request, initialStatus, await _timeProviderService.GetVietnamTime(), userid);
                await _unitOfWork.ConferenceRepository.CreateConferenceAsync(conference);

                // 2.5. T?o TechnicalConferenceDetail
                var technicalConferenceDetail = new TechnicalConferenceDetail
                {
                    ConferenceId = conference.ConferenceId,
                    TargetAudience = request.targetAudienceTechnicalConference,
                };

                await _unitOfWork.TechnicalConferenceDetailRepository.CreateTechnicalAsync(technicalConferenceDetail);

                await _unitOfWork.CommitAsync();
                return await GetConferenceBasicAsync(conference.ConferenceId);

            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<string> CreateSkeletonTechnicalConferenceBasicForCollaboratorAsync(string name, string collabId)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Phải có tên của hội nghị để có thể tạo một hội nghị cho collab");
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Draft.GetDescription());
            var now = await _timeProviderService.GetVietnamTime();
            Conference techConference = new Conference
            {
                ConferenceId = Guid.NewGuid().ToString(),
                IsInternalHosted = false,
                IsResearchConference = false,
                ConferenceName = name,
                CreatedAt = now,
                CreatedBy = collabId,
                ConferenceStatusId = draftStatus.ConferenceStatusId
            };
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ConferenceRepository.CreateConferenceAsync(techConference);
                TechnicalConferenceDetail technicalConferenceDetail = new TechnicalConferenceDetail()
                {
                    ConferenceId = techConference.ConferenceId,
                    TargetAudience = ""
                };
                await _unitOfWork.TechnicalConferenceDetailRepository.CreateTechnicalAsync(technicalConferenceDetail);
                await _unitOfWork.CommitAsync();
                return techConference.ConferenceId;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }

        }

        public async Task<TechnicalConferenceBasicStepResponse> GetConferenceBasicAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");
            var technical = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conferenceId);

            return new TechnicalConferenceBasicStepResponse
            {
                conferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                bannerImageFileUrl = conference.BannerImageUrl,
                createdAt = conference.CreatedAt,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                createdby = conference.CreatedBy,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                TargetAudience = technical?.TargetAudience,
            };
        }

        public async Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"H?i ngh? v?i ID {conferenceId} không tìm th?y");


            var technicalDetail = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conferenceId);
            if (technicalDetail == null) throw new NotFoundException($"Không tìm th?y chi ti?t k? thu?t (technical detail) cho h?i ngh? ID {conferenceId}");



            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n c?p nh?t h?i ngh? này.");
            await EnsureConferenceIsEditable(conference);



            var finalStartDate = request.StartDate ?? conference.StartDate;
            var finalEndDate = request.EndDate ?? conference.EndDate;
            var finalTicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
            var finalTicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;
            if (finalStartDate.HasValue && finalEndDate.HasValue && finalTicketSaleStart.HasValue && finalTicketSaleEnd.HasValue)
            {
                if (!IsValidConferenceAndTicketSaleDates(finalStartDate.Value, finalEndDate.Value, finalTicketSaleStart.Value, finalTicketSaleEnd.Value).Result)
                    throw new BadRequestException("Ngày tháng cung c?p không h?p l?.");
            }


            //if (request.TotalSlot.HasValue)
            //{
            //    int soldTickets = (conference.TotalSlot ?? 0) - (conference.AvailableSlot ?? 0);
            //    if (request.TotalSlot.Value < soldTickets)
            //        throw new BadRequestException($"Không th? gi?m t?ng s? vé xu?ng {request.TotalSlot.Value} vì dã có {soldTickets} vé du?c bán.");
            //}


            if (request.BannerImageFile != null && !_objectStorageFileService.IsValidImageFile(request.BannerImageFile))
                throw new BadRequestException("Định d?ng ?nh bìa không du?c h? tr?.");

            var userRoles = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(conference.CreatedBy);
            var roleNames = userRoles.Select(ur => ur.Role?.RoleName).ToHashSet();
            bool isCollaborator = roleNames.Contains(SystemRoleEnum.Collaborator.GetDescription());

            if (!string.IsNullOrWhiteSpace(request.CityId) && request.CityId != conference.CityId)
            {
                if (await _unitOfWork.CityRepository.GetCityByIdAsync(request.CityId) == null)
                {
                    throw new NotFoundException($"Thành ph? v?i ID '{request.CityId}' không t?n t?i.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.ConferenceCategoryId) && request.ConferenceCategoryId != conference.ConferenceCategoryId)
            {
                if (await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId) == null)
                {
                    // N?u Category ID m?i không t?n t?i, báo l?i NGAY L?P T?C
                    throw new NotFoundException($"Danh m?c h?i ngh? v?i ID '{request.ConferenceCategoryId}' không t?n t?i.");
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {


                // 2.1. C?p nh?t các thu?c tính c?a Conference
                conference.ConferenceName = request.ConferenceName ?? conference.ConferenceName;
                conference.Description = request.Description ?? conference.Description;
                conference.StartDate = request.StartDate ?? conference.StartDate;
                conference.EndDate = request.EndDate ?? conference.EndDate;
                conference.Address = request.Address ?? conference.Address;
                conference.ConferenceCategoryId = request.ConferenceCategoryId ?? conference.ConferenceCategoryId;
                conference.CityId = request.CityId ?? conference.CityId;
                conference.TicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
                conference.TicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;

                // C?p nh?t TotalSlot và AvailableSlot m?t cách chính xác
                if (request.TotalSlot.HasValue)
                {
                    conference.TotalSlot = request.TotalSlot.Value;
                    conference.AvailableSlot = request.TotalSlot.Value;
                }

                // 2.2. T?i và c?p nh?t URL file banner n?u có
                if (request.BannerImageFile != null)
                {
                    using var stream = request.BannerImageFile.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                    conference.BannerImageUrl = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
                }

                // 2.3. C?p nh?t các thu?c tính c?a TechnicalDetail
                technicalDetail.TargetAudience = request.targetaudience ?? technicalDetail.TargetAudience;


                // 2.4. Luu các thay d?i vào DB
                await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);
                await _unitOfWork.TechnicalConferenceDetailRepository.UpdateTechnicalAsync(technicalDetail);

                await _unitOfWork.CommitAsync();



                return await GetConferenceBasicAsync(conferenceId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Step 2: Prices

        public async Task<ConferencePriceListWithPhasesResponse> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
                throw new NotFoundException($"Hội nghị với ID {conferenceId} không thấy");

            if (conference.CreatedBy != userId)
                throw new BadRequestException("Bạn không có quyền thêm giá vé cho hội nghị này.");

            if (request.TypeOfTicket == null || !request.TypeOfTicket.Any())
                throw new BadRequestException("Yêu cầu phải chứa ít nhất một loại vé.");
            ConferencePriceListWithPhasesResponse result = new ConferencePriceListWithPhasesResponse
            {
                conferencePriceWithPhasesResponses = new List<ConferencePriceWithPhasesResponse>()
            };
            var existingConferencePrice = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
            var conferenceStatusName = conference.ConferenceStatus?.ConferenceStatusName ?? string.Empty;
            await EnsureConferenceIsEditable(conference);

            //get existing total slot from conference price to check if this addition will result in exceeding the conferene totol slot 
            var existingTotalSlot = existingConferencePrice.Sum(x => x.TotalSlot);

            //get related table for research if the conference is of type reserarch
            ResearchConferencePhase researchPhase = new();




            if (conference.IsResearchConference == true)
            {
                researchPhase = await ValidateAndGetResearchConferencePrerequisitesAsync(conferenceId, request);
            }
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create the conference price
                var conferencePriceRequest = request.TypeOfTicket;
                int? totalSlotFromToBeTickets = request.TypeOfTicket.Sum(ts => ts.TotalSlot);
                if (totalSlotFromToBeTickets + existingTotalSlot > conference.TotalSlot) 
                    throw new BadRequestException($"Số lượng totalSlot của từng loại vé tổng phải nhỏ hơn hoặc bằng capacity của conference: {existingTotalSlot} + {totalSlotFromToBeTickets} > {conference.TotalSlot}");
                foreach (CreateConferencePriceRequest toBeConferencePrice in conferencePriceRequest)
                {

                    //Phase for each ticket type
                    List<PricePhaseResponse> pricePhaseResponses = new();
                    if (existingConferencePrice.Any(p => p.TicketName.Equals(toBeConferencePrice.TicketName, StringComparison.OrdinalIgnoreCase)))
                        throw new BadRequestException($"Tên vé '{toBeConferencePrice.TicketName}' đã tồn tại trong hội nghị này.");
                    if (toBeConferencePrice.TicketPrice < 0) throw new BadRequestException($"Giá vé cho '{toBeConferencePrice.TicketName}' không được là số âm.");
                    if (toBeConferencePrice.TotalSlot <= 0) throw new BadRequestException($"Số lượng vé cho '{toBeConferencePrice.TicketName}' phải lớn hơn 0.");
                    //check if totalslot of phases in a ticket type is larger than the totalslot of the ticket itself
                    if (toBeConferencePrice.Phases == null || !toBeConferencePrice.Phases.Any()) throw new BadRequestException($"Loại vé '{toBeConferencePrice.TicketName}' phải có ít nhất một giai đoạn bán vé.");
                    var totalSlotFromPhases = toBeConferencePrice.Phases.Sum(phase => phase.Totalslot);
                    if (toBeConferencePrice.TotalSlot != totalSlotFromPhases)
                        throw new BadRequestException($"Với vé '{toBeConferencePrice.TicketName}', tổng số vé trong các giai đoạn ({totalSlotFromPhases}) không khớp với tổng số vé của loại vé đó ({toBeConferencePrice.TotalSlot}).");



                    // *** VALIDATION M?I: Các phase trong cùng 1 ticket không du?c ch?ng chéo ***
                    var sortedPhases = toBeConferencePrice.Phases.OrderBy(p => p.StartDate).ToList();
                    for (int i = 0; i < sortedPhases.Count - 1; i++)
                    {
                        if (sortedPhases[i].EndDate >= sortedPhases[i + 1].StartDate)
                        {
                            throw new BadRequestException($"Trong vé '{toBeConferencePrice.TicketName}', giai đoạn '{sortedPhases[i].PhaseName}' (kết thúc vào {sortedPhases[i].EndDate:dd/MM/yyyy}) bị chồng chéo hoặc quá sát với giai đoạn '{sortedPhases[i + 1].PhaseName}' (bắt đầu vào {sortedPhases[i + 1].StartDate:dd/MM/yyyy}).");
                        }
                    }
                    var CreatedConferencePrice = toBeConferencePrice.ToModel(conferenceId);
                    await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(CreatedConferencePrice);
                    foreach (CreatePricePhaseRequest createPricePhaseRequest in toBeConferencePrice.Phases)
                    {
                        List<ConfRadar.Services.DTOs.ConferenceStep.RefundPolicyResponse> refundPolicyResponses = new();


                        if (string.IsNullOrWhiteSpace(createPricePhaseRequest.PhaseName))
                            throw new BadRequestException($"Tên giai đoạnn trong vé '{createPricePhaseRequest.PhaseName}' không được để trùng.");
                        if (createPricePhaseRequest.ApplyPercent < 0 || createPricePhaseRequest.ApplyPercent > 1000)
                            throw new BadRequestException($"Tỷ lệ áp dụng cho giai đoạn '{createPricePhaseRequest.ApplyPercent}' phải từ 0 đến 1000.");
                        //check if each phase request is in valid date
                        //createPricePhaseRequest start must < end, 
                        if (createPricePhaseRequest.StartDate > createPricePhaseRequest.EndDate) throw new BadRequestException("Start phase phải lớn hơn end phase");
                        if (toBeConferencePrice.isAuthor == true)
                        {
                            //each phase of author ticket types must be in registation start/end interval
                            if (createPricePhaseRequest.StartDate < researchPhase.RegistrationStartDate || createPricePhaseRequest.EndDate > researchPhase.RegistrationEndDate)
                            {
                                throw new BadRequestException("Vé bán cho authors phải trong khoảng registration start và end");
                            }

                        }
                        //each phase of technical and non author must be in conference's ticket sale start and end
                        else if (createPricePhaseRequest.StartDate < conference.TicketSaleStart || createPricePhaseRequest.EndDate > conference.TicketSaleEnd)
                            throw new BadRequestException("Start phase phải và end phase phải nằm trong ticket sale start và ticket sale end của conference");
                        var CreatedPricePhase = createPricePhaseRequest.ToModel(CreatedConferencePrice.ConferencePriceId, researchPhase.ResearchConferencePhaseId);
                        await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(CreatedPricePhase);


                        // X? lý Refund Policies cho phase này
                        if (createPricePhaseRequest.refundInPhase != null && createPricePhaseRequest.refundInPhase.Any())
                        {

                            var sortedRefunds = createPricePhaseRequest.refundInPhase.OrderBy(r => r.RefundDeadline).ToList();
                            for (int i = 0; i < sortedRefunds.Count; i++)
                            {
                                var refundRequest = sortedRefunds[i];

                                if (!refundRequest.PercentRefund.HasValue || !refundRequest.RefundDeadline.HasValue)
                                    throw new BadRequestException("Chính sách hoàn tiền phải có đủ phần trăm và hạn chót.");
                                if (refundRequest.PercentRefund.Value < 0 || refundRequest.PercentRefund.Value > 100)
                                    throw new BadRequestException("PerentRefund phải nằm trong khoảng 0-100");

                                // *** VALIDATION M?I: Deadline c?a refund policy ***
                                // 1. Ph?i sau ngày b?t d?u c?a phase
                                if (refundRequest.RefundDeadline.Value <= createPricePhaseRequest.StartDate)
                                {
                                    throw new BadRequestException($"Trong giai đoạn '{createPricePhaseRequest.PhaseName}', hạn chót hoàn tiền ({refundRequest.RefundDeadline.Value:dd/MM/yyyy}) phải sau ngày bắt đầu giai đoạn ({createPricePhaseRequest.StartDate:dd/MM/yyyy}).");
                                }
                                // 2. Ph?i tru?c ngày b?t d?u bán vé c?a c? h?i ngh?
                                if (refundRequest.RefundDeadline.Value >= conference.TicketSaleEnd)
                                {
                                    throw new BadRequestException($"Trong giai đoạn '{createPricePhaseRequest.PhaseName}',  hạn chót hoàn tiền ({refundRequest.RefundDeadline.Value:dd/MM/yyyy}) phải trước ngày kết thúc bán vé của hội nghị ({conference.TicketSaleEnd:dd/MM/yyyy}).");
                                }


                                var refundModel = new RefundPolicy
                                {
                                    RefundPolicyId = Guid.NewGuid().ToString(),
                                    PricePhaseId = CreatedPricePhase.PricePhaseId,
                                    PercentRefund = refundRequest.PercentRefund.Value,
                                    RefundDeadline = refundRequest.RefundDeadline.Value,
                                    RefundOrder = i + 1 // T? d?ng gán th? t? d?a trên deadline
                                };
                                await _unitOfWork.ConferenceRefundPolicyRepository.CreateConferenceRefundPolicyAsync(refundModel);
                                refundPolicyResponses.Add(refundModel.ToResponse());
                            }
                        }



                        pricePhaseResponses.Add(new PricePhaseResponse
                        {
                            PhaseName = createPricePhaseRequest.PhaseName,
                            StartDate = createPricePhaseRequest.StartDate,
                            EndDate = createPricePhaseRequest.EndDate,
                            ApplyPercent = createPricePhaseRequest.ApplyPercent,
                            TotalSlot = createPricePhaseRequest.Totalslot,
                            PricePhaseId = CreatedPricePhase.PricePhaseId,
                            RefundPolicy = refundPolicyResponses
                        });

                    }
                    result.conferencePriceWithPhasesResponses.Add(new ConferencePriceWithPhasesResponse
                    {
                        ConferencePriceId = CreatedConferencePrice.ConferencePriceId,
                        TicketDescription = CreatedConferencePrice.TicketDescription,
                        TicketName = CreatedConferencePrice.TicketName,
                        PricePhases = pricePhaseResponses,
                        TicketPrice = CreatedConferencePrice.TicketPrice
                    });
                }
                await _unitOfWork.CommitAsync();
                //var conferencePrice = conferencePriceRequest.ToModel(conferenceId);
                // For technical conference, isAuthor must be false
                //conferencePrice.IsAuthor = false;


                // Return the created price with its phases
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ConferencePriceWithPhasesResponse>> GetConferencePricesAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var prices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
            var responses = new List<ConferencePriceWithPhasesResponse>();

            foreach (var price in prices)
            {
                var phases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(price.ConferencePriceId);
                responses.Add(price.ToResponseWithPhases(phases));
            }

            return responses;
        }


        public async Task<ConferencePriceWithPhasesResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request, string userId)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null)
            {
                throw new NotFoundException($"Không tìm th?y lo?i vé v?i ID {priceId}");
            }

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(price.ConferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Không tìm th?y h?i ngh? g?c liên quan d?n lo?i vé này.");
            }



            // 1. Phân quy?n
            if (conference.CreatedBy != userId)
            {
                throw new ForbiddenException("B?n không có quy?n c?p nh?t lo?i vé này.");
            }

            await EnsureConferenceIsEditable(conference);

            if (request.TicketPrice.HasValue && request.TicketPrice.Value < 0)
                throw new BadRequestException("Giá vé không du?c là s? âm.");

            if (request.TotalSlot.HasValue)
            {
                if (request.TotalSlot.Value <= 0)
                    throw new BadRequestException("S? lu?ng vé ph?i l?n hon 0.");

                int soldTicketsForThisPrice = price.TotalSlot.GetValueOrDefault() - price.AvailableSlot.GetValueOrDefault();
                if (request.TotalSlot.Value < soldTicketsForThisPrice)
                {
                    throw new BadRequestException($"Không th? gi?m s? lu?ng vé xu?ng {request.TotalSlot.Value} vì dã có {soldTicketsForThisPrice} vé du?c bán cho lo?i vé này.");
                }

                var allConferencePrices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(price.ConferenceId);
                var otherPricesTotalSlot = allConferencePrices
                    .Where(p => p.ConferencePriceId != priceId)
                    .Sum(p => p.TotalSlot ?? 0);
                var newConferenceTotalSlot = otherPricesTotalSlot + request.TotalSlot.Value;

                if (newConferenceTotalSlot > conference.TotalSlot)
                {
                    throw new BadRequestException($"C?p nh?t th?t b?i. T?ng s? vé m?i ({newConferenceTotalSlot}) s? vu?t quá gi?i h?n {conference.TotalSlot} c?a h?i ngh?.");
                }
            }

            // 5. Ngan ch?n tên vé trùng l?p
            if (!string.IsNullOrWhiteSpace(request.TicketName) && !request.TicketName.Equals(price.TicketName, StringComparison.OrdinalIgnoreCase))
            {
                var existingPrices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(price.ConferenceId);
                if (existingPrices.Any(p => p.TicketName.Equals(request.TicketName, StringComparison.OrdinalIgnoreCase) && p.ConferencePriceId != priceId))
                {
                    throw new BadRequestException($"Tên vé '{request.TicketName}' dã t?n t?i trong h?i ngh? này.");
                }
            }

            price.TicketPrice = request.TicketPrice ?? price.TicketPrice;
            price.TicketName = request.TicketName ?? price.TicketName;
            price.TicketDescription = request.TicketDescription ?? price.TicketDescription;

            if (request.TotalSlot.HasValue)
            {
                int slotDifference = request.TotalSlot.Value - price.TotalSlot.GetValueOrDefault();
                price.TotalSlot = request.TotalSlot.Value;
                price.AvailableSlot = (price.AvailableSlot.GetValueOrDefault() + slotDifference);
            }

            await _unitOfWork.ConferencePriceRepository.UpdateConferencePriceAsync(price);



            var phases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(price.ConferencePriceId);
            return price.ToResponseWithPhases(phases);
        }
        public async Task<bool> DeleteConferencePriceAsync(string priceId, string userId)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null)
            {
                // Gi? nguyên NotFoundException d? không ti?t l? s? t?n t?i c?a d? li?u
                throw new NotFoundException($"Không tìm thấy loại vé với ID {priceId}");
            }

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(price.ConferenceId);
            // 1. Phân quy?n
            if (conference.CreatedBy != userId)
            {
                throw new ForbiddenException("B?n không có quy?n xóa lo?i vé này.");
            }
            await EnsureConferenceIsEditable(conference);
            // Check if there are any tickets already sold for this price
            var ticketCount = await _unitOfWork.TicketRepository.GetTicketCountByConferencePriceIdAsync(priceId);
            if (ticketCount > 0) throw new BadRequestException("Cannot delete price because tickets have already been sold for this price");

            return await _unitOfWork.ConferencePriceRepository.DeleteConferencePriceAsync(price) > 0;
        }

        #endregion

        #region Step 3: Sessions

        public async Task<List<ConferenceSessionWithMediaResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new BadRequestException($"Không tìm thấy hội nghị với ID {conferenceId}");
            }


            // 1. Phân quy?n
            if (conference.CreatedBy != userId)
            {
                throw new BadRequestException("Bạn không có quyền thêm session cho hội nghị này.");
            }
            if (request.Sessions == null || !request.Sessions.Any())
            {
                throw new BadRequestException("Yêu cầu phải chứa ít nhất một phiên (session).");
            }


            await EnsureConferenceIsEditable(conference);
            List<DateOnly> newSessionDates = request.Sessions.Where(s => s.Date.HasValue).Select(s => s.Date.Value).Distinct().ToList();


            await checkEachDateHasConferenceSession(conference, newSessionDates, true);

            //check if session in request overlap

            //group by same roomId and same date
            var sessionGroupByRoomAndDate = request.Sessions.Where(s => s.RoomId != null).GroupBy(s => new { s.RoomId, s.Date });
            foreach (var group in sessionGroupByRoomAndDate)
            {
                var sortedSession = group.OrderBy(g => g.StartTime).ToList();
                for (int i = 0; i < sortedSession.Count - 1; i++)
                {
                    var currentSession = sortedSession[i];
                    var nextSession = sortedSession[i + 1];
                    if (currentSession.EndTime.HasValue && nextSession.StartTime.HasValue &&
                        currentSession.EndTime.Value > nextSession.StartTime.Value)
                    {
                        throw new Exception($"Dữ liệu request không hợp lệ: Session '{currentSession.Title}' (kết thúc lúc {currentSession.EndTime}) bị chồng chéo thời gian với Session '{nextSession.Title}' (bắt đầu lúc {nextSession.StartTime}) trong cùng một phòng roomId {group.Key.RoomId} và cùng một ngày {group.Key.Date}.");
                    }
                }
            }

            var responses = new List<ConferenceSessionWithMediaResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Sessions != null)
                {
                    foreach (var session in request.Sessions)
                    {
                        if (string.IsNullOrWhiteSpace(session.Title))
                            throw new BadRequestException("Tiêu đề của session không được để trống.");
                        if (session.StartTime == null || session.EndTime == null || session.Date == null)
                            throw new BadRequestException($"Session '{session.Title}' cần có đủ StartTime, EndTime, và Date.");

                        if (conference.IsInternalHosted == true)
                        {
                            if (session.RoomId == null)
                                throw new Exception($"Session '{session.Title}' bắt buộc phải có RoomId vì đây là hội nghị nội bộ.");
                            if (await _unitOfWork.RoomRepository.GetRoomByIdAsync(session.RoomId) == null)
                                throw new Exception($"Phòng với ID {session.RoomId} không tồn tại.");
                        }


                        if (session.Date.Value < conference.StartDate || session.Date.Value > conference.EndDate)
                        {
                            throw new BadRequestException($"Ngày của phiên '{session.Title}' ({session.Date.Value:dd/MM/yyyy}) nằm ngoài khoảng thời gian diễn ra hội nghị ({conference.StartDate:dd/MM/yyyy} - {conference.EndDate:dd/MM/yyyy}).");
                        }




                        // Step 2: Validate using these direct, local time values.


                        if (session.RoomId != null)
                        {
                            var sessionStartDateTime = session.Date.Value.ToDateTime(session.StartTime.Value);
                            var sessionEndDateTime = session.Date.Value.ToDateTime(session.EndTime.Value);
                            await ValidateSessionTimeAvailability(sessionStartDateTime, sessionEndDateTime, session.RoomId);
                        }
                        var conferenceSession = session.ToModel(conferenceId);

                        await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);
                        // Add speakers for the session
                        if (session.Speaker != null && session.Speaker.Any())
                        {
                            foreach (var speakerRequest in session.Speaker)
                            {
                                if ((string.IsNullOrWhiteSpace(speakerRequest.Name)))
                                    throw new BadRequestException($"Tên của diễn giả trong phiên '{session.Title}' không được để trống.");
                                if (speakerRequest.Image != null && !_objectStorageFileService.IsValidImageFile(speakerRequest.Image))
                                    throw new BadRequestException($"Ðịnh dạng ảnh của diễn giả '{speakerRequest.Name}' không được hỗ trợ.");

                                String speakerURL = "";

                                if (speakerRequest.Image != null)
                                {
                                    using var stream = speakerRequest.Image.OpenReadStream();
                                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(speakerRequest.Image.FileName);
                                    speakerURL = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, speakerRequest.Image.ContentType);
                                    speakerURL = _objectStorageSettings.EndPoint + speakerURL;
                                }
                                var speaker = speakerRequest.ToModel(conferenceSession.ConferenceSessionId, speakerURL);

                                await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
                            }
                        }

                        // Add media for the session
                        if (session.SessionMedias != null)
                        {
                            foreach (var mediaRequest in session.SessionMedias)
                            {
                                String mediaURl = "";
                                if (!_objectStorageFileService.IsValidVideoFile(mediaRequest.MediaFile) && !_objectStorageFileService.IsValidImageFile(mediaRequest.MediaFile))
                                    throw new Exception("Khong ho tro dinh dang cho sessionMedia nay");

                                if (mediaRequest.MediaFile != null)
                                {
                                    using var stream = mediaRequest.MediaFile.OpenReadStream();
                                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(mediaRequest.MediaFile.FileName);
                                    mediaURl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencesessionmedia.ToString(), uniqueFileName, stream, mediaRequest.MediaFile.ContentType);
                                    mediaURl = _objectStorageSettings.EndPoint + mediaURl;
                                }
                                var sessionMedia = mediaRequest.ToModel(conferenceSession.ConferenceSessionId, mediaURl);
                                await _unitOfWork.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(sessionMedia);
                            }
                        }

                        // Get updated session with all details
                        var createdSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(conferenceSession.ConferenceSessionId);
                        responses.Add(createdSession.ToResponseWithMedia());
                        //int result = await _unitOfWork.SaveChangesAsync();
                        //if (result <= 0) throw new Exception("Không t?o du?c");
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<ConferenceSessionWithMediaResponse>> GetConferenceSessionsAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
            var responses = new List<ConferenceSessionWithMediaResponse>();

            foreach (var session in sessions)
            {
                responses.Add(session.ToResponseWithMedia());
            }

            return responses;
        }



        public async Task<ConferenceSessionWithMediaResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request, string userId)
        {
            // L?y thông tin conference d? ki?m tra lo?i
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Không tìm thấyy phiên với ID {sessionId}");
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(session.ConferenceId);

            // Ki?m tra d?c thù c?a phuong th?c này
            if (conference.IsResearchConference == true)
                throw new BadRequestException("Chức năng này không dành cho phiên của hội nghị nghiên cứu.");

            // G?i hàm helper chung d? th?c hi?n t?t c? công vi?c
            var updatedSession = await UpdateSessionInternalAsync(sessionId, request, userId);

            // Tr? v? dúng ki?u response
            return updatedSession.ToResponseWithMedia();
        }

        public async Task<SpeakerResponse> UpdateSpeakerAsync(string sessionId, UpdateSpeakerRequest request)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Session with ID {sessionId} not found.");

            // Find an existing speaker for this session (if any)
            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerBySessionIdAsync(sessionId);
            if (speaker == null)
            {
                // Create a new speaker
                speaker = new Speaker
                {
                    SpeakerId = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                    ConferenceSessionId = sessionId
                };

                if (request.Image != null)
                {
                    using var stream = request.Image.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.Image.FileName);
                    speaker.Image = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, request.Image.ContentType);
                    speaker.Image = _objectStorageSettings.EndPoint + speaker.Image;
                }

                await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
            }
            else
            {
                // Update existing speaker
                speaker.Name = request.Name ?? speaker.Name;
                speaker.Description = request.Description ?? speaker.Description;

                if (request.Image != null)
                {
                    using var stream = request.Image.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.Image.FileName);
                    speaker.Image = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, request.Image.ContentType);
                    speaker.Image = _objectStorageSettings.EndPoint + speaker.Image;
                }

                await _unitOfWork.SpeakerRepository.UpdateSpeakerAsync(speaker);
            }

            return speaker.ToResponse();
        }

        public async Task<bool> DeleteConferenceSessionAsync(string sessionId)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            // Delete all speakers associated with this session
            var speakers = await _unitOfWork.SpeakerRepository.GetSpeakersBySessionIdAsync(sessionId);
            foreach (var speaker in speakers)
            {
                await _unitOfWork.SpeakerRepository.DeleteSpeakerAsync(speaker);
            }

            // Delete all media associated with this session
            var mediaList = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(sessionId);
            foreach (var media in mediaList)
            {
                await _unitOfWork.ConferenceSessionMediumRepository.DeleteConferenceSessionMediumAsync(media);
            }

            return await _unitOfWork.ConferenceSessionRepository.DeleteConferenceSessionAsync(session) > 0;
        }

        #endregion

        #region Step 4: Policies

        public async Task<List<ConferencePolicyResponse>> AddConferencePoliciesAsync(string conferenceId, AddConferencePoliciesRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            var responses = new List<ConferencePolicyResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Policies != null)
                {
                    foreach (var policy in request.Policies)
                    {
                        var conferencePolicy = policy.ToModel(conferenceId);
                        await _unitOfWork.ConferencePolicyRepository.CreateConferencePolicyAsync(conferencePolicy);
                        responses.Add(conferencePolicy.ToResponse());
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<ConferencePolicyResponse>> GetConferencePoliciesAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conferenceId);
            return policies.Select(p => p.ToResponse()).ToList();
        }

        public async Task<ConferencePolicyResponse> UpdateConferencePolicyAsync(string policyId, UpdateConferencePolicyRequest request, string userId)
        {
            var policy = await _unitOfWork.ConferencePolicyRepository.GetConferencePolicyByIdAsync(policyId);
            if (policy == null) throw new NotFoundException($"Conference policy with ID {policyId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(policy.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm thấy hội nghị nào có policy với ID {policyId}");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (!string.IsNullOrEmpty(request.PolicyName)) policy.PolicyName = request.PolicyName;
            if (!string.IsNullOrEmpty(request.Description)) policy.Description = request.Description;

            await _unitOfWork.ConferencePolicyRepository.UpdateConferencePolicyAsync(policy);
            return policy.ToResponse();
        }

        public async Task<bool> DeleteConferencePolicyAsync(string policyId, string userId)
        {
            var policy = await _unitOfWork.ConferencePolicyRepository.GetConferencePolicyByIdAsync(policyId);
            if (policy == null) throw new NotFoundException($"Conference policy with ID {policyId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(policy.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm thấy hội nghị nào có policy với ID {policyId}");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            return await _unitOfWork.ConferencePolicyRepository.DeleteConferencePolicyAsync(policy) > 0;
        }

        #endregion

        #region Step 5: Media

        public async Task<List<ConferenceMediaResponse>> AddConferenceMediaAsync(string conferenceId, AddConferenceMediaRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (!request.Media.Any())
                throw new Exception("Cần phải có ít nhất một media");

            var responses = new List<ConferenceMediaResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var media in request.Media)
                {
                    string? mediaUrl = media.MediaUrl;
                    if (media.MediaFile != null)
                    {
                        if (!_objectStorageFileService.IsValidVideoFile(media.MediaFile) && !_objectStorageFileService.IsValidImageFile(media.MediaFile))
                            throw new Exception($"Không hỗ trợ định dạng {media.MediaFile.ContentType}");
                        using var stream = media.MediaFile.OpenReadStream();
                        var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(media.MediaFile.FileName);
                        mediaUrl = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, media.MediaFile.ContentType);
                    }
                    var conferenceMedia = new ConferenceMedium { ConferenceMediaId = Guid.NewGuid().ToString(), ConferenceMediaUrl = mediaUrl, ConferenceId = conferenceId, };
                    await _unitOfWork.ConferenceMediaRepository.CreateConferenceMediaAsync(conferenceMedia);
                    responses.Add(new ConferenceMediaResponse { MediaId = conferenceMedia.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(conferenceMedia.ConferenceMediaUrl) });
                }
                await _unitOfWork.CommitAsync();
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackAsync();
            }
            return responses;
        }

        public async Task<List<ConferenceMediaResponse>> GetConferenceMediaAsync(string conferenceId)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetMediaByConferenceIdAsync(conferenceId);
            return media.Select(m => new ConferenceMediaResponse { MediaId = m.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(m.ConferenceMediaUrl) }).ToList();
        }

        public async Task<ConferenceMediaResponse> UpdateConferenceMediaAsync(string mediaId, UpdateConferenceMediaRequest request, string userId)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetConferenceMediaByIdAsync(mediaId);
            if (media == null) throw new NotFoundException($"Conference media with ID {mediaId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(media.ConferenceId);
            if (conference == null)
            {
                throw new Exception($"Không tìm thấy conference của conference media với Id {media.ConferenceMediaId}");
            }

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (request.MediaFile != null)
            {
                if (!_objectStorageFileService.IsValidVideoFile(request.MediaFile) && !_objectStorageFileService.IsValidImageFile(request.MediaFile))
                    throw new Exception($"Không hỗ trợ định dạng {request.MediaFile.ContentType}");


                using var stream = request.MediaFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.MediaFile.FileName);
                media.ConferenceMediaUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, request.MediaFile.ContentType);
                media.ConferenceMediaUrl = _objectStorageSettings.EndPoint + media.ConferenceMediaUrl;
            }
            else if (!string.IsNullOrEmpty(request.MediaUrl))
            {
                media.ConferenceMediaUrl = request.MediaUrl;
            }

            //media.MediaTypeId = request.MediaTypeId ?? media.MediaTypeId;
            await _unitOfWork.ConferenceMediaRepository.UpdateConferenceMediaAsync(media);
            return new ConferenceMediaResponse { MediaId = media.ConferenceMediaId, MediaUrl = media.ConferenceMediaUrl };
        }

        public async Task<bool> DeleteConferenceMediaAsync(string mediaId, string userId)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetConferenceMediaByIdAsync(mediaId);
            if (media == null) throw new NotFoundException($"Conference media with ID {mediaId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(media.ConferenceId);
            if (conference == null)
            {
                throw new Exception($"Không tìm thấy conference của media với Id {mediaId}");
            }

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");


            return await _unitOfWork.ConferenceMediaRepository.DeleteConferenceMediaAsync(media) > 0;
        }

        #endregion

        #region Step 6: Sponsors

        public async Task<List<SponsorResponse>> AddConferenceSponsorsAsync(string conferenceId, AddConferenceSponsorsRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            var responses = new List<SponsorResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Sponsors != null)
                {
                    foreach (var sponsor in request.Sponsors)
                    {
                        if (!_objectStorageFileService.IsValidImageFile(sponsor.ImageFile))
                            throw new BadRequestException($"Không hỗ trợ {sponsor.ImageFile.ContentType}");
                        string? imageUrl = sponsor.ImageUrl;
                        if (sponsor.ImageFile != null)
                        {
                            using var stream = sponsor.ImageFile.OpenReadStream();
                            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(sponsor.ImageFile.FileName);
                            imageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.sponsorimage.ToString(), uniqueFileName, stream, sponsor.ImageFile.ContentType);
                            imageUrl = _objectStorageSettings.EndPoint + imageUrl;
                        }

                        var conferenceSponsor = sponsor.ToModel(conferenceId);
                        conferenceSponsor.ImageUrl = imageUrl;

                        await _unitOfWork.SponsorRepository.CreateSponsorAsync(conferenceSponsor);
                        responses.Add(conferenceSponsor.ToResponse());
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<SponsorResponse>> GetConferenceSponsorsAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conferenceId);
            return sponsors.Select(s => s.ToResponse()).ToList();
        }

        public async Task<SponsorResponse> UpdateSponsorAsync(string sponsorId, UpdateSponsorRequest request, string userId)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetSponsorByIdAsync(sponsorId);
            if (sponsor == null) throw new NotFoundException($"Conference sponsor with ID {sponsorId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(sponsor.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho sponsor với ID {sponsorId}");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (!string.IsNullOrEmpty(request.Name)) sponsor.Name = request.Name;

            if (request.ImageFile != null)
            {
                if (!_objectStorageFileService.IsValidImageFile(request.ImageFile)) throw new Exception($"Không hỗ trợ định dạng {request.ImageFile.ContentType} ");
                using var stream = request.ImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.ImageFile.FileName);
                sponsor.ImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.sponsorimage.ToString(), uniqueFileName, stream, request.ImageFile.ContentType);
                sponsor.ImageUrl = _objectStorageSettings.EndPoint + sponsor.ImageUrl;
            }
            string name = request.Name ?? sponsor.Name;
            sponsor.Name = name;

            await _unitOfWork.SponsorRepository.UpdateSponsorAsync(sponsor);
            return sponsor.ToResponse();
        }

        public async Task<bool> DeleteSponsorAsync(string sponsorId, string userId)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetSponsorByIdAsync(sponsorId);
            if (sponsor == null) throw new NotFoundException($"Conference sponsor with ID {sponsorId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(sponsor.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho sponsor với ID {sponsorId}");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            return await _unitOfWork.SponsorRepository.DeleteSponsorAsync(sponsor) > 0;
        }

        #endregion

        #region Step 7: Refund Policies

        public async Task<List<RefundPolicyResponse>> AddRefundPoliciesAsync(string confId, string pricePhaseId, AddRefundPoliciesRequest request, string userId)
        {
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(pricePhaseId);
            if (pricePhase == null) throw new NotFoundException($"Không tìm th?y giai do?n bán vé (PricePhase) v?i ID {pricePhaseId}.");

            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(pricePhase.ConferencePriceId);
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferencePrice.ConferenceId);

            #region Xác th?c
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n thêm chính sách hoàn ti?n cho giai do?n này.");
            await EnsureConferenceIsEditable(conference);

            if (request.RefundPolicies == null || !request.RefundPolicies.Any())
                throw new BadRequestException("Yêu c?u ph?i ch?a ít nh?t m?t chính sách hoàn ti?n.");

            DateOnly today = await _timeProviderService.GetVietnamDate();
            var existingPolicies = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByPricePhaseId(pricePhaseId);
            var existingDeadlines = new HashSet<DateOnly>(existingPolicies.Select(p => p.RefundDeadline.Value));

            foreach (var policy in request.RefundPolicies)
            {
                if (!policy.PercentRefund.HasValue || !policy.RefundDeadline.HasValue)
                    throw new BadRequestException("Chính sách hoàn tiền phải có d? ph?n tram và h?n chót.");
                if (policy.PercentRefund < 0 || policy.PercentRefund > 100)
                    throw new BadRequestException("Ph?n tram hoàn ti?n ph?i n?m trong kho?ng t? 0 d?n 100.");
                if (policy.RefundDeadline.Value <= today)
                    throw new BadRequestException("H?n chót hoàn ti?n ph?i là m?t ngày trong tuong lai.");
                if (pricePhase.ConferencePrice.IsAuthor == false)
                {
                    // Validation quan tr?ng: Deadline hoàn ti?n ph?i TRU?C ngày b?t d?u c?a phase
                    if (policy.RefundDeadline.Value < pricePhase.StartDate)
                    {
                        throw new BadRequestException($"Hoàn chót hoàn tiền cho vé hội nghị  ({policy.RefundDeadline.Value:dd/MM/yyyy}) phải sau ngày bắt đầu củaa giai đoạn bán vé ({pricePhase.StartDate:dd/MM/yyyy}).");
                    }

                    if (policy.RefundDeadline.Value > conference.TicketSaleEnd)
                        throw new BadRequestException($"Hạn chót hoàn toàn tiền vé hội nghị  {policy.RefundDeadline.Value:dd/MM/yyyy} phải trước conference ticketsaleend {conference.TicketSaleEnd.Value:dd/MM/yyyy}");
                }
                else
                {
                    var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(pricePhase.ResearchConferencePhaseId);
                    if (researchPhase == null)
                        throw new Exception($"Không tìm ra researPhase cho conference price của price phase {pricePhaseId}");
                    if (policy.RefundDeadline.Value < pricePhase.StartDate)
                    {
                        throw new BadRequestException($"Hạn chót hoàn tiền cho vé hội nghị  ({policy.RefundDeadline.Value:dd/MM/yyyy}) phải sau ngày bắt đầu củaa giai đoạn bán vé ({pricePhase.StartDate:dd/MM/yyyy}).");
                    }

                    if (policy.RefundDeadline.Value > researchPhase.RegistrationEndDate)
                        throw new BadRequestException($"Hạn chót hoàn toàn tiền vé hội nghị  {policy.RefundDeadline.Value:dd/MM/yyyy} phải trước registration emd {researchPhase.RegistrationEndDate.Value:dd/MM/yyyy}");
                }


                if (!existingDeadlines.Add(policy.RefundDeadline.Value))
                    throw new BadRequestException($"Hạn chót hoàn ti?n '{policy.RefundDeadline.Value:dd/MM/yyyy}' dã t?n t?i trong giai do?n này.");
            }
            #endregion

            var responses = new List<RefundPolicyResponse>();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // S?p x?p các policy m?i d? gán RefundOrder
                var sortedNewPolicies = request.RefundPolicies.OrderBy(p => p.RefundDeadline).ToList();
                for (int i = 0; i < sortedNewPolicies.Count; i++)
                {
                    var newPolicyRequest = sortedNewPolicies[i];
                    var refundPolicyModel = new RefundPolicy
                    {
                        RefundPolicyId = Guid.NewGuid().ToString(),
                        PricePhaseId = pricePhaseId, // G?n v?i PricePhaseId
                        PercentRefund = newPolicyRequest.PercentRefund.Value,
                        RefundDeadline = newPolicyRequest.RefundDeadline.Value,
                        ConferenceId = confId,
                        RefundOrder = i + 1 // T? d?ng gán th? t? trong phase
                    };
                    await _unitOfWork.ConferenceRefundPolicyRepository.CreateConferenceRefundPolicyAsync(refundPolicyModel);
                    responses.Add(refundPolicyModel.ToResponse());
                }
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }

            // Tr? v? danh sách dã du?c s?p x?p
            return responses.OrderBy(r => r.RefundOrder).ToList();
        }
        public async Task<List<RefundPolicyResponse>> GetRefundPoliciesAsync(string pricephaseId)
        {
            var pricephase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(pricephaseId);
            if (pricephase == null)
            {
                throw new NotFoundException($"Không tìm th?y pricephase v?i ID {pricephaseId}");
            }

            var policiesFromDb = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByPricePhaseId(pricephaseId);

            // S?p x?p các chính sách theo h?n chót tang d?n
            // Sau dó, dùng phuong th?c Select có index d? t?o ra RefundOrder
            var sortedAndOrderedPolicies = policiesFromDb
                .OrderBy(p => p.RefundDeadline)
                .Select((policy, index) => new RefundPolicyResponse
                {
                    RefundPolicyId = policy.RefundPolicyId,
                    PercentRefund = policy.PercentRefund,
                    RefundDeadline = policy.RefundDeadline,
                    RefundOrder = index + 1 // Gán th? t?: index b?t d?u t? 0, nên c?n +1
                })
                .ToList();

            return sortedAndOrderedPolicies;
        }

        public async Task<RefundPolicyResponse> UpdateRefundPolicyAsync(string refundPolicyId, UpdateRefundPolicyRequest request, string userId)
        {
            var refundPolicy = await _unitOfWork.ConferenceRefundPolicyRepository.GetConferenceRefundPolicyByIdAsync(refundPolicyId);
            if (refundPolicy == null) throw new NotFoundException($"Không tìm th?y chính sách hoàn ti?n v?i ID {refundPolicyId}");

            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(refundPolicy.PricePhaseId);
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(pricePhase.ConferencePriceId);
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferencePrice.ConferenceId);

            #region Xác th?c
            if (conference.CreatedBy != userId) throw new ForbiddenException("B?n không có quy?n c?p nh?t chính sách này.");
            await EnsureConferenceIsEditable(conference);

            if (request.PercentRefund.HasValue && (request.PercentRefund < 0 || request.PercentRefund > 100))
                throw new BadRequestException("Ph?n tram hoàn ti?n ph?i n?m trong kho?ng t? 0 d?n 100.");

            if (request.RefundDeadline.HasValue)
            {
                if (request.RefundDeadline.Value <= await _timeProviderService.GetVietnamDate())
                    throw new BadRequestException("H?n chót hoàn ti?n ph?i là m?t ngày trong tuong lai.");
                if (request.RefundDeadline.Value < pricePhase.StartDate)
                    throw new BadRequestException($"H?n chót hoàn ti?n ({request.RefundDeadline.Value:dd/MM/yyyy}) ph?i sau ngày b?t d?u c?a giai do?n ({pricePhase.StartDate:dd/MM/yyyy}).");

                if (request.RefundDeadline.Value > conference.TicketSaleEnd)
                    throw new BadRequestException($"H?n chót hoàn toàn ti?n {request.RefundDeadline.Value:dd/MM/yyyy} ph?i tru?c conference ticketsaleend {conference.TicketSaleEnd.Value:dd/MM/yyyy}");


                var allPoliciesInPhase = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByPricePhaseId(pricePhase.PricePhaseId);
                if (allPoliciesInPhase.Any(p => p.RefundDeadline == request.RefundDeadline.Value && p.RefundPolicyId != refundPolicyId))
                    throw new BadRequestException($"H?n chót hoàn ti?n '{request.RefundDeadline.Value:dd/MM/yyyy}' dã t?n t?i trong giai do?n này.");
            }
            #endregion

            refundPolicy.PercentRefund = request.PercentRefund ?? refundPolicy.PercentRefund;
            refundPolicy.RefundDeadline = request.RefundDeadline ?? refundPolicy.RefundDeadline;

            await _unitOfWork.ConferenceRefundPolicyRepository.UpdateConferenceRefundPolicyAsync(refundPolicy);

            // N?u deadline thay d?i, th? t? có th? thay d?i. C?n tính toán l?i.
            var allPolicies = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByPricePhaseId(pricePhase.PricePhaseId);
            var sortedPolicies = allPolicies.OrderBy(p => p.RefundDeadline).ToList();
            for (int i = 0; i < sortedPolicies.Count; i++)
            {
                sortedPolicies[i].RefundOrder = i + 1;
                await _unitOfWork.ConferenceRefundPolicyRepository.UpdateConferenceRefundPolicyAsync(sortedPolicies[i]);
            }

            // Gán l?i order cho d?i tu?ng v?a c?p nh?t d? tr? v?
            refundPolicy.RefundOrder = sortedPolicies.First(p => p.RefundPolicyId == refundPolicyId).RefundOrder;

            return refundPolicy.ToResponse();
        }

        public async Task<bool> DeleteRefundPolicyAsync(string refundPolicyId, string userId)
        {
            var refundPolicy = await _unitOfWork.ConferenceRefundPolicyRepository.GetConferenceRefundPolicyByIdAsync(refundPolicyId);
            if (refundPolicy == null)
            {
                return true;
            }

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(refundPolicy.ConferenceId);
            if (conference == null)
            {
                // Tru?ng h?p hi?m g?p, nhung nên x? lý d? tránh l?i không mong mu?n
                throw new NotFoundException("Không tìm th?y h?i ngh? liên quan d?n chính sách này.");
            }

            #region Xác th?c

            // 1. Phân quy?n
            if (conference.CreatedBy != userId)
            {
                throw new ForbiddenException("B?n không có quy?n xóa chính sách hoàn ti?n này.");
            }

            // 2. Xác th?c tr?ng thái h?i ngh? (s? d?ng helper dã có)
            await EnsureConferenceIsEditable(conference);

            #endregion

            // Th?c hi?n xóa và tr? v? k?t qu?
            return await _unitOfWork.ConferenceRefundPolicyRepository.DeleteConferenceRefundPolicyAsync(refundPolicy) > 0;
        }

        #endregion

        #region Research Conference Step 1: Basic Research Conference

        public async Task<ResearchConferenceBasicStepResponse> CreateResearchConferenceBasicAsync(CreateResearchConferenceBasicRequest request, string userid)
        {
            if (!request.IsResearchConference.HasValue || !request.IsResearchConference.Value)
                throw new BadRequestException("Phải là hội nghị học thuật và giá trị IsResearchConference phải bằng true");
            var category = await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId);
            if (category == null)
            {
                throw new Exception($"Category {request.ConferenceCategoryId} does not exist");
            }

            if (string.IsNullOrEmpty(request.ConferenceName))
                throw new BadRequestException("Tên không thể để trống");

            if (request.BannerImageFile == null)
                throw new BadRequestException("Cần phải có banner ảnh");

            if (!_objectStorageFileService.IsValidImageFile(request.BannerImageFile)) 
                throw new BadRequestException($"Banner ?nh không h? tr? extension{request.BannerImageFile.ContentType}");

            if (await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId) == null)
                throw new NotFoundException($"Danh mục hội nghị với ID '{request.ConferenceCategoryId}' không tồn tại.");
            if (await _unitOfWork.CityRepository.GetCityByIdAsync(request.CityId) == null)
                throw new NotFoundException($"Thành phố với ID '{request.CityId}' không tồn tại.");

            

            //Must be research conference


            //Must be internally hosted
            if (!request.IsInternalHosted.HasValue || !request.IsInternalHosted.Value)
                throw new BadRequestException("Hội nghị nghiên cứu phải được tổ chức bởi người thuộc ConfRadar");

            const long maxBannerSize = 5 * 1024 * 1024; // 5 MB
            if (request.BannerImageFile.Length > maxBannerSize)
                throw new BadRequestException("Kích thước tệp ảnh bìa không được vượt quá 5 MB.");

            var isValidDateValues = IsValidConferenceAndTicketSaleDates(request.StartDate, request.EndDate, request.TicketSaleStart, request.TicketSaleEnd);
            if (!isValidDateValues.Result)
                throw new BadRequestException("Ngày mở bán vé phải trước ngày conference diễn ra và tất cả phải trước hôm nay");

            if (request.TotalSlot <= 0)
                throw new Exception("Total slot must be positive");

            using var stream = request.BannerImageFile.OpenReadStream();
            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
            request.bannerImageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
            request.bannerImageFileUrl = _objectStorageSettings.EndPoint + request.bannerImageFileUrl;

           
            await _unitOfWork.BeginTransactionAsync();
            try
            {
               



                Conference toBeCreatedConference;
                var confStatus = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
                toBeCreatedConference = request.ToModel(confStatus.Where(s => s.ConferenceStatusName == "Preparing").FirstOrDefault(), await _timeProviderService.GetVietnamTime());

                await _unitOfWork.ConferenceRepository.CreateConferenceAsync(toBeCreatedConference);
                // Note: No TechnicalConferenceDetail for research conference

                await _unitOfWork.CommitAsync();
                return await GetResearchConferenceBasicAsync(toBeCreatedConference.ConferenceId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ResearchConferenceBasicStepResponse> GetResearchConferenceBasicAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            return conference.ToResearchResponse();
        }

        public async Task<ResearchConferenceBasicStepResponse> UpdateResearchConferenceBasicAsync(string conferenceId, UpdateResearchConferenceBasicRequest request,string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            if (!string.IsNullOrWhiteSpace(request.ConferenceCategoryId) && request.ConferenceCategoryId != conference.ConferenceCategoryId)
            {
                if (await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId) == null)
                {
                    // N?u Category ID m?i không t?n t?i, báo l?i NGAY L?P T?C
                    throw new BadRequestException($"Danh mục hội nghị với ID '{request.ConferenceCategoryId}' không tồn tại.");
                }
            }

            if (conference.CreatedBy != userId)
                throw new BadRequestException("Bạn không có quyền cập nhật hội nghị này");

            if (!string.IsNullOrWhiteSpace(request.CityId) && request.CityId != conference.CityId)
            {
                if (await _unitOfWork.CityRepository.GetCityByIdAsync(request.CityId) == null)
                {
                    throw new NotFoundException($"Thành phố với ID '{request.CityId}' không tồn tại.");
                }
            }

            if (request.BannerImageFile != null && !_objectStorageFileService.IsValidImageFile(request.BannerImageFile))
                throw new BadRequestException("Định dạng ảnh bìa không được hỗ trợ.");

            await EnsureConferenceIsEditable(conference);
            if (conference.IsResearchConference != true)
                throw new Exception("Phải là conference research mới update bằng endpoint này được");

            var finalStartDate = request.StartDate ?? conference.StartDate;
            var finalEndDate = request.EndDate ?? conference.EndDate;
            var finalTicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
            var finalTicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;
            if (finalStartDate.HasValue && finalEndDate.HasValue && finalTicketSaleStart.HasValue && finalTicketSaleEnd.HasValue)
            {
                if (!IsValidConferenceAndTicketSaleDates(finalStartDate.Value, finalEndDate.Value, finalTicketSaleStart.Value, finalTicketSaleEnd.Value).Result)
                    throw new BadRequestException("Ngày tháng cung cấp không hợp lệ.");
            }

            var Waitlist = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseIsWaitListByConferenceIdAsync(conferenceId);
            if (Waitlist != null && Waitlist.CameraReadyEndDate > finalTicketSaleStart)
            {
                throw new BadRequestException("TicketSaleStart phải diễn ra sau Waitlist của hội nghị");
            }

            conference.ConferenceName = request.ConferenceName ?? conference.ConferenceName;
            conference.Description = request.Description ?? conference.Description;
            conference.StartDate = request.StartDate;  // Fixed nullable DateOnly
            conference.EndDate = request.EndDate;         // Fixed nullable DateOnly
            conference.TotalSlot = request.TotalSlot ?? conference.TotalSlot;
            conference.AvailableSlot = request.TotalSlot ?? conference.AvailableSlot; 
            conference.Address = request.Address ?? conference.Address;
            conference.ConferenceCategoryId = request.ConferenceCategoryId ?? conference.ConferenceCategoryId;
            conference.CityId = request.CityId ?? conference.CityId;
            conference.TicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
            conference.TicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;

            if (request.BannerImageFile != null)
            {
                using var stream = request.BannerImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                conference.BannerImageUrl = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
            }

            await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);
            return await GetResearchConferenceBasicAsync(conferenceId);
        }

        #endregion

        #region Research Conference Step 2: Research Conference Detail

        public async Task<ResearchConferenceDetailResponse> CreateResearchConferenceDetailAsync(string conferenceId, CreateResearchConferenceDetailRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Không tìm th?y h?i ngh? v?i ID {conferenceId}");
            }

            // 1. Phân quy?n
            if (conference.CreatedBy != userId)
            {
                throw new BadRequestException("B?n không có quy?n thêm chi ti?t cho h?i ngh? này.");
            }

            if (request.NumberPaperAccept <= 0)
                throw new BadRequestException("Số lượng bài báo nhận vào phải lớn hơn 0");

            if (request.RevisionAttemptAllowed == null || request.RevisionAttemptAllowed <= 0)
                throw new BadRequestException("Số vòng revision phải lớn hơn 0");


            if (request.ReviewFee < 0)
                throw new BadRequestException("Review fee phải là số dương");


            // 4. Ð?m b?o chi ti?t này chua du?c t?o tru?c dó (m?i h?i ngh? ch? có 1)
            var existingDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (existingDetail != null)
            {
                throw new BadRequestException("Chi ti?t nghiên c?u cho h?i ngh? này dã t?n t?i.");
            }

            if (conference.IsResearchConference != true)
            {
                throw new BadRequestException("Ch? có th? thêm chi ti?t nghiên c?u cho m?t h?i ngh? lo?i 'nghiên c?u'.");
            }

         



            // 6. Xác th?c s? t?n t?i c?a RankingCategoryId (d?a trên hình ?nh b?n cung c?p)
            await ValidatePaperFormat(request.PaperFormat);
            await ValidateRankValueAsync(request.RankingCategoryId, request.RankValue);
            if (conference.TotalSlot < request.NumberPaperAccept)
                throw new Exception($"Không thể có số bài báo có thể nhận lớn hơn totalslot của toàn hội nghị (numberPaperAccept{request.NumberPaperAccept} > conference totalslot{conference.TotalSlot})");

            // 7. Xác th?c nam x?p h?ng h?p l?
            if (request.RankYear.HasValue)
            {
                int currentYear = DateTime.Now.Year;
                if (request.RankYear.Value < currentYear - 20 || request.RankYear.Value > currentYear + 5)
                {
                    throw new BadRequestException($"Nam x?p h?ng '{request.RankYear.Value}' không h?p l?.");
                }
            }

            // 2. Xác th?c tr?ng thái h?i ngh?
            await EnsureConferenceIsEditable(conference);

            // 3. Ð?m b?o dây là m?t h?i ngh? nghiên c?u

            var researchDetail = request.ToModel(conferenceId);

            await _unitOfWork.ResearchConferenceDetailRepository.CreateResearchConferenceDetailAsync(researchDetail);
            return await GetResearchConferenceDetailAsync(conferenceId);
        }

        public async Task<ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId)
        {
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (researchDetail == null) throw new NotFoundException($"Research conference detail for conference ID {conferenceId} not found");

            return researchDetail.ToResponse();
        }

        public async Task<ResearchConferenceDetailResponse> UpdateResearchConferenceDetailAsync(string conferenceId, UpdateResearchConferenceDetailRequest request, string userId)
        {
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (researchDetail == null) throw new NotFoundException($"Không tìm th?y chi ti?t nghiên c?u cho h?i ngh? ID {conferenceId}.");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(researchDetail.ConferenceId);


            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n c?p nh?t chi ti?t cho h?i ngh? này.");

            var finalRankingCategoryId = request.RankingCategoryId ?? researchDetail.RankingCategoryId;
            var finalRankValue = request.RankValue ?? researchDetail.RankValue;

            // N?u RankingCategoryId du?c thay d?i, xác th?c s? t?n t?i c?a ID m?i
            if (request.RankingCategoryId != null && request.RankingCategoryId != researchDetail.RankingCategoryId)
            {
                if (await _unitOfWork.RankingCategoryRepository.GetRankingCategoryByIdAsync(request.RankingCategoryId) == null)
                    throw new NotFoundException($"Lo?i x?p h?ng v?i ID '{request.RankingCategoryId}' không t?n t?i.");
            }
            if (request.NumberPaperAccept.HasValue)
                if (conference.TotalSlot < request.NumberPaperAccept)
                    throw new Exception($"Không thể có số bài báo có thể nhận lớn hơn totalslot của toàn hội nghị (numberPaperAccept{request.NumberPaperAccept} > conference totalslot{conference.TotalSlot})");
            // *** G?I VALIDATION Ð?NG M?I ***
            // Luôn g?i v?i các giá tr? cu?i cùng d? d?m b?o tính nh?t quán
            if (request.PaperFormat != null)
            {
                await ValidatePaperFormat(request.PaperFormat);
            }
            await ValidateRankValueAsync(finalRankingCategoryId, finalRankValue);
            await EnsureConferenceIsEditable(conference);


            researchDetail.PaperFormat = request.PaperFormat ?? researchDetail.PaperFormat;
            researchDetail.NumberPaperAccept = request.NumberPaperAccept ?? researchDetail.NumberPaperAccept;
            researchDetail.RevisionAttemptAllowed = request.RevisionAttemptAllowed ?? researchDetail.RevisionAttemptAllowed;
            researchDetail.RankingDescription = request.RankingDescription ?? researchDetail.RankingDescription;
            researchDetail.AllowListener = request.AllowListener ?? researchDetail.AllowListener;
            researchDetail.RankValue = request.RankValue ?? researchDetail.RankValue;
            researchDetail.RankYear = request.RankYear ?? researchDetail.RankYear;
            researchDetail.ReviewFee = request.ReviewFee ?? researchDetail.ReviewFee;
            researchDetail.RankingCategoryId = request.RankingCategoryId ?? researchDetail.RankingCategoryId;

            await _unitOfWork.ResearchConferenceDetailRepository.UpdateResearchConferenceDetailAsync(researchDetail);
            return await GetResearchConferenceDetailAsync(conferenceId);
        }

        #endregion

        #region Research Conference Step 3: Research Conference Phases

        public async Task<CreatePhasesResponse> CreateResearchConferencePhaseAsync(string conferenceId, CreateResearchConferencePhasesRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Không tìm thấy hội nghị với ID {conferenceId}");

            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (researchDetail == null) throw new BadRequestException("Hội nghị này chưa có chi tiết nghiên cứu (Research Detail).");


            // 1. Phân quy?n, tr?ng thái, và lo?i h?i ngh?
            if (conference.CreatedBy != userId)
                throw new BadRequestException("Bạn không có quyền thực hiện thao tác này.");
            await EnsureConferenceIsEditable(conference);
            if (conference.IsResearchConference != true)
                throw new BadRequestException("Chức năng này chỉ dành cho hội nghị nghiên cứu.");

            // 2. Ki?m tra xem h?i ngh? dã có phase nào chua (ch? cho phép t?o m?t l?n)
            var existingPhases = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conferenceId);
            if (existingPhases.Any())
                throw new BadRequestException("Hội nghị này đã có các giai đoạn (phase). Vui lòng sử dụng chức năng cập nhật.");

            if (request.Phases == null)
                throw new BadRequestException("Phải có phase để thực hiện ");

            // 3. Validation logic cho danh sách các phase t? request
            var newPhases = request.Phases.OrderBy(p => p.RegistrationStartDate).ToList();
            // 3a. Ph?i có dúng M?T phase chính (IsWaitlist = false) 
            if (newPhases.Count(p => p.IsWaitlist == false) != 1)
                throw new BadRequestException("Yêu cầu phải có chính xác một phase chính (IsWaitlist = false).");

            // 3b. Phase d?u tiên ph?i là phase chính
            if (newPhases.First().IsWaitlist == true)
                throw new BadRequestException("Phase đầu tiên (địa theo ngày bắt đầu) phải là phase chính.");


            // 3c. Ph?i có ít nh?t M?T phase waitlist
            if (!newPhases.Any(p => p.IsWaitlist == true))
                throw new BadRequestException("Yêu cầu phải có ít nhất một phase dự phòng (IsWaitlist = true).");

            var requestWaitlist = request.Phases.FirstOrDefault(p => p.IsWaitlist == true);
            var requestNotWaitlist = request.Phases.FirstOrDefault(p => p.IsWaitlist == false);
            if (newPhases.First().IsWaitlist == true) throw new BadRequestException("Phase đâu tiên phải là phase chính.");

            if (newPhases.Count != 2) throw new BadRequestException("Phải có chính xác 2 phase 1 cho chính thức và 1 cho waitlist");
            // 4. Validation logic cho ngày tháng (tu?n t? và h?p l?)
            DateOnly? lastPhaseEndDate = null;
            foreach (var phase in newPhases)
            {
                //// 4a. Các m?c th?i gian trong cùng m?t phase ph?i tu?n t?
                if (phase.RegistrationStartDate > phase.RegistrationEndDate ||
                    phase.RegistrationEndDate > phase.AbstractDecideStatusStart ||
                    phase.AbstractDecideStatusStart > phase.AbstractDecideStatusEnd ||
                    phase.AbstractDecideStatusEnd > phase.FullPaperStartDate ||
                    phase.FullPaperStartDate > phase.FullPaperEndDate ||
                    phase.FullPaperEndDate > phase.ReviewStartDate ||
                    phase.ReviewStartDate > phase.ReviewEndDate ||
                    phase.ReviewEndDate > phase.FullPaperDecideStatusStart ||
                    phase.FullPaperDecideStatusStart > phase.FullPaperDecideStatusEnd ||
                    phase.FullPaperDecideStatusEnd > phase.ReviseStartDate ||
                    phase.ReviseStartDate > phase.ReviseEndDate ||
                    phase.ReviseEndDate > phase.RevisionPaperDecideStatusStart ||
                    phase.RevisionPaperDecideStatusStart > phase.RevisionPaperDecideStatusEnd ||
                    phase.RevisionPaperDecideStatusEnd > phase.CameraReadyStartDate ||
                    phase.CameraReadyStartDate > phase.CameraReadyEndDate ||
                    phase.CameraReadyEndDate > phase.CameraReadyDecideStatusStart ||
                    phase.CameraReadyDecideStatusStart > phase.CameraReadyDecideStatusEnd
                    )
                {
                    throw new BadRequestException("Các mốc thời gian trong một phase không theo dúng thứ tự.");
                }

                // 4b. Các phase ph?i di?n ra n?i ti?p nhau, không du?c g?i lên nhau
                if (lastPhaseEndDate.HasValue && phase.RegistrationStartDate <= lastPhaseEndDate)
                {
                    throw new BadRequestException($"Ngày bắt đầu của một phase phải sau ngày kết thúc của phase trước đó. Cụ thể ngày kết thúc phase liền trước {lastPhaseEndDate.Value} > {phase.RegistrationStartDate.Value} ngày bắt đầu phase liền sau là sai");
                }
                lastPhaseEndDate = phase.CameraReadyEndDate;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var createdPhaseIds = new List<string>();
                for (int i = 0; i < newPhases.Count; i++)
                {
                    var phaseRequest = newPhases[i];

                    #region === 2. VALIDATION RIÊNG CHO T?NG PHASE (VÀ REVISION DEADLINES) ===

                    // Ch? phase chính m?i c?n ki?m tra và t?o Revision Deadlines
                    if (phaseRequest.IsWaitlist == false)
                    {
                        var deadlines = phaseRequest.RevisionRoundDeadlines;
                        int allowedAttempts = researchDetail.RevisionAttemptAllowed ?? 0;

                        // 2a. S? lu?ng deadline ph?i kh?p chính xác v?i s? l?n cho phép
                        if (deadlines == null || deadlines.Count != allowedAttempts)
                        {
                            throw new BadRequestException($"Phase chính phải có chính xác {allowedAttempts} Revision Deadline(s), nhưng nhận được {deadlines?.Count ?? 0}.");
                        }

                        // 2b. S?p x?p và ki?m tra tu?n t?, ch?ng chéo cho các deadline
                        var sortedDeadlines = deadlines.OrderBy(d => d.StartSubmissionDate).ToList();
                        DateOnly? lastEndDate = null;
                        foreach (var deadline in sortedDeadlines)
                        {
                            if (deadline.StartSubmissionDate >= deadline.EndSubmissionDate)
                                throw new BadRequestException($"Trong Revision Deadline, ngày bắt đầu ({deadline.StartSubmissionDate:dd/MM/yyyy}) phải trước ngày kết thúc ({deadline.EndSubmissionDate:dd/MM/yyyy}).");

                            // Kho?ng th?i gian c?a deadline ph?i n?m trong kho?ng Revise c?a Phase
                            if (deadline.StartSubmissionDate < phaseRequest.ReviseStartDate || deadline.EndSubmissionDate > phaseRequest.ReviseEndDate)
                                throw new BadRequestException($"Revision Deadline ({deadline.StartSubmissionDate:dd/MM/yyyy} - {deadline.EndSubmissionDate:dd/MM/yyyy}) phải nằm trong giai đoạn sửa đổi của phase ({phaseRequest.ReviseStartDate:dd/MM/yyyy} - {phaseRequest.ReviseEndDate:dd/MM/yyyy}).");

                            if (lastEndDate.HasValue && deadline.StartSubmissionDate <= lastEndDate)
                                throw new BadRequestException("Các Revision Deadline không được chồng chéo lên nhau.");

                            lastEndDate = deadline.EndSubmissionDate;
                        }
                    }
                    #endregion

                    #region === 3. TH?C THI ===
                    var phaseModel = phaseRequest.ToModel(conferenceId);
                    phaseModel.IsActive = (i == 0); // Phase chính active
                    await _unitOfWork.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(phaseModel);
                    createdPhaseIds.Add(phaseModel.ResearchConferencePhaseId);

                    // T?o các RevisionRoundDeadline n?u có
                    if (phaseRequest.RevisionRoundDeadlines != null && phaseRequest.RevisionRoundDeadlines.Any())
                    {
                        var sortedDeadlines = phaseRequest.RevisionRoundDeadlines.OrderBy(d => d.StartSubmissionDate).ToList();
                        for (int j = 0; j < sortedDeadlines.Count; j++)
                        {
                            var deadlineRequest = sortedDeadlines[j];
                            var revisionRoundDeadline = new RevisionRoundDeadline
                            {
                                RevisionRoundDeadlineId = Guid.NewGuid().ToString(),
                                ResearchConferencePhaseId = phaseModel.ResearchConferencePhaseId,
                                StartSubmissionDate = deadlineRequest.StartSubmissionDate,
                                EndSubmissionDate = deadlineRequest.EndSubmissionDate,
                                RoundNumber = j + 1 // Gán Round Number t? d?ng
                            };
                            await _unitOfWork.RevisionRoundDeadlineRepository.CreateCsAsync(revisionRoundDeadline);
                        }
                    }
                    #endregion
                }
                await _unitOfWork.CommitAsync();
                return new CreatePhasesResponse
                {
                    CreatedPhaseIds = createdPhaseIds,
                    Message = "T?o các giai đoạn cho hội nghị thành công.",
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
        }

        public async Task<ResearchConferencePhaseResponse> GetResearchConferencePhaseAsync(string conferenceId)
        {
            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(conferenceId);
            if (phase == null) throw new NotFoundException($"Research conference phase for conference ID {conferenceId} not found");

            return phase.ToResponse();
        }



        public async Task<ResearchConferencePhaseResponse> UpdateResearchConferencePhaseAsync(string phaseId, UpdateResearchConferencePhaseRequest request, string userId)
        {
            // BU?C 1: L?y d? li?u m?t cách chính xác
            var phaseToUpdate = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(phaseId);
            if (phaseToUpdate == null) throw new NotFoundException($"Không tìm thấy giai đoạn (phase) với ID {phaseId}");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(phaseToUpdate.ConferenceId);
            if (conference == null) throw new BadRequestException("Không tìm thấy hội nghị cho phase này.");

            #region === 2. VALIDATION ===
            // 2.1. Phân quy?n và tr?ng thái h?i ngh?
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("Bạn không có quyền cập nhật giai đoạn này.");
            await EnsureConferenceIsEditable(conference);

            // 2.2. Ki?m tra nghi?p v? v? c? IsWaitlist và IsActive
            //if (request.IsWaitlist.HasValue && request.IsWaitlist != phaseToUpdate.IsWaitlist)
            //    throw new BadRequestException("Không du?c phép thay d?i thu?c tính 'IsWaitlist' c?a m?t phase sau khi dã t?o.");
            //if (request.IsActive.HasValue && request.IsActive != phaseToUpdate.IsActive)
            //    throw new BadRequestException("Không th? thay d?i tr?ng thái 'IsActive' tr?c ti?p. Vui lòng s? d?ng ch?c nang 'ActivateWaitlist'.");

            // 2.3. Xác d?nh các giá tr? ngày tháng cu?i cùng sau khi c?p nh?t
            // 1. CHUẨN BỊ DỮ LIỆU (Merge dữ liệu mới và cũ)
            // Dùng toán tử ?? : Nếu request có dữ liệu thì lấy, nếu null thì giữ nguyên dữ liệu cũ trong DB

            // Giai đoạn 1: Registration
            var finalRegStart = request.RegistrationStartDate ?? phaseToUpdate.RegistrationStartDate;
            var finalRegEnd = request.RegistrationEndDate ?? phaseToUpdate.RegistrationEndDate;

            // Giai đoạn 2: Abstract Decide
            var finalAbsDecideStart = request.AbstractDecideStatusStart ?? phaseToUpdate.AbstractDecideStatusStart;
            var finalAbsDecideEnd = request.AbstractDecideStatusEnd ?? phaseToUpdate.AbstractDecideStatusEnd;

            // Giai đoạn 3: Full Paper Submit
            var finalFullPaperStart = request.FullPaperStartDate ?? phaseToUpdate.FullPaperStartDate;
            var finalFullPaperEnd = request.FullPaperEndDate ?? phaseToUpdate.FullPaperEndDate;

            // Giai đoạn 4: Review Full Paper
            var finalReviewStart = request.ReviewStartDate ?? phaseToUpdate.ReviewStartDate;
            var finalReviewEnd = request.ReviewEndDate ?? phaseToUpdate.ReviewEndDate;

            // Giai đoạn 5: Full Paper Decide
            var finalFullPaperDecideStart = request.FullPaperDecideStatusStart ?? phaseToUpdate.FullPaperDecideStatusStart;
            var finalFullPaperDecideEnd = request.FullPaperDecideStatusEnd ?? phaseToUpdate.FullPaperDecideStatusEnd;

            // Giai đoạn 6: Revise (Chỉnh sửa)
            var finalReviseStart = request.ReviseStartDate ?? phaseToUpdate.ReviseStartDate;
            var finalReviseEnd = request.ReviseEndDate ?? phaseToUpdate.ReviseEndDate;

            // Giai đoạn 7: Review lại bài đã sửa (Revision Review)
            //var finalReviseReviewStart = request.RevisionPaperReviewStart ?? phaseToUpdate.RevisionPaperReviewStart;
            //var finalReviseReviewEnd = request.RevisionPaperReviewEnd ?? phaseToUpdate.RevisionPaperReviewEnd;s

            // Giai đoạn 8: Quyết định bài sửa (Revision Decide)
            var finalReviseDecideStart = request.RevisionPaperDecideStatusStart ?? phaseToUpdate.RevisionPaperDecideStatusStart;
            var finalReviseDecideEnd = request.RevisionPaperDecideStatusEnd ?? phaseToUpdate.RevisionPaperDecideStatusEnd;

            // Giai đoạn 9: Camera Ready (Nộp bản in)
            var finalCameraStart = request.CameraReadyStartDate ?? phaseToUpdate.CameraReadyStartDate;
            var finalCameraEnd = request.CameraReadyEndDate ?? phaseToUpdate.CameraReadyEndDate;

            // Giai đoạn 10: Camera Ready Decide
            var finalCameraDecideStart = request.CameraReadyDecideStatusStart ?? phaseToUpdate.CameraReadyDecideStatusStart;
            var finalCameraDecideEnd = request.CameraReadyDecideStatusEnd ?? phaseToUpdate.CameraReadyDecideStatusEnd;


            // 2. KIỂM TRA LOGIC (Validation)
            // Kiểm tra dây chuyền từ trên xuống dưới theo đúng thứ tự DTO
            if (
                // 1. Registration
                finalRegStart > finalRegEnd ||
                finalRegEnd > finalAbsDecideStart ||

                // 2. Abstract Decide
                finalAbsDecideStart > finalAbsDecideEnd ||
                finalAbsDecideEnd > finalFullPaperStart ||

                // 3. Full Paper
                finalFullPaperStart > finalFullPaperEnd ||
                finalFullPaperEnd > finalReviewStart ||

                // 4. Review
                finalReviewStart > finalReviewEnd ||
                finalReviewEnd > finalFullPaperDecideStart ||

                // 5. Full Paper Decide
                finalFullPaperDecideStart > finalFullPaperDecideEnd ||
                finalFullPaperDecideEnd > finalReviseStart ||

                // 6. Revise
                finalReviseStart > finalReviseEnd ||
                /* finalReviseEnd > finalReviseReviewStart ||

                 // 7. Revision Review
                 finalReviseReviewStart > finalReviseReviewEnd ||
                 finalReviseReviewEnd > finalReviseDecideStart || */

                // 8. Revision Decide
                finalReviseDecideStart > finalReviseDecideEnd ||
                finalReviseDecideEnd > finalCameraStart ||

                // 9. Camera Ready
                finalCameraStart > finalCameraEnd ||
                finalCameraEnd > finalCameraDecideStart ||

                // 10. Camera Ready Decide
                finalCameraDecideStart > finalCameraDecideEnd
            )
            {
                throw new BadRequestException("Các mốc thời gian sau khi cập nhật không tuân thủ đúng thứ tự quy trình.");
            }
            // 2.5. Ki?m tra ch?ng chéo v?i các phase khác
            var allOtherPhases = (await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conference.ConferenceId))
                .Where(p => p.ResearchConferencePhaseId != phaseId)
                .ToList();

            foreach (var otherPhase in allOtherPhases)
            {
                // Ki?m tra xem phase dang c?p nh?t có "nu?t" phase khác không
                if (finalRegStart < otherPhase.CameraReadyEndDate && finalCameraEnd > otherPhase.RegistrationStartDate)
                {
                    throw new BadRequestException($"Kho?ng th?i gian m?i ({finalRegStart:dd/MM/yyyy} - {finalCameraEnd:dd/MM/yyyy}) b? ch?ng chéo v?i m?t phase khác dã t?n t?i.");
                }
            }

            // 2.6. Ki?m tra các Revision Deadlines có còn n?m trong kho?ng Revise m?i không
            var deadlines = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(phaseId);
            foreach (var deadline in deadlines)
            {
                if (deadline.StartSubmissionDate < finalReviseStart || deadline.EndSubmissionDate > finalReviseEnd)
                {
                    throw new BadRequestException($"Không th? c?p nh?t. Kho?ng th?i gian s?a d?i m?i ({finalReviseStart:dd/MM/yyyy} - {finalReviseEnd:dd/MM/yyyy}) không còn ch?a Revision Deadline Round {deadline.RoundNumber} ({deadline.StartSubmissionDate:dd/MM/yyyy} - {deadline.EndSubmissionDate:dd/MM/yyyy}). Vui lòng c?p nh?t các deadline tru?c.");
                }
            }
            #endregion

            #region === 3. TH?C THI ===
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 3. CẬP NHẬT MODEL (Gán giá trị đã validate vào Entity)

                // 1. Registration
                phaseToUpdate.RegistrationStartDate = finalRegStart;
                phaseToUpdate.RegistrationEndDate = finalRegEnd;

                // 2. Abstract Decide (Thêm mới)
                phaseToUpdate.AbstractDecideStatusStart = finalAbsDecideStart;
                phaseToUpdate.AbstractDecideStatusEnd = finalAbsDecideEnd;

                // 3. Full Paper
                phaseToUpdate.FullPaperStartDate = finalFullPaperStart;
                phaseToUpdate.FullPaperEndDate = finalFullPaperEnd;

                // 4. Review
                phaseToUpdate.ReviewStartDate = finalReviewStart;
                phaseToUpdate.ReviewEndDate = finalReviewEnd;

                // 5. Full Paper Decide (Thêm mới)
                phaseToUpdate.FullPaperDecideStatusStart = finalFullPaperDecideStart;
                phaseToUpdate.FullPaperDecideStatusEnd = finalFullPaperDecideEnd;

                // 6. Revise
                phaseToUpdate.ReviseStartDate = finalReviseStart;
                phaseToUpdate.ReviseEndDate = finalReviseEnd;

                // 7. Revision Review (Thêm mới)
                //phaseToUpdate.RevisionPaperReviewStart = finalReviseReviewStart;
                //phaseToUpdate.RevisionPaperReviewEnd = finalReviseReviewEnd;

                // 8. Revision Decide (Thêm mới)
                phaseToUpdate.RevisionPaperDecideStatusStart = finalReviseDecideStart;
                phaseToUpdate.RevisionPaperDecideStatusEnd = finalReviseDecideEnd;

                // 9. Camera Ready
                phaseToUpdate.CameraReadyStartDate = finalCameraStart;
                phaseToUpdate.CameraReadyEndDate = finalCameraEnd;

                // 10. Camera Ready Decide (Thêm mới)
                phaseToUpdate.CameraReadyDecideStatusStart = finalCameraDecideStart;
                phaseToUpdate.CameraReadyDecideStatusEnd = finalCameraDecideEnd;

                // Sau bước này, bạn gọi lệnh SaveChangesAsync() để lưu xuống DB
                // await _context.SaveChangesAsync();
                // Không c?p nh?t IsWaitlist và IsActive ? dây

                await _unitOfWork.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(phaseToUpdate);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return phaseToUpdate.ToResponse();
            #endregion
        }

        #endregion

        #region Research Conference Step 4: Research Conference Sessions (without speakers)

        public async Task<List<ResearchSessionWithMediaResponse>> AddResearchSessionsAsync(string conferenceId, AddResearchSessionsRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) 
                throw new NotFoundException($"Không tìm thấy hội nghị với ID {conferenceId}");

            #region Xác th?c
            // 1. Phân quy?n và tr?ng thái
            if (conference.CreatedBy != userId)
                throw new BadRequestException("Bạn không có quyền thêm session cho hội nghị này.");
            await EnsureConferenceIsEditable(conference);

            // 2. Ð?m b?o dây là h?i ngh? nghiên c?u
            if (conference.IsResearchConference != true)
                throw new BadRequestException("Chức năng này chỉ dành cho hội nghị nghiên cứu.");

            // 3. Ki?m tra request h?p l?
            if (request.Sessions == null || !request.Sessions.Any())
                throw new BadRequestException("Yêu cầu phải chứa ít nhất một session.");

            // 4. Ki?m tra xem t?t c? các ngày c?a h?i ngh? d?u có session 
            var sessionDates = request.Sessions
                                    .Where(s => s.Date.HasValue)
                                    .Select(s => s.Date.Value)
                                    .Distinct()
                                    .ToList();
            await checkEachDateHasConferenceSession(conference, sessionDates, true);
            #endregion

            var sessionsGroupedByRoomAndDate = request.Sessions.Where(c => c.RoomId != null).GroupBy(s => new { s.RoomId, s.Date });
            foreach (var group in sessionsGroupedByRoomAndDate)
            {
                var sortedSessionsInGroup = group.OrderBy(s => s.StartTime).ToList();
                for (int i = 0; i < sortedSessionsInGroup.Count - 1; i++)
                {
                    var currentSession = sortedSessionsInGroup[i];
                    var nextSession = sortedSessionsInGroup[i + 1];

                    if (currentSession.EndTime.Value > nextSession.StartTime.Value)
                    {
                        throw new BadRequestException($"Dữ liệu request không hợp lệ: Phiên '{currentSession.Title}' (kết thúc lúc {currentSession.EndTime:HH:mm}) bị chồnng chéo thời gian v?i phiên '{nextSession.Title}' (b?t d?u lúc {nextSession.StartTime:HH:mm}) trong cùng phòng và cùng ngày.");
                    }
                }
            }

            var responses = new List<ResearchSessionWithMediaResponse>();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var sessionRequest in request.Sessions)
                {
                    #region Xác thực cho từng Session

                    if (string.IsNullOrWhiteSpace(sessionRequest.Title))
                        throw new BadRequestException("Tiêu đề của session không được để trống.");

                    if (sessionRequest.Date == null || sessionRequest.StartTime == null || sessionRequest.EndTime == null)
                        throw new BadRequestException($"Phiên '{sessionRequest.Title}' cần có đủ Date, StartTime và EndTime.");


                    // Ngày của session phải nằm trong khoảng ngày của hội nghị
                    if (sessionRequest.Date.Value < conference.StartDate || sessionRequest.Date.Value > conference.EndDate)
                        throw new BadRequestException($"Ngày của phiên '{sessionRequest.Title}' nằm ngoài thời gian hội nghị ({conference.StartDate:dd/MM/yyyy} - {conference.EndDate:dd/MM/yyyy}).");

                    if (!string.IsNullOrEmpty(sessionRequest.RoomId))
                    {
                        if (await _unitOfWork.RoomRepository.GetRoomByIdAsync(sessionRequest.RoomId) == null)
                            throw new NotFoundException($"Phòng với ID {sessionRequest.RoomId} không tồn tại.");

                        var sessionStartDateTime = sessionRequest.Date.Value.ToDateTime(sessionRequest.StartTime.Value);
                        var sessionEndDateTime = sessionRequest.Date.Value.ToDateTime(sessionRequest.EndTime.Value);

                        await ValidateSessionTimeAvailability(sessionStartDateTime, sessionEndDateTime, sessionRequest.RoomId);
                    }

                    #endregion

                    var conferenceSession = sessionRequest.ToModel(conferenceId);
                    await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);

                    // X? lý media (không có speaker)
                    if (sessionRequest.SessionMedias != null)
                    {
                        foreach (var mediaRequest in sessionRequest.SessionMedias)
                        {
                            if (!_objectStorageFileService.IsValidVideoFile(mediaRequest.MediaFile) && !_objectStorageFileService.IsValidImageFile(mediaRequest.MediaFile)) 
                                throw new BadRequestException($"Không h? tr? d?nh d?ng {mediaRequest.MediaFile.ContentType}");
                            const long maxSize = 5 * 1024 * 1024; // 5 MB
                            if (mediaRequest.MediaFile.Length > maxSize)
                                throw new BadRequestException("Kích thước tệp ảnh bìa không được vượt quá 5 MB.");
                            if (mediaRequest.MediaFile == null && string.IsNullOrWhiteSpace(mediaRequest.MediaUrl))
                                continue;

                            string mediaUrl = mediaRequest.MediaUrl;
                            if (mediaRequest.MediaFile != null)
                            {
                                using var stream = mediaRequest.MediaFile.OpenReadStream();
                                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(mediaRequest.MediaFile.FileName);
                                mediaUrl = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencesessionmedia.ToString(), uniqueFileName, stream, mediaRequest.MediaFile.ContentType);
                            }
                            var conferenceSessionMedia = mediaRequest.ToModel(conferenceSession.ConferenceSessionId, mediaUrl);
                            await _unitOfWork.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(conferenceSessionMedia);
                        }
                    }

                    var createdSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(conferenceSession.ConferenceSessionId);
                    responses.Add(createdSession.ToResearchResponseWithMedia());
                }
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<ResearchSessionWithMediaResponse>> GetResearchSessionsAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
            var responses = new List<ResearchSessionWithMediaResponse>();

            foreach (var session in sessions)
            {
                responses.Add(session.ToResearchResponseWithMedia());
            }

            return responses;
        }

        public async Task<List<ResearchSessionWithMediaResponse>> GetResearchSessionsWithoutRoomAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Hội nghị với ID {conferenceId} không tìm thấy");

            if (conference.IsResearchConference != true)
                throw new Exception("Chức năng chỉ có thể cho hội nghị nghiên cứu");

            var researchSessionWithoutRoom = await _unitOfWork.ConferenceSessionRepository.GetSessionWithoutRoom(conferenceId);
            var responses = new List<ResearchSessionWithMediaResponse>();

            foreach (var session in researchSessionWithoutRoom)
            {
                responses.Add(session.ToResearchResponseWithMedia());
            }

            return responses;
        }

        public async Task<ResearchSessionWithMediaResponse> UpdateResearchSessionAsync(string sessionId, UpdateConferenceSessionRequest request, string userId)
        {
            // L?y thông tin conference d? ki?m tra lo?i
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Không tìm thấy session ");
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(session.ConferenceId);

            // Ki?m tra d?c thù c?a phuong th?c này
            if (conference.IsResearchConference != true)
                throw new BadRequestException("Chức năng này chỉ dành cho phiên của hội nghị nghiên c?u.");

            // G?i hàm helper chung d? th?c hi?n t?t c? công vi?c
            var updatedSession = await UpdateSessionInternalAsync(sessionId, request, userId);

            // Tr? v? dúng ki?u response
            return updatedSession.ToResearchResponseWithMedia();
        }

        public async Task<bool> DeleteResearchSessionAsync(string sessionId, string userId)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null)
            {
                return false; // Tr? v? false thay vì NotFound
            }

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(session.ConferenceId);

            #region Xác th?c
            // 1. Phân quy?n và tr?ng thái
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n xóa phiên này.");
            EnsureConferenceIsEditable(conference);

            //// 2. Ki?m tra xem dã có presenter nào du?c gán vào session này chua
            //var presentersInSession = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorsBySessionIdAsync(sessionId);
            //if (presentersInSession.Any())
            //{
            //    throw new BadRequestException("Không th? xóa phiên này vì dã có bài báo du?c gán d? trình bày.");
            //}
            #endregion

            // Xóa t?t c? media liên quan
            var mediaList = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(sessionId);
            foreach (var media in mediaList)
            {
                // (Tùy ch?n: Xóa file kh?i Object Storage ? dây n?u c?n)
                await _unitOfWork.ConferenceSessionMediumRepository.DeleteConferenceSessionMediumAsync(media);
            }

            // Xóa session
            return await _unitOfWork.ConferenceSessionRepository.DeleteConferenceSessionAsync(session) > 0;
        }

        #endregion

        #region Research Conference Step 5: Material Downloads

        public async Task<MaterialDownloadResponse> CreateMaterialDownloadAsync(string conferenceId, CreateMaterialDownloadRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (request.File == null)
                throw new Exception("Cần phải có file");
            if (!_objectStorageFileService.IsValidDocumentFile(request.File))
                throw new Exception($"Không hỗ trợ định dạng {request.File.ContentType}");
            string fileName = "";
            // Handle file upload if provided
            using var stream = request.File.OpenReadStream();
            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
            fileName = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.materialdownload.ToString(), uniqueFileName, stream, request.File.ContentType);
            var model = request.ToModel(conferenceId, fileName);

            await _unitOfWork.MaterialDownloadRepository.CreateMaterialDownloadAsync(model);
            return model.ToResponse();
        }

        public async Task<List<MaterialDownloadResponse>> GetMaterialDownloadsByConferenceIdAsync(string conferenceId)
        {
            var materials = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conferenceId);
            return materials.Select(m => m.ToResponse()).ToList();
        }

        public async Task<MaterialDownloadResponse> UpdateMaterialDownloadAsync(string materialDownloadId, UpdateMaterialDownloadRequest request, string userId)
        {
            var materialDownload = await _unitOfWork.MaterialDownloadRepository.GetMaterialDownloadByIdAsync(materialDownloadId);
            if (materialDownload == null) throw new NotFoundException($"Material download with ID {materialDownloadId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(materialDownload.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho material download với ID {materialDownloadId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (request.File == null)
                throw new Exception("Cần phải có file");
            if (!_objectStorageFileService.IsValidDocumentFile(request.File))
                throw new Exception($"Không hỗ trợ định dạng {request.File.ContentType}");



            // Handle file upload if provided
            if (request.File != null)
            {
                using var stream = request.File.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
                materialDownload.FileName = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.materialdownload.ToString(), uniqueFileName, stream, request.File.ContentType);
            }
            materialDownload.FileDescription = request.FileDescription ?? materialDownload.FileDescription;

            await _unitOfWork.MaterialDownloadRepository.UpdateMaterialDownloadAsync(materialDownload);
            return materialDownload.ToResponse();
        }

        public async Task<bool> DeleteMaterialDownloadAsync(string materialDownloadId, string userId)
        {
            var materialDownload = await _unitOfWork.MaterialDownloadRepository.GetMaterialDownloadByIdAsync(materialDownloadId);
            if (materialDownload == null) throw new NotFoundException($"Material download with ID {materialDownloadId} not found");


            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(materialDownload.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho material download với ID {materialDownloadId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            return await _unitOfWork.MaterialDownloadRepository.DeleteMaterialDownloadAsync(materialDownload) > 0;
        }

        #endregion

        #region Research Conference Step 6: Ranking File URLs

        public async Task<RankingFileUrlResponse> CreateRankingFileUrlAsync(string conferenceId, CreateRankingFileUrlRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (!_objectStorageFileService.IsValidDocumentFile(request.File))
                throw new Exception($"Không hỗ trợ định dạng {request.File.ContentType}");

            var rankingFileUrl = request.ToModel(conferenceId);

            using var stream = request.File.OpenReadStream();
            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
            rankingFileUrl.FileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.rankingfile.ToString(), uniqueFileName, stream, request.File.ContentType);
            rankingFileUrl.FileUrl = _objectStorageSettings.EndPoint + rankingFileUrl.FileUrl;

            await _unitOfWork.RankingFileUrlRepository.CreateRankingFileUrlAsync(rankingFileUrl);
            return rankingFileUrl.ToResponse();
        }

        public async Task<List<RankingFileUrlResponse>> GetRankingFileUrlsByConferenceIdAsync(string conferenceId)
        {
            var fileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conferenceId);
            return fileUrls.Select(f => f.ToResponse()).ToList();
        }

        public async Task<RankingFileUrlResponse> UpdateRankingFileUrlAsync(string rankingFileUrlId, UpdateRankingFileUrlRequest request, string userId)
        {
            var rankingFileUrl = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlByIdAsync(rankingFileUrlId);
            if (rankingFileUrl == null) throw new NotFoundException($"Ranking file URL with ID {rankingFileUrlId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(rankingFileUrl.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho ranking file với ID {rankingFileUrlId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (!_objectStorageFileService.IsValidDocumentFile(request.File))
                throw new Exception($"Không hỗ trợ định dạng {request.File.ContentType}");

            using var stream = request.File.OpenReadStream();
            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
            rankingFileUrl.FileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.rankingfile.ToString(), uniqueFileName, stream, request.File.ContentType);
            rankingFileUrl.FileUrl = _objectStorageSettings.EndPoint + rankingFileUrl.FileUrl;

            await _unitOfWork.RankingFileUrlRepository.UpdateRankingFileUrlAsync(rankingFileUrl);
            return rankingFileUrl.ToResponse();
        }

        public async Task<bool> DeleteRankingFileUrlAsync(string rankingFileUrlId, string userId)
        {
            var rankingFileUrl = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlByIdAsync(rankingFileUrlId);
            if (rankingFileUrl == null) throw new NotFoundException($"Ranking file URL with ID {rankingFileUrlId} not found");


            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(rankingFileUrl.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho ranking file với ID {rankingFileUrlId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");


            return await _unitOfWork.RankingFileUrlRepository.DeleteRankingFileUrlAsync(rankingFileUrl) > 0;
        }

        #endregion

        #region Research Conference Step 7: Ranking Reference URLs

        public async Task<RankingReferenceUrlResponse> CreateRankingReferenceUrlAsync(string conferenceId, CreateRankingReferenceUrlRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");



            if (conference == null)
                throw new Exception($"Không tìm được conference với ID {conferenceId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");


            var rankingReferenceUrl = request.ToModel(conferenceId);

            await _unitOfWork.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(rankingReferenceUrl);
            return rankingReferenceUrl.ToResponse();
        }

        public async Task<List<RankingReferenceUrlResponse>> GetRankingReferenceUrlsByConferenceIdAsync(string conferenceId)
        {
            var referenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conferenceId);
            return referenceUrls.Select(r => r.ToResponse()).ToList();
        }

        public async Task<RankingReferenceUrlResponse> UpdateRankingReferenceUrlAsync(string referenceUrlId, UpdateRankingReferenceUrlRequest request, string userId)
        {
            var rankingReferenceUrl = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(referenceUrlId);
            if (rankingReferenceUrl == null) throw new NotFoundException($"Ranking reference URL with ID {referenceUrlId} not found");


            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(rankingReferenceUrl.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho URL tham khảo ranking với ID {rankingReferenceUrl}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            rankingReferenceUrl.ReferenceUrl = request.ReferenceUrlId;


            await _unitOfWork.RankingReferenceUrlRepository.UpdateRankingReferenceUrlAsync(rankingReferenceUrl);
            return rankingReferenceUrl.ToResponse();
        }

        public async Task<bool> DeleteRankingReferenceUrlAsync(string referenceUrlId, string userId)
        {
            var rankingReferenceUrl = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(referenceUrlId);
            if (rankingReferenceUrl == null) throw new NotFoundException($"Ranking reference URL with ID {referenceUrlId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(rankingReferenceUrl.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho URL tham khảo ranking với ID {rankingReferenceUrl}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");


            return await _unitOfWork.RankingReferenceUrlRepository.DeleteRankingReferenceUrlAsync(rankingReferenceUrl) > 0;
        }

        #endregion

        #region PricePhase CRUD Operations

        // DÁN TOÀN B? PHIÊN B?N NÀY Ð? THAY TH? PHIÊN B?N CU

        // DÁN TOÀN B? PHIÊN B?N NÀY Ð? THAY TH? PHIÊN B?N CU

        public async Task<List<PricePhaseResponse>> AddPricePhasesAsync(string conferencePriceId, AddPricePhasesRequest request, string userId)
        {
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(conferencePriceId);
            if (conferencePrice == null) throw new NotFoundException($"Không tìm th?y lo?i vé v?i ID {conferencePriceId}");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferencePrice.ConferenceId);
            if (conference == null) throw new InvalidOperationException("Không tìm th?y h?i ngh? cho lo?i vé này.");

            #region === 1. VALIDATION CO B?N & PHÂN QUY?N ===
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n thêm giai do?n cho lo?i vé này.");
            await EnsureConferenceIsEditable(conference);

            if (request.PricePhases == null || !request.PricePhases.Any())
                throw new BadRequestException("Yêu c?u ph?i ch?a ít nh?t m?t giai do?n bán vé.");

            // Ki?m tra t?ng slot
            var existingPhases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(conferencePriceId);
            var existingTotalSlot = existingPhases.Sum(p => p.TotalSlot ?? 0);
            var requestTotalSlot = request.PricePhases.Sum(p => p.TotalSlot);
            if (existingTotalSlot + requestTotalSlot > conferencePrice.TotalSlot)
                throw new BadRequestException($"T?ng s? vé trong các giai do?n ({existingTotalSlot + requestTotalSlot}) vu?t quá gi?i h?n {conferencePrice.TotalSlot} c?a lo?i vé này.");

            // Ki?m tra ch?ng chéo ngày tháng
            var allPhasesForCheck = new List<PricePhase>(existingPhases);
            allPhasesForCheck.AddRange(request.PricePhases.Select(p => new PricePhase { StartDate = p.StartDate, EndDate = p.EndDate }));
            var sortedPhases = allPhasesForCheck.OrderBy(p => p.StartDate).ToList();
            for (int i = 0; i < sortedPhases.Count - 1; i++)
            {
                if (sortedPhases[i].EndDate >= sortedPhases[i + 1].StartDate)
                    throw new BadRequestException("Các giai do?n bán vé không du?c có ngày ch?ng chéo ho?c quá sát nhau.");
            }
            #endregion

            #region === 2. VALIDATION NGÀY THÁNG & GÁN RESEARCH PHASE ID ===
            ResearchConferencePhase? targetResearchPhase = null;

            // Ch? th?c hi?n logic ph?c t?p này n?u là vé tác gi? c?a h?i ngh? nghiên c?u
            if (conference.IsResearchConference == true && conferencePrice.IsAuthor == true)
            {
                // G?I TR?C TI?P ? ÐÂY - ÐÚNG NHU B?N NÓI
                targetResearchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(conference.ConferenceId);
                if (targetResearchPhase == null)
                    throw new InvalidOperationException("Không tìm th?y phase nào dang ho?t d?ng cho h?i ngh? nghiên c?u này.");

                foreach (var phaseRequest in request.PricePhases)
                {
                    // Luôn ki?m tra v?i phase dang ho?t d?ng t?i th?i di?m dó
                    if (phaseRequest.StartDate < targetResearchPhase.RegistrationStartDate || phaseRequest.EndDate > targetResearchPhase.RegistrationEndDate)
                        throw new BadRequestException($"Giai do?n '{phaseRequest.PhaseName}' ph?i n?m trong kho?ng dang ký c?a phase dang ho?t d?ng ({targetResearchPhase.RegistrationStartDate:dd/MM/yyyy} - {targetResearchPhase.RegistrationEndDate:dd/MM/yyyy}).");
                }
            }
            else // Vé thu?ng ho?c h?i ngh? k? thu?t
            {
                foreach (var phaseRequest in request.PricePhases)
                {
                    if (phaseRequest.StartDate < conference.TicketSaleStart || phaseRequest.EndDate > conference.TicketSaleEnd)
                        throw new BadRequestException($"Giai do?n '{phaseRequest.PhaseName}' ph?i n?m trong kho?ng bán vé c?a h?i ngh? ({conference.TicketSaleStart:dd/MM/yyyy} - {conference.TicketSaleEnd:dd/MM/yyyy}).");
                }
            }
            #endregion

            #region === 3. TH?C THI ===
            var responses = new List<PricePhaseResponse>();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var pricePhaseRequest in request.PricePhases)
                {
                    // Truy?n ID c?a phase dã xác d?nh du?c vào mapper
                    var pricePhase = pricePhaseRequest.ToModel(conferencePriceId, targetResearchPhase?.ResearchConferencePhaseId);
                    await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(pricePhase);
                    responses.Add(pricePhase.ToResponse());
                }
                await _unitOfWork.CommitAsync();
            }
            catch (Exception) { await _unitOfWork.RollbackAsync(); throw; }

            return responses;
            #endregion
        }


        public async Task<List<PricePhaseResponse>> AddPricePhaseForWaitList(string conferencePriceId, PhaseForWaitList request, string userId)
        {
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(conferencePriceId);
            if (conferencePrice == null) throw new NotFoundException($"Không tìm thấy loại vé với ID {conferencePriceId}");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferencePrice.ConferenceId);
            if (conference == null) throw new InvalidOperationException("Không tìm thấy hội nghị cho loại vé này.");

            #region === 1. VALIDATION ===
            // 1.1. Phân quy?n và các quy t?c nghi?p v? c?a Waitlist
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("Bạn không có quyền thêm giai đoạn cho loại vé này.");
            if (conferencePrice.IsAuthor != true)
                throw new BadRequestException("Chức nang này chỉ dành để thêm giai đoạn cho loại vé 'isAuthor'.");
            if (conferencePrice.AvailableSlot == 0)
                throw new BadRequestException("Không thể thêm giai đoạn mới vì loại vé này đã hết vé (available slot = 0).");

            // 1.2. Ki?m tra th?i di?m h?p l?
            var notWaitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseNotWaitListByConferenceIdAsync(conference.ConferenceId);
            var waitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseIsWaitListByConferenceIdAsync(conference.ConferenceId);
            if (notWaitlistPhase == null || waitlistPhase == null)
                throw new BadRequestException("Hội nghị chưa được cấu hình đầy đủ phase chính và phase waitlist.");

            DateOnly today = await _timeProviderService.GetVietnamDate();
            if (today <= notWaitlistPhase.CameraReadyEndDate)
                throw new BadRequestException($"Chua d?n th?i di?m h?p l?. C?n ph?i sau khi phase chính k?t thúc (sau ngày {notWaitlistPhase.CameraReadyEndDate:dd/MM/yyyy}).");

            // 1.3. S?a l?i logic tính toán Slot
            var allExistingPhases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(conferencePriceId);
            var existingWaitlistPhases = allExistingPhases.Where(p => p.ResearchConferencePhaseId == waitlistPhase.ResearchConferencePhaseId).ToList();

            var existingWaitlistSlot = existingWaitlistPhases.Sum(p => p.TotalSlot ?? 0);
            var requestWaitlistSlot = request.Phases.Sum(p => p.Totalslot);
            if (existingWaitlistSlot + requestWaitlistSlot > conferencePrice.AvailableSlot)
            {
                throw new BadRequestException($"T?ng s? vé cho các giai do?n waitlist ({existingWaitlistSlot + requestWaitlistSlot}) không du?c vu?t quá s? vé còn l?i c?a lo?i vé này ({conferencePrice.AvailableSlot}).");
            }

            // Ki?m tra ch?ng chéo: s? d?ng l?i allExistingPhases dã l?y ? trên
            var allPhasesForCheck = new List<PricePhase>(allExistingPhases); // <-- Tái s? d?ng ? dây
            allPhasesForCheck.AddRange(request.Phases.Select(p => new PricePhase { StartDate = p.StartDate, EndDate = p.EndDate }));
            var sortedPhases = allPhasesForCheck.OrderBy(p => p.StartDate).ToList();
            for (int i = 0; i < sortedPhases.Count - 1; i++)
            {
                if (sortedPhases[i].EndDate >= sortedPhases[i + 1].StartDate)
                    throw new BadRequestException("Các giai do?n bán vé không du?c có ngày ch?ng chéo ho?c quá sát nhau.");
            }

            foreach (var phaseRequest in request.Phases)
            {
                if (phaseRequest.StartDate < waitlistPhase.RegistrationStartDate || phaseRequest.EndDate > waitlistPhase.RegistrationEndDate)
                    throw new BadRequestException($"Giai đoạn '{phaseRequest.PhaseName}' phải nằm trong khoảng đăng ký của waitlist ({waitlistPhase.RegistrationStartDate:dd/MM/yyyy} - {waitlistPhase.RegistrationEndDate:dd/MM/yyyy}).");

                if (phaseRequest.StartDate > phaseRequest.EndDate)
                    throw new BadRequestException("Start phase phải lớn hơn end");

                if (phaseRequest.refundInPhase != null && phaseRequest.refundInPhase.Any())
                {
                    var sortedRefunds = phaseRequest.refundInPhase.OrderBy(r => r.RefundDeadline).ToList();
                    for (int i = 0; i < sortedRefunds.Count; i++)
                    {
                        var refundRequest = sortedRefunds[i];

                        if (!refundRequest.PercentRefund.HasValue || !refundRequest.RefundDeadline.HasValue)
                            throw new BadRequestException("Chính sách hoàn tiền phải có đủ phần trăm và hạn chót.");
                        if (refundRequest.PercentRefund.Value < 0 || refundRequest.PercentRefund.Value > 100)
                            throw new BadRequestException("PerentRefund phải nằm trong khoảng 0-100");

                        // *** VALIDATION M?I: Deadline c?a refund policy ***
                        // 1. Ph?i sau ngày b?t d?u c?a phase
                        if (refundRequest.RefundDeadline.Value <= phaseRequest.StartDate)
                        {
                            throw new BadRequestException($"Trong giai do?n '{phaseRequest.PhaseName}', hạn chót hoàn tiền ({refundRequest.RefundDeadline.Value:dd/MM/yyyy}) ph?i sau ngày bắt đầu giai đoạn ({phaseRequest.StartDate:dd/MM/yyyy}).");
                        }
                        // 2. Ph?i tru?c ngày b?t d?u bán vé c?a c? h?i ngh?
                        if (refundRequest.RefundDeadline.Value >= waitlistPhase.RegistrationEndDate)
                        {
                            throw new BadRequestException($"Trong giai do?n '{phaseRequest.PhaseName}',  hạn chót hoàn tiền ({refundRequest.RefundDeadline.Value:dd/MM/yyyy}) phải trước ngày kết thúc đăng ký nộp paper của hội nghị ({waitlistPhase.RegistrationEndDate:dd/MM/yyyy}).");
                        }


                    }
                }
                #endregion



            }
            #region === 2. TH?C THI ===
            var responses = new List<DTOs.ConferenceStep.PricePhaseResponse>();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var pricePhaseRequest in request.Phases)
                {
                    var pricePhase = pricePhaseRequest.ToModel(conferencePriceId, waitlistPhase.ResearchConferencePhaseId);
                    await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(pricePhase);
                    List<ConfRadar.Services.DTOs.ConferenceStep.RefundPolicyResponse> refundPolicyResponses = new();
                    var sortedRefunds = pricePhaseRequest.refundInPhase.OrderBy(r => r.RefundDeadline).ToList();
                    for (int i = 0; i < sortedRefunds.Count; i++)
                    {
                        var refundRequest = sortedRefunds[i];
                        var refundModel = new RefundPolicy
                        {
                            RefundPolicyId = Guid.NewGuid().ToString(),
                            PricePhaseId = pricePhase.PricePhaseId,
                            PercentRefund = refundRequest.PercentRefund.Value,
                            RefundDeadline = refundRequest.RefundDeadline,
                            RefundOrder = i + 1 // T? d?ng gán th? t? d?a trên deadline
                        };
                        await _unitOfWork.ConferenceRefundPolicyRepository.CreateConferenceRefundPolicyAsync(refundModel);
                        refundPolicyResponses.Add(refundModel.ToResponse());
                    }


                    responses.Add(pricePhase.ToResponse(refundPolicyResponses));
                }
                await _unitOfWork.CommitAsync();
                return responses;
                #endregion
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                return responses;
                throw;
            }
        }

        public async Task<List<PricePhaseResponse>> GetPricePhasesByConferencePriceIdAsync(string conferencePriceId)
        {
            var pricePhases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(conferencePriceId);
            return pricePhases.Select(p => p.ToResponse()).ToList();
        }

        //public async Task<PricePhaseResponse> UpdatePricePhaseAsync(string pricePhaseId, UpdatePricePhaseRequest request,string userId)
        //{
        //    var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(pricePhaseId);
        //    if (pricePhase == null) throw new NotFoundException($"Price phase v?i ID {pricePhaseId} không tìm th?y du?c");

        //    var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(pricePhase.ConferencePriceId);
        //    if (conferencePrice == null)
        //        throw new Exception($"Không tìm th?y conferenceprice tuong ?ng cho price phase {pricePhaseId}");

        //    var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferencePrice.ConferenceId);

        //    if (conference.CreatedBy != userId)
        //        throw new ForbiddenException("B?n không có quy?n c?p nh?t giai do?n này.");

        //    if (!string.IsNullOrEmpty(request.PhaseName))
        //    {
        //        var existingPricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(pricePhase.ConferencePriceId);
        //        if (existingPricePhase.Any(pp => pp.PhaseName.Equals(request.PhaseName)))
        //            throw new BadRequestException($"{request.PhaseName} dã du?c s? d?ng cho pricephase trong cùng conference price v?i ID {pricePhase.ConferencePriceId}");
        //    }

        //    //update price phase dedicated to waitlist

        //    var waitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseIsWaitListByConferenceIdAsync(conference.ConferenceId);
        //    if (pricePhase.ResearchConferencePhaseId != null)
        //    {
        //        if (pricePhase.ResearchConferencePhaseId != waitlistPhase.ResearchConferencePhaseId)
        //        {
        //            await EnsureConferenceIsEditable(conference);
        //        }
        //    }
        //    else
        //    {
        //        await EnsureConferenceIsEditable(conference);
        //    }

        //    int soldPricePhase = pricePhase.TotalSlot.Value - pricePhase.AvailableSlot.Value;
        //    if (soldPricePhase != 0 && request.TotalSlot.HasValue)
        //        throw new Exception("B?n không th? thay d?i totalSlot vì dã có vé du?c mua t?i phase này");
        //    DateOnly? finalStartDate = request.StartDate ?? pricePhase.StartDate;
        //    DateOnly? finalEndDate = request.EndDate ?? pricePhase.EndDate;




        //    if (conferencePrice.IsAuthor == false)
        //    {
        //        if (finalStartDate.Value < conference.TicketSaleStart || finalEndDate > conference.TicketSaleEnd)
        //            throw new BadRequestException($"Conference price v?i ID {conferencePrice.ConferencePriceId} là vé thu?ng không ph?i cho tác gi? c?n nam trong kho?ng ticketsale window {conference.TicketSaleStart:dd/MM/yyyy} - {conference.TicketSaleEnd:dd/MM/yyyy}. Vé v?i start date {finalStartDate:dd/MM/yyyy} và end date {finalEndDate} không h?p l?");
        //    }else if (conferencePrice.IsAuthor == true)
        //    {
        //        //get researchPhase of pricephase
        //        var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(pricePhase.ResearchConferencePhaseId);
        //        if (researchPhase == null)
        //            throw new Exception("Không tìm ra du?c researchPhase c?a price phase");
        //        if (finalStartDate.Value < researchPhase.RegistrationStartDate || finalEndDate > researchPhase.RegistrationEndDate)
        //            throw new BadRequestException($"Conference price v?i ID {conferencePrice.ConferencePriceId} là vé cho tác gi? c?n nam trong kho?ng registration window {researchPhase.RegistrationStartDate:dd/MM/yyyy} - {researchPhase.RegistrationEndDate:dd/MM/yyyy}. Vé v?i start date {finalStartDate} và end date {finalEndDate} không h?p l?");

        //    }




        //        //must between
        //        if (request.TotalSlot.HasValue)
        //        {
        //            pricePhase.TotalSlot = request.TotalSlot;
        //            pricePhase.AvailableSlot = request.TotalSlot; // Update available slot when total is changed
        //            pricePhase.StartDate = finalStartDate;
        //            pricePhase.EndDate = finalEndDate;
        //        }

        //    await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);
        //    return pricePhase.ToResponse();
        //}

        // DÁN TOÀN B? PHIÊN B?N NÀY Ð? THAY TH? PHIÊN B?N CU

        public async Task<PricePhaseResponse> UpdatePricePhaseAsync(string pricePhaseId, UpdatePricePhaseRequest request, string userId)
        {
            var pricePhaseToUpdate = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(pricePhaseId);
            if (pricePhaseToUpdate == null) throw new NotFoundException($"Không tìm th?y giai do?n bán vé v?i ID {pricePhaseId}.");

            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(pricePhaseToUpdate.ConferencePriceId);
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferencePrice.ConferenceId);

            #region === 1. VALIDATION ===
            // 1.1. Phân quy?n
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n c?p nh?t giai do?n này.");

            // 1.2. Ki?m tra tr?ng thái linh ho?t
            bool isWaitlistRelated = false;
            if (pricePhaseToUpdate.ResearchConferencePhaseId != null)
            {
                var waitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseIsWaitListByConferenceIdAsync(conference.ConferenceId);
                isWaitlistRelated = pricePhaseToUpdate.ResearchConferencePhaseId == waitlistPhase?.ResearchConferencePhaseId;
            }
            if (!isWaitlistRelated) // Ch? ki?m tra tr?ng thái Editable n?u không ph?i phase c?a waitlist
            {
                await EnsureConferenceIsEditable(conference);
            }

            // 1.3. C?m thay d?i n?u dã có vé bán
            int soldTickets = (pricePhaseToUpdate.TotalSlot ?? 0) - (pricePhaseToUpdate.AvailableSlot ?? 0);
            if (soldTickets > 0 && (request.StartDate.HasValue || request.EndDate.HasValue || request.TotalSlot.HasValue))
                throw new BadRequestException("Không th? thay d?i ngày ho?c s? lu?ng vé vì dã có ngu?i mua vé trong giai do?n này.");

            // 1.4. Validation tên trùng l?p
            if (!string.IsNullOrEmpty(request.PhaseName) && request.PhaseName != pricePhaseToUpdate.PhaseName)
            {
                var existingPhases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(pricePhaseToUpdate.ConferencePriceId);
                if (existingPhases.Any(pp => pp.PhaseName.Equals(request.PhaseName, StringComparison.OrdinalIgnoreCase) && pp.PricePhaseId != pricePhaseId))
                    throw new BadRequestException($"Tên giai do?n '{request.PhaseName}' dã du?c s? d?ng.");
            }

            // 1.5. Validation ngày tháng và ch?ng chéo (ch? ch?y n?u chua có vé bán)
            if (soldTickets == 0)
            {
                var finalStartDate = request.StartDate ?? pricePhaseToUpdate.StartDate;
                var finalEndDate = request.EndDate ?? pricePhaseToUpdate.EndDate;

                if (finalStartDate >= finalEndDate)
                    throw new BadRequestException("Ngày b?t d?u ph?i tru?c ngày k?t thúc.");

                var otherPhases = (await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(pricePhaseToUpdate.ConferencePriceId))
                    .Where(p => p.PricePhaseId != pricePhaseId);
                foreach (var other in otherPhases)
                {
                    if (finalStartDate < other.EndDate && finalEndDate > other.StartDate)
                        throw new BadRequestException($"Kho?ng th?i gian m?i b? ch?ng chéo v?i giai do?n '{other.PhaseName}'.");
                }
                // (B?n nên thêm l?i logic ki?m tra ngày n?m trong registration/ticketsale window ? dây)
            }
            #endregion

            #region === 2. TH?C THI ===
            pricePhaseToUpdate.PhaseName = request.PhaseName ?? pricePhaseToUpdate.PhaseName;
            pricePhaseToUpdate.ApplyPercent = request.ApplyPercent ?? pricePhaseToUpdate.ApplyPercent;

            if (soldTickets == 0)
            {
                pricePhaseToUpdate.StartDate = request.StartDate ?? pricePhaseToUpdate.StartDate;
                pricePhaseToUpdate.EndDate = request.EndDate ?? pricePhaseToUpdate.EndDate;
                if (request.TotalSlot.HasValue)
                {
                    // S?a l?i logic c?p nh?t AvailableSlot
                    int slotDifference = request.TotalSlot.Value - (pricePhaseToUpdate.TotalSlot ?? 0);
                    pricePhaseToUpdate.TotalSlot = request.TotalSlot.Value;
                    pricePhaseToUpdate.AvailableSlot = (pricePhaseToUpdate.AvailableSlot ?? 0) + slotDifference;
                }
            }

            await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhaseToUpdate);
            return pricePhaseToUpdate.ToResponse();
            #endregion
        }


        public async Task<bool> DeletePricePhaseAsync(string pricePhaseId)
        {
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(pricePhaseId);
            if (pricePhase == null) throw new NotFoundException($"Price phase with ID {pricePhaseId} not found");

            return await _unitOfWork.PricePhaseRepository.DeletePricePhaseAsync(pricePhase) > 0;
        }

        #endregion

        #region Speaker CRUD Operations

        public async Task<List<SpeakerResponse>> AddSpeakersAsync(string conferenceSessionId, AddSpeakersRequest request, string userId)
        {
            var conferenceSession = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(conferenceSessionId);
            if (conferenceSession == null) throw new NotFoundException($"Conference session with ID {conferenceSessionId} not found");


            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceSession.Conference.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho conference session với ID {conferenceSessionId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");

            if (!request.Speakers.Any())
                throw new Exception("Phải có ít nhất một speaker");


            var responses = new List<SpeakerResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Speakers != null)
                {
                    foreach (var speakerRequest in request.Speakers)
                    {
                        string? imageUrl = null;
                        if (speakerRequest.Image != null)
                        {
                            if (!_objectStorageFileService.IsValidImageFile(speakerRequest.Image))
                                throw new Exception($"Không hỗ trợ định dạng {speakerRequest.Image.ContentType}");
                            using var stream = speakerRequest.Image.OpenReadStream();
                            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(speakerRequest.Image.FileName);
                            imageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, speakerRequest.Image.ContentType);
                            imageUrl = _objectStorageSettings.EndPoint + imageUrl;
                        }

                        var speaker = speakerRequest.ToModel(conferenceSessionId, imageUrl);
                        await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
                        responses.Add(speaker.ToResponse());
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<SpeakerResponse>> GetSpeakersByConferenceSessionIdAsync(string conferenceSessionId)
        {
            var speakers = await _unitOfWork.SpeakerRepository.GetSpeakersBySessionIdAsync(conferenceSessionId);
            return speakers.Select(s => s.ToResponse()).ToList();
        }

        public async Task<SpeakerResponse> UpdateSpeakerBySpeakerIdAsync(string speakerId, UpdateSpeakerRequestForConferenceSession request, string userId)
        {
            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(speakerId);
            if (speaker == null) throw new NotFoundException($"Speaker with ID {speakerId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(speaker.ConferenceSession.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho speaker với ID {speakerId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");


            speaker.Name = request.Name ?? speaker.Name;
            speaker.Description = request.Description ?? speaker.Description;

            if (request.Image != null)
            {
                if (!_objectStorageFileService.IsValidImageFile(request.Image))
                    throw new Exception($"Không hỗ trợ định dạng {request.Image.ContentType}");
                using var stream = request.Image.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.Image.FileName);
                speaker.Image = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, request.Image.ContentType);
                speaker.Image = _objectStorageSettings.EndPoint + speaker.Image;
            }

            await _unitOfWork.SpeakerRepository.UpdateSpeakerAsync(speaker);
            return speaker.ToResponse();
        }

        public async Task<bool> DeleteSpeakerAsync(string speakerId, string userId)
        {
            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(speakerId);
            if (speaker == null) throw new NotFoundException($"Speaker with ID {speakerId} not found");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(speaker.ConferenceSession.ConferenceId);
            if (conference == null)
                throw new Exception($"Không tìm được conference cho speaker với ID {speakerId}");


            if (conference.CreatedBy != userId)
                throw new Exception("Bạn phải là người tạo mới có quyền thực hiện hành động này");


            return await _unitOfWork.SpeakerRepository.DeleteSpeakerAsync(speaker) > 0;
        }

        #endregion

        #region Revision Round Deadline CRUD Operations

        public async Task<List<RevisionRoundDeadlineResponse>> AddRevisionRoundDeadlinesAsync(string researchConferencePhaseId, addRevisionRequest request, string userId)
        {
            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(researchConferencePhaseId);
            if (phase == null) throw new NotFoundException($"Không tìm th?y giai do?n (phase) v?i ID {researchConferencePhaseId}.");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(phase.ConferenceId);
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(phase.ConferenceId);

            #region Xác th?c
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n th?c hi?n thao tác này.");
            await EnsureConferenceIsEditable(conference);
            if (researchDetail == null)
                throw new BadRequestException("H?i ngh? này chua có chi ti?t nghiên c?u (Research Detail).");
            if (request == null || !request.revision.Any())
                throw new BadRequestException("Yêu c?u ph?i ch?a ít nh?t m?t deadline.");

            var existingDeadlines = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(researchConferencePhaseId);

            // 1. S? lu?ng deadline không du?c vu?t quá s? l?n cho phép
            int allowedAttempts = researchDetail.RevisionAttemptAllowed ?? 0;
            if (existingDeadlines.Count + request.revision.Count > allowedAttempts)
            {
                throw new BadRequestException($"Không th? thêm. T?ng s? deadline ({existingDeadlines.Count + request.revision.Count}) s? vu?t quá s? l?n cho phép s?a d?i ({allowedAttempts}).");
            }

            // 2. S?p x?p các deadline m?i d? ki?m tra tu?n t?
            var sortedNewDeadlines = request.revision.OrderBy(d => d.StartSubmissionDate).ToList();
            var allDeadlinesSorted = existingDeadlines
                .Select(d => new { d.StartSubmissionDate, d.EndSubmissionDate })
                .Union(sortedNewDeadlines.Select(d => new { d.StartSubmissionDate, d.EndSubmissionDate }))
                .OrderBy(d => d.StartSubmissionDate)
                .ToList();

            DateOnly? lastEndDate = null;
            foreach (var deadline in allDeadlinesSorted)
            {
                // 2a. Start Date ph?i tru?c End Date
                if (deadline.StartSubmissionDate >= deadline.EndSubmissionDate)
                    throw new BadRequestException($"Ngày b?t d?u ({deadline.StartSubmissionDate:dd/MM/yyyy}) ph?i tru?c ngày k?t thúc ({deadline.EndSubmissionDate:dd/MM/yyyy}).");

                // 2b. Kho?ng th?i gian c?a deadline ph?i n?m trong kho?ng Revise c?a Phase cha (S?A L?I LOGIC)
                if (deadline.StartSubmissionDate < phase.ReviseStartDate || deadline.EndSubmissionDate > phase.ReviseEndDate)
                {
                    throw new BadRequestException($"Kho?ng th?i gian deadline ({deadline.StartSubmissionDate:dd/MM/yyyy} - {deadline.EndSubmissionDate:dd/MM/yyyy}) ph?i n?m trong giai do?n s?a d?i c?a phase ({phase.ReviseStartDate:dd/MM/yyyy} - {phase.ReviseEndDate:dd/MM/yyyy}).");
                }

                // 2c. Các deadline không du?c ch?ng chéo lên nhau
                if (lastEndDate.HasValue && deadline.StartSubmissionDate <= lastEndDate)
                {
                    throw new BadRequestException($"Deadline b?t d?u vào ngày {deadline.StartSubmissionDate:dd/MM/yyyy} b? ch?ng chéo v?i deadline tru?c dó (k?t thúc vào {lastEndDate:dd/MM/yyyy}).");
                }
                lastEndDate = deadline.EndSubmissionDate;
            }
            #endregion

            var responses = new List<RevisionRoundDeadlineResponse>();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Tìm round number l?n nh?t hi?n có d? b?t d?u t? s? ti?p theo
                int lastRoundNumber = existingDeadlines.Any() ? existingDeadlines.Max(d => d.RoundNumber ?? 0) : 0;

                foreach (var deadlineRequest in sortedNewDeadlines)
                {
                    lastRoundNumber++; // Tang round number t? d?ng
                    var revisionRoundDeadline = new RevisionRoundDeadline
                    {
                        RevisionRoundDeadlineId = Guid.NewGuid().ToString(),
                        ResearchConferencePhaseId = researchConferencePhaseId,
                        StartSubmissionDate = deadlineRequest.StartSubmissionDate,
                        EndSubmissionDate = deadlineRequest.EndSubmissionDate,
                        RoundNumber = lastRoundNumber
                    };
                    await _unitOfWork.RevisionRoundDeadlineRepository.CreateCsAsync(revisionRoundDeadline);
                    responses.Add(revisionRoundDeadline.ToRevisionRoundDeadlineResponse());
                }
                await _unitOfWork.CommitAsync();
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackAsync();
                throw e;
            }

            return responses;
        }
        public async Task<List<RevisionRoundDeadlineResponse>> GetRevisionRoundDeadlinesByResearchPhaseIdAsync(string researchConferencePhaseId)
        {
            var deadlines = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(researchConferencePhaseId);
            return deadlines.Select(d => d.ToRevisionRoundDeadlineResponse()).ToList();
        }


        public async Task<RevisionRoundDeadlineResponse> UpdateRevisionRoundDeadlineAsync(string revisionRoundDeadlineId, UpdateRevisionRoundDeadlineRequest request, string userId)
        {
            var deadlineToUpdate = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByIdAsync(revisionRoundDeadlineId);
            if (deadlineToUpdate == null) throw new NotFoundException($"Không tìm th?y deadline v?i ID {revisionRoundDeadlineId}");

            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(deadlineToUpdate.ResearchConferencePhaseId);
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(phase.ConferenceId);

            #region Xác th?c
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n c?p nh?t deadline này.");
            await EnsureConferenceIsEditable(conference);

            var finalStartDate = request.StartSubmissionDate ?? deadlineToUpdate.StartSubmissionDate;
            var finalEndDate = request.EndSubmissionDate ?? deadlineToUpdate.EndSubmissionDate;

            // 1. Start date ph?i tru?c end date
            if (finalStartDate >= finalEndDate)
                throw new BadRequestException("Ngày b?t d?u ph?i tru?c ngày k?t thúc.");

            // 2. Kho?ng th?i gian m?i ph?i n?m trong Phase cha
            if (finalStartDate < phase.ReviseStartDate || finalEndDate > phase.ReviseEndDate)
                throw new BadRequestException($"Kho?ng th?i gian deadline m?i ph?i n?m trong giai do?n s?a d?i c?a phase ({phase.ReviseStartDate:dd/MM/yyyy} - {phase.ReviseEndDate:dd/MM/yyyy}).");

            // 3. Kho?ng th?i gian m?i không du?c ch?ng chéo v?i các deadline KHÁC
            var otherDeadlines = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(phase.ResearchConferencePhaseId);
            foreach (var other in otherDeadlines.Where(d => d.RevisionRoundDeadlineId != revisionRoundDeadlineId))
            {
                if (finalStartDate < other.EndSubmissionDate && finalEndDate > other.StartSubmissionDate)
                {
                    throw new BadRequestException($"Kho?ng th?i gian m?i b? ch?ng chéo v?i Round {other.RoundNumber} ({other.StartSubmissionDate:dd/MM/yyyy} - {other.EndSubmissionDate:dd/MM/yyyy}).");
                }
            }
            #endregion

            deadlineToUpdate.StartSubmissionDate = finalStartDate;
            deadlineToUpdate.EndSubmissionDate = finalEndDate;
            // Không cho phép c?p nh?t RoundNumber

            await _unitOfWork.RevisionRoundDeadlineRepository.UpdateCsAsync(deadlineToUpdate);
            return deadlineToUpdate.ToRevisionRoundDeadlineResponse();
        }

        public async Task<bool> DeleteRevisionRoundDeadlineAsync(string revisionRoundDeadlineId, string userId)
        {
            var deadline = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByIdAsync(revisionRoundDeadlineId);
            if (deadline == null)
            {
                return false; // Không tìm th?y, tr? v? false thay vì NotFound
            }

            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(deadline.ResearchConferencePhaseId);
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(phase.ConferenceId);

            #region Xác th?c
            if (conference.CreatedBy != userId)
                throw new ForbiddenException("B?n không có quy?n xóa deadline này.");
            await EnsureConferenceIsEditable(conference);

            // Thêm ki?m tra: Không cho phép xóa n?u dã có bài n?p trong round này
            var submissionsInRound = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByDeadlineId(revisionRoundDeadlineId);
            if (submissionsInRound.Any())
            {
                throw new BadRequestException($"Không th? xóa Round {deadline.RoundNumber} vì dã có bài báo du?c n?p trong giai do?n này.");
            }
            #endregion

            return await _unitOfWork.RevisionRoundDeadlineRepository.DeleteCsAsync(deadline) > 0;
        }


        #endregion
    }
}