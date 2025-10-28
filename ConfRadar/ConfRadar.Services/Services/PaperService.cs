using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPaperService
    {
        Task<string> SubmitAbstract(CreateAbstractRequest request, string userId);
        Task<FullPaperResponse> SubmitFullPaper (CreateFullPaperRequest request, string userId);

    }
    public class PaperService : IPaperService 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMomoService _momoService;
        private readonly ITokenService _tokenService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;
        private readonly IObjectStorageFileService _objectStorageFileService;
        public PaperService(IUnitOfWork unitOfWork,IMomoService momoService,ITokenService tokenService,IOptions<ObjectStorageSettings> objectStorageSettings,IObjectStorageFileService objectStorageFileService)
        {
            _unitOfWork = unitOfWork;
            _momoService = momoService;
            _tokenService = tokenService;   
            _objectStorageSettings = objectStorageSettings;
            _objectStorageFileService = objectStorageFileService;
        }
        public async Task<string> SubmitAbstract(CreateAbstractRequest request,string userId)
        {
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null) 
            {
                throw new BadRequestException($"Giá hội nghị với id {request.ConferencePriceId} không tìm thấy");
            }
            if (conferencePrice.Conference.IsResearchConference ==false)
            {
                throw new BadRequestException($"Bạn chỉ có thể nộp abstract cho research conference");
            }
            if (conferencePrice.IsAuthor == false) 
            {
                throw new BadRequestException($"Giá vé hiện tại không dành cho tác giả, xin hãy chọn mức giá khác");
            }
            if (conferencePrice.Conference.IsInternalHosted == false)
            {
                throw new BadRequestException($"Bạn chỉ có thể nộp abstract cho research conference tổ chức bởi confradar");
            }
            if (conferencePrice.AvailableSlot <= 0)
            {
                throw new BadRequestException($"Hiện tại slot cho research conference đã hết");
            }
            string abstractFileUrl=string.Empty;
            if (request.AbstractFile != null)
            {
                if (request.AbstractFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }
                using var stream = request.AbstractFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.AbstractFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.abstractfile.ToString(), uniqueFileName, stream, request.AbstractFile.ContentType);
                abstractFileUrl = baseUri + objectStorageFileUrl;
            }
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.MoMo.GetDescription());
            decimal applyPercent = 0;
            var dateNow = ExtensionHelper.GetVietnamDate();
            var validPhases = conferencePrice.PricePhases
            .Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow)
            .OrderBy(p => p.StartDate)
            .ToList();

            if (!validPhases.Any())
            {
                throw new BadRequestException("Hiện tại không có phase hợp lệ để nộp abstract");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);

            if (currentPhase == null)
            {
                throw new BadRequestException("Tất cả các phase hợp lệ hiện tại đã hết slot");
            }
            var sessionIdsList = conferencePrice.Conference.ConferenceSessions.Select(cs => cs.ConferenceSessionId).ToList();
            applyPercent = currentPhase.ApplyPercent ?? 0;

            var finalPrice = (long)(conferencePrice.TicketPrice -(conferencePrice.TicketPrice * applyPercent / 100));

            var result = await _momoService.ProcessPaymentForAbstract(request,conferencePrice.ConferenceId,userId, finalPrice, paymentMethod.PaymentMethodId, sessionIdsList, abstractFileUrl, $"Thanh toán abstract");
            return result;
        }

        public async Task<FullPaperResponse> SubmitFullPaper(CreateFullPaperRequest request, string userId)
        {
            if (request.PaperId == null) throw new Exception("Cần có paperid để nộp fullpaper");
            var PaperBase = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (PaperBase == null) throw new Exception($"Không tìm thấy paper với id{request.PaperId}");
            string fullPaperURL = string.Empty;
            if(request.FullPaperFile != null)
            {
                if (request.FullPaperFile.ContentType == null) throw new Exception("Không có dữ liệu file đầu vào để nộp");
                using var stream = request.FullPaperFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken + Path.GetExtension(request.FullPaperFile.FileName);
                fullPaperURL = _objectStorageSettings.Value.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.fullpaperfile.ToString(),uniqueFileName,stream,request.FullPaperFile.ContentType);
            }
            var pendingStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByName("Pending");
            var fullPaperObject = request.toModel(fullPaperURL, pendingStatus.ReviewStatusId);
            await _unitOfWork.BeginTransactionAsync();
            try {
                await _unitOfWork.FullPaperRepository.CreateFullPaperAsync(fullPaperObject);
                PaperBase.FullPaperId = fullPaperObject.FullPaperId;
                await _unitOfWork.CommitAsync();
                return fullPaperObject.toResponse();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(); 
                throw;
            }

        }
    }
}
