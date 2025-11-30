using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.QrCode;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{
    public class ProcessCallBackForResearchConferenceAbstractSubmissionTest
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
        private readonly Mock<IPaperPhaseRepository> _mockPaperPhaseRepo = new();
        private readonly Mock<IGlobalStatusRepository> _mockGlobalStatusRepo = new();
        private readonly Mock<IPaperRepository> _mockPaperRepo = new();

        private PaymentService _service;

        public ProcessCallBackForResearchConferenceAbstractSubmissionTest()
        {
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository).Returns(_mockPricePhaseRepo.Object);
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(_mockWalletRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository).Returns(_mockWalletTransactionRepo.Object);
            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository).Returns(_mockCheckInStatusRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository).Returns(_mockPaperPhaseRepo.Object);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository).Returns(_mockGlobalStatusRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(_mockPaperRepo.Object);

            _service = new PaymentService(
                _mockUnitOfWork.Object,
                Options.Create(new MomoSettings()),
                _mockRedis.Object,
                null,
                null,
                null,
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
                ResearchConferencePhaseId = "RCP1",
                ConferenceSessionIds = new List<string> { "S1", "S2" },
                PricePhaseId = "PP1",
                Title = "Sample Paper",
                Description = "Sample Desc",
                PaymentConferenceLockKey = "lock1",
                PaymentPhaseLockKey = "lock2"
            };
        }

        [Fact]
        public async Task ShouldThrow_WhenTransactionNotInRedis()
        {
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync((string)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ProcessCallBackForResearchConferenceAbstractSubmission("order1", 100000, "trans1", false));
        }

        [Fact]
        public async Task ShouldThrow_WhenStatusesOrPaperPhaseNotFound()
        {
            var transactionData = CreateSampleTransactionData();
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(transactionData));

            _mockCheckInStatusRepo.Setup(c => c.GetCheckInStatusByNameAsync(It.IsAny<string>())).ReturnsAsync((CheckinStatus)null);
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync((GlobalStatus)null);
            _mockPaperPhaseRepo.Setup(p => p.GetPaperPhaseByNameAsync(It.IsAny<string>())).ReturnsAsync((PaperPhase)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.ProcessCallBackForResearchConferenceAbstractSubmission("order1", 100000, "trans1", false));
        }

        [Fact]
        public async Task ShouldThrow_WhenPricePhaseNotFound()
        {
            var transactionData = CreateSampleTransactionData();
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(transactionData));

            _mockCheckInStatusRepo.Setup(c => c.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "PendingId" });
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "PendingId" });
            _mockPaperPhaseRepo.Setup(p => p.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PPAbstract" });

            _mockPricePhaseRepo.Setup(p => p.GetPricePhaseByPricePhaseId("PP1")).ReturnsAsync((PricePhase)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ProcessCallBackForResearchConferenceAbstractSubmission("order1", 100000, "trans1", false));
        }

        [Fact]
        public async Task ShouldThrow_WhenPricePhaseSlotZero()
        {
            var transactionData = CreateSampleTransactionData();
            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(transactionData));

            var pricePhase = new PricePhase
            {
                PricePhaseId = "PP1",
                AvailableSlot = 0,
                ConferencePrice = new ConferencePrice
                {
                    AvailableSlot = 0,
                    Conference = new Conference { AvailableSlot = 0 }
                }
            };
            _mockPricePhaseRepo.Setup(p => p.GetPricePhaseByPricePhaseId("PP1")).ReturnsAsync(pricePhase);

            _mockCheckInStatusRepo.Setup(c => c.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "PendingId" });
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "PendingId" });
            _mockPaperPhaseRepo.Setup(p => p.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PPAbstract" });

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ProcessCallBackForResearchConferenceAbstractSubmission("order1", 100000, "trans1", false));
        }

        [Fact]
        public async Task ShouldThrow_WhenWalletNotFound()
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
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "PendingId" });
            _mockPaperPhaseRepo.Setup(p => p.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PPAbstract" });
            _mockWalletRepo.Setup(w => w.GetWalletByUserIdAsync("U1")).ReturnsAsync((Wallet)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.ProcessCallBackForResearchConferenceAbstractSubmission("order1", 100000, "trans1", true));
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
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "PendingId" });
            _mockPaperPhaseRepo.Setup(p => p.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PPAbstract" });

            var mockedQrData = new QrDataPayload { userCheckinId = "UCI1" };
            _mockQrService.Setup(q => q.CreateQrDataPayload(It.IsAny<QrDataPayload>())).Returns(mockedQrData);
            _mockQrService.Setup(q => q.GenerateQrCode(It.IsAny<string>())).ReturnsAsync("https://qr.url");

            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.TicketRepository.CreateTicketAsync(It.IsAny<Ticket>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.PaperRepository.CreatePaperAsync(It.IsAny<Paper>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.UpdatePricePhaseAsync(It.IsAny<PricePhase>())).ReturnsAsync(1);

            var exception = await Record.ExceptionAsync(() =>
                _service.ProcessCallBackForResearchConferenceAbstractSubmission("order1", 100000, "trans1", false));

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
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "PendingId" });
            _mockPaperPhaseRepo.Setup(p => p.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PPAbstract" });
            _mockWalletRepo.Setup(w => w.GetWalletByUserIdAsync("U1")).ReturnsAsync(new Wallet { WalletId = "W1", Balance = 200000 });

            var mockedQrData = new QrDataPayload { userCheckinId = "UCI1" };
            _mockQrService.Setup(q => q.CreateQrDataPayload(It.IsAny<QrDataPayload>())).Returns(mockedQrData);
            _mockQrService.Setup(q => q.GenerateQrCode(It.IsAny<string>())).ReturnsAsync("https://qr.url");

            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.TicketRepository.CreateTicketAsync(It.IsAny<Ticket>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.PaperRepository.CreatePaperAsync(It.IsAny<Paper>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.UpdatePricePhaseAsync(It.IsAny<PricePhase>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.WalletRepository.UpdateWalletAsync(It.IsAny<Wallet>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository.CreateWalletTransactionAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(1);

            var exception = await Record.ExceptionAsync(() =>
                _service.ProcessCallBackForResearchConferenceAbstractSubmission("order1", 100000, "trans1", true));

            Assert.Null(exception);
        }
    }
}
