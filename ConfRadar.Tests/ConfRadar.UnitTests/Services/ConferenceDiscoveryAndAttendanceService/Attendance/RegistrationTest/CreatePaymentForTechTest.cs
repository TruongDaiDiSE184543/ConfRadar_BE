using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Payment;
using Microsoft.Extensions.Options;
using Moq;
using PayOS.Models.V2.PaymentRequests;
using static ConfRadar.Services.Common.AppSettingConfig;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{

    public class CreatePaymentForTechTest
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IRedisService> _mockRedis;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IMomoService> _mockMomo;
        private readonly Mock<IPayOsService> _mockPayOs;
        private readonly Mock<IVnPayService> _mockVnPay;
        private readonly Mock<IQRCoderService> _mockQrCoder;
        private readonly Mock<ITimeProviderService> _mockTime;

        private readonly PaymentService _service;

        public CreatePaymentForTechTest()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockRedis = new Mock<IRedisService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockMomo = new Mock<IMomoService>();
            _mockPayOs = new Mock<IPayOsService>();
            _mockVnPay = new Mock<IVnPayService>();
            _mockQrCoder = new Mock<IQRCoderService>();
            _mockTime = new Mock<ITimeProviderService>();

            var momoOptions = Options.Create(new MomoSettings());
            var payOsOptions = Options.Create(new PayOsSettings());

            _service = new PaymentService(
                _mockUow.Object,
                momoOptions,
                _mockRedis.Object,
                _mockTokenService.Object,
                _mockMomo.Object,
                _mockPayOs.Object,
                payOsOptions,
                _mockVnPay.Object,
                _mockQrCoder.Object,
                _mockTime.Object
            );
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task CreatePayment_ShouldThrow_WhenPaymentMethodNotFound()
        {
            // Arrange
            var req = new CreateTechPaymentRequest { PaymentMethodId = "PM01", ConferencePriceId = "CP01" };
            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync((PaymentMethod)null);

            // Act + Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForTechConference(req, "U01"));
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task CreatePayment_ShouldThrow_WhenConferencePriceNotFound()
        {
            var req = new CreateTechPaymentRequest { PaymentMethodId = "PM01", ConferencePriceId = "CP01" };

            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync(new PaymentMethod());

            _mockUow.Setup(x => x.ConferencePriceRepository.GetConferencePriceByIdAsync("CP01"))
                .ReturnsAsync((ConferencePrice)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForTechConference(req, "U01"));
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task CreatePayment_ShouldThrow_WhenConferenceSoldOut()
        {
            var req = new CreateTechPaymentRequest { PaymentMethodId = "PM01", ConferencePriceId = "CP01" };

            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync(new PaymentMethod());

            _mockUow.Setup(x => x.ConferencePriceRepository.GetConferencePriceByIdAsync("CP01"))
                .ReturnsAsync(new ConferencePrice
                {
                    Conference = new Conference { AvailableSlot = 0 }
                });

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForTechConference(req, "U01"));
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task CreatePayment_ShouldReturnOldUrl_WhenPaymentLockExists()
        {
            var req = new CreateTechPaymentRequest { PaymentMethodId = "PM01", ConferencePriceId = "CP01" };

            var lockValue = new PaymentLockKeyDTO()
            {
                OldCheckOutUrl = "https://old",
                PaymentMethodId = "PM01"
            };

            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync(new PaymentMethod());

            _mockUow.Setup(x => x.ConferencePriceRepository.GetConferencePriceByIdAsync("CP01"))
                .ReturnsAsync(new ConferencePrice
                {
                    Conference = new Conference { AvailableSlot = 10 }
                });

            _mockRedis.Setup(x => x.KeyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockRedis.Setup(x => x.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(lockValue));

            var result = await _service.CreatePaymentForTechConference(req, "U01");

            Assert.False(result.PaymentCreateSuccess);
            Assert.Equal("https://old", result.CheckOutUrl);
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task CreatePayment_ShouldThrow_WhenUserAlreadyBoughtTicket()
        {
            var req = new CreateTechPaymentRequest { PaymentMethodId = "PM01", ConferencePriceId = "CP01" };

            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync(new PaymentMethod());

            _mockUow.Setup(x => x.ConferencePriceRepository.GetConferencePriceByIdAsync("CP01"))
                .ReturnsAsync(new ConferencePrice
                {
                    Conference = new Conference { AvailableSlot = 10 }
                });

            _mockRedis.Setup(x => x.KeyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockUow.Setup(x => x.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId("U01", It.IsAny<string>()))
                .ReturnsAsync(new List<Ticket>
                {
                new Ticket{ IsRefunded = false }
                });

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForTechConference(req, "U01"));
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task CreatePayment_ShouldReturnFail_WhenPhaseSlotsLockedExternally()
        {
            var req = new CreateTechPaymentRequest { PaymentMethodId = "PM01", ConferencePriceId = "CP01" };

            var phase = new PricePhase
            {
                PricePhaseId = "PP01",
                AvailableSlot = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            };

            var confPrice = new ConferencePrice
            {
                TicketPrice = 200000,
                PricePhases = new List<PricePhase> { phase },
                Conference = new Conference
                {
                    AvailableSlot = 10,
                    TicketSaleStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                    TicketSaleEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                    ConferenceSessions = new List<ConferenceSession>()
                }
            };

            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync(new PaymentMethod { MethodName = "PayOs" });

            _mockUow.Setup(x => x.ConferencePriceRepository.GetConferencePriceByIdAsync("CP01"))
                .ReturnsAsync(confPrice);

            _mockRedis.Setup(x => x.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            _mockUow.Setup(x => x.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Ticket>());

            _mockTime.Setup(x => x.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            _mockRedis.Setup(x => x.GetKeysByPatternAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "lock1" }); // =1 locked, = slot 1 → full

            var result = await _service.CreatePaymentForTechConference(req, "U01");

            Assert.False(result.PaymentCreateSuccess);
            Assert.Null(result.CheckOutUrl);
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task CreatePayment_ShouldCreatePayOsPayment()
        {
            var req = new CreateTechPaymentRequest { PaymentMethodId = "PM01", ConferencePriceId = "CP01" };
            _mockUow.Setup(r => r.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
    .ReturnsAsync(new ConferenceStatus
    {
        ConferenceStatusId = "CS_READY",
        ConferenceStatusName = "Ready"
    });
   
         

            var conf = new Conference
            {
                ConferenceId = "C1",
                ConferenceName = "Tech Conf",
                AvailableSlot = 100,
                TicketSaleStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                TicketSaleEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                ConferenceStatus = new ConferenceStatus
                {
                    ConferenceStatusId = "CS_READY",
                    ConferenceStatusName = "Ready"
                },
                ConferenceSessions = new List<ConferenceSession>()
            };

            var cp = new ConferencePrice
            {
                ConferencePriceId = "CP1",
                ConferenceId = "C1",
                TicketPrice = 50000,
                IsAuthor = false,
                Conference = conf,
                PricePhases = new List<PricePhase>() {
        new PricePhase {
            PricePhaseId = "PP1",
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddHours(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddHours(1)),
            ApplyPercent = 100,
            AvailableSlot = 100,
        }
    }
            };


            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync(new PaymentMethod { MethodName = "PayOs" });

            _mockUow.Setup(x => x.ConferencePriceRepository.GetConferencePriceByIdAsync("CP01"))
                .ReturnsAsync(cp);

            _mockRedis.Setup(x => x.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            _mockUow.Setup(x => x.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Ticket>());

            _mockTime.Setup(x => x.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            _mockRedis.Setup(x => x.GetKeysByPatternAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<string>()); // slot not full

            _mockPayOs.Setup(x => x.CreatePayOsPayment(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<List<PaymentLinkItem>>()))
                .ReturnsAsync("https://payos");

            var result = await _service.CreatePaymentForTechConference(req, "U01");

            Assert.True(result.PaymentCreateSuccess);
            Assert.Equal("https://payos", result.CheckOutUrl);
        }

    }
}
