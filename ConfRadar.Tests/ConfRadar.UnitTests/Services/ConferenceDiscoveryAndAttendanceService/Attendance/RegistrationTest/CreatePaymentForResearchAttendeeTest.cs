using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services;
using ConfRadar.Services.DTOs;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Payment;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{
    public class CreatePaymentForResearchAttendeeTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IRedisService> _mockRedis = new();
        private readonly Mock<IMomoService> _mockMomo = new();
        private readonly Mock<IPayOsService> _mockPayOs = new();
        private readonly Mock<IVnPayService> _mockVnPay = new();
        private readonly Mock<ITimeProviderService> _mockTime = new();

        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo = new();
        private readonly Mock<IConferencePriceRepository> _mockConferencePriceRepo = new();
        private readonly Mock<ITicketRepository> _mockTicketRepo = new();
        private readonly Mock<IWalletRepository> _mockWalletRepo = new();
        private readonly Mock<IPaperWaitListRepository> _mockPaperWaitListRepo = new();

        private PaymentService _service;

        public CreatePaymentForResearchAttendeeTest()
        {
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(_mockWalletRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperWaitListRepository).Returns(_mockPaperWaitListRepo.Object);

            _service = new PaymentService(
                _mockUnitOfWork.Object,
                Options.Create(new MomoSettings()),
                _mockRedis.Object,
                null, // token service không dùng ở đây
                _mockMomo.Object,
                _mockPayOs.Object,
                Options.Create(new PayOsSettings()),
                _mockVnPay.Object,
                null, // QR service không dùng
                _mockTime.Object
            );
        }

        private ConferencePrice CreateValidConferencePrice(decimal ticketPrice = 200000)
        {
            return new ConferencePrice
            {
                ConferencePriceId = "CP1",
                TicketPrice = ticketPrice,
                IsAuthor = false,
                AvailableSlot = 5,
                ConferenceId = "C1",
                Conference = new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "ResearchConf",
                    AvailableSlot = 5,
                    IsResearchConference = true,
                    IsInternalHosted = true,
                    TicketSaleStart = DateOnly.Parse("2025-01-01"),
                    TicketSaleEnd = DateOnly.Parse("2025-12-30"),
                    ResearchConferenceDetail = new ResearchConferenceDetail
                    {
                        AllowListener = true
                    },
                    ResearchConferencePhases = new List<ResearchConferencePhase>
                    {
                        new ResearchConferencePhase
                        {
                            ResearchConferencePhaseId = "RCP1",
                            IsActive = true,
                            RegistrationStartDate = DateOnly.Parse("2025-01-01"),
                            RegistrationEndDate = DateOnly.Parse("2025-12-30")
                        }
                    },
                    ConferenceSessions = new List<ConferenceSession>
                    {
                        new ConferenceSession { ConferenceSessionId = "S1" }
                    }
                },
                PricePhases = new List<PricePhase>
                {
                    new PricePhase
                    {
                        PricePhaseId = "PP1",
                        StartDate = DateOnly.Parse("2025-01-01"),
                        EndDate = DateOnly.Parse("2025-12-30"),
                        AvailableSlot = 5,
                        ApplyPercent = 100
                    }
                }
            };
        }

        private void MockDate(string yyyyMMdd)
        {
            _mockTime.Setup(t => t.GetVietnamDate())
                     .ReturnsAsync(DateOnly.Parse(yyyyMMdd));
        }

        [Fact]
        public async Task ShouldThrow_WhenPaymentMethodNotFound()
        {
            var req = new CreateResearchAttendeePaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync((PaymentMethod)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForResearchAsAttendee(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenConferencePriceNotFound()
        {
            var req = new CreateResearchAttendeePaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync(new PaymentMethod());
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync((ConferencePrice)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForResearchAsAttendee(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenConferenceSoldOut()
        {
            var req = new CreateResearchAttendeePaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };
            var cp = CreateValidConferencePrice();
            cp.Conference.AvailableSlot = 0;
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync(new PaymentMethod());
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForResearchAsAttendee(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenNotAllowedListener()
        {
            var req = new CreateResearchAttendeePaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };
            var cp = CreateValidConferencePrice();
            cp.Conference.ResearchConferenceDetail.AllowListener = false;
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync(new PaymentMethod());
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForResearchAsAttendee(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenRedisLockExists_WithDifferentPaymentMethod()
        {
            var req = new CreateResearchAttendeePaymentRequest { PaymentMethodId = "PM2", ConferencePriceId = "CP1" };
            var cp = CreateValidConferencePrice();
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM2")).ReturnsAsync(new PaymentMethod { MethodName = "MoMo" });
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);

            var lockData = new PaymentLockKeyDTO { PaymentMethodId = "PM1", OldCheckOutUrl = "urlOld" };
            _mockRedis.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(lockData));
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync(new PaymentMethod { MethodName = "PayOs" });

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForResearchAsAttendee(req, "U1"));
        }

        [Fact]
        public async Task ShouldReturnExistingPayment_WhenRedisLockExists_AndSameMethod()
        {
            var req = new CreateResearchAttendeePaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };
            var cp = CreateValidConferencePrice();
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync(new PaymentMethod { MethodName = "MoMo" });
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);

            var lockData = new PaymentLockKeyDTO { PaymentMethodId = "PM1", OldCheckOutUrl = "https://oldurl.com" };
            _mockRedis.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(lockData));

            var result = await _service.CreatePaymentForResearchAsAttendee(req, "U1");

            Assert.False(result.PaymentCreateSuccess);
            Assert.Equal("https://oldurl.com", result.CheckOutUrl);
        }

        // TODO: Thêm các test cho happy path: PayOs, MoMo, VnPay, Wallet
    }
}
