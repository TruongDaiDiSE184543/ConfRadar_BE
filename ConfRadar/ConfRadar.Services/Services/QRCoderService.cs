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
            // có 4 mặc ECC (giúp qr quét được ngay cả khi bị che khuất hay mất 1 góc
            // L: low : simple , scan nhanh
            //M: medium : cân bằng
            //Q : Quartile : tốt , che 1/4 vẫn scan được
            //H: high : mạnh, che 1/3 vẫn quét được nhưng qr phải lớn hơn
            #endregion
            using var qrData = qrGenerator.CreateQrCode(hashedContent, QRCodeGenerator.ECCLevel.M);

            using var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20);

            using var ms = new MemoryStream(qrBytes);
            //reset con trỏ stream về 0 để save
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
                    throw new BadRequestException("Thông tin không tìm thấy");
                }
                return jsonData;
            }
            catch (JsonException)
            {
                throw new BadRequestException("Dữ liệu QR không hợp lệ hoặc bị thay đổi");
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Dữ liệu không thuộc về confradar");
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
                throw new BadRequestException("Dữ liệu trong payment không khả dụng ");
            }
            var checkedInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.CheckedIn.GetDescription());
            var expiredCheckInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Expired.GetDescription());


            var readyStatusConf = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());

            if (checkedInStatus == null || expiredCheckInStatus == null || readyStatusConf ==null)
            {
                throw new NotFoundException("Không tìm thấy các trạng thái tương ứng");
            }
            var conferenceSessionDetail = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(data.ConferenceSessionId);
            if (conferenceSessionDetail == null)
            {
                throw new NotFoundException($"Không tìm thấy session với id {data.ConferenceSessionId}");
            }
            if (data.ConferenceSessionId != qrDataPayload.conferenceSessionId)
            {
                throw new BadRequestException($"Bạn dã check in nhầm session rồi. Session hiện tại là " +
                    $"{conferenceSessionDetail.Title} diễn ra từ {conferenceSessionDetail.StartTime?.ToString("dd/MM/yyyy HH:mm:ss tt")} đến {conferenceSessionDetail.EndTime?.ToString("dd/MM/yyyy HH:mm:ss tt")}");
            }

            var userCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByIdAsync(qrDataPayload.userCheckinId);
            if (userCheckIn == null)
            {
                throw new NotFoundException("Không tìm thấy user check in trong hệ thống");
            }
            var confStatus = userCheckIn.ConferenceSession?.Conference?.ConferenceStatus;
            if (confStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái của conference");
            }
            if (confStatus != readyStatusConf)
            {
                throw new BadRequestException("Chỉ có thể check in cho hội nghị trong trạng thái ready");

            }
            var ticket = userCheckIn.Ticket;
            if (ticket!.IsRefunded == true)
            {
                throw new BadRequestException("Vé này đã được refund nên không thể checkin");
            }
            if (userCheckIn.UserId != qrDataPayload.userId || userCheckIn.TicketId != qrDataPayload.ticketId || userCheckIn.ConferenceSessionId != qrDataPayload.conferenceSessionId)
            {
                throw new BadRequestException("Thông tin trong qr không trùng khớp");
            }
            var userConferenceSession = userCheckIn.ConferenceSession!;
            var timeNow = await _timeProviderService.GetVietnamTime();
            if (userCheckIn.CheckinStatus == expiredCheckInStatus)
            {
                throw new BadRequestException("Vé check in hiện đã hết hạn.");
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
                throw new BadRequestException($"Nguời dùng với tên {userCheckIn.User!.FullName} đã checkin vào lúc {formattedTime}");
            }
            if (timeNow < userConferenceSession.StartTime)
            {
                throw new BadRequestException($"Vé này chưa thể check in được vì thời gian diễn ra check in từ {userConferenceSession.StartTime}");
            }
            if (timeNow > userConferenceSession.EndTime)
            {
                throw new BadRequestException($"Vé này dã hết hạn check in vì session {userConferenceSession.Title} dã hết hạn vào lúc {userConferenceSession.EndTime}");
            }
            userCheckIn.CheckinStatus = checkedInStatus;
            userCheckIn.CheckInTime = timeNow;
            var result = await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(userCheckIn);
            if (result > 0)
            {
                return $"Nguời dùng với tên {userCheckIn.User!.FullName} đã check in cho hội nghị {userConferenceSession.Title} vào lúc {userCheckIn.CheckInTime?.ToString("dd/MM/yyyy HH:mm:ss tt")}";
            }
            return string.Empty;

        }
    }
}
