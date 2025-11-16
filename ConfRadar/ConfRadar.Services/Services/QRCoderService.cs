using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.QrCode;
using Microsoft.Extensions.Options;
using QRCoder;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IQRCoderService
    {
        Task<string> GenerateQrCode<T>(T data);
        //T DecryptQrContent<T>(string hasedContent);
        QrDataPayload CreateQrDataPayload(QrDataPayload data);
        //bool CheckValidQrPayload(QrDataPayload data);
        Task<string> ProceedQrCode(VerifyQrDataRequest data);
    }
    public class QRCoderService : IQRCoderService
    {
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;
        private readonly IOptions<QrSettings> _qrSettings;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        public QRCoderService(IObjectStorageFileService objectStorageFileService,
            IOptions<ObjectStorageSettings> objectStorageSettings,
            IOptions<QrSettings> qrSettings,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            ITimeProviderService timeProviderService)
        {
            _objectStorageFileService = objectStorageFileService;
            _objectStorageSettings = objectStorageSettings;
            _qrSettings = qrSettings;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;
        }
        public async Task<string> GenerateQrCode<T>(T data)
        {
            string contentType = "image/png";
            string jsonData = JsonSerializer.Serialize(data);
            var hashedContent = _tokenService.EncryptString(jsonData, _qrSettings.Value.HashKey);
            string uniqueFileName = _tokenService.GenerateSecureRandomToken();

            using var qrGenerator = new QRCodeGenerator();
            #region ecc level
            // có 4 m?c ECC (mã s?a l?i => giúp qr d?c du?c ngay c? khi b? che m?, u?t hay h?ng
            // L: low : simple , scan nhanh
            //M: medium : cân b?ng
            //Q : Quartile : t?t , che 1/4 v?n d?c du?c
            //H: high : m?nh, che 1/3 v?n cân du?c nhung c?n qr l?n hon
            #endregion
            using var qrData = qrGenerator.CreateQrCode(hashedContent, QRCodeGenerator.ECCLevel.M);

            using var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20);

            using var ms = new MemoryStream(qrBytes);
            //reset con tr? stream v? 0 d? save
            ms.Position = 0;


            var baseUri = _objectStorageSettings.Value.EndPoint;
            var uploadPath = baseUri + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.qrcodefile.ToString(), uniqueFileName, ms, contentType);
            return uploadPath;
        }
        private T DecryptQrContent<T>(string hasedContent)
        {
            try
            {
                var decryptContet = _tokenService.DecryptString(hasedContent, _qrSettings.Value.HashKey);
                var jsonData = JsonSerializer.Deserialize<T>(decryptContet);
                if (jsonData == null)
                {
                    throw new BadRequestException("Thông tin không tìm th?y");
                }
                return jsonData;
            }
            catch (JsonException)
            {
                throw new BadRequestException("D? li?u QR không h?p l? ho?c dã b? thay d?i");
            }
            catch (Exception ex)
            {
                throw new BadRequestException("D? li?u không kh? d?ng ho?c không thu?c v? confradar");
            }
        }
        public QrDataPayload CreateQrDataPayload(QrDataPayload data)
        {
            var inputParams = new SortedList<string, string>(StringComparer.Ordinal);
            inputParams.Add("usercheckinId", data.userCheckinId);
            inputParams.Add("userId", data.userId);
            inputParams.Add("ticketId", data.ticketId);
            inputParams.Add("conferenceSessionId", data.conferenceSessionId);
            inputParams.Add("createAt", data.createAt.ToString("dd/MM/yyyy HH:mm:ss"));
            string rawData = string.Join("&", inputParams.Select(kv => $"{kv.Key}={kv.Value}"));
            string signature = _tokenService.CreateSignature512(rawData, _qrSettings.Value.CheckSumKey);
            data.signature = signature;
            return data;
        }
        private bool CheckValidQrPayload(QrDataPayload data)
        {
            var inputParams = new SortedList<string, string>(StringComparer.Ordinal);
            inputParams.Add("usercheckinId", data.userCheckinId);
            inputParams.Add("userId", data.userId);
            inputParams.Add("ticketId", data.ticketId);
            inputParams.Add("conferenceSessionId", data.conferenceSessionId);
            inputParams.Add("createAt", data.createAt.ToString("dd/MM/yyyy HH:mm:ss"));
            string rawData = string.Join("&", inputParams.Select(kv => $"{kv.Key}={kv.Value}"));
            string signature = _tokenService.CreateSignature512(rawData, _qrSettings.Value.CheckSumKey);
            var result = string.Equals(signature, data.signature, StringComparison.OrdinalIgnoreCase);
            if (result == true)
            {
                return true;
            }
            return false;
        }

        public async Task<string> ProceedQrCode(VerifyQrDataRequest data)
        {
            var qrDataPayload = DecryptQrContent<QrDataPayload>(data.Content);
            var qrPayLoadChecker = CheckValidQrPayload(qrDataPayload);
            if (!qrPayLoadChecker)
            {
                throw new BadRequestException("D? li?u trong payment không kh? d?ng ");
            }
            var checkedInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.CheckedIn.GetDescription());
            var expiredCheckInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Expired.GetDescription());
            if (checkedInStatus == null || expiredCheckInStatus == null)
            {
                throw new NotFoundException("Không tìm th?y các tr?ng thái checkin tuong ?ng");
            }
            var conferenceSessionDetail = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(data.ConferenceSessionId);
            if (conferenceSessionDetail == null)
            {
                throw new NotFoundException($"Không tìm th?y session v?i id {data.ConferenceSessionId}");
            }
            if (data.ConferenceSessionId != qrDataPayload.conferenceSessionId)
            {
                throw new BadRequestException($"B?n dã check in nh?m session r?i. Session hi?n t?i là " +
                    $"{conferenceSessionDetail.Title} di?n ra t? {conferenceSessionDetail.StartTime?.ToString("dd/MM/yyyy HH:mm:ss tt")} d?n {conferenceSessionDetail.EndTime?.ToString("dd/MM/yyyy HH:mm:ss tt")}");
            }

            var userCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByIdAsync(qrDataPayload.userCheckinId);
            if (userCheckIn == null)
            {
                throw new NotFoundException("Không tìm th?y user check in trong h? th?ng");
            }
            if (userCheckIn.UserId != qrDataPayload.userId || userCheckIn.TicketId != qrDataPayload.ticketId || userCheckIn.ConferenceSessionId != qrDataPayload.conferenceSessionId)
            {
                throw new BadRequestException("Thông tin trong qr không trùng h?p v?i trên h? th?ng");
            }
            var userConferenceSession = userCheckIn.ConferenceSession!;
            var timeNow = await _timeProviderService.GetVietnamTime();
            if (userCheckIn.CheckinStatus == expiredCheckInStatus)
            {
                throw new BadRequestException("Vé check in hi?n dã h?t h?n.");
            }
            if (userCheckIn.CheckinStatus == checkedInStatus && userCheckIn.CheckInTime != null)
            {
                string formattedTime;
                if (userCheckIn.CheckInTime.HasValue)
                {
                    formattedTime = userCheckIn.CheckInTime.Value.ToString("dd/MM/yyyy HH:mm:ss tt");
                }
                else
                {
                    formattedTime = "";
                }
                throw new BadRequestException($"Ngu?i dùng v?i tên {userCheckIn.User!.FullName} dã có checked in vào lúc {formattedTime}");
            }
            if (timeNow < userConferenceSession.StartTime)
            {
                throw new BadRequestException($"Vé này chua th? check in du?c vì th?i gian di?n ra check in t? {userConferenceSession.StartTime}");
            }
            if (timeNow > userConferenceSession.EndTime)
            {
                throw new BadRequestException($"Vé này dã h?t h?n check in vì session {userConferenceSession.Title} dã h?t h?n vào lúc {userConferenceSession.EndTime}");
            }
            userCheckIn.CheckinStatus = checkedInStatus;
            userCheckIn.CheckInTime = await _timeProviderService.GetVietnamTime();
            var result = await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(userCheckIn);
            if (result > 0)
            {
                return $"Ngu?i dùng v?i tên {userCheckIn.User!.FullName} dã check in cho h?i ngh? {userConferenceSession.Title} vào lúc {userCheckIn.CheckInTime?.ToString("dd/MM/yyyy HH:mm:ss tt")}";
            }
            return string.Empty;

        }
    }
}
