using ConfRadar.Shared.DTO.Payment;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System.Text;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPayOsService
    {
        Task<string> CreatePayOsPayment(long orderCode, long amount, string description, double expireMinute, List<PaymentLinkItem> payOsItems);
        Task<bool> VerifyPayOs(Webhook data);
        Task CancelPayOs(string orderCode);
    }
    public class PayOsService : IPayOsService
    {
        private readonly IOptions<PayOsSettings> _payOsSettings;
        public PayOsService(IOptions<PayOsSettings> payOsSettings)
        {
            _payOsSettings = payOsSettings;

        }
        private PayOSClient InitPayOs()
        {
            return new PayOSClient(_payOsSettings.Value.ClientId, _payOsSettings.Value.ApiKey, _payOsSettings.Value.CheckSumKey);
        }
        public async Task<string> CreatePayOsPayment(long orderCode, long amount, string description, double expireMinute, List<PaymentLinkItem> payOsItems)
        {
            try
            {
                var client = InitPayOs();

                string ipnLink = _payOsSettings.Value.IpnLink;
                var paymentRequest = new CreatePaymentLinkRequest()
                {
                    OrderCode = orderCode,
                    Amount = amount,
                    Description = description,
                    CancelUrl = _payOsSettings.Value.CancelUrl,
                    ReturnUrl = _payOsSettings.Value.ReturnUrl,
                    Items = payOsItems,
                    ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(expireMinute).ToUnixTimeSeconds(),
                };

                CreatePaymentLinkResponse paymentResponse = await client.PaymentRequests.CreateAsync(paymentRequest);
                string checkOutUrl = paymentResponse.CheckoutUrl;
                return checkOutUrl;
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                Console.WriteLine($"Status Code: {ex.StatusCode}");
                Console.WriteLine($"Error Code: {ex.ErrorCode}");
                throw;
            }
            catch (PayOSException ex)
            {
                Console.WriteLine($"PayOS Error: {ex.Message}");
                throw;
            }

        }

        public async Task<bool> VerifyPayOs(Webhook data)
        {
            try
            {
                var client = InitPayOs();
                var result = await client.Webhooks.VerifyAsync(data);
                return true;
            }
            catch (PayOS.Exceptions.WebhookException)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public enum PayOsCancelStatusEnum
        {
            PAID,
            PENDING,
            PROCESSING,
            CANCELLED
        }
        public async Task CancelPayOs(string orderCode)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-client-id", _payOsSettings.Value.ClientId);
            httpClient.DefaultRequestHeaders.Add("x-api-key", _payOsSettings.Value.ApiKey);
            var payload = new
            {
                cancellationReason = "Huy giao dich"
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, encoding: Encoding.UTF8, "application/json");
            string cancelLink = $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}/cancel";
            HttpResponseMessage response = await httpClient.PostAsync(cancelLink, content);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonOption = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            var cancelResponse = JsonSerializer.Deserialize<PayOSCancelOrderResponse>(responseBody, jsonOption);
            if (cancelResponse == null)
            {
                throw new BadRequestException("Không nhận được phản hồi từ PayOS");
            }
            if (cancelResponse.Code != "00")
            {
                throw new BadRequestException("Invalid params");
            }
            if (cancelResponse.Data.Status != PayOsCancelStatusEnum.CANCELLED.ToString())
            {
                throw new BadRequestException("Hủy thất bại");
            }


        }

    }
}

