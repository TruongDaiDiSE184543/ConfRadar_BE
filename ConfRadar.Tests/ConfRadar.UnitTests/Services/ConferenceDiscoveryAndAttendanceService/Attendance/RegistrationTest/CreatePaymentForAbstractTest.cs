using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Payment;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{
    public class CreatePaymentForAbstractTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IRedisService> _mockRedis = new();
        private readonly Mock<IMomoService> _mockMomo = new();
        private readonly Mock<IPayOsService> _mockPayOs = new();
        private readonly Mock<IVnPayService> _mockVnPay = new();
        private readonly Mock<IQRCoderService> _mockQr = new();
        private readonly Mock<ITimeProviderService> _mockTime = new();
        private readonly Mock<ITokenService> _mockToken = new();
        private readonly Mock<IRedisService> _mockRedisService = new();
        // Repositories
        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo = new();
        private readonly Mock<IConferencePriceRepository> _mockConferencePriceRepo = new();
        private readonly Mock<IPaperRepository> _mockPaperRepo = new();
        private readonly Mock<ITransactionRepository> _mockTransactionRepo = new();
        private readonly Mock<IConferenceStatusRepository> _mockConferenceStatusRepo = new();

        private readonly Mock<ICameraReadyRepository> _mockCameraReadyRepo = new();
        private readonly Mock<IGlobalStatusRepository> _mockGlobalStatusRepo = new();
        private readonly Mock<IPaperWaitListRepository> _mockPaperWaitListRepo = new();
        private readonly Mock<IAuditLogRepository> _mockAuditRepo = new();
        private readonly Mock<INotificationRepository> _mockNotificationRepo = new();


        private readonly Mock<ITicketRepository> _mockTicketRepo = new();
        private readonly Mock<IWalletRepository> _mockWalletRepo = new();

        private PaymentService _service;

        public CreatePaymentForAbstractTest()
        {
            // Gán repo vào UnitOfWork
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(_mockWalletRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(_mockPaperRepo.Object);
            _mockUnitOfWork.Setup(u => u.AuditLogRepository).Returns(_mockAuditRepo.Object);
            _mockUnitOfWork.Setup(u => u.NotificationRepository).Returns(_mockNotificationRepo.Object);
            _mockUnitOfWork.Setup(u => u.TransactionRepository).Returns(_mockTransactionRepo.Object);

            _mockUnitOfWork.Setup(u => u.CameraReadyRepository).Returns(_mockCameraReadyRepo.Object);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository).Returns(_mockGlobalStatusRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperWaitListRepository).Returns(_mockPaperWaitListRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository).Returns(_mockConferenceStatusRepo.Object);


            // Create service AFTER setup
            _service = new PaymentService(
                _mockUnitOfWork.Object,
                Options.Create(new MomoSettings()),
                _mockRedis.Object,
                _mockToken.Object,
                _mockMomo.Object,
                _mockPayOs.Object,
                Options.Create(new PayOsSettings()),
                _mockVnPay.Object,
                _mockQr.Object,
                _mockTime.Object
            );
        }
        private ConferencePrice CreateValidConferencePrice(decimal ticketPrice = 200000)
        {
            return new ConferencePrice
            {
                ConferencePriceId = "CP1",
                TicketPrice = ticketPrice,
                IsAuthor = true,
                AvailableSlot = 10,
                ConferenceId = "C1",
                Conference = new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "ResearchConf",
                    AvailableSlot = 5,
                    IsResearchConference = true,
                    IsInternalHosted = true,
                    ResearchConferenceDetail = new ResearchConferenceDetail
                    {
                        NumberPaperAccept = 10
                    },
                    ResearchConferencePhases = new List<ResearchConferencePhase>()
                    {
                        new ResearchConferencePhase
                        {
                            ResearchConferencePhaseId = "RCP1",
                            IsActive = true,
                            AuthorPaymentStart = DateOnly.Parse("2025-01-01"),
                            AuthorPaymentEnd = DateOnly.Parse("2025-12-30"),
                            RegistrationStartDate = DateOnly.Parse("2025-01-01"),
                            RegistrationEndDate = DateOnly.Parse("2025-12-30")
                        }
                    },
                    ConferenceSessions = new List<ConferenceSession>()
                    {
                        new ConferenceSession { ConferenceSessionId = "S1" }
                    }
                },
                PricePhases = new List<PricePhase>()
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

        // Setup base valid mocks for "happy path" then tests override what they want
        private void SetupBaseValidMocks(string paymentMethodId = "PM1", string paymentMethodName = "MoMo", string paperId = "P1", string userId = "U1")
        {
            MockDate("2025-02-01");

            // payment method
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById(paymentMethodId))
                .ReturnsAsync(new PaymentMethod { PaymentMethodId = paymentMethodId, MethodName = paymentMethodName });

            // global status
            _mockGlobalStatusRepo.Setup(r => r.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "GS_ACCEPTED" });

            // paper + root author + camera ready
            var paper = new Paper
            {
                PaperId = paperId,
                CameraReadyId = "CR1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = userId, IsRootAuthor = true } },
                TicketId = null,
                ConferenceId = "C1",
                Conference = new Conference
                {
                    ConferenceId = "C1",
                    ConferenceStatus = new ConferenceStatus
                    {
                        ConferenceStatusId = "CS_READY",
                        ConferenceStatusName = "Ready"
                    }
                },

            };
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync(paperId)).ReturnsAsync(paper);

            _mockCameraReadyRepo.Setup(r => r.GetCameraReadyByIdAsync("CR1"))
                .ReturnsAsync(new CameraReady { CameraReadyId = "CR1", GlobalStatusId = "GS_ACCEPTED" });

            // conference price
            var cp = CreateValidConferencePrice();
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);

            // paper count below limit
            _mockPaperRepo.Setup(r => r.GetPaperCountByConference("C1")).ReturnsAsync(0);

            // no tickets
            _mockTicketRepo.Setup(r => r.GetAttendeeTicketByUserIdAndConferenceId(userId, "C1"))
                .ReturnsAsync(new List<Ticket>());
            _mockTicketRepo.Setup(r => r.GetAuthorTicketByUserIdAndConferenceId(userId, "C1"))
                .ReturnsAsync(new List<Ticket>());

            // redis default: no lock, no phase lock
            _mockRedis.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockRedis.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            // paper waitlist default none
            _mockPaperWaitListRepo.Setup(r => r.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, "C1"))
                .ReturnsAsync((PaperWaitList)null);

            // wallet default (not used unless Wallet path)
            _mockWalletRepo.Setup(r => r.GetWalletByUserIdAsync(userId)).ReturnsAsync(new Wallet { WalletId = "W1", Balance = 1000000 });
        }
        [Fact]
        public async Task ShouldThrow_WhenConferenceNotReady()
        {
            // Arrange
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks(); // giữ các mock cơ bản

            // Override: make the paper's conference status NOT ready
            var paper = new Paper
            {
                PaperId = "P1",
                CameraReadyId = "CR1",
                TicketId = null,
                ConferenceId = "C1",
                Conference = new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "ResearchConf",
                    ConferenceStatus = new ConferenceStatus
                    {
                        ConferenceStatusId = "CS_NOT_READY", // khác với CS_READY
                        ConferenceStatusName = "Preparing"
                    },
                    IsResearchConference = true,
                    IsInternalHosted = true,
                    ResearchConferenceDetail = new ResearchConferenceDetail { NumberPaperAccept = 10 },
                    ConferenceSessions = new List<ConferenceSession> { new() { ConferenceSessionId = "S1" } }
                },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "U1", IsRootAuthor = true } }
            };
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync("P1")).ReturnsAsync(paper);

            // Ensure the repo that returns the "ready" status still returns CS_READY
            _mockConferenceStatusRepo.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "CS_READY", ConferenceStatusName = "Ready" });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenPaperNotBelongToConference()
        {
            // Arrange
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks(); // tạo mock mặc định (cp.ConferenceId = "C1")

            // Override: trả về paper có ConferenceId khác (ví dụ "C_OTHER")
            var paper = new Paper
            {
                PaperId = "P1",
                CameraReadyId = "CR1",
                TicketId = null,
                ConferenceId = "C_OTHER", // khác với conferencePrice.ConferenceId = "C1"
                Conference = new Conference
                {
                    ConferenceId = "C_OTHER",
                    ConferenceName = "OtherConf",
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "CS_READY", ConferenceStatusName = "Ready" }
                },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "U1", IsRootAuthor = true } }
            };
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync("P1")).ReturnsAsync(paper);

            // Ensure conference status repo still returns ready (nếu dùng)
            _mockConferenceStatusRepo.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "CS_READY", ConferenceStatusName = "Ready" });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        // -------------------------
        // 1. Payment method / GlobalStatus
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenPaymentMethodNotFound()
        {
            // Arrange
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            MockDate("2025-02-01");
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync((PaymentMethod)null);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenGlobalStatusAcceptedNotFound()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            MockDate("2025-02-01");
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1")).ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM1", MethodName = "MoMo" });
            _mockGlobalStatusRepo.Setup(r => r.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync((GlobalStatus)null);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        // -------------------------
        // 2. Paper checks
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenPaperNotFound()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks(); // other things valid
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync("P1")).ReturnsAsync((Paper)null);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenUserIsNotRootAuthor()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            // override paper authors to not include U1 as root
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1", CameraReadyId = "CR1", PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "Other", IsRootAuthor = true } } });
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenPaperAlreadyHasTicket()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1", CameraReadyId = "CR1", PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "U1", IsRootAuthor = true } }, TicketId = "T1" });
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenCameraReadyIdNull()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1", CameraReadyId = null, PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "U1", IsRootAuthor = true } } });
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        // -------------------------
        // 3. Camera ready checks
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenCameraReadyNotFound()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockCameraReadyRepo.Setup(r => r.GetCameraReadyByIdAsync("CR1")).ReturnsAsync((CameraReady)null);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenCameraReadyNotAccepted()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            _mockCameraReadyRepo.Setup(r => r.GetCameraReadyByIdAsync("CR1")).ReturnsAsync(new CameraReady { CameraReadyId = "CR1", GlobalStatusId = "NOT_ACCEPTED" });
            _mockGlobalStatusRepo.Setup(r => r.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync(new GlobalStatus { GlobalStatusId = "GS_ACCEPTED" });

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        // -------------------------
        // 4. Conference price checks
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenConferencePriceNotFound()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync((ConferencePrice)null);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenConferenceSoldOut()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.Conference.AvailableSlot = 0;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenConferenceNotResearch()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.Conference.IsResearchConference = false;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenPriceIsNotAuthor()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.IsAuthor = false;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenConferenceNotInternalHosted()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.Conference.IsInternalHosted = false;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenResearchConferenceDetailNotFound()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.Conference.ResearchConferenceDetail = null;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenPaperCountExceedsLimit()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockPaperRepo.Setup(r => r.GetPaperCountByConference("C1")).ReturnsAsync(20); // exceeds
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        // -------------------------
        // 5. Redis Lock checks
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenPaymentLockExistsWithDifferentMethod()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM2", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks(paymentMethodId: "PM2", paymentMethodName: "MoMo");

            // Redis says lock exists
            var lockKey = ExtensionHelper.GetPaymentConfereceLockKeyResult("U1", "C1");
            _mockRedis.Setup(r => r.KeyExistsAsync(lockKey)).ReturnsAsync(true);

            var dto = new PaymentLockKeyDTO { PaymentMethodId = "PM1", OldCheckOutUrl = "old" };
            _mockRedis.Setup(r => r.GetStringAsync(lockKey)).ReturnsAsync(JsonSerializer.Serialize(dto));

            // PaymentMethod in lock info
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM1", MethodName = "MoMo" });
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldReturnExistingPayment_WhenRedisLockExistsAndSameMethod()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks(paymentMethodId: "PM1", paymentMethodName: "MoMo");

            _mockConferenceStatusRepo
    .Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
    .ReturnsAsync(new ConferenceStatus
    {
        ConferenceStatusId = "CS_READY",
        ConferenceStatusName = "Ready"
    });

            var lockKey = ExtensionHelper.GetPaymentConfereceLockKeyResult("U1", "C1");
            _mockRedis.Setup(r => r.KeyExistsAsync(lockKey)).ReturnsAsync(true);
            var dto = new PaymentLockKeyDTO { PaymentMethodId = "PM1", OldCheckOutUrl = "https://oldurl.com" };
            _mockRedis.Setup(r => r.GetStringAsync(lockKey)).ReturnsAsync(JsonSerializer.Serialize(dto));

            var result = await _service.CreatePaymentForAbstract(req, "U1");

            Assert.False(result.PaymentCreateSuccess);
            Assert.Equal("https://oldurl.com", result.CheckOutUrl);
        }

        // -------------------------
        // 6. Phase/time checks
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenResearchConferencePhasesNull()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.Conference.ResearchConferencePhases = null;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenNoActivePhaseFound()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.Conference.ResearchConferencePhases = new List<ResearchConferencePhase> { new ResearchConferencePhase { IsActive = false } };
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenPaymentStartDateNotReached()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            // make date later than start
            MockDate("2024-01-01");
            var cp = CreateValidConferencePrice();
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenPaymentEndDatePassed()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            // set date after end
            MockDate("2026-01-01");
            var cp = CreateValidConferencePrice();
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        // -------------------------
        // 7. Ticket checks
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenUserAlreadyHasAttendeeTicket()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockTicketRepo.Setup(r => r.GetAttendeeTicketByUserIdAndConferenceId("U1", "C1"))
                .ReturnsAsync(new List<Ticket> { new Ticket { TicketId = "T1", IsRefunded = false } });
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenUserAlreadyHasAuthorTicket()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            _mockTicketRepo.Setup(r => r.GetAuthorTicketByUserIdAndConferenceId("U1", "C1"))
                .ReturnsAsync(new List<Ticket> { new Ticket { TicketId = "T2", IsRefunded = false } });
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        // -------------------------
        // 8. Price phase details
        // -------------------------
        [Fact]
        public async Task ShouldThrow_WhenNoValidPricePhase()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.PricePhases = new List<PricePhase>(); // empty
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenPricePhaseHasNoSlot()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.PricePhases.First().AvailableSlot = 0;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenApplyPercentLessThanZero()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice();
            cp.PricePhases.First().ApplyPercent = -10;
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenFinalPriceTooLow()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1", PaperId = "P1" };
            SetupBaseValidMocks();
            var cp = CreateValidConferencePrice(ticketPrice: 10000); // finalPrice <= 10000 when applyPercent 100%
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1")).ReturnsAsync(cp);
            _mockConferenceStatusRepo
.Setup(r => r.GetConferenceStatusByNameAsync(It.IsAny<string>()))
.ReturnsAsync(new ConferenceStatus
{
    ConferenceStatusId = "CS_READY",
    ConferenceStatusName = "Ready"
});
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreatePaymentForAbstract(req, "U1"));
        }



        // end of tests
    }





}