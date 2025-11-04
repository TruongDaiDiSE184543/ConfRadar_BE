using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.Extensions.Options;
using Minio.Exceptions;
using System.Collections.Generic;

namespace ConfRadar.Services.Services
{
    public interface IConferenceStepService
    {
        Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid);
        // Step 1: Basic Conference Creation
        Task<TechnicalConferenceBasicStepResponse> GetConferenceBasicAsync(string conferenceId);
        Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request, string userId);

        // Step 2: Add Conference Prices
        Task<ConferencePriceListWithPhasesResponse> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request, string userId);
        Task<List<ConferencePriceWithPhasesResponse>> GetConferencePricesAsync(string conferenceId);
        Task<ConferencePriceWithPhasesResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request, string user);
        Task<bool> DeleteConferencePriceAsync(string priceId, string userId);

        // Step 3: Add Conference Sessions
        Task<List<ConferenceSessionWithMediaResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request);
        Task<List<ConferenceSessionWithMediaResponse>> GetConferenceSessionsAsync(string conferenceId);
        Task<ConferenceSessionWithMediaResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request);
        Task<SpeakerResponse> UpdateSpeakerAsync(string sessionId, UpdateSpeakerRequest request);
        Task<bool> DeleteConferenceSessionAsync(string sessionId);

        // Step 4: Add Conference Policies
        Task<List<ConferencePolicyResponse>> AddConferencePoliciesAsync(string conferenceId, AddConferencePoliciesRequest request);
        Task<List<ConferencePolicyResponse>> GetConferencePoliciesAsync(string conferenceId);
        Task<ConferencePolicyResponse> UpdateConferencePolicyAsync(string policyId, UpdateConferencePolicyRequest request);
        Task<bool> DeleteConferencePolicyAsync(string policyId);

        // Step 5: Add Conference Media
        Task<List<ConferenceMediaResponse>> AddConferenceMediaAsync(string conferenceId, AddConferenceMediaRequest request);
        Task<List<ConferenceMediaResponse>> GetConferenceMediaAsync(string conferenceId);
        Task<ConferenceMediaResponse> UpdateConferenceMediaAsync(string mediaId, UpdateConferenceMediaRequest request);
        Task<bool> DeleteConferenceMediaAsync(string mediaId);

        // Step 6: Add Conference Sponsors
        Task<List<SponsorResponse>> AddConferenceSponsorsAsync(string conferenceId, AddConferenceSponsorsRequest request);
        Task<List<SponsorResponse>> GetConferenceSponsorsAsync(string conferenceId);
        Task<SponsorResponse> UpdateSponsorAsync(string sponsorId, UpdateSponsorRequest request);
        Task<bool> DeleteSponsorAsync(string sponsorId);

        // Step 7: Add Refund Policies
        Task<List<RefundPolicyResponse>> AddRefundPoliciesAsync(string conferenceId, AddRefundPoliciesRequest request);
        Task<List<RefundPolicyResponse>> GetRefundPoliciesAsync(string conferenceId);
        Task<RefundPolicyResponse> UpdateRefundPolicyAsync(string refundPolicyId, UpdateRefundPolicyRequest request);
        Task<bool> DeleteRefundPolicyAsync(string refundPolicyId);

        // Research Conference Step 1: Basic Research Conference Creation
        Task<ResearchConferenceBasicStepResponse> CreateResearchConferenceBasicAsync(CreateResearchConferenceBasicRequest request, string userid);
        Task<ResearchConferenceBasicStepResponse> GetResearchConferenceBasicAsync(string conferenceId);
        Task<ResearchConferenceBasicStepResponse> UpdateResearchConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request);

        // Research Conference Step 2: Research Conference Detail
        Task<ResearchConferenceDetailResponse> CreateResearchConferenceDetailAsync(string conferenceId, CreateResearchConferenceDetailRequest request);
        Task<ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId);
        Task<ResearchConferenceDetailResponse> UpdateResearchConferenceDetailAsync(string conferenceId, UpdateResearchConferenceDetailRequest request);

        // Research Conference Step 3: Research Conference Phases
        Task<ResearchConferencePhaseResponse> CreateResearchConferencePhaseAsync(string conferenceId, CreateResearchConferencePhaseRequest request);
        Task<ResearchConferencePhaseResponse> GetResearchConferencePhaseAsync(string conferenceId);
        Task<ResearchConferencePhaseResponse> UpdateResearchConferencePhaseAsync(string phaseId, UpdateResearchConferencePhaseRequest request);

        // Research Conference Step 4: Research Conference Sessions (without speakers)
        Task<List<ResearchSessionWithMediaResponse>> AddResearchSessionsAsync(string conferenceId, AddResearchSessionsRequest request);
        Task<List<ResearchSessionWithMediaResponse>> GetResearchSessionsAsync(string conferenceId);
        Task<ResearchSessionWithMediaResponse> UpdateResearchSessionAsync(string sessionId, UpdateConferenceSessionRequest request);
        Task<bool> DeleteResearchSessionAsync(string sessionId);

        // Research Conference Step 5: Material Downloads
        Task<MaterialDownloadResponse> CreateMaterialDownloadAsync(string conferenceId, CreateMaterialDownloadRequest request);
        Task<List<MaterialDownloadResponse>> GetMaterialDownloadsByConferenceIdAsync(string conferenceId);
        Task<MaterialDownloadResponse> UpdateMaterialDownloadAsync(string materialDownloadId, UpdateMaterialDownloadRequest request);
        Task<bool> DeleteMaterialDownloadAsync(string materialDownloadId);

        // Research Conference Step 6: Ranking File URLs
        Task<RankingFileUrlResponse> CreateRankingFileUrlAsync(string conferenceId, CreateRankingFileUrlRequest request);
        Task<List<RankingFileUrlResponse>> GetRankingFileUrlsByConferenceIdAsync(string conferenceId);
        Task<RankingFileUrlResponse> UpdateRankingFileUrlAsync(string rankingFileUrlId, UpdateRankingFileUrlRequest request);
        Task<bool> DeleteRankingFileUrlAsync(string rankingFileUrlId);

        // Research Conference Step 7: Ranking Reference URLs
        Task<RankingReferenceUrlResponse> CreateRankingReferenceUrlAsync(string conferenceId, CreateRankingReferenceUrlRequest request);
        Task<List<RankingReferenceUrlResponse>> GetRankingReferenceUrlsByConferenceIdAsync(string conferenceId);
        Task<RankingReferenceUrlResponse> UpdateRankingReferenceUrlAsync(string referenceUrlId, UpdateRankingReferenceUrlRequest request);
        Task<bool> DeleteRankingReferenceUrlAsync(string referenceUrlId);

        // PricePhase CRUD operations - Create with conferencePriceId, RUD with its own id
        Task<List<PricePhaseResponse>> AddPricePhasesAsync(string conferencePriceId, AddPricePhasesRequest request);
        Task<List<PricePhaseResponse>> GetPricePhasesByConferencePriceIdAsync(string conferencePriceId);
        Task<PricePhaseResponse> UpdatePricePhaseAsync(string pricePhaseId, UpdatePricePhaseRequest request);
        Task<bool> DeletePricePhaseAsync(string pricePhaseId);

        // Speaker CRUD operations - Create with conferenceSessionId, RUD with its own id
        Task<List<SpeakerResponse>> AddSpeakersAsync(string conferenceSessionId, AddSpeakersRequest request);
        Task<List<SpeakerResponse>> GetSpeakersByConferenceSessionIdAsync(string conferenceSessionId);
        Task<SpeakerResponse> UpdateSpeakerBySpeakerIdAsync(string speakerId, UpdateSpeakerRequestForConferenceSession request);
        Task<bool> DeleteSpeakerAsync(string speakerId);

        // Revision Round Deadline CRUD operations - Create with researchConferencePhaseId, RUD with its own id
        Task<List<RevisionRoundDeadlineResponse>> AddRevisionRoundDeadlinesAsync(string researchConferencePhaseId, List<CreateRevisionRoundDeadlineRequest> request);
        Task<List<RevisionRoundDeadlineResponse>> GetRevisionRoundDeadlinesByResearchPhaseIdAsync(string researchConferencePhaseId);
        Task<RevisionRoundDeadlineResponse> UpdateRevisionRoundDeadlineAsync(string revisionRoundDeadlineId, UpdateRevisionRoundDeadlineRequest request);
        Task<bool> DeleteRevisionRoundDeadlineAsync(string revisionRoundDeadlineId);
    }

    public class ConferenceStepService : IConferenceStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITokenService _tokenService;
        private readonly IConferenceService _conferenceService;
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferenceStepService(
            IUnitOfWork unitOfWork,
            IObjectStorageFileService objectStorageFileService,
            ITokenService tokenService,
            IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings,
            IConferenceService conferenceService)
        {
            _unitOfWork = unitOfWork;
            _objectStorageFileService = objectStorageFileService;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings.Value;
            _conferenceService = conferenceService;
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

                // The values from the DB are already the correct local Vietnam time.
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
            // Sử dụng ngày hôm nay theo múi giờ của máy chủ.
            var today = DateOnly.FromDateTime(DateTime.Now);

            // 1. Không có ngày nào được nằm trong quá khứ.
            if (startDate < today || ticketSaleStart < today)
            {
                return false;
            }

            // 2. Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.
            if (startDate > endDate)
            {
                return false;
            }

            // 3. Ngày bắt đầu bán vé phải trước hoặc bằng ngày kết thúc bán vé.
            if (ticketSaleStart > ticketSaleEnd)
            {
                return false;
            }

            // 4. Việc bán vé phải kết thúc trước hoặc trong ngày hội nghị bắt đầu.
            if (ticketSaleEnd > startDate)
            {
                return false;
            }

            // Tất cả kiểm tra đều hợp lệ
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
                throw new BadRequestException("Hội nghị nghiên cứu cần hoàn thành bước 'chi tiết' và 'giai đoạn' trước khi thêm giá vé.");
            }

            if (!request.TypeOfTicket.Any(tot => tot.isAuthor == true))
            {
                throw new BadRequestException("Hội nghị nghiên cứu cần có ít nhất một loại vé dành cho tác giả.");
            }

            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (researchDetail.AllowListener == true)
            {
                if (!request.TypeOfTicket.Any(tot => tot.isAuthor == false))
                {
                    throw new BadRequestException("Hội nghị nghiên cứu này cho phép thính giả, do đó cần có ít nhất một loại vé không dành cho tác giả.");
                }
            }

            return await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByConferenceIdAsync(conferenceId);
        }

        private async Task<bool> checkEachDateHasConferenceSession(Conference conference, List<DateOnly> sessionDate)
        {
            List<DateOnly> allConferenceDate = new();
            for(var date = conference.StartDate; date <= conference.EndDate; date = date.Value.AddDays(1))
            {
                allConferenceDate.Add(date.Value);
            }
            var missingDates = allConferenceDate.Except(sessionDate);
            if (missingDates.Any()) throw new BadRequestException($"Tất cả ngày trong hội nghị phải có session: Đây là những ngày còn thiếu{allConferenceDate.Select(d => d.ToString("yyyy-MM-dd"))}");
            return true;
        }

        private void EnsureConferenceIsEditable(Conference conference)
        {
            var conferenceStatusName = conference.ConferenceStatus?.ConferenceStatusName ?? string.Empty;
            if (conferenceStatusName != "Preparing" && conferenceStatusName != "Pending")
            {
                throw new BadRequestException($"Thao tác không được phép. Hội nghị đang ở trạng thái '{conferenceStatusName}' và không thể chỉnh sửa.");
            }
        }


        #endregion

        #region Step 1: Basic Conference

        public async Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid)
        {
            if (string.IsNullOrWhiteSpace(request.ConferenceName))
                throw new BadRequestException("Tên hội nghị là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Address))
                throw new BadRequestException("Địa chỉ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.ConferenceCategoryId))
                throw new BadRequestException("ID danh mục hội nghị là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.CityId))
                throw new BadRequestException("ID thành phố là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.targetAudienceTechnicalConference))
                throw new BadRequestException("Đối tượng tham dự là bắt buộc đối với hội nghị kỹ thuật.");
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var category = await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId);
                if (category == null)
                {
                    throw new Exception($"Category {request.ConferenceCategoryId} does not exist");
                }

                //Banner image must be image
                if (!_objectStorageFileService.IsValidImageFile(request.BannerImageFile)) throw new BadRequestException($"Không hỗ trợ banner ảnh với đuôi {request.BannerImageFile.ContentType}");


                //Must be conference of type technical
                if (!request.IsResearchConference.HasValue || request.IsResearchConference.Value) throw new BadRequestException("Hội nghị công nghệ yêu cầu IsResearchConference là false");

                if (request.BannerImageFile == null)
                    throw new BadRequestException("Ảnh bìa (banner) là bắt buộc để tạo hội nghị.");

                if (!_objectStorageFileService.IsValidImageFile(request.BannerImageFile))
                    throw new BadRequestException($"Loại ảnh bìa không được hỗ trợ: '{request.BannerImageFile.ContentType}'. Vui lòng sử dụng định dạng ảnh hợp lệ.");

                const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                if (request.BannerImageFile.Length > maxFileSize)
                    throw new BadRequestException("Kích thước tệp ảnh bìa không được vượt quá 5 MB.");

                using var stream = request.BannerImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                request.bannerImageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
                request.bannerImageFileUrl = _objectStorageSettings.EndPoint + request.bannerImageFileUrl;
                


                //Must have category
                if (await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId) == null)
                    throw new NotFoundException($"Danh mục hội nghị với ID '{request.ConferenceCategoryId}' không tồn tại.");

                //Cần có city id
                if (await _unitOfWork.CityRepository.GetCityByIdAsync(request.CityId) == null)
                    throw new NotFoundException($"Thành phố với ID '{request.CityId}' không tồn tại.");


                // check if any of the date is before today and ticketsale Start/end must before start/end date

                var isValidDateValues = IsValidConferenceAndTicketSaleDates(request.StartDate, request.EndDate, request.TicketSaleStart, request.TicketSaleEnd);
                if (!isValidDateValues.Result) throw new BadRequestException("Ngày tháng cung cấp không hợp lệ. Vui lòng đảm bảo các ngày không nằm trong quá khứ, ngày bắt đầu/kết thúc theo đúng thứ tự, và ngày kết thúc bán vé phải trước ngày bắt đầu hội nghị.");

                // must have target audience for the technical detail
                if (string.IsNullOrEmpty(request.targetAudienceTechnicalConference)) throw new BadRequestException("Cần phải có khán giả hướng tới cho buổi hội nghị công nghệ");


                //Total slot for conference must be > 0
                if (request.TotalSlot < 0) throw new Exception("Total slot must be positive");

                //get current time for conference.createAt
                var vietNamTimeZoneNow = ExtensionHelper.GetVietnamDate();

                //check if user is in role Organizer => confstatus = preparing
                //If user is in role Collaborator => confstatus = pendings
                var userRole = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userid);
                var OrganizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Conference Organizer");
                var roleOfUser = userRole.Select(S => S.RoleId);
                Conference toBeCreatedConference;


                var confStatus = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
                if (roleOfUser.Contains(OrganizerRole.RoleId)) toBeCreatedConference = ConferenceStepBasicCreateToModel.creatBasicConference(request, confStatus.Where(s => s.ConferenceStatusName == "Preparing").FirstOrDefault(), vietNamTimeZoneNow, userid);
                else toBeCreatedConference = ConferenceStepBasicCreateToModel.creatBasicConference(request, confStatus.Where(s => s.ConferenceStatusName == "Pending").FirstOrDefault(), vietNamTimeZoneNow, userid);

                await _unitOfWork.ConferenceRepository.CreateConferenceAsync(toBeCreatedConference);
                await _unitOfWork.TechnicalConferenceDetailRepository.CreateTechnicalAsync(new TechnicalConferenceDetail
                {
                    ConferenceId = toBeCreatedConference.ConferenceId,
                    TargetAudience = request.targetAudienceTechnicalConference
                });

                await _unitOfWork.CommitAsync();
                return await GetConferenceBasicAsync(toBeCreatedConference.ConferenceId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
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
                TargetAudience = technical?.TargetAudience
            };
        }

        public async Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Hội nghị với ID {conferenceId} không tìm thấy");

            if (conference.CreatedBy != userId)
            {
                throw new ForbiddenException("Bạn không có quyền cập nhật hội nghị này.");
            }

            var conferenceStatusName = conference.ConferenceStatus?.ConferenceStatusName ?? string.Empty;
            if (conferenceStatusName != "Preparing" && conferenceStatusName != "Pending")
            {
                throw new BadRequestException($"Không thể cập nhật hội nghị vì trạng thái hiện tại là '{conferenceStatusName}'. Chỉ cho phép cập nhật khi ở trạng thái 'Chuẩn bị' (Preparing) hoặc 'Chờ duyệt' (Pending).");
            }

            var finalStartDate = request.StartDate ?? conference.StartDate;
            var finalEndDate = request.EndDate ?? conference.EndDate;
            var finalTicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
            var finalTicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;
            bool isValidDateValues = await IsValidConferenceAndTicketSaleDates(
            finalStartDate.Value,
            finalEndDate.Value,
            finalTicketSaleStart.Value,
            finalTicketSaleEnd.Value
        );

            if (!isValidDateValues) throw new BadRequestException("Ngày mở bán vé phải trước ngày conference diễn và tất cả phải trước hôm nay");

            if (request.TotalSlot <= 0) throw new BadRequestException("Totalslot phải lớn hơn 0");


            if (!string.IsNullOrWhiteSpace(request.ConferenceCategoryId) && request.ConferenceCategoryId != conference.ConferenceCategoryId)
            {
                if (await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId) == null)
                    throw new NotFoundException($"Danh mục hội nghị với ID '{request.ConferenceCategoryId}' không tồn tại.");
            }
            if (!string.IsNullOrWhiteSpace(request.CityId) && request.CityId != conference.CityId)
            {
                if (await _unitOfWork.CityRepository.GetCityByIdAsync(request.CityId) == null)
                    throw new NotFoundException($"Thành phố với ID '{request.CityId}' không tồn tại.");
            }

            if (request.BannerImageFile != null)
            {
                if (!_objectStorageFileService.IsValidImageFile(request.BannerImageFile))
                    throw new BadRequestException($"Loại ảnh bìa không được hỗ trợ: '{request.BannerImageFile.ContentType}'. Vui lòng sử dụng định dạng ảnh hợp lệ.");

                const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                if (request.BannerImageFile.Length > maxFileSize)
                    throw new BadRequestException("Kích thước tệp ảnh bìa không được vượt quá 5 MB.");
            }

            conference.ConferenceName = request.ConferenceName ?? conference.ConferenceName;
            conference.Description = request.Description ?? conference.Description;
            conference.StartDate = request.StartDate ?? conference.StartDate;  // Fixed nullable DateOnly
            conference.EndDate = request.EndDate ?? conference.EndDate;         // Fixed nullable DateOnly
            conference.TotalSlot = request.TotalSlot ?? conference.TotalSlot;
            conference.AvailableSlot = request.TotalSlot ?? conference.AvailableSlot; // Update available slot if total is changed
            conference.Address = request.Address ?? conference.Address;
            //conference.IsInternalHosted = request.IsInternalHosted ?? conference.IsInternalHosted;
            //conference.IsResearchConference = request.IsResearchConference ?? conference.IsResearchConference;
            conference.ConferenceCategoryId = request.ConferenceCategoryId ?? conference.ConferenceCategoryId;
            conference.CityId = request.CityId ?? conference.CityId;
            conference.TicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
            conference.TicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;

            if (request.BannerImageFile != null)
            {
                using var stream = request.BannerImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                conference.BannerImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
            }

            await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);
            return await GetConferenceBasicAsync(conferenceId);
        }

        #endregion

        #region Step 2: Prices

        public async Task<ConferencePriceListWithPhasesResponse> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request, string userId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Hội nghị với ID {conferenceId} không thấy");

            if (conference.CreatedBy != userId)
            {
                throw new ForbiddenException("Bạn không có quyền thêm giá vé cho hội nghị này.");
            }

            ConferencePriceListWithPhasesResponse result = new ConferencePriceListWithPhasesResponse
            {
                conferencePriceWithPhasesResponses = new List<ConferencePriceWithPhasesResponse>()
            };
            var existingConferencePrice = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
            var conferenceStatusName = conference.ConferenceStatus?.ConferenceStatusName ?? string.Empty;
            if (conferenceStatusName != "Preparing" && conferenceStatusName != "Pending")
            {
                throw new BadRequestException($"Không thể thêm giá vé vì trạng thái hội nghị là '{conferenceStatusName}'.");
            }

            if (request.TypeOfTicket == null || !request.TypeOfTicket.Any())
            {
                throw new BadRequestException("Yêu cầu phải chứa ít nhất một loại vé.");
            }

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
                if (totalSlotFromToBeTickets + existingTotalSlot > conference.TotalSlot) throw new BadRequestException($"Số lượng totalSlot của từng loại vé tổng phải nhỏ hơn hoặc bằng capicity của conference: {existingTotalSlot}+ {totalSlotFromToBeTickets} > {conference.TotalSlot} ");
                foreach (CreateConferencePriceRequest toBeConferencePrice in conferencePriceRequest)
                {
                    //Phase for each ticket type
                    List<PricePhaseResponse> pricePhaseResponses = new ();
                    if (toBeConferencePrice.TicketPrice < 0) throw new BadRequestException($"Giá vé cho '{toBeConferencePrice.TicketName}' không được là số âm.");
                    if (toBeConferencePrice.TotalSlot <= 0) throw new BadRequestException($"Số lượng vé cho '{toBeConferencePrice.TicketName}' phải lớn hơn 0.");
                    //check if totalslot of phases in a ticket type is larger than the totalslot of the ticket itself
                    if (toBeConferencePrice.Phases == null || !toBeConferencePrice.Phases.Any()) throw new BadRequestException($"Loại vé '{toBeConferencePrice.TicketName}' phải có ít nhất một giai đoạn bán vé.");
                    int? totalSlotFromPhase = toBeConferencePrice.Phases.Sum(phase => phase.Totalslot);
                    if (toBeConferencePrice.TotalSlot != totalSlotFromPhase) throw new BadRequestException("Tổng totalslot qua từng giai đoạn của vé không thể lớn hơn totalslot của loại vé đó");
                    var CreatedConferencePrice = toBeConferencePrice.ToModel(conferenceId);
                    await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(CreatedConferencePrice);
                    foreach (CreatePricePhaseRequest createPricePhaseRequest in toBeConferencePrice.Phases)
                    {

                        if (string.IsNullOrWhiteSpace(createPricePhaseRequest.PhaseName))
                            throw new BadRequestException($"Tên giai đoạn trong vé '{createPricePhaseRequest.PhaseName}' không được để trống.");
                        if (createPricePhaseRequest.ApplyPercent < 0 || createPricePhaseRequest.ApplyPercent > 100)
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
                        else if (createPricePhaseRequest.StartDate < conference.TicketSaleStart || createPricePhaseRequest.EndDate > conference.TicketSaleEnd) throw new BadRequestException("Start phase phải và endphase phải nằm trong ticket sale start và ticket sale end của conference");
                        var CreatedPricePhase = createPricePhaseRequest.ToModel(CreatedConferencePrice.ConferencePriceId);
                        await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(CreatedPricePhase);
                        pricePhaseResponses.Add(new PricePhaseResponse
                        {
                            PhaseName = createPricePhaseRequest.PhaseName,
                            StartDate = createPricePhaseRequest.StartDate,
                            EndDate = createPricePhaseRequest.EndDate,
                            ApplyPercent = createPricePhaseRequest.ApplyPercent,
                            TotalSlot = createPricePhaseRequest.Totalslot,
                            PricePhaseId = CreatedPricePhase.PricePhaseId,
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
                throw new NotFoundException($"Không tìm thấy loại vé với ID {priceId}");
            }

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(price.ConferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Không tìm thấy hội nghị gốc liên quan đến loại vé này.");
            }

            

            // 1. Phân quyền
            if (conference.CreatedBy != userId)
            {
                throw new ForbiddenException("Bạn không có quyền cập nhật loại vé này.");
            }

            EnsureConferenceIsEditable(conference);

            if (request.TicketPrice.HasValue && request.TicketPrice.Value < 0)
                throw new BadRequestException("Giá vé không được là số âm.");

            if (request.TotalSlot.HasValue)
            {
                if (request.TotalSlot.Value <= 0)
                    throw new BadRequestException("Số lượng vé phải lớn hơn 0.");

                int soldTicketsForThisPrice = price.TotalSlot.GetValueOrDefault() - price.AvailableSlot.GetValueOrDefault();
                if (request.TotalSlot.Value < soldTicketsForThisPrice)
                {
                    throw new BadRequestException($"Không thể giảm số lượng vé xuống {request.TotalSlot.Value} vì đã có {soldTicketsForThisPrice} vé được bán cho loại vé này.");
                }

                var allConferencePrices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(price.ConferenceId);
                var otherPricesTotalSlot = allConferencePrices
                    .Where(p => p.ConferencePriceId != priceId)
                    .Sum(p => p.TotalSlot ?? 0);
                var newConferenceTotalSlot = otherPricesTotalSlot + request.TotalSlot.Value;

                if (newConferenceTotalSlot > conference.TotalSlot)
                {
                    throw new BadRequestException($"Cập nhật thất bại. Tổng số vé mới ({newConferenceTotalSlot}) sẽ vượt quá giới hạn {conference.TotalSlot} của hội nghị.");
                }
            }

            // 5. Ngăn chặn tên vé trùng lặp
            if (!string.IsNullOrWhiteSpace(request.TicketName) && !request.TicketName.Equals(price.TicketName, StringComparison.OrdinalIgnoreCase))
            {
                var existingPrices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(price.ConferenceId);
                if (existingPrices.Any(p => p.TicketName.Equals(request.TicketName, StringComparison.OrdinalIgnoreCase) && p.ConferencePriceId != priceId))
                {
                    throw new BadRequestException($"Tên vé '{request.TicketName}' đã tồn tại trong hội nghị này.");
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
                // Giữ nguyên NotFoundException để không tiết lộ sự tồn tại của dữ liệu
                throw new NotFoundException($"Không tìm thấy loại vé với ID {priceId}");
            }

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(price.ConferenceId);
            // 1. Phân quyền
            if (conference.CreatedBy != userId)
            {
                throw new ForbiddenException("Bạn không có quyền xóa loại vé này.");
            }
            EnsureConferenceIsEditable(conference);
            // Check if there are any tickets already sold for this price
            var ticketCount = await _unitOfWork.TicketRepository.GetTicketCountByConferencePriceIdAsync(priceId);
            if (ticketCount > 0) throw new BadRequestException("Cannot delete price because tickets have already been sold for this price");

            return await _unitOfWork.ConferencePriceRepository.DeleteConferencePriceAsync(price) > 0;
        }

        #endregion

        #region Step 3: Sessions

        public async Task<List<ConferenceSessionWithMediaResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");
            List<DateOnly> sessionDates = (List<DateOnly>)request.Sessions.Select(s => s.Date);
            if (!checkEachDateHasConferenceSession(conference, sessionDates).Result) throw new Exception();
            var responses = new List<ConferenceSessionWithMediaResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Sessions != null)
                {
                    foreach (var session in request.Sessions)
                    {
                        if (session.RoomId == null || session.StartTime == null || session.EndTime == null || session.Date == null)
                            throw new BadRequestException("Session cần có RoomId, StartTime, EndTime, and Date.");

                        if (await _unitOfWork.RoomRepository.GetRoomByIdAsync(session.RoomId) == null)
                            throw new NotFoundException($"Room with ID {session.RoomId} not found");

                        var sessionStartDateTime = session.Date.Value.ToDateTime(session.StartTime.Value);
                        var sessionEndDateTime = session.Date.Value.ToDateTime(session.EndTime.Value);

                        // Step 2: Validate using these direct, local time values.
                        await ValidateSessionTimeAvailability(sessionStartDateTime, sessionEndDateTime, session.RoomId);

                        var conferenceSession = session.ToModel(conferenceId);
                        await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);

                        // Add speakers for the session
                        if (session.Speaker != null)
                        {
                            foreach (var speakerRequest in session.Speaker)
                            {
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
                        //if (result <= 0) throw new Exception("Không tạo được");
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

        public async Task<ConferenceSessionWithMediaResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            var newStartTime = request.StartTime ?? TimeOnly.FromDateTime(session.StartTime.Value);
            var newEndTime = request.EndTime ?? TimeOnly.FromDateTime(session.EndTime.Value);
            var newDate = request.Date ?? session.SessionDate;
            var newRoomId = request.RoomId ?? session.RoomId;

            if (newStartTime == null || newEndTime == null || newDate == null || newRoomId == null)
                throw new BadRequestException("Session must have a RoomId, StartTime, EndTime, and Date.");

            // Validate session time availability
            var startDateTime = new DateTime(newDate.Value.Year, newDate.Value.Month, newDate.Value.Day);
            var endDateTime = new DateTime(newDate.Value.Year, newDate.Value.Month, newDate.Value.Day);

            startDateTime = startDateTime.AddHours(newStartTime.Hour).AddMinutes(newStartTime.Minute);
            endDateTime = endDateTime.AddHours(newEndTime.Hour).AddMinutes(newEndTime.Minute);

            await ValidateSessionTimeAvailability(startDateTime, endDateTime, newRoomId, sessionId);

            session.Title = request.Title ?? session.Title;
            session.Description = request.Description ?? session.Description;
            session.StartTime = startDateTime;
            session.EndTime = endDateTime;
            session.SessionDate = newDate;
            session.RoomId = newRoomId;

            await _unitOfWork.ConferenceSessionRepository.UpdateConferenceSessionAsync(session);

            var updatedSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
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

        public async Task<List<ConferencePolicyResponse>> AddConferencePoliciesAsync(string conferenceId, AddConferencePoliciesRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

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

        public async Task<ConferencePolicyResponse> UpdateConferencePolicyAsync(string policyId, UpdateConferencePolicyRequest request)
        {
            var policy = await _unitOfWork.ConferencePolicyRepository.GetConferencePolicyByIdAsync(policyId);
            if (policy == null) throw new NotFoundException($"Conference policy with ID {policyId} not found");

            if (!string.IsNullOrEmpty(request.PolicyName)) policy.PolicyName = request.PolicyName;
            if (!string.IsNullOrEmpty(request.Description)) policy.Description = request.Description;

            await _unitOfWork.ConferencePolicyRepository.UpdateConferencePolicyAsync(policy);
            return policy.ToResponse();
        }

        public async Task<bool> DeleteConferencePolicyAsync(string policyId)
        {
            var policy = await _unitOfWork.ConferencePolicyRepository.GetConferencePolicyByIdAsync(policyId);
            if (policy == null) throw new NotFoundException($"Conference policy with ID {policyId} not found");

            return await _unitOfWork.ConferencePolicyRepository.DeleteConferencePolicyAsync(policy) > 0;
        }

        #endregion

        #region Step 5: Media

        public async Task<List<ConferenceMediaResponse>> AddConferenceMediaAsync(string conferenceId, AddConferenceMediaRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferenceMediaResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var media in request.Media)
                {
                    string? mediaUrl = media.MediaUrl;
                    if (media.MediaFile != null)
                    {
                        //if (await _unitOfWork.MediaTypeRepository.GetMediaTypeByIdAsync(media.MediaTypeId) == null) throw new NotFoundException($"Media type with ID {media.MediaTypeId} not found");
                        using var stream = media.MediaFile.OpenReadStream();
                        var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(media.MediaFile.FileName);
                        mediaUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, media.MediaFile.ContentType);
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

        public async Task<ConferenceMediaResponse> UpdateConferenceMediaAsync(string mediaId, UpdateConferenceMediaRequest request)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetConferenceMediaByIdAsync(mediaId);
            if (media == null) throw new NotFoundException($"Conference media with ID {mediaId} not found");

            if (request.MediaFile != null)
            {
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
            return new ConferenceMediaResponse { MediaId = media.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(media.ConferenceMediaUrl) };
        }

        public async Task<bool> DeleteConferenceMediaAsync(string mediaId)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetConferenceMediaByIdAsync(mediaId);
            if (media == null) throw new NotFoundException($"Conference media with ID {mediaId} not found");
            return await _unitOfWork.ConferenceMediaRepository.DeleteConferenceMediaAsync(media) > 0;
        }

        #endregion

        #region Step 6: Sponsors

        public async Task<List<SponsorResponse>> AddConferenceSponsorsAsync(string conferenceId, AddConferenceSponsorsRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<SponsorResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Sponsors != null)
                {
                    foreach (var sponsor in request.Sponsors)
                    {
                        if (!_objectStorageFileService.IsValidImageFile(sponsor.ImageFile)) throw new BadRequestException($"Không hỗ trợ{sponsor.ImageFile.ContentType}");
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

        public async Task<SponsorResponse> UpdateSponsorAsync(string sponsorId, UpdateSponsorRequest request)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetSponsorByIdAsync(sponsorId);
            if (sponsor == null) throw new NotFoundException($"Conference sponsor with ID {sponsorId} not found");

            if (!string.IsNullOrEmpty(request.Name)) sponsor.Name = request.Name;

            if (request.ImageFile != null)
            {
                using var stream = request.ImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.ImageFile.FileName);
                sponsor.ImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.sponsorimage.ToString(), uniqueFileName, stream, request.ImageFile.ContentType);
                sponsor.ImageUrl = _objectStorageSettings.EndPoint + sponsor.ImageUrl;
            }
            else if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                sponsor.ImageUrl = request.ImageUrl;
            }

            await _unitOfWork.SponsorRepository.UpdateSponsorAsync(sponsor);
            return sponsor.ToResponse();
        }

        public async Task<bool> DeleteSponsorAsync(string sponsorId)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetSponsorByIdAsync(sponsorId);
            if (sponsor == null) throw new NotFoundException($"Conference sponsor with ID {sponsorId} not found");

            return await _unitOfWork.SponsorRepository.DeleteSponsorAsync(sponsor) > 0;
        }

        #endregion

        #region Step 7: Refund Policies

        public async Task<List<RefundPolicyResponse>> AddRefundPoliciesAsync(string conferenceId, AddRefundPoliciesRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<RefundPolicyResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.RefundPolicies != null)
                {
                    foreach (var refundPolicy in request.RefundPolicies)
                    {
                        var refundPolicyModel = refundPolicy.ToModel(conferenceId);
                        await _unitOfWork.ConferenceRefundPolicyRepository.CreateConferenceRefundPolicyAsync(refundPolicyModel);
                        responses.Add(refundPolicyModel.ToResponse());
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

        public async Task<List<RefundPolicyResponse>> GetRefundPoliciesAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var refundPolicies = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByConferenceIdAsync(conferenceId);
            return refundPolicies.Select(rp => rp.ToResponse()).ToList();
        }

        public async Task<RefundPolicyResponse> UpdateRefundPolicyAsync(string refundPolicyId, UpdateRefundPolicyRequest request)
        {
            var refundPolicy = await _unitOfWork.ConferenceRefundPolicyRepository.GetConferenceRefundPolicyByIdAsync(refundPolicyId);
            if (refundPolicy == null) throw new NotFoundException($"Refund policy with ID {refundPolicyId} not found");

            if (request.PercentRefund.HasValue) refundPolicy.PercentRefund = request.PercentRefund;
            if (request.RefundDeadline.HasValue) refundPolicy.RefundDeadline = request.RefundDeadline;
            if (request.RefundOrder.HasValue) refundPolicy.RefundOrder = request.RefundOrder;

            await _unitOfWork.ConferenceRefundPolicyRepository.UpdateConferenceRefundPolicyAsync(refundPolicy);
            return refundPolicy.ToResponse();
        }

        public async Task<bool> DeleteRefundPolicyAsync(string refundPolicyId)
        {
            var refundPolicy = await _unitOfWork.ConferenceRefundPolicyRepository.GetConferenceRefundPolicyByIdAsync(refundPolicyId);
            if (refundPolicy == null) throw new NotFoundException($"Refund policy with ID {refundPolicyId} not found");

            return await _unitOfWork.ConferenceRefundPolicyRepository.DeleteConferenceRefundPolicyAsync(refundPolicy) > 0;
        }

        #endregion

        #region Research Conference Step 1: Basic Research Conference

        public async Task<ResearchConferenceBasicStepResponse> CreateResearchConferenceBasicAsync(CreateResearchConferenceBasicRequest request, string userid)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var category = await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId);
                if (category == null)
                {
                    throw new Exception($"Category {request.ConferenceCategoryId} does not exist");
                }

                if (!_objectStorageFileService.IsValidImageFile(request.BannerImageFile)) throw new BadRequestException($"Banner ảnh không hỗ trợ extension{request.BannerImageFile.ContentType}");
                request.createdby = userid;

                //Must be research conference
                if (!request.IsResearchConference.HasValue || !request.IsResearchConference.Value) throw new BadRequestException("Phải là hội nghị học thuật và giá trị IsResearchConference phải bằng true");


                //Must be internally hosted
                if (!request.IsInternalHosted.HasValue || !request.IsInternalHosted.Value) throw new BadRequestException("Hội nghị nghiên cứu phải được tổ chức bởi người thuộc ConfRadar");

                if (request.BannerImageFile != null)
                {
                    using var stream = request.BannerImageFile.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                    request.bannerImageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
                    request.bannerImageFileUrl = _objectStorageSettings.EndPoint + request.bannerImageFileUrl;
                }

                var isValidDateValues = IsValidConferenceAndTicketSaleDates(request.StartDate, request.EndDate, request.TicketSaleStart, request.TicketSaleEnd);
                if (!isValidDateValues.Result) throw new BadRequestException("Ngày mở bán vé phải trước ngày conference diễn và tất cả phải trước hôm nay");

                if (request.TotalSlot < 0)
                    throw new Exception("Total slot must be positive");



                Conference toBeCreatedConference;
                var confStatus = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
                toBeCreatedConference = request.ToModel(confStatus.Where(s => s.ConferenceStatusName == "Preparing").FirstOrDefault(), ExtensionHelper.GetVietnamDate());

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

        public async Task<ResearchConferenceBasicStepResponse> UpdateResearchConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            conference.ConferenceName = request.ConferenceName ?? conference.ConferenceName;
            conference.Description = request.Description ?? conference.Description;
            conference.StartDate = request.StartDate;  // Fixed nullable DateOnly
            conference.EndDate = request.EndDate;         // Fixed nullable DateOnly
            conference.TotalSlot = request.TotalSlot ?? conference.TotalSlot;
            conference.AvailableSlot = request.TotalSlot ?? conference.AvailableSlot; // Update available slot if total is changed
            conference.Address = request.Address ?? conference.Address;
            //conference.IsInternalHosted = request.IsInternalHosted ?? conference.IsInternalHosted;
            //conference.IsResearchConference = request.IsResearchConference ?? conference.IsResearchConference;
            conference.ConferenceCategoryId = request.ConferenceCategoryId ?? conference.ConferenceCategoryId;
            conference.CityId = request.CityId ?? conference.CityId;
            conference.TicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
            conference.TicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;

            if (request.BannerImageFile != null)
            {
                using var stream = request.BannerImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                conference.BannerImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
            }

            await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);
            return await GetResearchConferenceBasicAsync(conferenceId);
        }

        #endregion

        #region Research Conference Step 2: Research Conference Detail

        public async Task<ResearchConferenceDetailResponse> CreateResearchConferenceDetailAsync(string conferenceId, CreateResearchConferenceDetailRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var researchDetail = request.ToModel(conferenceId);

            await _unitOfWork.ResearchConferenceDetailRepository.CreateResearchConferenceDetailAsync(researchDetail);
            return researchDetail.ToResponse();
        }

        public async Task<ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId)
        {
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (researchDetail == null) throw new NotFoundException($"Research conference detail for conference ID {conferenceId} not found");

            return researchDetail.ToResponse();
        }

        public async Task<ResearchConferenceDetailResponse> UpdateResearchConferenceDetailAsync(string conferenceId, UpdateResearchConferenceDetailRequest request)
        {
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
            if (researchDetail == null) throw new NotFoundException($"Research conference detail for conference ID {conferenceId} not found");

            researchDetail.Name = request.Name ?? researchDetail.Name;
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
            return researchDetail.ToResponse();
        }

        #endregion

        #region Research Conference Step 3: Research Conference Phases

        public async Task<ResearchConferencePhaseResponse> CreateResearchConferencePhaseAsync(string conferenceId, CreateResearchConferencePhaseRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var phase = request.ToModel(conferenceId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(phase);

                // Create revision round deadlines if provided
                if (request.RevisionRoundDeadlines != null)
                {
                    foreach (var deadline in request.RevisionRoundDeadlines)
                    {
                        var revisionRoundDeadline = deadline.ToModel(phase.ResearchConferencePhaseId);
                        await _unitOfWork.RevisionRoundDeadlineRepository.CreateCsAsync(revisionRoundDeadline);
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return phase.ToResponse();
        }

        public async Task<ResearchConferencePhaseResponse> GetResearchConferencePhaseAsync(string conferenceId)
        {
            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByConferenceIdAsync(conferenceId);
            if (phase == null) throw new NotFoundException($"Research conference phase for conference ID {conferenceId} not found");

            return phase.ToResponse();
        }

        public async Task<ResearchConferencePhaseResponse> UpdateResearchConferencePhaseAsync(string phaseId, UpdateResearchConferencePhaseRequest request)
        {
            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByConferenceIdAsync(phaseId);
            if (phase == null) throw new NotFoundException($"Research conference phase with ID {phaseId} not found");

            phase.RegistrationStartDate = request.RegistrationStartDate ?? phase.RegistrationStartDate;
            phase.RegistrationEndDate = request.RegistrationEndDate ?? phase.RegistrationEndDate;
            phase.FullPaperStartDate = request.FullPaperStartDate ?? phase.FullPaperStartDate;
            phase.FullPaperEndDate = request.FullPaperEndDate ?? phase.FullPaperEndDate;
            phase.ReviewStartDate = request.ReviewStartDate ?? phase.ReviewStartDate;
            phase.ReviewEndDate = request.ReviewEndDate ?? phase.ReviewEndDate;
            phase.ReviseStartDate = request.ReviseStartDate ?? phase.ReviseStartDate;
            phase.ReviseEndDate = request.ReviseEndDate ?? phase.ReviseEndDate;
            phase.CameraReadyStartDate = request.CameraReadyStartDate ?? phase.CameraReadyStartDate;
            phase.CameraReadyEndDate = request.CameraReadyEndDate ?? phase.CameraReadyEndDate;
            phase.IsWaitlist = request.IsWaitlist ?? phase.IsWaitlist;
            phase.IsActive = request.IsActive ?? phase.IsActive;

            await _unitOfWork.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(phase);
            return phase.ToResponse();
        }

        #endregion

        #region Research Conference Step 4: Research Conference Sessions (without speakers)

        public async Task<List<ResearchSessionWithMediaResponse>> AddResearchSessionsAsync(string conferenceId, AddResearchSessionsRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");
            List<DateOnly> sessionDates = (List<DateOnly>)request.Sessions.Select(s => s.Date);
            if (!checkEachDateHasConferenceSession(conference, sessionDates).Result) throw new Exception();

            var responses = new List<ResearchSessionWithMediaResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Sessions != null)
                {
                    foreach (var session in request.Sessions)
                    {
                        if (session.RoomId == null || session.StartTime == null || session.EndTime == null || session.Date == null)
                            throw new BadRequestException("Session must have a RoomId, StartTime, EndTime, and Date.");

                        if (await _unitOfWork.RoomRepository.GetRoomByIdAsync(session.RoomId) == null)
                            throw new NotFoundException($"Room with ID {session.RoomId} not found");

                        // Validate session time availability
                        var sessionStartDateTime = session.Date.Value.ToDateTime(session.StartTime.Value);
                        var sessionEndDateTime = session.Date.Value.ToDateTime(session.EndTime.Value);

                        // Step 2: Validate using these direct, local time values.
                        await ValidateSessionTimeAvailability(sessionStartDateTime, sessionEndDateTime, session.RoomId);



                        var conferenceSession = session.ToModel(conferenceId);
                        await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);

                        // Add media for the session (no speakers for research sessions)
                        if (session.SessionMedias != null)
                        {
                            foreach (var mediaRequest in session.SessionMedias)
                            {
                                string sessionMedia = "";
                                if (mediaRequest.MediaFile != null)
                                {
                                    using var stream = mediaRequest.MediaFile.OpenReadStream();
                                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(mediaRequest.MediaFile.FileName);
                                    sessionMedia = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencesessionmedia.ToString(), uniqueFileName, stream, mediaRequest.MediaFile.ContentType);
                                    sessionMedia = _objectStorageSettings.EndPoint + sessionMedia;
                                }
                                var conferenceSessionMedia = mediaRequest.ToModel(conferenceSession.ConferenceSessionId, sessionMedia);

                                await _unitOfWork.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(conferenceSessionMedia);
                            }
                        }

                        // Get updated session with all details
                        var createdSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(conferenceSession.ConferenceSessionId);
                        responses.Add(createdSession.ToResearchResponseWithMedia());
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

        public async Task<ResearchSessionWithMediaResponse> UpdateResearchSessionAsync(string sessionId, UpdateConferenceSessionRequest request)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            var newStartTime = request.StartTime ?? TimeOnly.FromDateTime(session.StartTime.Value);
            var newEndTime = request.EndTime ?? TimeOnly.FromDateTime(session.EndTime.Value);
            var newDate = request.Date ?? session.SessionDate;
            var newRoomId = request.RoomId ?? session.RoomId;

            if (newStartTime == null || newEndTime == null || newDate == null || newRoomId == null)
                throw new BadRequestException("Session must have a RoomId, StartTime, EndTime, and Date.");

            // Validate session time availability
            var startDateTime = new DateTime(newDate.Value.Year, newDate.Value.Month, newDate.Value.Day);
            var endDateTime = new DateTime(newDate.Value.Year, newDate.Value.Month, newDate.Value.Day);

            startDateTime = startDateTime.AddHours(newStartTime.Hour).AddMinutes(newStartTime.Minute);
            endDateTime = endDateTime.AddHours(newEndTime.Hour).AddMinutes(newEndTime.Minute);

            await ValidateSessionTimeAvailability(startDateTime, endDateTime, newRoomId, sessionId);

            session.Title = request.Title ?? session.Title;
            session.Description = request.Description ?? session.Description;
            session.StartTime = startDateTime;
            session.EndTime = endDateTime;
            session.SessionDate = newDate;
            session.RoomId = newRoomId;

            await _unitOfWork.ConferenceSessionRepository.UpdateConferenceSessionAsync(session);

            var updatedSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            return updatedSession.ToResearchResponseWithMedia();
        }

        public async Task<bool> DeleteResearchSessionAsync(string sessionId)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            // Delete all media associated with this session (no speakers for research sessions)
            var mediaList = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(sessionId);
            foreach (var media in mediaList)
            {
                await _unitOfWork.ConferenceSessionMediumRepository.DeleteConferenceSessionMediumAsync(media);
            }

            return await _unitOfWork.ConferenceSessionRepository.DeleteConferenceSessionAsync(session) > 0;
        }

        #endregion

        #region Research Conference Step 5: Material Downloads

        public async Task<MaterialDownloadResponse> CreateMaterialDownloadAsync(string conferenceId, CreateMaterialDownloadRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var materialDownload = request.ToModel(conferenceId);

            // Handle file upload if provided
            if (request.File != null)
            {
                using var stream = request.File.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
                materialDownload.FileName = _objectStorageSettings.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.materialdownload.ToString(), uniqueFileName, stream, request.File.ContentType);
            }

            await _unitOfWork.MaterialDownloadRepository.CreateMaterialDownloadAsync(materialDownload);
            return materialDownload.ToResponse();
        }

        public async Task<List<MaterialDownloadResponse>> GetMaterialDownloadsByConferenceIdAsync(string conferenceId)
        {
            var materials = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conferenceId);
            return materials.Select(m => m.ToResponse()).ToList();
        }

        public async Task<MaterialDownloadResponse> UpdateMaterialDownloadAsync(string materialDownloadId, UpdateMaterialDownloadRequest request)
        {
            var materialDownload = await _unitOfWork.MaterialDownloadRepository.GetMaterialDownloadByIdAsync(materialDownloadId);
            if (materialDownload == null) throw new NotFoundException($"Material download with ID {materialDownloadId} not found");


            if (!string.IsNullOrEmpty(request.FileDescription)) materialDownload.FileDescription = request.FileDescription;

            // Handle file upload if provided
            if (request.File != null)
            {
                using var stream = request.File.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
                materialDownload.FileName = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.materialdownload.ToString(), uniqueFileName, stream, request.File.ContentType);
            }

            await _unitOfWork.MaterialDownloadRepository.UpdateMaterialDownloadAsync(materialDownload);
            return materialDownload.ToResponse();
        }

        public async Task<bool> DeleteMaterialDownloadAsync(string materialDownloadId)
        {
            var materialDownload = await _unitOfWork.MaterialDownloadRepository.GetMaterialDownloadByIdAsync(materialDownloadId);
            if (materialDownload == null) throw new NotFoundException($"Material download with ID {materialDownloadId} not found");

            return await _unitOfWork.MaterialDownloadRepository.DeleteMaterialDownloadAsync(materialDownload) > 0;
        }

        #endregion

        #region Research Conference Step 6: Ranking File URLs

        public async Task<RankingFileUrlResponse> CreateRankingFileUrlAsync(string conferenceId, CreateRankingFileUrlRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var rankingFileUrl = request.ToModel(conferenceId);

            // Handle file upload if provided
            if (request.File != null)
            {
                using var stream = request.File.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
                rankingFileUrl.FileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.rankingfile.ToString(), uniqueFileName, stream, request.File.ContentType);
                rankingFileUrl.FileUrl = _objectStorageSettings.EndPoint + rankingFileUrl.FileUrl;
            }

            await _unitOfWork.RankingFileUrlRepository.CreateRankingFileUrlAsync(rankingFileUrl);
            return rankingFileUrl.ToResponse();
        }

        public async Task<List<RankingFileUrlResponse>> GetRankingFileUrlsByConferenceIdAsync(string conferenceId)
        {
            var fileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conferenceId);
            return fileUrls.Select(f => f.ToResponse()).ToList();
        }

        public async Task<RankingFileUrlResponse> UpdateRankingFileUrlAsync(string rankingFileUrlId, UpdateRankingFileUrlRequest request)
        {
            var rankingFileUrl = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlByIdAsync(rankingFileUrlId);
            if (rankingFileUrl == null) throw new NotFoundException($"Ranking file URL with ID {rankingFileUrlId} not found");

            if (!string.IsNullOrEmpty(request.FileUrl)) rankingFileUrl.FileUrl = request.FileUrl;

            // Handle file upload if provided
            if (request.File != null)
            {
                using var stream = request.File.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.File.FileName);
                rankingFileUrl.FileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.rankingfile.ToString(), uniqueFileName, stream, request.File.ContentType);
                rankingFileUrl.FileUrl = _objectStorageSettings.EndPoint + rankingFileUrl.FileUrl;
            }

            await _unitOfWork.RankingFileUrlRepository.UpdateRankingFileUrlAsync(rankingFileUrl);
            return rankingFileUrl.ToResponse();
        }

        public async Task<bool> DeleteRankingFileUrlAsync(string rankingFileUrlId)
        {
            var rankingFileUrl = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlByIdAsync(rankingFileUrlId);
            if (rankingFileUrl == null) throw new NotFoundException($"Ranking file URL with ID {rankingFileUrlId} not found");

            return await _unitOfWork.RankingFileUrlRepository.DeleteRankingFileUrlAsync(rankingFileUrl) > 0;
        }

        #endregion

        #region Research Conference Step 7: Ranking Reference URLs

        public async Task<RankingReferenceUrlResponse> CreateRankingReferenceUrlAsync(string conferenceId, CreateRankingReferenceUrlRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var rankingReferenceUrl = request.ToModel(conferenceId);

            await _unitOfWork.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(rankingReferenceUrl);
            return rankingReferenceUrl.ToResponse();
        }

        public async Task<List<RankingReferenceUrlResponse>> GetRankingReferenceUrlsByConferenceIdAsync(string conferenceId)
        {
            var referenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conferenceId);
            return referenceUrls.Select(r => r.ToResponse()).ToList();
        }

        public async Task<RankingReferenceUrlResponse> UpdateRankingReferenceUrlAsync(string referenceUrlId, UpdateRankingReferenceUrlRequest request)
        {
            var rankingReferenceUrl = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(referenceUrlId);
            if (rankingReferenceUrl == null) throw new NotFoundException($"Ranking reference URL with ID {referenceUrlId} not found");

            if (!string.IsNullOrEmpty(request.ReferenceUrl)) rankingReferenceUrl.ReferenceUrl = request.ReferenceUrl;

            await _unitOfWork.RankingReferenceUrlRepository.UpdateRankingReferenceUrlAsync(rankingReferenceUrl);
            return rankingReferenceUrl.ToResponse();
        }

        public async Task<bool> DeleteRankingReferenceUrlAsync(string referenceUrlId)
        {
            var rankingReferenceUrl = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(referenceUrlId);
            if (rankingReferenceUrl == null) throw new NotFoundException($"Ranking reference URL with ID {referenceUrlId} not found");

            return await _unitOfWork.RankingReferenceUrlRepository.DeleteRankingReferenceUrlAsync(rankingReferenceUrl) > 0;
        }

        #endregion

        #region PricePhase CRUD Operations

        public async Task<List<PricePhaseResponse>> AddPricePhasesAsync(string conferencePriceId, AddPricePhasesRequest request)
        {
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(conferencePriceId);
            if (conferencePrice == null) throw new NotFoundException($"Conference price with ID {conferencePriceId} not found");

            var responses = new List<PricePhaseResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.PricePhases != null)
                {
                    foreach (var pricePhaseRequest in request.PricePhases)
                    {
                        // Check if the sum of total slots doesn't exceed the conference price total slot
                        var existingPhases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(conferencePriceId);
                        var existingTotalSlot = existingPhases.Sum(p => p.TotalSlot) ?? 0;
                        if (existingTotalSlot + pricePhaseRequest.TotalSlot > conferencePrice.TotalSlot)
                        {
                            throw new BadRequestException($"Tổng số lượng slot của các giai đoạn vượt quá tổng slot của vé: {conferencePrice.TotalSlot}");
                        }

                        var pricePhase = pricePhaseRequest.ToModel(conferencePriceId);
                        await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(pricePhase);
                        responses.Add(pricePhase.ToResponse());
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

        public async Task<List<PricePhaseResponse>> GetPricePhasesByConferencePriceIdAsync(string conferencePriceId)
        {
            var pricePhases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(conferencePriceId);
            return pricePhases.Select(p => p.ToResponse()).ToList();
        }

        public async Task<PricePhaseResponse> UpdatePricePhaseAsync(string pricePhaseId, UpdatePricePhaseRequest request)
        {
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(pricePhaseId);
            if (pricePhase == null) throw new NotFoundException($"Price phase with ID {pricePhaseId} not found");

            if (!string.IsNullOrEmpty(request.PhaseName)) pricePhase.PhaseName = request.PhaseName;
            if (request.ApplyPercent.HasValue) pricePhase.ApplyPercent = request.ApplyPercent;
            if (request.StartDate.HasValue) pricePhase.StartDate = request.StartDate;
            if (request.EndDate.HasValue) pricePhase.EndDate = request.EndDate;
            if (request.TotalSlot.HasValue)
            {
                pricePhase.TotalSlot = request.TotalSlot;
                pricePhase.AvailableSlot = request.TotalSlot; // Update available slot when total is changed
            }

            await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);
            return pricePhase.ToResponse();
        }

        public async Task<bool> DeletePricePhaseAsync(string pricePhaseId)
        {
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(pricePhaseId);
            if (pricePhase == null) throw new NotFoundException($"Price phase with ID {pricePhaseId} not found");

            return await _unitOfWork.PricePhaseRepository.DeletePricePhaseAsync(pricePhase) > 0;
        }

        #endregion

        #region Speaker CRUD Operations

        public async Task<List<SpeakerResponse>> AddSpeakersAsync(string conferenceSessionId, AddSpeakersRequest request)
        {
            var conferenceSession = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(conferenceSessionId);
            if (conferenceSession == null) throw new NotFoundException($"Conference session with ID {conferenceSessionId} not found");

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

        public async Task<SpeakerResponse> UpdateSpeakerBySpeakerIdAsync(string speakerId, UpdateSpeakerRequestForConferenceSession request)
        {
            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(speakerId);
            if (speaker == null) throw new NotFoundException($"Speaker with ID {speakerId} not found");

            if (!string.IsNullOrEmpty(request.Name)) speaker.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) speaker.Description = request.Description;

            if (request.Image != null)
            {
                using var stream = request.Image.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.Image.FileName);
                speaker.Image = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, request.Image.ContentType);
                speaker.Image = _objectStorageSettings.EndPoint + speaker.Image;
            }

            await _unitOfWork.SpeakerRepository.UpdateSpeakerAsync(speaker);
            return speaker.ToResponse();
        }

        public async Task<bool> DeleteSpeakerAsync(string speakerId)
        {
            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(speakerId);
            if (speaker == null) throw new NotFoundException($"Speaker with ID {speakerId} not found");

            return await _unitOfWork.SpeakerRepository.DeleteSpeakerAsync(speaker) > 0;
        }

        #endregion

        #region Revision Round Deadline CRUD Operations

        public async Task<List<RevisionRoundDeadlineResponse>> AddRevisionRoundDeadlinesAsync(string researchConferencePhaseId, List<CreateRevisionRoundDeadlineRequest> request)
        {
            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(researchConferencePhaseId);
            if (phase == null) throw new NotFoundException($"Phase Hội nghị nghiên cứu  ID {researchConferencePhaseId} không thấy");

            //list round phải bắt đầu từ 1 và khác nhau
            List<int> round = request.Select( r => r.RoundNumber ).ToList();
            bool isValid = round.Count > 0 &&
                           round.Min() == 1 &&
                           round.Max() == round.Count &&
                           round.Distinct().Count() == round.Count;
            if (!isValid) throw new BadRequestException("Round phải bắt đầu từ 1 và khác nhau");
            // Validate that all dates in the request are between the revise start and end date of the phase
            if (phase.ReviseStartDate.HasValue && phase.ReviseEndDate.HasValue)
            {
                foreach (var deadline in request)
                {
                    if (deadline.StartSubmissionDate != null && deadline.EndSubmissionDate != null)
                    {
                        if (deadline.StartSubmissionDate >= phase.ReviseStartDate.Value || deadline.EndSubmissionDate <= phase.ReviseEndDate.Value)
                        {
                            throw new BadRequestException($"Ngày hạn nộp bắt đầu {deadline.StartSubmissionDate} và kết thúc {deadline.EndSubmissionDate} phải nằm trong khoảng từ ngày bắt đầu chỉnh sửa ({phase.ReviseStartDate.Value}) đến ngày kết thúc chỉnh sửa ({phase.ReviseEndDate.Value})");
                        }
                    }
                }
            }

            var responses = new List<RevisionRoundDeadlineResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var deadlineRequest in request)
                {
                    var revisionRoundDeadline = deadlineRequest.ToModel(researchConferencePhaseId);
                    await _unitOfWork.RevisionRoundDeadlineRepository.CreateCsAsync(revisionRoundDeadline);
                    responses.Add(revisionRoundDeadline.ToRevisionRoundDeadlineResponse());
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

        public async Task<List<RevisionRoundDeadlineResponse>> GetRevisionRoundDeadlinesByResearchPhaseIdAsync(string researchConferencePhaseId)
        {
            var deadlines = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(researchConferencePhaseId);
            return deadlines.Select(d => d.ToRevisionRoundDeadlineResponse()).ToList();
        }

        public async Task<RevisionRoundDeadlineResponse> UpdateRevisionRoundDeadlineAsync(string revisionRoundDeadlineId, UpdateRevisionRoundDeadlineRequest request)
        {
            var deadline = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByIdAsync(revisionRoundDeadlineId);
            if (deadline == null) throw new NotFoundException($"Revision round deadline with ID {revisionRoundDeadlineId} not found");

            var phase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByIdAsync(deadline.ResearchConferencePhaseId);
            if (phase == null) throw new NotFoundException($"Research conference phase for revision round deadline {revisionRoundDeadlineId} not found");

            // Validate that the new date is between the revise start and end date of the phase
            if (phase.ReviseStartDate.HasValue && phase.ReviseEndDate.HasValue && request.EndDate.HasValue)
            {
                if (request.EndDate.Value < phase.ReviseStartDate.Value || request.EndDate.Value > phase.ReviseEndDate.Value)
                {
                    throw new BadRequestException($"Ngày hết hạn {request.EndDate.Value} phải nằm trong khoảng từ ngày bắt đầu chỉnh sửa ({phase.ReviseStartDate.Value}) đến ngày kết thúc chỉnh sửa ({phase.ReviseEndDate.Value})");
                }
            }

            deadline.EndSubmissionDate = request.EndDate ?? deadline.EndSubmissionDate;
            deadline.RoundNumber = request.RoundNumber ?? deadline.RoundNumber;

            await _unitOfWork.RevisionRoundDeadlineRepository.UpdateCsAsync(deadline);
            return deadline.ToRevisionRoundDeadlineResponse();
        }

        public async Task<bool> DeleteRevisionRoundDeadlineAsync(string revisionRoundDeadlineId)
        {
            var deadline = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByIdAsync(revisionRoundDeadlineId);
            if (deadline == null) throw new NotFoundException($"Revision round deadline with ID {revisionRoundDeadlineId} not found");

            return await _unitOfWork.RevisionRoundDeadlineRepository.DeleteCsAsync(deadline) > 0;
        }

        #endregion
    }
}