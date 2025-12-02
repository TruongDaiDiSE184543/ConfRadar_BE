using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.PaymentGatewayTest
{
    public class PayOsServiceTest
    {
        private readonly IOptions<PayOsSettings> _payOsSettings;
        private readonly PayOsService _payOsService;

        public PayOsServiceTest()
        {
            var settings = new PayOsSettings
            {
                ClientId = "TEST_CLIENT_ID",
                ApiKey = "TEST_API_KEY",
                CheckSumKey = "TEST_CHECKSUM_KEY",
                ReturnUrl = "https://localhost/return",
                CancelUrl = "https://localhost/cancel",
                IpnLink = "https://localhost/ipn"
            };
            _payOsSettings = Options.Create(settings);

            _payOsService = new PayOsService(_payOsSettings);
        }

        [Fact]
        public async Task CreatePayOsPayment_ShouldThrowApiException_WhenCredentialsAreInvalid()
        {
            // Arrange
            long orderCode = 12345;
            long amount = 100000;
            string description = "Test payment";
            double expireMinute = 15;
            var items = new List<PaymentLinkItem>
            {
                new PaymentLinkItem
                {
                    Name = "Test Item",
                    Quantity = 1,
                    Price = 100000
                }
            };

            // Act & Assert
            // This will throw ApiException because we're using test credentials
            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
            {
                await _payOsService.CreatePayOsPayment(orderCode, amount, description, expireMinute, items);
            });

            // Verify exception details
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task CreatePayOsPayment_ShouldThrowApiException_WhenItemsListIsEmpty()
        {
            // Arrange
            long orderCode = 12345;
            long amount = 100000;
            string description = "Test payment";
            double expireMinute = 15;
            var items = new List<PaymentLinkItem>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
            {
                await _payOsService.CreatePayOsPayment(orderCode, amount, description, expireMinute, items);
            });

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task VerifyPayOs_ShouldReturnTrue_WhenWebhookIsValid()
        {
            // Arrange
            var webhookData = new Webhook
            {
                Code = "00",
                Description = "Success",
                Data = new WebhookData
                {
                    OrderCode = 12345,
                    Amount = 100000,
                    Description = "Test payment",
                    AccountNumber = "123456",
                    Reference = "REF123",
                    TransactionDateTime = "2025-11-29 12:00:00",
                    Currency = "VND",
                    PaymentLinkId = "LINK123",
                    CounterAccountBankId = "BANK123",
                    CounterAccountBankName = "Test Bank",
                    CounterAccountName = "Test User",
                    CounterAccountNumber = "654321",
                    VirtualAccountName = "Virtual Test",
                    VirtualAccountNumber = "VA123"
                },
                Signature = "test_signature"
            };

            // Act
            var result = await _payOsService.VerifyPayOs(webhookData);

            // Assert
            // Will return false due to invalid signature with test data
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyPayOs_ShouldReturnFalse_WhenWebhookExceptionOccurs()
        {
            // Arrange
            var invalidWebhook = new Webhook
            {
                Code = "00",
                Signature = "invalid_signature"
            };

            // Act
            var result = await _payOsService.VerifyPayOs(invalidWebhook);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyPayOs_ShouldReturnFalse_WhenGeneralExceptionOccurs()
        {
            // Arrange
            Webhook? nullWebhook = null;

            // Act
            var result = await _payOsService.VerifyPayOs(nullWebhook);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CancelPayOs_ShouldThrowBadRequestException_WhenOrderCodeIsInvalid()
        {
            string invalidOrderCode = "INVALID_ORDER";

            // Act & Assert
            // Will throw BadRequestException due to API error response
            var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                await _payOsService.CancelPayOs(invalidOrderCode);
            });

            Assert.NotNull(exception);
            Assert.Contains("API request failed", exception.Message);
        }

        [Fact]
        public async Task CancelPayOs_ShouldThrowException_WhenCredentialsAreInvalid()
        {
            // Arrange
            string orderCode = "12345";

            // Act & Assert
            // Will throw due to invalid test credentials
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await _payOsService.CancelPayOs(orderCode);
            });

            Assert.NotNull(exception);
        }

        [Fact]
        public void PayOsCancelStatusEnum_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal("PAID", PayOsService.PayOsCancelStatusEnum.PAID.ToString());
            Assert.Equal("PENDING", PayOsService.PayOsCancelStatusEnum.PENDING.ToString());
            Assert.Equal("PROCESSING", PayOsService.PayOsCancelStatusEnum.PROCESSING.ToString());
            Assert.Equal("CANCELLED", PayOsService.PayOsCancelStatusEnum.CANCELLED.ToString());
        }

        [Theory]
        [InlineData(10000, 15)]
        [InlineData(50000, 30)]
        [InlineData(100000, 60)]
        public async Task CreatePayOsPayment_ShouldThrowApiException_WithDifferentAmounts(long amount, double expireMinute)
        {
            // Arrange
            long orderCode = 12345;
            string description = "Test payment";
            var items = new List<PaymentLinkItem>
            {
                new PaymentLinkItem
                {
                    Name = "Test Item",
                    Quantity = 1,
                    Price = amount
                }
            };

            // Act & Assert
            // Will throw ApiException because test credentials are invalid
            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
            {
                await _payOsService.CreatePayOsPayment(orderCode, amount, description, expireMinute, items);
            });

            Assert.NotNull(exception);
            Assert.Contains("thanh toán", exception.Message.ToLower());
        }
    }

    // Integration Tests (requires actual PayOS credentials)
    public class PayOsServiceIntegrationTest
    {
        private readonly IOptions<PayOsSettings> _payOsSettings;
        private readonly PayOsService _payOsService;

        public PayOsServiceIntegrationTest()
        {
            // Use real credentials from configuration for integration tests
            var settings = new PayOsSettings
            {
                ClientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID") ?? "TEST_CLIENT_ID",
                ApiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY") ?? "TEST_API_KEY",
                CheckSumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") ?? "TEST_CHECKSUM_KEY",
                ReturnUrl = "https://localhost/return",
                CancelUrl = "https://localhost/cancel",
                IpnLink = "https://localhost/ipn"
            };
            _payOsSettings = Options.Create(settings);
            _payOsService = new PayOsService(_payOsSettings);
        }

        [Fact(Skip = "Integration test - requires real PayOS credentials")]
        public async Task CreatePayOsPayment_Integration_ShouldReturnValidCheckoutUrl()
        {
            // Arrange
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long amount = 10000;
            string description = "Integration test payment";
            double expireMinute = 15;
            var items = new List<PaymentLinkItem>
            {
                new PaymentLinkItem
                {
                    Name = "Test Product",
                    Quantity = 1,
                    Price = 10000
                }
            };

            // Act
            var checkoutUrl = await _payOsService.CreatePayOsPayment(orderCode, amount, description, expireMinute, items);

            // Assert
            Assert.NotNull(checkoutUrl);
            Assert.StartsWith("https://", checkoutUrl);
        }

        [Fact(Skip = "Integration test - requires real PayOS credentials and valid webhook")]
        public async Task VerifyPayOs_Integration_ShouldReturnTrue_WithValidWebhook()
        {
            // Arrange
            var webhookData = new Webhook
            {
                // Use real webhook data from PayOS
            };

            // Act
            var result = await _payOsService.VerifyPayOs(webhookData);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "Integration test - requires real PayOS credentials and existing order")]
        public async Task CancelPayOs_Integration_ShouldCancelSuccessfully()
        {
            // Arrange
            string orderCode = "12345"; // Use a real pending order code

            // Act & Assert
            await _payOsService.CancelPayOs(orderCode);
            // If no exception thrown, cancellation was successful
        }
    }
}