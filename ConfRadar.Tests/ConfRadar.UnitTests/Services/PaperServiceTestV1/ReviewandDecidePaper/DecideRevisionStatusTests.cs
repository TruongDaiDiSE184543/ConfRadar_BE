using ConfRadar.Repositories.Models;
using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfRadar.Services.Common.AppSettingConfig;
using Microsoft.Extensions.Options;
using ConfRadar.Services.Exceptions;
using FluentAssertions;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class DecideRevisionStatusTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTime;
        private readonly Mock<ITicketService> _mockTicket;
        private readonly Mock<INotificationService> _mockNoti;
        private readonly PaperService _paperService;

        public DecideRevisionStatusTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTime = new Mock<ITimeProviderService>();
            _mockTicket = new Mock<ITicketService>();
            _mockNoti = new Mock<INotificationService>();

            var mockMomo = new Mock<IMomoService>();
            var mockToken = new Mock<ITokenService>();
            var mockFile = new Mock<IObjectStorageFileService>();
            var mockStep = new Mock<IConferenceStepService>();
            var options = Options.Create(new ObjectStorageSettings());

            _paperService = new PaperService(
                _mockUnitOfWork.Object, mockMomo.Object, mockToken.Object, options,
                mockFile.Object, _mockTicket.Object, _mockTime.Object, _mockNoti.Object, mockStep.Object
            );
        }

        private void SetupHappyPathMocks(string userId, string paperId, string revisionPaperId, bool isHead)
        {
            var now = DateTime.Now;
            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

            var paper = new Paper
            {
                PaperId = paperId,
                RevisionPaperId = revisionPaperId,
                PaperPhaseId = "phase-revise",
                TicketId = "t1",
                Conference = new Conference { ResearchConferenceDetail = new ResearchConferenceDetail { RevisionAttemptAllowed = 1 } },
                ResearchConferencePhase = new ResearchConferencePhase { RevisionPaperDecideStatusStart = DateOnly.FromDateTime(now.AddDays(-1)), RevisionPaperDecideStatusEnd = DateOnly.FromDateTime(now.AddDays(1)) },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { IsRootAuthor = true, UserId = "author1" } }
            };
            var revisionPaper = new RevisionPaper { RevisionPaperId = revisionPaperId, RevisionPaperSubmissions = new List<RevisionPaperSubmission> { new RevisionPaperSubmission() } };

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync((string name) => new GlobalStatus { GlobalStatusId = $"status-{name.ToLower()}", Name = name });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync("Revise")).ReturnsAsync(new PaperPhase { PaperPhaseId = "phase-revise" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync("CameraReady")).ReturnsAsync(new PaperPhase { PaperPhaseId = "phase-camera" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(paperId)).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(revisionPaperId)).ReturnsAsync(revisionPaper);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paperId)).ReturnsAsync(new PaperReviewer { IsHeadReviewer = isHead });
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>())).ReturnsAsync(new User());

            // Setup cho hàm update để tránh lỗi
            _mockUnitOfWork.Setup(u => u.RevisionPaperRepository.UpdateRevisionPaperAsync(It.IsAny<RevisionPaper>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.PaperRepository.UpdatePaperAsync(It.IsAny<Paper>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateNotificationAsync(It.IsAny<Notification>())).ReturnsAsync(1);
        }

        [Fact]
        public async Task DecideReviseStatus_Should_Accept_When_SubmissionsMatchAllowedAttempts()
        {
            // ARRANGE
            var request = new UpdateRevisionStatusRequest { PaperId = "p1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "rev1", true);

            // ACT
            await _paperService.DecideReviseStatus(request, "head1");

            // ASSERT
            _mockUnitOfWork.Verify(u => u.RevisionPaperRepository.UpdateRevisionPaperAsync(It.Is<RevisionPaper>(r => r.GlobalStatusId == "status-accepted")), Times.Once);
            _mockUnitOfWork.Verify(u => u.PaperRepository.UpdatePaperAsync(It.Is<Paper>(p => p.PaperPhaseId == "phase-camera")), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DecideReviseStatus_Should_Accept_When_MarkedAsCompleteByHead()
        {
            var request = new UpdateRevisionStatusRequest { PaperId = "p1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "rev1", true);

            // Override mocks để test logic byPassDecideRevise
            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync("p1");
            paper.Conference.ResearchConferenceDetail.RevisionAttemptAllowed = 5; // Yêu cầu 5 lần

            var revisionPaper = await _mockUnitOfWork.Object.RevisionPaperRepository.GetRevisionPaperByIdAsync("rev1");
            revisionPaper.RevisionPaperSubmissions.Clear(); // Mới nộp 0 lần
            revisionPaper.RevisionRoundDeadlineId = "deadline-marked-complete"; // Nhưng đã được Head đánh dấu

            // ACT
            await _paperService.DecideReviseStatus(request, "head1");

            // ASSERT
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once, "Should pass because it was marked complete.");
        }

        [Fact]
        public async Task DecideReviseStatus_Should_Reject_And_TriggerRefund()
        {
            var request = new UpdateRevisionStatusRequest { PaperId = "p1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Rejected };
            SetupHappyPathMocks("head1", "p1", "rev1", true);

            await _paperService.DecideReviseStatus(request, "head1");

            _mockUnitOfWork.Verify(u => u.RevisionPaperRepository.UpdateRevisionPaperAsync(It.Is<RevisionPaper>(r => r.GlobalStatusId == "status-rejected")), Times.Once);
            _mockTicket.Verify(t => t.RefundAuthorCloneFunction("author1", "t1", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DecideReviseStatus_Should_Throw_When_UserIsNotHeadReviewer()
        {
            var request = new UpdateRevisionStatusRequest { PaperId = "p1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Rejected };
            SetupHappyPathMocks("normal-reviewer", "p1", "rev1", false); // isHead = false

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideReviseStatus(request, "normal-reviewer"));
            ex.Message.Should().Contain("Bạn không phải head reviewer");
        }

        [Fact]
        public async Task DecideReviseStatus_Should_Throw_When_NotEnoughSubmissions_And_NotMarkedComplete()
        {
            var request = new UpdateRevisionStatusRequest { PaperId = "p1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "rev1", true);

            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync("p1");
            paper.Conference.ResearchConferenceDetail.RevisionAttemptAllowed = 5; // Yêu cầu 5 lần

            var revisionPaper = await _mockUnitOfWork.Object.RevisionPaperRepository.GetRevisionPaperByIdAsync("rev1");
            revisionPaper.RevisionPaperSubmissions = new List<RevisionPaperSubmission> { new RevisionPaperSubmission() }; // Mới nộp 1
            revisionPaper.RevisionRoundDeadlineId = null; // Chưa được mark

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideReviseStatus(request, "head1"));
            ex.Message.Should().Contain("họ phải đi hết revision round");
        }

        [Fact]
        public async Task DecideReviseStatus_Should_Throw_When_PaperNotInRevisePhase()
        {
            var request = new UpdateRevisionStatusRequest { PaperId = "p1", RevisionPaperId = "rev1" };
            SetupHappyPathMocks("head1", "p1", "rev1", true);
            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync("p1");
            paper.PaperPhaseId = "phase-fullpaper"; // Sai phase

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideReviseStatus(request, "head1"));
        }

        [Fact]
        public async Task DecideReviseStatus_Should_Throw_When_DecisionPeriodIsOver()
        {
            var request = new UpdateRevisionStatusRequest { PaperId = "p1", RevisionPaperId = "rev1" };
            SetupHappyPathMocks("head1", "p1", "rev1", true);

            var now = DateTime.Now;
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));
            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync("p1");
            paper.ResearchConferencePhase.RevisionPaperDecideStatusStart = DateOnly.FromDateTime(now.AddDays(-10));
            paper.ResearchConferencePhase.RevisionPaperDecideStatusEnd = DateOnly.FromDateTime(now.AddDays(-5)); // Đã hết hạn

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideReviseStatus(request, "head1"));
        }
    }
}
