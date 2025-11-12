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
        Task<string> GenerateQrCodeAsync<T>(T data, string uniqueFileName, string contentType);
        void ProcessScanQr(string data);
    }
    public class QRCoderService : IQRCoderService
    {
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;
        private readonly ITokenService _tokenService;
        private string secretKey = "";
        public QRCoderService(IObjectStorageFileService objectStorageFileService,
            IOptions<ObjectStorageSettings> objectStorageSettings,
            ITokenService tokenService)
        {
            _objectStorageFileService = objectStorageFileService;
            _objectStorageSettings = objectStorageSettings;
            _tokenService = tokenService;
        }
        public async Task<string> GenerateQrCodeAsync<T>(T data, string uniqueFileName, string contentType)
        {
            string jsonData = JsonSerializer.Serialize(data);
            var hashedContent = _tokenService.EncryptString(jsonData, secretKey);


            using var qrGenerator = new QRCodeGenerator();
            #region ecc level
            // có 4 mức ECC (mã sửa lỗi => giúp qr đọc được ngay cả khi bị che mờ, ướt hay hỏng
            // L: low : simple , scan nhanh
            //M: medium : cân bằng
            //Q : Quartile : tốt , che 1/4 vẫn đọc được
            //H: high : mạnh, che 1/3 vẫn cân được nhưng cần qr lớn hơn
            #endregion
            using var qrData = qrGenerator.CreateQrCode(hashedContent, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new QRCode(qrData);
            using var bitmap = qrCode.GetGraphic(20);

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //reset con trỏ stream về 0 để save
            ms.Position = 0;

            var baseUri = _objectStorageSettings.Value.EndPoint;
            var uploadPath = baseUri + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.qrcodefile.ToString(), uniqueFileName, ms, contentType);
            return baseUri + uploadPath;
        }
        public void ProcessScanQr(string data)
        {
            try
            {
                var decryptContet = _tokenService.DecryptString(data, secretKey);
                var jsonData = JsonSerializer.Deserialize<QrDataPayload>(decryptContet);
                if (jsonData == null)
                {
                    throw new BadRequestException("Không tìm thấy thông tin");
                }
                Console.WriteLine(JsonSerializer.Serialize(jsonData));
                Console.WriteLine(jsonData);
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Dữ liệu không khả dụng");
            }
        }
    }
}
