using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IMomoService
    {
        Task CreateMomoPayment();
        void VerifyMomoPaymentData(MomoPaymentRequestResponse data);
    }
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoSettings> _momoSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        public MomoService(IOptions<MomoSettings> momoSettings,IUnitOfWork unitOfWork, IRedisService redisService)
        {
            _momoSettings = momoSettings;
            _unitOfWork = unitOfWork;
            _redisService = redisService;
        }
        public async Task HandleMomoPaymentWithTechConf(CreateTechPaymentRequest request)
        {
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByConferencePriceId(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException("Conference price not found");
            }
            if (conferencePrice.Conference?.Capacity == 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} is sold out!");
            }
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            if (conferencePrice.PricePhase?.EarlierBirdEndInterval > dateNow || conferencePrice.PricePhase?.LateEndInterval < dateNow)
            {
                throw new BadRequestException("Price phase interval is not available");
            }

            int discountPercent = 0;
            if (conferencePrice.PricePhase?.EarlierBirdEndInterval<= dateNow && dateNow < conferencePrice.PricePhase?.StandardEndInterval)
            {
                discountPercent = conferencePrice.PricePhase?.PercentForEarly ?? 0;
            }else if (dateNow<= conferencePrice.PricePhase?.LateEndInterval && dateNow > conferencePrice.PricePhase?.StandardEndInterval)
            {
                discountPercent = conferencePrice.PricePhase?.PercentForEnd ?? 0;
            }
            else
            {
                discountPercent = 0;
            }
            var finalAmount = conferencePrice.ActualPrice - (conferencePrice.ActualPrice * discountPercent / 100);





        }
        public async Task CreateMomoPayment()
        {
            var orderId = _momoSettings.Value.PartnerCode + Guid.NewGuid().ToString();
            var rawSignature = "accessKey=" + _momoSettings.Value.AccessKey + "&amount=" + _momoSettings.Value.Amount + 
                "&extraData=" + _momoSettings.Value.ExtraData + "&ipnUrl=" + _momoSettings.Value.IpnUrl + "&orderId=" +
                orderId + "&orderInfo=" + _momoSettings.Value.OrderInfo + "&partnerCode=" + _momoSettings.Value.PartnerCode +
                "&redirectUrl=" + _momoSettings.Value.RedirectUrl + "&requestId=" + orderId + "&requestType=" + _momoSettings.Value.RequestType;
            var signature = CreateSignature(rawSignature, _momoSettings.Value.SecretKey);


            var requestBody = JsonSerializer.Serialize(new
            {
                partnerCode = _momoSettings.Value.PartnerCode,
                partnerName = "Test",
                storeId = "MomoTestStore",
                requestId =orderId,
                amount = _momoSettings.Value.Amount,
                orderId = orderId,
                orderInfo = _momoSettings.Value.OrderInfo,
                redirectUrl = _momoSettings.Value.RedirectUrl,
                ipnUrl = _momoSettings.Value.IpnUrl,
                lang = _momoSettings.Value.Lang,
                requestType = _momoSettings.Value.RequestType,
                autoCapture = _momoSettings.Value.AutoCapture,
                extraData = _momoSettings.Value.ExtraData,
                orderGroupId = "",
                signature = signature,
            });
            using var client = new HttpClient();
            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", content);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Response: " + body);
        }
        public static string CreateSignature(string rawData, string secretKey)
        {
            string signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                 signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            return signature;

        }

        public  void  VerifyMomoPaymentData(MomoPaymentRequestResponse data)
        {
            var raw_Response_Signature = $"accessKey={_momoSettings.Value.AccessKey}&amount={data.amount}&extraData={data.extraData}&message={data.message}&orderId={data.orderId}&orderInfo={data.orderInfo}&orderType={data.orderType}&partnerCode={data.partnerCode}&payType={data.payType}&requestId={data.requestId}&responseTime={data.responseTime}&resultCode={data.resultCode}&transId={data.transId}";
            var signature = CreateSignature(raw_Response_Signature, _momoSettings.Value.SecretKey);
            if (!string.Equals(signature,data.signature,StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Data is not verified");
            }
            Console.WriteLine("Verfied data successfully");
        }
    }
}
