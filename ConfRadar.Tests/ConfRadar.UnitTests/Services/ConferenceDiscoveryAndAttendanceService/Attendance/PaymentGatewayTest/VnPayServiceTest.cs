using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Payment;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.PaymentGatewayTest
{
    public class VnPayServiceTest
    {
        private readonly Mock<ITimeProviderService> _mockTimeProvider;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly IOptions<VnPaySettings> _vnPaySettings;
        private readonly VnPayService _vnPayService;

        public VnPayServiceTest()
        {
            _mockTimeProvider = new Mock<ITimeProviderService>();
            _mockTokenService = new Mock<ITokenService>();

            var settings = new VnPaySettings
            {
                TmnCode = "TESTCODE",
                ReturnUrl = "https://localhost/return",
                HashSecret = "SECRET123"
            };
            _vnPaySettings = Options.Create(settings);

            _vnPayService = new VnPayService(_vnPaySettings, _mockTokenService.Object, _mockTimeProvider.Object);
        }

        [Fact]
        public void CreateVnPayPayment_ShouldReturnValidLink()
        {
            // Arrange
            long orderCode = 123;
            long amount = 10000;
            double expireMinute = 15;

            var fixedTime = new DateTime(2025, 11, 29, 12, 0, 0);
            _mockTimeProvider.Setup(tp => tp.GetVietnamTime()).ReturnsAsync(fixedTime);

            _mockTokenService
                .Setup(ts => ts.CreateSignature512(It.IsAny<string>(), _vnPaySettings.Value.HashSecret))
                .Returns("FAKESIGNATURE");

            // Act
            var result = _vnPayService.CreateVnPayPayment(orderCode, amount, expireMinute);

            // Assert
            Assert.Contains("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?", result);
            Assert.Contains("vnp_SecureHash=FAKESIGNATURE", result);
        }

        [Fact]
        public void VerifyVnPayPayment_ShouldReturnTrue_WhenSignatureMatches()
        {
            // Arrange
            var data = new VnPayResponse
            {
                Vnp_TmnCode = "TESTCODE",
                Vnp_Amount = 10000,
                Vnp_BankCode = "NCB",
                Vnp_BankTranNo = "123456",
                Vnp_CardType = "ATM",
                Vnp_PayDate = "20251129120000",
                Vnp_OrderInfo = "giaodich",
                Vnp_TransactionNo = "78910",
                Vnp_ResponseCode = "00",
                Vnp_TransactionStatus = "00",
                Vnp_TxnRef = "123",
                Vnp_SecureHash = "FAKESIGNATURE"
            };

            _mockTokenService
                .Setup(ts => ts.CreateSignature512(It.IsAny<string>(), _vnPaySettings.Value.HashSecret))
                .Returns("FAKESIGNATURE");

            // Act
            var result = _vnPayService.VerifyVnPayPayment(data);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyVnPayPayment_ShouldReturnFalse_WhenSignatureDoesNotMatch()
        {
            // Arrange
            var data = new VnPayResponse
            {
                Vnp_TmnCode = "TESTCODE",
                Vnp_Amount = 10000,
                Vnp_BankCode = "NCB",
                Vnp_BankTranNo = "123456",
                Vnp_CardType = "ATM",
                Vnp_PayDate = "20251129120000",
                Vnp_OrderInfo = "giaodich",
                Vnp_TransactionNo = "78910",
                Vnp_ResponseCode = "00",
                Vnp_TransactionStatus = "00",
                Vnp_TxnRef = "123",
                Vnp_SecureHash = "WRONGSIGNATURE"
            };

            _mockTokenService
                .Setup(ts => ts.CreateSignature512(It.IsAny<string>(), _vnPaySettings.Value.HashSecret))
                .Returns("FAKESIGNATURE");

            // Act
            var result = _vnPayService.VerifyVnPayPayment(data);

            // Assert
            Assert.False(result);
        }
    }
}