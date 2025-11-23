using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPayOsService
    {
        Task<string> CreatePayOsPayment(long orderCode, long amount, string description, double expireMinute, List<PaymentLinkItem> payOsItems);
        Task<bool> VerifyPayOs(Webhook data);
        Task CancelPayOs(string id);
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
        public async Task CancelPayOs(string id)
        {
            var client = InitPayOs();
            var payLoad = new PayOS.Models.V2.PaymentRequests.CancelPaymentLinkRequest
            {
                CancellationReason = "Huy giao dich"
            };
            var options = new PayOS.Models.RequestOptions<object>
            {
                Body = payLoad
            };
            string cancelLink = $"https://api-merchant.payos.vn/v2/payment-requests/{id}/cancel";
            var response = client.PostAsync<object, object>(cancelLink, options);

            Console.WriteLine("---- RAW RESPONSE ----");
            Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        }
    

           

        }

    }

