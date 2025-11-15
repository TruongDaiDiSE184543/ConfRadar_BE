using ConfRadar.Services.Common;
using ConfRadar.Shared.DTO.Payment;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IVnPayService
    {
        Task<string> CreateVnPayPayment(long orderCode, long amount, double expireMinute);
        bool VerifyVnPayPayment(VnPayResponse data);
    }
    public class VnPayService : IVnPayService
    {
        private readonly IOptions<VnPaySettings> _vnPaySettings;
        private readonly ITokenService _tokenService;
        private readonly ITimeProviderService _timeProviderService;
        public VnPayService(IOptions<VnPaySettings> vnPaySettings, ITokenService tokenService, ITimeProviderService timeProviderService)
        {
            _vnPaySettings = vnPaySettings;
            _tokenService = tokenService;
            _timeProviderService = timeProviderService;
        }
        public async Task<string> CreateVnPayPayment(long orderCode, long amount, double expireMinute)
        {
            var timeNow = await _timeProviderService.GetVietnamTime();
            string vnPayCreateLink = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?";
            string vnp_Version = "2.1.0";
            string vnp_Command = "pay";
            string vnp_TmnCode = _vnPaySettings.Value.TmnCode;
            string vnp_Amount = (amount * 100).ToString();
            string vnp_CreateDate = timeNow.ToString("yyyyMMddHHmmss");
            string vnp_CurrCode = "VND";
            string vnp_IpAddr = "127.0.0.1";
            string vnp_Locale = "vn";
            // order info của vnpay ko có khoảng trắng=> có gây lỗi 
            string orderInfo = "ThanhToanHang";
            string vnp_OrderInfo = Uri.EscapeDataString(orderInfo);
            string vnp_OrderType = "other";
            string vnp_ReturnUrl = Uri.EscapeDataString(_vnPaySettings.Value.ReturnUrl);
            string vnp_ExpireDate = timeNow.AddMinutes(expireMinute).ToString("yyyyMMddHHmmss");
            string vnp_TxnRef = orderCode.ToString();
            var inputData = new SortedList<string, string>(StringComparer.Ordinal);
            inputData.Add("vnp_Version", vnp_Version);
            inputData.Add("vnp_Command", vnp_Command);
            inputData.Add("vnp_TmnCode", vnp_TmnCode);
            inputData.Add("vnp_Amount", vnp_Amount);
            inputData.Add("vnp_CreateDate", vnp_CreateDate);
            inputData.Add("vnp_CurrCode", vnp_CurrCode);
            inputData.Add("vnp_IpAddr", vnp_IpAddr);
            inputData.Add("vnp_Locale", vnp_Locale);
            inputData.Add("vnp_OrderInfo", vnp_OrderInfo);
            inputData.Add("vnp_OrderType", vnp_OrderType);
            inputData.Add("vnp_ReturnUrl", vnp_ReturnUrl);
            inputData.Add("vnp_ExpireDate", vnp_ExpireDate);
            inputData.Add("vnp_TxnRef", vnp_TxnRef);
            string rawData = string.Join("&", inputData.Select(x => $"{x.Key}={x.Value}"));
            string signature = _tokenService.CreateSignature512(rawData, _vnPaySettings.Value.HashSecret);
            string vnPayFinalLink = $"{vnPayCreateLink}{rawData}&vnp_SecureHash={signature}";
            return vnPayFinalLink;
        }

        public bool VerifyVnPayPayment(VnPayResponse data)
        {
            var sortedData = new SortedList<string, string>(StringComparer.Ordinal);
            sortedData.Add("vnp_TmnCode", data.Vnp_TmnCode!);
            sortedData.Add("vnp_Amount", data.Vnp_Amount.ToString()!);
            sortedData.Add("vnp_BankCode", data.Vnp_BankCode!);
            sortedData.Add("vnp_BankTranNo", data.Vnp_BankTranNo!);
            sortedData.Add("vnp_CardType", data.Vnp_CardType!);
            sortedData.Add("vnp_PayDate", data.Vnp_PayDate!);
            sortedData.Add("vnp_OrderInfo", data.Vnp_OrderInfo!);
            sortedData.Add("vnp_TransactionNo", data.Vnp_TransactionNo!);
            sortedData.Add("vnp_ResponseCode", data.Vnp_ResponseCode!);
            sortedData.Add("vnp_TransactionStatus", data.Vnp_TransactionStatus!);
            sortedData.Add("vnp_TxnRef", data.Vnp_TxnRef!);
            string rawData = string.Join("&", sortedData.Select(s => $"{s.Key}={s.Value}"));
            var signature = _tokenService.CreateSignature512(rawData, _vnPaySettings.Value.HashSecret);
            if (!string.Equals(signature, data.Vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }
    }
}
