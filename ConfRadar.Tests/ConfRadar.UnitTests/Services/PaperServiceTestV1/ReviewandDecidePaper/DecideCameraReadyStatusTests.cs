//using ConfRadar.Repositories;
//using ConfRadar.Repositories.Models;
//using ConfRadar.Services.Common;
//using ConfRadar.Services.DTOs.Paper;
//using ConfRadar.Services.Exceptions;
//using ConfRadar.Services.Services;
//using FluentAssertions;
//using Microsoft.Extensions.Options;
//using Moq;
//using static ConfRadar.Services.Common.AppSettingConfig;
//using ConfRadar.Repositories;
//using ConfRadar.Repositories.Models;
//using ConfRadar.Services.Common;
//using ConfRadar.Services.DTOs.Paper;
//using ConfRadar.Services.Exceptions;
//using ConfRadar.Services.Services;
//using FluentAssertions;
//using Microsoft.Extensions.Options;
//using Moq;
//using static ConfRadar.Services.Common.AppSettingConfig;

//namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
//{
//    public class DecideCameraReadyStatusTests
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<ITimeProviderService> _mockTime;
//        private readonly Mock<ITicketService> _mockTicket;
//        private readonly Mock<INotificationService> _mockNoti;
//        private readonly PaperService _paperService;
//namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
//{
//    public class DecideCameraReadyStatusTests
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<ITimeProviderService> _mockTime;
//        private readonly Mock<ITicketService> _mockTicket;
//        private readonly Mock<INotificationService> _mockNoti;
//        private readonly PaperService _paperService;

//        public DecideCameraReadyStatusTests()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockTime = new Mock<ITimeProviderService>();
//            _mockTicket = new Mock<ITicketService>();
//            _mockNoti = new Mock<INotificationService>();
//        public DecideCameraReadyStatusTests()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockTime = new Mock<ITimeProviderService>();
//            _mockTicket = new Mock<ITicketService>();
//            _mockNoti = new Mock<INotificationService>();

//            var mockMomo = new Mock<IMomoService>();
//            var mockToken = new Mock<ITokenService>();
//            var mockEmail = new Mock<IEmailService>();
//            var mockFile = new Mock<IObjectStorageFileService>();
//            var mockStep = new Mock<IConferenceStepService>();
//            var options = Options.Create(new ObjectStorageSettings());
//            var mockMomo = new Mock<IMomoService>();
//            var mockToken = new Mock<ITokenService>();
//            var mockEmail = new Mock<IEmailService>();
//            var mockFile = new Mock<IObjectStorageFileService>();
//            var mockStep = new Mock<IConferenceStepService>();
//            var options = Options.Create(new ObjectStorageSettings());

//            _paperService = new PaperService(
//                _mockUnitOfWork.Object, mockMomo.Object, mockToken.Object, options,
//                mockFile.Object, _mockTicket.Object, _mockTime.Object, _mockNoti.Object, mockStep.Object,
//                mockEmail.Object
//            );
//        }
//            _paperService = new PaperService(
//                _mockUnitOfWork.Object, mockMomo.Object, mockToken.Object, options,
//                mockFile.Object, _mockTicket.Object, _mockTime.Object, _mockNoti.Object, mockStep.Object,
//                mockEmail.Object
//            );
//        }

//        private void SetupHappyPathMocks(string userId, string paperId, string cameraReadyId, bool isHead)
//        {
//            var now = DateTime.Now;
//            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
//            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));
//        private void SetupHappyPathMocks(string userId, string paperId, string cameraReadyId, bool isHead)
//        {
//            var now = DateTime.Now;
//            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
//            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

//            var paper = new Paper
//            {
//                PaperId = paperId,
//                Title = "Title",
//                CameraReadyId = cameraReadyId,
//                TicketId = "t1",
//                ResearchConferencePhase = new ResearchConferencePhase { CameraReadyDecideStatusStart = DateOnly.FromDateTime(now.AddDays(-1)), CameraReadyDecideStatusEnd = DateOnly.FromDateTime(now.AddDays(1)) },
//                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { IsRootAuthor = true, UserId = "author1" } }
//            };
//            var cameraReady = new CameraReady { CameraReadyId = cameraReadyId, GlobalStatusId = "status-pending" };

//            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
//                .ReturnsAsync((string name) => new GlobalStatus { GlobalStatusId = $"status-{name.ToLower()}", Name = name });
//            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
//                .ReturnsAsync((string name) => new GlobalStatus { GlobalStatusId = $"status-{name.ToLower()}", Name = name });

//            _mockUnitOfWork.Setup(u => u.CameraReadyRepository.GetCameraReadyByIdAsync(cameraReadyId)).ReturnsAsync(cameraReady);
//            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByCameraReadyIdAsync(cameraReadyId)).ReturnsAsync(paper);
//            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(paperId)).ReturnsAsync(paper);
//            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paperId)).ReturnsAsync(new PaperReviewer { IsHeadReviewer = isHead });
//            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>())).ReturnsAsync(new User());

//            _mockUnitOfWork.Setup(u => u.CameraReadyRepository.UpdateCameraReadyAsync(It.IsAny<CameraReady>())).ReturnsAsync(1);
//            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateNotificationAsync(It.IsAny<Notification>())).ReturnsAsync(1);
//        }
//            _mockUnitOfWork.Setup(u => u.CameraReadyRepository.UpdateCameraReadyAsync(It.IsAny<CameraReady>())).ReturnsAsync(1);
//            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateNotificationAsync(It.IsAny<Notification>())).ReturnsAsync(1);
//        }

//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Accept_When_Valid()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1", GlobalStatus = GlobalStatusEnum.Accepted };
//            SetupHappyPathMocks("head1", "p1", "cr1", true);
//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Accept_When_Valid()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1", GlobalStatus = GlobalStatusEnum.Accepted };
//            SetupHappyPathMocks("head1", "p1", "cr1", true);

//            await _paperService.DecideCameraReadyStatus(request, "head1");
//            await _paperService.DecideCameraReadyStatus(request, "head1");

//            _mockUnitOfWork.Verify(u => u.CameraReadyRepository.UpdateCameraReadyAsync(
//                It.Is<CameraReady>(cr => cr.GlobalStatusId == "status-accepted" && cr.ReviewAt != null)), Times.Once);
//        }


//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_NotHeadReviewer()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1" };
//            SetupHappyPathMocks("normal-reviewer", "p1", "cr1", false);
//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_NotHeadReviewer()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1" };
//            SetupHappyPathMocks("normal-reviewer", "p1", "cr1", false);

//            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "normal-reviewer"));
//            ex.Message.Should().Contain("Chỉ head reviewer mới có thể quyết định bài báo");
//        }
//            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "normal-reviewer"));
//            ex.Message.Should().Contain("Chỉ head reviewer mới có thể quyết định bài báo");
//        }

//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_CameraReadyNotFound()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr-not-found" };
//            _mockUnitOfWork.Setup(u => u.CameraReadyRepository.GetCameraReadyByIdAsync("cr-not-found")).ReturnsAsync((CameraReady)null);
//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_CameraReadyNotFound()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr-not-found" };
//            _mockUnitOfWork.Setup(u => u.CameraReadyRepository.GetCameraReadyByIdAsync("cr-not-found")).ReturnsAsync((CameraReady)null);

//            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "head1"));
//        }
//            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "head1"));
//        }

//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_CameraReadyNotPending()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1" };
//            SetupHappyPathMocks("head1", "p1", "cr1", true);
//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_CameraReadyNotPending()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1" };
//            SetupHappyPathMocks("head1", "p1", "cr1", true);

//            var cameraReady = await _mockUnitOfWork.Object.CameraReadyRepository.GetCameraReadyByIdAsync("cr1");
//            cameraReady.GlobalStatusId = "status-accepted"; // Not pending

//            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "head1"));
//        }
//            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "head1"));
//        }

//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_DecisionDeadlineExpired()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1" };
//            SetupHappyPathMocks("head1", "p1", "cr1", true);
//        [Fact]
//        public async Task DecideCameraReadyStatus_Should_Throw_When_DecisionDeadlineExpired()
//        {
//            var request = new UpdateCameraReadyStatusRequest { CameraReadyId = "cr1" };
//            SetupHappyPathMocks("head1", "p1", "cr1", true);

//            var expiredDate = DateTime.Now.AddDays(-5);
//            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(expiredDate));
//            var expiredDate = DateTime.Now.AddDays(-5);
//            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(expiredDate));

//            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync("p1");
//            paper.ResearchConferencePhase.CameraReadyDecideStatusStart = DateOnly.FromDateTime(expiredDate.AddDays(-10));
//            paper.ResearchConferencePhase.CameraReadyDecideStatusEnd = DateOnly.FromDateTime(expiredDate.AddDays(-2));

//            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "head1"));
//        }
//    }
//}
//            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideCameraReadyStatus(request, "head1"));
//        }
//    }
//}
