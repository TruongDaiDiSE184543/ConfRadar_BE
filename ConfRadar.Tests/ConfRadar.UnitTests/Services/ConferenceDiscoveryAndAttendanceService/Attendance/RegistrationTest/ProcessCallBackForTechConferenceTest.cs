using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.QrCode;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{
    public class ProcessCallBackForTechConferenceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IRedisService> _mockRedis = new();
        private readonly Mock<ITimeProviderService> _mockTime = new();
        private readonly Mock<IQRCoderService> _mockQrService = new();

        private readonly Mock<IPricePhaseRepository> _mockPricePhaseRepo = new();
        private readonly Mock<ITicketRepository> _mockTicketRepo = new();
        private readonly Mock<IWalletRepository> _mockWalletRepo = new();
        private readonly Mock<IWalletTransactionRepository> _mockWalletTransactionRepo = new();
        private readonly Mock<ICheckInStatusRepository> _mockCheckInStatusRepo = new();

        private PaymentService _service;

        public ProcessCallBackForTechConferenceTest()
        {
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository).Returns(_mockPricePhaseRepo.Object);
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(_mockWalletRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository).Returns(_mockWalletTransactionRepo.Object);
            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository).Returns(_mockCheckInStatusRepo.Object);

            _service = new PaymentService(
                _mockUnitOfWork.Object,
                Options.Create(new MomoSettings()),
                _mockRedis.Object,
                null,
                null, null,
                Options.Create(new PayOsSettings()),
                null,
                _mockQrService.Object,
                _mockTime.Object
            );
        }

        private TransactionDataHolder CreateSampleTransactionData()
        {
            return new TransactionDataHolder
            {
                TicketId = "T1",
                UserId = "U1",
                PaymentMethodId = "PM1",
                ConferencePriceId = "CP1",
                ConferenceId = "C1",
                ConferenceSessionIds = new List<string> { "S1", "S2" },
                PricePhaseId = "PP1"
            };
        }

        [Fact]
        public async Task ShouldThrow_WhenTransactionNotInRedis()
        {
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>()))
              .ReturnsAsync((string)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ProcessCallBackForTechConference("order1", 100000, "trans1", false));
        }

        [Fact]
        public async Task ShouldThrow_WhenPricePhaseNotFound()
        {
            var transactionData = new TransactionDataHolder
            {
                TicketId = "T1",
                UserId = "U1",
                PaymentMethodId = "PM1",
                ConferencePriceId = "CP1",
                ConferenceId = "C1",
                ConferenceSessionIds = new List<string> { "S1" },
                PricePhaseId = "PP1"
            };

            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>()))
                      .ReturnsAsync(JsonSerializer.Serialize(transactionData));

            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhaseByPricePhaseId("PP1"))
                           .ReturnsAsync((PricePhase)null);

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                           .ReturnsAsync(new CheckinStatus { CheckinStatusId = "PendingId" });

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ProcessCallBackForTechConference("order1", 100000, "trans1", false));
        }

        [Fact]
        public async Task ShouldThrow_WhenPricePhaseSlotZero()
        {
            var transactionData = new TransactionDataHolder
            {
                TicketId = "T1",
                UserId = "U1",
                PaymentMethodId = "PM1",
                ConferencePriceId = "CP1",
                ConferenceId = "C1",
                ConferenceSessionIds = new List<string> { "S1" },
                PricePhaseId = "PP1"
            };

            // Mock Redis trả về transactionData serialized
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>()))
                      .ReturnsAsync(JsonSerializer.Serialize(transactionData));

            // Mock PricePhaseRepository trả về PricePhase với AvailableSlot = 0
            var pricePhase = new PricePhase
            {
                PricePhaseId = "PP1",
                AvailableSlot = 0,
                ConferencePrice = new ConferencePrice
                {
                    AvailableSlot = 5,
                    Conference = new Conference { AvailableSlot = 5 }
                }
            };
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhaseByPricePhaseId("PP1"))
                           .ReturnsAsync(pricePhase);

            // Mock CheckInStatusRepository để code không NullReference
            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                           .ReturnsAsync(new CheckinStatus
                           {
                               CheckinStatusId = "PendingId"
                           });

            // Act + Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ProcessCallBackForTechConference("order1", 100000, "trans1", false));
        }

        [Fact]
        public async Task ShouldThrow_WhenWalletNotFound()
        {
            var transactionData = CreateSampleTransactionData();
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(transactionData));

            var pricePhase = new PricePhase { PricePhaseId = "PP1", AvailableSlot = 5, ConferencePrice = new ConferencePrice { AvailableSlot = 5, Conference = new Conference { AvailableSlot = 5 } } };
            _mockPricePhaseRepo.Setup(p => p.GetPricePhaseByPricePhaseId("PP1")).ReturnsAsync(pricePhase);
            _mockCheckInStatusRepo.Setup(c => c.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "PendingId" });
            _mockWalletRepo.Setup(w => w.GetWalletByUserIdAsync("U1")).ReturnsAsync((Wallet)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.ProcessCallBackForTechConference("order1", 100000, "trans1", true));
        }

        [Fact]
        public async Task ShouldProcessSuccessfully_WhenUseWalletFalse()
        {
            var transactionData = CreateSampleTransactionData();
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(transactionData));

            var pricePhase = new PricePhase
            {
                PricePhaseId = "PP1",
                AvailableSlot = 5,
                ConferencePrice = new ConferencePrice { AvailableSlot = 5, Conference = new Conference { AvailableSlot = 5 } }
            };
            _mockPricePhaseRepo.Setup(p => p.GetPricePhaseByPricePhaseId("PP1")).ReturnsAsync(pricePhase);
            _mockCheckInStatusRepo.Setup(c => c.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "PendingId" });
            var mockedQrData = new QrDataPayload
            {
                userCheckinId = "UCI1",
                userId = "U1",
                ticketId = "T1",
                conferenceSessionId = "S1",
                createAt = DateTime.Now,
                signature = "mockedSignature"
            };
            _mockQrService.Setup(q => q.CreateQrDataPayload(It.IsAny<QrDataPayload>()))
               .Returns(mockedQrData);
            _mockQrService.Setup(q => q.GenerateQrCode(It.IsAny<string>())).ReturnsAsync("https://qr.url");

            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var exception = await Record.ExceptionAsync(() =>
                _service.ProcessCallBackForTechConference("order1", 100000, "trans1", false));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ShouldProcessSuccessfully_WhenUseWalletTrue()
        {
            var transactionData = CreateSampleTransactionData();
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(transactionData));

            var pricePhase = new PricePhase
            {
                PricePhaseId = "PP1",
                AvailableSlot = 5,
                ConferencePrice = new ConferencePrice { AvailableSlot = 5, Conference = new Conference { AvailableSlot = 5 } }
            };
            _mockPricePhaseRepo.Setup(p => p.GetPricePhaseByPricePhaseId("PP1")).ReturnsAsync(pricePhase);
            _mockCheckInStatusRepo.Setup(c => c.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "PendingId" });
            _mockWalletRepo.Setup(w => w.GetWalletByUserIdAsync("U1")).ReturnsAsync(new Wallet { WalletId = "W1", Balance = 200000 });
            var mockedQrData = new QrDataPayload
            {
                userCheckinId = "UCI1",
                userId = "U1",
                ticketId = "T1",
                conferenceSessionId = "S1",
                createAt = DateTime.Now,
                signature = "mockedSignature"
            };
            _mockQrService.Setup(q => q.CreateQrDataPayload(It.IsAny<QrDataPayload>())).Returns(mockedQrData);
            _mockQrService.Setup(q => q.GenerateQrCode(It.IsAny<string>())).ReturnsAsync("https://qr.url");

            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.WalletRepository.UpdateWalletAsync(It.IsAny<Wallet>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository.CreateWalletTransactionAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(1);

            var exception = await Record.ExceptionAsync(() =>
                _service.ProcessCallBackForTechConference("order1", 100000, "trans1", true));

            Assert.Null(exception);
        }
    }
}
