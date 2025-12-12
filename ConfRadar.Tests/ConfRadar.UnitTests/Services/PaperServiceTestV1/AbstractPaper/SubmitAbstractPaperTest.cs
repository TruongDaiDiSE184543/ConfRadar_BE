using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.AbstractPaper
{
    public class SubmitAbstractPaperTest
    {
        private readonly Mock<IUnitOfWork> _mockUow = new();
        private readonly Mock<ITimeProviderService> _mockTime = new();
        private readonly Mock<IObjectStorageFileService> _mockStorage = new();
        private readonly Mock<ITokenService> _mockToken = new();
        private readonly Mock<INotificationService> _mockNoti = new();
        private readonly Mock<IConferenceStepService> _mockConfStep = new();
        private readonly Mock<IMomoService> _mockMomo = new();
        private readonly IOptions<ObjectStorageSettings> _options;
        private readonly Mock<IRedisService> _mockRedis = new();
        private readonly PaperService _service;
        private readonly Mock<IEmailService> _mockEmailService = new();
        public SubmitAbstractPaperTest()
        {
            _options = Options.Create(new ObjectStorageSettings { EndPoint = "https://minio/" });

            _service = new PaperService(
                _mockUow.Object,
                _mockMomo.Object,
                _mockToken.Object,
                _options,
                _mockStorage.Object,
                Mock.Of<ITicketService>(),
                _mockTime.Object,
                _mockNoti.Object,
                _mockConfStep.Object,
                _mockEmailService.Object

            );
        }
        private void MockCommon()
        {
            // Phase + status skip logic theo yêu cầu
            _mockUow.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "GS_PEN" });

            _mockUow.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PP_ABS" });

            _mockUow.Setup(u => u.AuditLogCategoryRepository.GetAuditLogCategoryByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuditLogCategory { CategoryId = "AUD_PAPER" });

            _mockUow.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "CS_READY" });

            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.UtcNow);
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));
        }


        [Fact]
        public async Task ShouldFail_WhenUserNotFound()
        {
            MockCommon();

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync((User?)null);

            var req = new CreateAbstractRequest { ConferenceId = "C1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.SubmitAbstract(req, "U1"));

            Assert.Equal("Không tìm thấy người dùng với id U1", ex.Message);
        }
        [Fact]
        public async Task ShouldFail_WhenConferenceNotFound()
        {
            MockCommon();

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User());

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("C1"))
                .ReturnsAsync((Conference?)null);

            var req = new CreateAbstractRequest { ConferenceId = "C1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(req, "U1"));

            Assert.Equal("Hội nghị với id C1 không tồn tại", ex.Message);
        }
        [Fact]
        public async Task ShouldFail_WhenConferenceNotReady()
        {
            MockCommon();

            var conf = new Conference { ConferenceId = "C1", ConferenceStatusId = "CS_NOT_READY" };

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User());

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("C1"))
                .ReturnsAsync(conf);

            var req = new CreateAbstractRequest { ConferenceId = "C1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(req, "U1"));

            Assert.Equal("Hội nghị chưa ready nên không thể thực thi", ex.Message);
        }
        [Fact]
        public async Task ShouldFail_WhenNoActiveResearchPhase()
        {
            MockCommon();

            var conf = new Conference
            {
                ConferenceId = "C1",
                ConferenceStatusId = "CS_READY",
                ResearchConferencePhases = new List<ResearchConferencePhase>()
            };

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U1")).ReturnsAsync(new User());
            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("C1")).ReturnsAsync(conf);

            var req = new CreateAbstractRequest { ConferenceId = "C1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                 _service.SubmitAbstract(req, "U1"));

            Assert.Equal("Không tìm thấy giai đoạn hiệu lực nào của hội nghị", ex.Message);
        }
        [Fact]
        public async Task ShouldFail_WhenPaperAlreadySubmitted()
        {
            MockCommon();

            var conf = new Conference
            {
                ConferenceId = "C1",
                ConferenceStatusId = "CS_READY",
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase { IsActive = true, RegistrationStartDate = DateOnly.MinValue, RegistrationEndDate = DateOnly.MaxValue }
        }
            };

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U1")).ReturnsAsync(new User());
            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("C1")).ReturnsAsync(conf);

            _mockUow.Setup(u => u.PaperRepository.GetPaperByRootUserAndConference("C1", "U1"))
                .ReturnsAsync(new Paper
                {
                    Conference = new Conference { ConferenceName = "ResearchConf" },
                    CreatedAt = DateTime.Parse("2025-01-01")
                });

            var req = new CreateAbstractRequest { ConferenceId = "C1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                 _service.SubmitAbstract(req, "U1"));

            Assert.Contains("Bạn đã nộp báo cho hội nghị", ex.Message);
        }
        [Fact]
        public async Task ShouldFail_WhenCoAuthorIsReviewer()
        {
            MockCommon();

            var conf = new Conference
            {
                ConferenceId = "C1",
                ConferenceStatusId = "CS_READY",
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase { IsActive = true,
                RegistrationStartDate = DateOnly.MinValue,
                RegistrationEndDate = DateOnly.MaxValue }
        }
            };

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U1")).ReturnsAsync(new User());
            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U2")).ReturnsAsync(new User());
            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("C1")).ReturnsAsync(conf);

            _mockUow.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync("C1"))
                .ReturnsAsync(new List<PaperReviewer> { new PaperReviewer { UserId = "U2" } });

            var req = new CreateAbstractRequest
            {
                ConferenceId = "C1",
                Title = "T1",
                Description = "D",
                CoAuthorId = new List<string> { "U2" }
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(req, "U1"));

            Assert.Contains("đang là reviewer", ex.Message);
        }
        [Fact]
        public async Task ShouldSubmitSuccessfully_WhenValid()
        {
            MockCommon();

            var conf = new Conference
            {
                ConferenceId = "C1",
                ConferenceStatusId = "CS_READY",
                ConferenceName = "Conf",
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase { IsActive = true,
                RegistrationStartDate = DateOnly.MinValue,
                RegistrationEndDate = DateOnly.MaxValue }
        }
            };

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("U1")).ReturnsAsync(new User { UserId = "U1", FullName = "AAA" });
            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("C1")).ReturnsAsync(conf);

            _mockUow.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync("C1"))
                .ReturnsAsync(new List<PaperReviewer>());

            _mockUow.Setup(u => u.PaperRepository.CreatePaperAsync(It.IsAny<Paper>())).ReturnsAsync(1);
            _mockUow.Setup(u => u.AuditLogRepository.CreateAuditLogAsync(It.IsAny<AuditLog>())).ReturnsAsync(1);

            _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var req = new CreateAbstractRequest
            {
                ConferenceId = "C1",
                Title = "T",
                Description = "D"
            };

            var result = await _service.SubmitAbstract(req, "U1");

            Assert.Equal(2, result);
        }
        [Fact]
        public async Task CreatePayment_ShouldFail_WhenOutOfRegistrationWindow()
        {
            var req = new CreateResearchAttendeePaymentRequest
            {
                ConferencePriceId = "C01",
                PaymentMethodId = "PM01"
            };

            // --- Fake Phase (không trong thời gian đăng ký) ---
            var researchPhase = new ResearchConferencePhase
            {
                RegistrationStartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                RegistrationEndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            };

            var conference = new Conference
            {
                ConferenceId = "C01",
                AvailableSlot = 10,
                ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "CS_READY" },
                ConferenceSessions = new List<ConferenceSession>()
            };

            var confPrice = new ConferencePrice
            {
                Conference = conference
            };

            // ---------------- MOCK ----------------
            _mockUow.Setup(x => x.PaymentMethodRepository.GetPaymentMethodById("PM01"))
                .ReturnsAsync(new PaymentMethod());

            _mockUow.Setup(x => x.ConferenceRepository.GetConferenceByIdAsync("C01"))
                .ReturnsAsync(conference);

            _mockUow.Setup(x => x.ResearchConferencePhaseRepository
                .GetActiveResearchConferencePhaseByConferenceIdAsync("C01"))
                .ReturnsAsync(researchPhase);

            _mockUow.Setup(x => x.TicketRepository
                .GetAttendeeTicketByUserIdAndConferenceId("U01", "C01"))
                .ReturnsAsync(new List<Ticket>());

            _mockTime.Setup(x => x.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.Now)); // Today nằm ngoài window

            _mockRedis.Setup(x => x.KeyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);


        }


    }
}
