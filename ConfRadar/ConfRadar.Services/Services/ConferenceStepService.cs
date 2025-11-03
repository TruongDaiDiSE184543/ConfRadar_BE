using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.Extensions.Options;

namespace ConfRadar.Services.Services
{
    public interface IConferenceStepService
    {
        Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid);
        // Step 1: Basic Conference Creation
        Task<TechnicalConferenceBasicStepResponse> GetConferenceBasicAsync(string conferenceId);
        Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request);

        // Step 2: Add Conference Prices
        Task<ConferencePriceListWithPhasesResponse> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request);
        Task<List<ConferencePriceWithPhasesResponse>> GetConferencePricesAsync(string conferenceId);
        Task<ConferencePriceWithPhasesResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request);
        Task<bool> DeleteConferencePriceAsync(string priceId);

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
    }

    public class ConferenceStepService : IConferenceStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITokenService _tokenService;
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferenceStepService(
            IUnitOfWork unitOfWork,
            IObjectStorageFileService objectStorageFileService,
            ITokenService tokenService,
            IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings)
        {
            _unitOfWork = unitOfWork;
            _objectStorageFileService = objectStorageFileService;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings.Value;
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

        private async Task<bool> CheckIfStartDateAndTicketSaleDate(DateOnly startDate, DateOnly endDate, DateOnly ticketSaleStart, DateOnly ticketSaleEnd)
        {
            if (startDate < DateOnly.FromDateTime(DateTime.UtcNow) &&
                endDate < DateOnly.FromDateTime(DateTime.UtcNow) &&
                ticketSaleStart < DateOnly.FromDateTime(DateTime.UtcNow) &&
                ticketSaleEnd < DateOnly.FromDateTime(DateTime.UtcNow)
                ) return false;
            if (startDate.CompareTo(endDate) > 0) return false;
            if (ticketSaleStart.CompareTo(ticketSaleEnd) > 0) return false;
            if (ticketSaleEnd.CompareTo(startDate) > 0 || ticketSaleStart.CompareTo(startDate) > 0) return false;
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


        #endregion

        #region Step 1: Basic Conference

        public async Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid)
        {
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
                if (request.BannerImageFile != null)
                {
                    using var stream = request.BannerImageFile.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                    request.bannerImageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
                    request.bannerImageFileUrl = _objectStorageSettings.EndPoint + request.bannerImageFileUrl;
                }

                // check if any of the date is before today and ticketsale Start/end must before start/end date

                var isValidDateValues = CheckIfStartDateAndTicketSaleDate(request.StartDate, request.EndDate, request.TicketSaleStart, request.TicketSaleEnd);
                if (!isValidDateValues.Result) throw new BadRequestException("Ngày mở bán vé phải trước ngày conference diễn và tất cả phải trước hôm nay");

                // must have target audience for the technical detail
                if (string.IsNullOrEmpty(request.targetAudienceTechnicalConference)) throw new BadRequestException("Cần phải có khán giả hướng tới cho buổi hội nghị công nghệ");


                //Total slot for conference must be > 0
                if (request.TotalSlot < 0) throw new Exception("Total slot must be positive");
                var vietNamTimeZoneNow = ExtensionHelper.GetVietnamDate();
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

        public async Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var isValidDateValues = CheckIfStartDateAndTicketSaleDate(request.StartDate, request.EndDate, request.TicketSaleStart.Value, request.TicketSaleEnd.Value);
            if (!isValidDateValues.Result) throw new BadRequestException("Ngày mở bán vé phải trước ngày conference diễn và tất cả phải trước hôm nay");

            if (request.TotalSlot <= 0) throw new BadRequestException("Totalslot phải lớn hơn 0");

            conference.ConferenceName = request.ConferenceName ?? conference.ConferenceName;
            conference.Description = request.Description ?? conference.Description;
            conference.StartDate = request.StartDate;  // Fixed nullable DateOnly
            conference.EndDate = request.EndDate;         // Fixed nullable DateOnly
            conference.TotalSlot = request.TotalSlot ?? conference.TotalSlot;
            conference.AvailableSlot = request.TotalSlot ?? conference.AvailableSlot; // Update available slot if total is changed
            conference.Address = request.Address ?? conference.Address;
            conference.IsInternalHosted = request.IsInternalHosted ?? conference.IsInternalHosted;
            conference.IsResearchConference = request.IsResearchConference ?? conference.IsResearchConference;
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

        public async Task<ConferencePriceListWithPhasesResponse> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");
            ConferencePriceListWithPhasesResponse result = new ConferencePriceListWithPhasesResponse
            {
                conferencePriceWithPhasesResponses = new List<ConferencePriceWithPhasesResponse>()
            };
            var existingConferencePrice = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
            //get existing total slot from conference price to check if this addition will result in exceeding the conferene totol slot 
            var existingTotalSlot = existingConferencePrice.Sum(x => x.TotalSlot);
            List<PricePhaseResponse> pricePhaseResponses = new();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create the conference price
                var conferencePriceRequest = request.TypeOfTicket;
                int? totalSlotFromToBeTickets = request.TypeOfTicket.Sum(ts => ts.TotalSlot);
                if (totalSlotFromToBeTickets + existingTotalSlot > conference.TotalSlot) throw new BadRequestException($"Số lượng totalSlot của từng loại vé tổng phải nhỏ hơn hoặc bằng capicity của conference: {existingTotalSlot}+ {totalSlotFromToBeTickets} > {conference.TotalSlot} ");
                foreach (CreateConferencePriceRequest toBeConferencePrice in conferencePriceRequest)
                {
                    //check if totalslot of phases in a ticket type is larger than the totalslot of the ticket itself
                    int? totalSlotFromPhase = toBeConferencePrice.Phases.Sum(phase => phase.Totalslot);
                    if (toBeConferencePrice.TotalSlot != totalSlotFromPhase) throw new BadRequestException("Tổng totalslot qua từng giai đoạn của vé không thể lớn hơn totalslot của loại vé đó");
                    var CreatedConferencePrice = toBeConferencePrice.ToModel(conferenceId);
                    await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(CreatedConferencePrice);
                    foreach (CreatePricePhaseRequest createPricePhaseRequest in toBeConferencePrice.Phases)
                    {
                        //check if each phase request is in valid date
                        //createPricePhaseRequest start must < end, 
                        if (createPricePhaseRequest.StartDate > createPricePhaseRequest.EndDate) throw new BadRequestException("Start phase phải lớn hơn end phase");
                        //each phase must be in conference's ticket sale start and end
                        if (createPricePhaseRequest.StartDate < conference.StartDate || createPricePhaseRequest.EndDate > conference.EndDate) throw new BadRequestException("Start phase phải và endphase phải nằm trong ticket sale start và ticket sale end của conference");
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

        public async Task<ConferencePriceWithPhasesResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null) throw new NotFoundException($"Conference price with ID {priceId} not found");

            if (request.TicketPrice.HasValue) price.TicketPrice = request.TicketPrice.Value;
            if (!string.IsNullOrEmpty(request.TicketName)) price.TicketName = request.TicketName;
            if (!string.IsNullOrEmpty(request.TicketDescription)) price.TicketDescription = request.TicketDescription;
            if (request.TotalSlot.HasValue)
            {
                price.TotalSlot = request.TotalSlot.Value;
                price.AvailableSlot = request.TotalSlot.Value; // Reset available slot when total is updated
            }

            await _unitOfWork.ConferencePriceRepository.UpdateConferencePriceAsync(price);

            var phases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(price.ConferencePriceId);
            return price.ToResponseWithPhases(phases);
        }

        public async Task<bool> DeleteConferencePriceAsync(string priceId)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null) throw new NotFoundException($"Conference price with ID {priceId} not found");

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

            var responses = new List<ConferenceSessionWithMediaResponse>();

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

                var isValidDateValues = CheckIfStartDateAndTicketSaleDate(request.StartDate, request.EndDate, request.TicketSaleStart, request.TicketSaleEnd);
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
            conference.IsInternalHosted = request.IsInternalHosted ?? conference.IsInternalHosted;
            conference.IsResearchConference = request.IsResearchConference ?? conference.IsResearchConference;
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
                        var startDateTime = new DateTime(session.Date.Value.Year, session.Date.Value.Month, session.Date.Value.Day);
                        var endDateTime = new DateTime(session.Date.Value.Year, session.Date.Value.Month, session.Date.Value.Day);

                        startDateTime = startDateTime.AddHours(session.StartTime.Value.Hour).AddMinutes(session.StartTime.Value.Minute);
                        endDateTime = endDateTime.AddHours(session.EndTime.Value.Hour).AddMinutes(session.EndTime.Value.Minute);

                        await ValidateSessionTimeAvailability(startDateTime, endDateTime, session.RoomId);


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
    }
}