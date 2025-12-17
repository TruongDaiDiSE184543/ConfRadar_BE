using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Abstract;
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
            _mockUow.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "PENDING" });

            _mockUow.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "ABSTRACT" });

            _mockUow.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "READY" });

            _mockUow.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role { RoleId = "REVIEWER" });
        }
        [Fact]
        public async Task SubmitAbstract_ConferenceNotFound_ShouldThrowBadRequest()
        {
            MockCommon();

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync(new User());

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Conference?)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(new CreateAbstractRequest { ConferenceId = "conf-1" }, "user-1"));

            Assert.Contains("Hội nghị", ex.Message);
        }
        //[Fact]
        //public async Task SubmitAbstract_SessionNotBelongConference_ShouldThrowBadRequest()
        //{
        //    MockCommon();

        //    _mockUow.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
        //        .ReturnsAsync(new User());

        //    _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
        //        .ReturnsAsync(new Conference { ConferenceId = "conf-1", ConferenceStatusId = "READY" });

        //    _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId(It.IsAny<string>()))
        //        .ReturnsAsync(new ConferenceSession { ConferenceId = "conf-2" });

        //    var request = new CreateAbstractRequest
        //    {
        //        ConferenceId = "conf-1",
        //        //ConferenceSessionId = "session-1"
        //    };

        //    var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
        //        _service.SubmitAbstract(request, "user-1"));

        //    Assert.Contains("không thuộc hội nghị", ex.Message);
        //}
        [Fact]
        public async Task SubmitAbstract_AddSelfAsCoAuthor_ShouldThrowBadRequest()
        {
            MockCommon();

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow); ;

            // time
            _mockTime.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(dateNow);

            _mockTime.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            // user
            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("user-1"))
                .ReturnsAsync(new User { UserId = "user-1" });

            var phase = (ResearchConferencePhase)Activator.CreateInstance(
    typeof(ResearchConferencePhase),
    nonPublic: true
)!;

            // set bằng reflection (CHỈ trong test)
            typeof(ResearchConferencePhase)
                .GetProperty("IsActive")!
                .SetValue(phase, true);

            typeof(ResearchConferencePhase)
                .GetProperty("RegistrationStartDate")!
                .SetValue(phase, dateNow.AddDays(-1));

            typeof(ResearchConferencePhase)
                .GetProperty("RegistrationEndDate")!
                .SetValue(phase, dateNow.AddDays(1));


            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-1"))
                .ReturnsAsync(new Conference
                {
                    ConferenceId = "conf-1",
                    ConferenceStatusId = "READY",
                    ResearchConferencePhases = new List<ResearchConferencePhase>
                    {
                phase
                    }
                });

            // session hợp lệ
            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId("session-1"))
                .ReturnsAsync(new ConferenceSession
                {
                    ConferenceId = "conf-1"
                });

            // không có paper trước đó
            _mockUow.Setup(u => u.PaperRepository
                .GetPaperByRootUserAndConference("conf-1", "user-1"))
                .ReturnsAsync((Paper?)null);
            _mockUow.Setup(u => u.PaperReviewerRepository
    .GetPaperReviewersByConferenceIdAsync("conf-1"))
    .ReturnsAsync(new List<PaperReviewer>());
            // không reviewer contract
            _mockUow.Setup(u => u.ReviewerContractRepository
                .GetContractByUserAndConferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((ReviewerContract?)null);

            // không reviewer role
            _mockUow.Setup(u => u.UserRoleRepository
                .GetUserRoleByUserAndRole(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UserRole?)null);

            // không ticket
            _mockUow.Setup(u => u.TicketRepository
                .GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Ticket>());

            var request = new CreateAbstractRequest
            {
                ConferenceId = "conf-1",
                //ConferenceSessionId = "session-1",
                CoAuthorId = new List<string> { "user-1" }
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(request, "user-1"));

            Assert.Contains("chính mình", ex.Message);
        }

        //[Fact]
        //public async Task SubmitAbstract_SessionNotFound_ShouldThrowBadRequest()
        //{
        //    MockCommon();

        //    _mockUow.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
        //        .ReturnsAsync(new User());

        //    _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
        //        .ReturnsAsync(new Conference { ConferenceStatusId = "READY" });

        //    _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId(It.IsAny<string>()))
        //        .ReturnsAsync((ConferenceSession?)null);

        //    var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
        //        _service.SubmitAbstract(new CreateAbstractRequest(), "user-1"));

        //    Assert.Contains("Không tìm thấy phiên", ex.Message);
        //}
        [Fact]
        public async Task SubmitAbstract_ConferenceNotReady_ShouldThrowBadRequest()
        {
            MockCommon();

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync(new User());

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference { ConferenceStatusId = "DRAFT" });

            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceSession());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(new CreateAbstractRequest(), "user-1"));

            Assert.Contains("chưa ready", ex.Message);
        }
        [Fact]
        public async Task SubmitAbstract_NoActiveResearchPhase_ShouldThrowBadRequest()
        {
            MockCommon();

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync(new User());

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference
                {
                    ConferenceStatusId = "READY",
                    ResearchConferencePhases = new List<ResearchConferencePhase>()
                });

            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceSession());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(new CreateAbstractRequest(), "user-1"));

            Assert.Contains("giai đoạn hiệu lực", ex.Message);
        }
        [Fact]
        public async Task SubmitAbstract_ExistingPaper_ShouldThrowBadRequest()
        {
            MockCommon();

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow);

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = "user-1" });

            var phase = (ResearchConferencePhase)Activator.CreateInstance(
                typeof(ResearchConferencePhase),
                nonPublic: true)!;

            typeof(ResearchConferencePhase).GetProperty("IsActive")!
                .SetValue(phase, true);

            typeof(ResearchConferencePhase).GetProperty("RegistrationStartDate")!
                .SetValue(phase, dateNow.AddDays(-1));

            typeof(ResearchConferencePhase).GetProperty("RegistrationEndDate")!
                .SetValue(phase, dateNow.AddDays(1));

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference
                {
                    ConferenceId = "conf-1",
                    ConferenceStatusId = "READY",
                    ResearchConferencePhases = new List<ResearchConferencePhase> { phase }
                });

            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceSession
                {
                    ConferenceSessionId = "session-1",
                    ConferenceId = "conf-1"
                });

            _mockUow.Setup(u => u.PaperRepository
                .GetPaperByRootUserAndConference(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Paper
                {
                    Conference = new Conference { ConferenceName = "Test Conf" }
                });

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(new CreateAbstractRequest
                {
                    ConferenceId = "conf-1",
                    //ConferenceSessionId = "session-1"
                }, "user-1"));

            Assert.Contains("đã nộp", ex.Message);
        }
        public class TestResearchConferencePhase : ResearchConferencePhase
        {
            public new bool IsActive { get; set; }
            public new DateOnly RegistrationStartDate { get; set; }
            public new DateOnly RegistrationEndDate { get; set; }
        }
        [Fact]
        public async Task SubmitAbstract_OutOfRegistrationDate_ShouldThrowBadRequest()
        {
            MockCommon();
            // Arrange

            var currentDate = DateOnly.FromDateTime(new DateTime(2024, 1, 15));
            var registrationStartDate = DateOnly.FromDateTime(new DateTime(2024, 2, 1));
            var registrationEndDate = DateOnly.FromDateTime(new DateTime(2024, 2, 28));

            _mockTime.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(currentDate);
            _mockTime.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            var conference = new Conference
            {
                ConferenceId = "CONF1",
                ConferenceName = "Test Conference",
                ConferenceStatusId = "READY",
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase
            {
                IsActive = true,
                RegistrationStartDate = registrationStartDate,
                RegistrationEndDate = registrationEndDate
            }
        }
            };

            var session = new ConferenceSession
            {
                ConferenceSessionId = "SESSION1",
                ConferenceId = "CONF1"
            };

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("CONF1"))
                .ReturnsAsync(conference);
            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId("SESSION1"))
                .ReturnsAsync(session);
            _mockUow.Setup(u => u.PaperRepository.GetPaperByRootUserAndConference("CONF1", "USER1"))
                .ReturnsAsync((Paper)null);
            _mockUow.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("USER1", "CONF1"))
                .ReturnsAsync((ReviewerContract)null);
            _mockUow.Setup(u => u.UserRoleRepository.GetUserRoleByUserAndRole("USER1", "REVIEWER"))
                .ReturnsAsync((UserRole)null);
            _mockUow.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId("USER1", "CONF1"))
                .ReturnsAsync(new List<Ticket>());

            var request = new CreateAbstractRequest
            {
                ConferenceId = "CONF1",
                //ConferenceSessionId = "SESSION1",
                Title = "Test Abstract",
                Description = "Test Description"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _service.SubmitAbstract(request, "USER1")
            );

            Assert.Contains($"Không thể nộp abstract, do ngày đăng kí nằm trong khoảng {registrationStartDate} - {registrationEndDate}",
                exception.Message);

        }
        [Fact]
        public async Task SubmitAbstract_UserHasReviewerContract_ShouldThrowBadRequest()
        {
            // Arrange
            MockCommon();

            var currentDate = DateOnly.FromDateTime(new DateTime(2024, 2, 15));
            var registrationStartDate = DateOnly.FromDateTime(new DateTime(2024, 2, 1));
            var registrationEndDate = DateOnly.FromDateTime(new DateTime(2024, 2, 28));

            _mockTime.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(currentDate);
            _mockTime.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            var conference = new Conference
            {
                ConferenceId = "CONF1",
                ConferenceName = "Test Conference",
                ConferenceStatusId = "READY",
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase
            {
                IsActive = true,
                RegistrationStartDate = registrationStartDate,
                RegistrationEndDate = registrationEndDate
            }
        }
            };

            var session = new ConferenceSession
            {
                ConferenceSessionId = "SESSION1",
                ConferenceId = "CONF1"
            };

            // Mock các dependencies theo thứ tự trong code
            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(conference);
            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId(It.IsAny<string>()))
                .ReturnsAsync(session);
            _mockUow.Setup(u => u.PaperRepository.GetPaperByRootUserAndConference(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Paper)null);

            // Mock user có reviewer contract - điểm cần test
            _mockUow.Setup(u => u.ReviewerContractRepository
                .GetContractByUserAndConferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ReviewerContract
                {
                    UserId = "user-1",
                    ConferenceId = "CONF1"
                });

            _mockUow.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync(new User { FullName = "Test User" });

            var request = new CreateAbstractRequest
            {
                ConferenceId = "CONF1",
                //ConferenceSessionId = "SESSION1",
                Title = "Test Abstract",
                Description = "Test Description"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _service.SubmitAbstract(request, "user-1")
            );

            Assert.Contains("hợp đồng", ex.Message);
        }
        [Fact]
        public async Task SubmitAbstract_UserIsReviewer_ShouldThrowBadRequest()
        {
            // Arrange
            MockCommon();

            var currentDate = DateOnly.FromDateTime(new DateTime(2024, 2, 15));
            var registrationStartDate = DateOnly.FromDateTime(new DateTime(2024, 2, 1));
            var registrationEndDate = DateOnly.FromDateTime(new DateTime(2024, 2, 28));

            _mockTime.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(currentDate);
            _mockTime.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            var conference = new Conference
            {
                ConferenceId = "CONF1",
                ConferenceName = "Test Conference",
                ConferenceStatusId = "READY",
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase
            {
                IsActive = true,
                RegistrationStartDate = registrationStartDate,
                RegistrationEndDate = registrationEndDate
            }
        }
            };

            var session = new ConferenceSession
            {
                ConferenceSessionId = "SESSION1",
                ConferenceId = "CONF1"
            };

            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("CONF1"))
                .ReturnsAsync(conference);
            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId("SESSION1"))
                .ReturnsAsync(session);
            _mockUow.Setup(u => u.PaperRepository.GetPaperByRootUserAndConference("CONF1", "USER1"))
                .ReturnsAsync((Paper)null);
            _mockUow.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("USER1", "CONF1"))
                .ReturnsAsync((ReviewerContract)null);
            _mockUow.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId("USER1", "CONF1"))
                .ReturnsAsync(new List<Ticket>());

            // Mock user có role reviewer trong hệ thống
            _mockUow.Setup(u => u.UserRoleRepository.GetUserRoleByUserAndRole("USER1", "REVIEWER"))
                .ReturnsAsync(new UserRole
                {
                    UserId = "USER1",
                    RoleId = "REVIEWER"
                });

            var request = new CreateAbstractRequest
            {
                ConferenceId = "CONF1",
                //ConferenceSessionId = "SESSION1",
                Title = "Test Abstract",
                Description = "Test Description"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _service.SubmitAbstract(request, "USER1")
            );

            Assert.Contains("Bạn không thể mua vé này vì bạn là reviewer trong hệ thống",
                exception.Message);


        }
        [Fact]
        public async Task SubmitAbstract_UserAlreadyBoughtTicket_ShouldThrowBadRequest()
        {
            // Arrange
            MockCommon();

            var currentDate = DateOnly.FromDateTime(new DateTime(2024, 2, 15));
            var registrationStartDate = DateOnly.FromDateTime(new DateTime(2024, 2, 1));
            var registrationEndDate = DateOnly.FromDateTime(new DateTime(2024, 2, 28));

            _mockTime.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(currentDate);
            _mockTime.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            var conference = new Conference
            {
                ConferenceId = "CONF1",
                ConferenceName = "Test Conference",
                ConferenceStatusId = "READY",
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase
            {
                IsActive = true,
                RegistrationStartDate = registrationStartDate,
                RegistrationEndDate = registrationEndDate
            }
        }
            };

            var session = new ConferenceSession
            {
                ConferenceSessionId = "SESSION1",
                ConferenceId = "CONF1"
            };

            // Mock tất cả dependencies theo đúng thứ tự trong code
            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(conference);
            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId(It.IsAny<string>()))
                .ReturnsAsync(session);
            _mockUow.Setup(u => u.PaperRepository.GetPaperByRootUserAndConference(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Paper)null);
            _mockUow.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((ReviewerContract)null);
            _mockUow.Setup(u => u.UserRoleRepository.GetUserRoleByUserAndRole(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UserRole)null);

            // Mock user đã mua vé - điểm cần test
            _mockUow.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Ticket> { new Ticket() });

            var request = new CreateAbstractRequest
            {
                ConferenceId = "CONF1",
                //ConferenceSessionId = "SESSION1",
                Title = "Test Abstract",
                Description = "Test Description"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _service.SubmitAbstract(request, "user-1")
            );

            Assert.Contains("đã mua vé", ex.Message);
        }
        [Fact]
        public async Task SubmitAbstract_CoAuthorIsReviewer_ShouldThrowBadRequest()
        {
            MockCommon();

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            _mockTime.Setup(t => t.GetVietnamDate())
    .ReturnsAsync(dateNow);

            var phase = (ResearchConferencePhase)Activator.CreateInstance(
                typeof(ResearchConferencePhase),
                nonPublic: true)!;

            typeof(ResearchConferencePhase).GetProperty("IsActive")!
                .SetValue(phase, true);

            typeof(ResearchConferencePhase).GetProperty("RegistrationStartDate")!
                .SetValue(phase, dateNow.AddDays(-1));

            typeof(ResearchConferencePhase).GetProperty("RegistrationEndDate")!
                .SetValue(phase, dateNow.AddDays(1));

            var request = new CreateAbstractRequest
            {
                ConferenceId = "conf-1",
                //ConferenceSessionId = "session-1",
                CoAuthorId = new List<string> { "coauthor-1" }
            };
            _mockUow.Setup(u => u.RoleRepository
    .GetRoleByRoleName(It.IsAny<string>()))
    .ReturnsAsync(new Role
    {
        RoleId = "role-reviewer",
        RoleName = SystemRoleEnum.LocalReviewer.GetDescription()
    });

            _mockUow.Setup(u => u.UserRoleRepository
                .GetUserRoleByUserAndRole(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UserRole?)null);

            _mockUow.Setup(u => u.PaperReviewerRepository
  .GetPaperReviewersByConferenceIdAsync(It.IsAny<string>()))
  .ReturnsAsync(new List<PaperReviewer>
  {
        new PaperReviewer { UserId = "coauthor-1" }
  });
            // user root
            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("user-1"))
                .ReturnsAsync(new User { UserId = "user-1" });

            // user coauthor
            _mockUow.Setup(u => u.UserRepository.GetUserByUserId("coauthor-1"))
                .ReturnsAsync(new User { UserId = "coauthor-1" });

            // conference
            _mockUow.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-1"))
                .ReturnsAsync(new Conference
                {
                    ConferenceId = "conf-1",
                    ConferenceStatusId = "READY",
                    ResearchConferencePhases = new List<ResearchConferencePhase> { phase }
                });

            // session
            _mockUow.Setup(u => u.ConferenceSessionRepository.GetSessionBySessionId("session-1"))
                .ReturnsAsync(new ConferenceSession
                {
                    ConferenceSessionId = "session-1",
                    ConferenceId = "conf-1"
                });

            // no existing paper
            _mockUow.Setup(u => u.PaperRepository
                .GetPaperByRootUserAndConference("conf-1", "user-1"))
                .ReturnsAsync((Paper?)null);

            // no reviewer contract
            _mockUow.Setup(u => u.ReviewerContractRepository
                .GetContractByUserAndConferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((ReviewerContract?)null);

            // no ticket
            _mockUow.Setup(u => u.TicketRepository
                .GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Ticket>());




            // Act
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitAbstract(request, "user-1"));

            // Assert
            Assert.Contains("reviewer", ex.Message);
        }

    }


}

