using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponse> CreateMomoPayment(string orderId, long amount, string orderInfo, string ipnUrl, string redirectUrl);
        bool VerifyMomoPaymentData(MomoPaymentCallBackResponse data);
    }
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoSettings> _momoSettings;
        private readonly ITokenService _tokenService;
        public MomoService(IOptions<MomoSettings> momoSettings, ITokenService tokenService)
        {
            _momoSettings = momoSettings;
            _tokenService = tokenService;
        }

        public async Task<MomoCreatePaymentResponse> CreateMomoPayment(string orderId, long amount, string orderInfo, string ipnUrl, string redirectUrl)
        {
            var rawSignature = "accessKey=" + _momoSettings.Value.AccessKey + "&amount=" + amount +
                "&extraData=" + _momoSettings.Value.ExtraData + "&ipnUrl=" + ipnUrl + "&orderId=" +
                orderId + "&orderInfo=" + orderInfo + "&partnerCode=" + _momoSettings.Value.PartnerCode +
                "&redirectUrl=" + redirectUrl + "&requestId=" + orderId + "&requestType=" + _momoSettings.Value.RequestType;
            var signature = _tokenService.CreateSignature(rawSignature, _momoSettings.Value.SecretKey);
            var requestBody = JsonSerializer.Serialize(new
            {
                partnerCode = _momoSettings.Value.PartnerCode,
                partnerName = "ConfRadar",
                storeId = "MomoTestStore",
                requestId = orderId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = redirectUrl,
                ipnUrl = ipnUrl,
                lang = _momoSettings.Value.Lang,
                requestType = _momoSettings.Value.RequestType,
                autoCapture = _momoSettings.Value.AutoCapture,
                extraData = _momoSettings.Value.ExtraData,
                signature = signature,
            });
            using var client = new HttpClient();
            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", content);
            var body = await response.Content.ReadAsStringAsync();
            var momoResponse = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(body, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
            if (momoResponse.resultCode != 0)
            {
                throw new BadRequestException($"Error {momoResponse.message} with status code ${momoResponse.resultCode}");
            }
            return momoResponse;

        }
        public bool VerifyMomoPaymentData(MomoPaymentCallBackResponse data)
        {
            var raw_Response_Signature = $"accessKey={_momoSettings.Value.AccessKey}&amount={data.amount}&extraData={data.extraData}&message={data.message}&orderId={data.orderId}&orderInfo={data.orderInfo}&orderType={data.orderType}&partnerCode={data.partnerCode}&payType={data.payType}&requestId={data.requestId}&responseTime={data.responseTime}&resultCode={data.resultCode}&transId={data.transId}";
            var signature = _tokenService.CreateSignature(raw_Response_Signature, _momoSettings.Value.SecretKey);
            if (!string.Equals(signature, data.signature, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }


    }
}
