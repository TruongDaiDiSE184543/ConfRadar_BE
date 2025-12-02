using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
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

        // Repositories
        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo = new();
        private readonly Mock<IConferencePriceRepository> _mockConferencePriceRepo = new();
        private readonly Mock<IPaperRepository> _mockPaperRepo = new();

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


        // -------------------------------------------
        // Helper: tạo ConferencePrice hợp lệ
        // -------------------------------------------
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
                        RegistrationStartDate = DateOnly.Parse("2025-01-01"),
                        RegistrationEndDate = DateOnly.Parse("2025-12-30")
                    }
                },
                    ConferenceSessions = new List<ConferenceSession>()
                {
                    new ConferenceSession {
                        ConferenceSessionId = "S1"
                    }
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
        [Fact]
        public async Task CreatePaymentForAbstract_ShouldThrow_WhenPaymentMethodNotFound()
        {
            // Arrange
            var req = new CreatePaperPaymentRequest
            {
                PaymentMethodId = "PM1",
                ConferencePriceId = "CP1"
            };

            // 1. Setup repo trong UnitOfWork (nếu chưa setup)
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);

            // 2. Setup hành vi trả về null
            _mockPaymentMethodRepo
                .Setup(r => r.GetPaymentMethodById("PM1"))
                .ReturnsAsync((PaymentMethod)null);


            // Act + Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task CreatePaymentForAbstract_ShouldThrow_WhenConferencePriceNotFound()
        {
            // Arrange
            var req = new CreatePaperPaymentRequest
            {
                PaymentMethodId = "PM1",
                ConferencePriceId = "CP1"
            };

            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository)
                    .Returns(_mockPaymentMethodRepo.Object);

            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                                  .ReturnsAsync(new PaymentMethod());


            // Bước 1: Gán repo cho UnitOfWork
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository)
                           .Returns(_mockConferencePriceRepo.Object);

            // Bước 2: Mock hành vi repo
            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                                    .ReturnsAsync((ConferencePrice)null);


            // Act + Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenConferenceAvailableSlotZero()
        {
            var req = new CreatePaperPaymentRequest
            {
                PaymentMethodId = "PM1",
                ConferencePriceId = "CP1"
            };

            var cp = CreateValidConferencePrice();
            cp.Conference.AvailableSlot = 0;

            // 1. Gán repo cho UnitOfWork
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Mock hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                                  .ReturnsAsync(new PaymentMethod());

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                                    .ReturnsAsync(cp);


            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenNotResearchConference()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };

            var cp = CreateValidConferencePrice();
            cp.Conference.IsResearchConference = false;

            // 1. Setup các repo trả về từ UnitOfWork
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                .ReturnsAsync(new PaymentMethod());

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                .ReturnsAsync(cp);


            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenIsNotAuthorTicket()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };

            var cp = CreateValidConferencePrice();
            cp.IsAuthor = false;

            // 1. Setup UnitOfWork trả về các repo mock
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                .ReturnsAsync(new PaymentMethod());

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                .ReturnsAsync(cp);


            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenNotInternalHosted()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };

            var cp = CreateValidConferencePrice();
            cp.Conference.IsInternalHosted = false;

            // 1. Setup UnitOfWork trả về các repo mock
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                .ReturnsAsync(new PaymentMethod());

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                .ReturnsAsync(cp);


            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenResearchConferenceDetailNull()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };
            var cp = CreateValidConferencePrice();
            cp.Conference.ResearchConferenceDetail = null;

            // 1. Setup UnitOfWork trả về các repo mock
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                .ReturnsAsync(new PaymentMethod());

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                .ReturnsAsync(cp);


            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenPaperCountExceedLimit()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };

            var cp = CreateValidConferencePrice();

            // 1. Setup UnitOfWork trả về các repo mock
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                .ReturnsAsync(new PaymentMethod());

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                .ReturnsAsync(cp);


            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(_mockPaperRepo.Object);

            // 3. Setup hành vi của repo
            _mockPaperRepo.Setup(r => r.GetPaperCountByConference("C1"))
                          .ReturnsAsync(20);


            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldThrow_WhenRedisLockExists_WithDifferentPaymentMethod()
        {
            var req = new CreatePaperPaymentRequest
            {
                PaymentMethodId = "PM2",
                ConferencePriceId = "CP1"
            };

            var cp = CreateValidConferencePrice();
            // 1. Setup repo trong UnitOfWork
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM2"))
                                  .ReturnsAsync(new PaymentMethod { MethodName = "PayOs" });

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                                     .ReturnsAsync(cp); // cp là đối tượng ConferencePrice bạn đã tạo


            // Redis báo là có lock
            _mockRedis.Setup(r => r.KeyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var lockObj = new PaymentLockKeyDTO
            {
                PaymentMethodId = "PM1",
                OldCheckOutUrl = "existingUrl"
            };

            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(JsonSerializer.Serialize(lockObj));

            // 1. Setup repo trong UnitOfWork
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                                  .ReturnsAsync(new PaymentMethod { MethodName = "MoMo" });


            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreatePaymentForAbstract(req, "U1"));
        }
        [Fact]
        public async Task ShouldReturnExistingPayment_WhenRedisLockExists_AndSameMethod()
        {
            var req = new CreatePaperPaymentRequest { PaymentMethodId = "PM1", ConferencePriceId = "CP1" };

            var cp = CreateValidConferencePrice();
            // 1. Setup repo trong UnitOfWork
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository).Returns(_mockConferencePriceRepo.Object);

            // 2. Setup hành vi của repo
            _mockPaymentMethodRepo.Setup(r => r.GetPaymentMethodById("PM1"))
                                  .ReturnsAsync(new PaymentMethod { MethodName = "MoMo" });

            _mockConferencePriceRepo.Setup(r => r.GetConferencePriceByIdAsync("CP1"))
                                    .ReturnsAsync(cp);


            _mockRedis.Setup(r => r.KeyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var dto = new PaymentLockKeyDTO
            {
                PaymentMethodId = "PM1",
                OldCheckOutUrl = "https://oldurl.com"
            };

            _mockRedis.Setup(r => r.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(JsonSerializer.Serialize(dto));

            var result = await _service.CreatePaymentForAbstract(req, "U1");

            Assert.False(result.PaymentCreateSuccess);
            Assert.Equal("https://oldurl.com", result.CheckOutUrl);
        }


    }
}