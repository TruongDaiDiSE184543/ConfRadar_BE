using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPayOsService
    {
        Task<string> CreatePayOsPayment(long orderCode, long amount, string description, double expireMinute, List<PaymentLinkItem> payOsItems);
        Task<bool> VerifyPayOs(Webhook data);
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
                ConfirmWebhookResponse confirmResult = await client.Webhooks.ConfirmAsync(ipnLink);
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


    }
}
