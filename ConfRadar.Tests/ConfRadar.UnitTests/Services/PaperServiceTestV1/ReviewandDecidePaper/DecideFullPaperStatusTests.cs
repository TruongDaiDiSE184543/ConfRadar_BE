using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class DecideFullPaperStatusTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTime;
        private readonly Mock<ITicketService> _mockTicket;
        private readonly Mock<INotificationService> _mockNoti;
        private readonly PaperService _paperService;

        public DecideFullPaperStatusTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTime = new Mock<ITimeProviderService>();
            _mockTicket = new Mock<ITicketService>();
            _mockNoti = new Mock<INotificationService>();

            // Các mock phụ
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

        private void SetupHappyPathMocks(string userId, string paperId, string fullPaperId, bool isHead)
        {
            var now = DateTime.Now;
            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

            var paper = new Paper
            {
                PaperId = paperId,
                FullPaperId = fullPaperId,
                PaperPhaseId = "phase-fullpaper",
                TicketId = "t1",
                ResearchConferencePhase = new ResearchConferencePhase { FullPaperDecideStatusStart = DateOnly.FromDateTime(now.AddDays(-1)), FullPaperDecideStatusEnd = DateOnly.FromDateTime(now.AddDays(1)) },
                Conference = new Conference(),
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { IsRootAuthor = true, UserId = "author1" } },
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string name) => new ReviewStatus { ReviewStatusId = $"status-{name.ToLower()}", Name = name });

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string name) => new PaperPhase { PaperPhaseId = $"phase-{name.ToLower()}", PhaseName = name });


            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync(new GlobalStatus());

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(paperId)).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(fullPaperId)).ReturnsAsync(new FullPaper { FullPaperId = fullPaperId, ReviewStatusId = "status-pending" });

            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(paperId))
                .ReturnsAsync(new List<PaperReviewer> { new PaperReviewer { UserId = userId, IsHeadReviewer = isHead } });

            _mockUnitOfWork.Setup(u => u.FullPaperReviewRepository.GetFullPaperReviewsByFullPaperIdAsync(fullPaperId)).ReturnsAsync(new List<FullPaperReview> { new FullPaperReview() });

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>())).ReturnsAsync(new User());
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Accept_And_AdvanceToCameraReadyPhase()
        {
            // ARRANGE
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "fp1", true);

            // ACT
            await _paperService.DecideFullPaperFinalStatus(request, "head1");

            // ASSERT
            _mockUnitOfWork.Verify(u => u.FullPaperRepository.UpdateFullPaperAsync(It.Is<FullPaper>(fp => fp.ReviewStatusId == "status-accepted")), Times.Once);
            _mockUnitOfWork.Verify(u => u.PaperRepository.UpdatePaperAsync(It.Is<Paper>(p => p.PaperPhaseId == "phase-cameraready")), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Reject_And_TriggerRefund()
        {
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Rejected };
            SetupHappyPathMocks("head1", "p1", "fp1", true);

            await _paperService.DecideFullPaperFinalStatus(request, "head1");

            _mockUnitOfWork.Verify(u => u.FullPaperRepository.UpdateFullPaperAsync(It.Is<FullPaper>(fp => fp.ReviewStatusId == "status-rejected")), Times.Once);
            _mockTicket.Verify(t => t.RefundAuthorCloneFunction("author1", "t1", It.IsAny<string>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Revise_And_AdvanceToRevisePhase()
        {
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Revise };
            SetupHappyPathMocks("head1", "p1", "fp1", true);

            await _paperService.DecideFullPaperFinalStatus(request, "head1");

            _mockUnitOfWork.Verify(u => u.FullPaperRepository.UpdateFullPaperAsync(It.Is<FullPaper>(fp => fp.ReviewStatusId == "status-revise")), Times.Once);
            _mockUnitOfWork.Verify(u => u.PaperRepository.UpdatePaperAsync(It.Is<Paper>(p => p.PaperPhaseId == "phase-revise")), Times.Once);
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Throw_When_UserIsNotHeadReviewer()
        {
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("normal-reviewer", "p1", "fp1", false); // isHead = false

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.DecideFullPaperFinalStatus(request, "normal-reviewer"));
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Throw_When_DecisionDeadlineExpired()
        {
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "fp1", true);

            // Override time to be in the past
            var expiredDate = DateTime.Now.AddDays(-5);
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(expiredDate));

            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync("p1");
            paper.ResearchConferencePhase.FullPaperDecideStatusStart = DateOnly.FromDateTime(expiredDate.AddDays(-10));
            paper.ResearchConferencePhase.FullPaperDecideStatusEnd = DateOnly.FromDateTime(expiredDate.AddDays(-2));

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideFullPaperFinalStatus(request, "head1"));
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Throw_When_FullPaperIsNotPending()
        {
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "fp1", true);

            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp1"))
                .ReturnsAsync(new FullPaper { FullPaperId = "fp1", ReviewStatusId = "status-accepted" }); // Not pending

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideFullPaperFinalStatus(request, "head1"));
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Throw_When_PaperIsNotInFullPaperPhase()
        {
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "fp1", true);

            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync("p1");
            paper.PaperPhaseId = "phase-abstract"; // Wrong phase

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideFullPaperFinalStatus(request, "head1"));
        }

        [Fact]
        public async Task DecideFullPaperStatus_Should_Throw_When_NoReviewsHaveBeenSubmitted()
        {
            var request = new UpdateFullPaperStatusRequest { PaperId = "p1", FullPaperId = "fp1", ReviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("head1", "p1", "fp1", true);

            _mockUnitOfWork.Setup(u => u.FullPaperReviewRepository.GetFullPaperReviewsByFullPaperIdAsync("fp1"))
                .ReturnsAsync(new List<FullPaperReview>()); // No reviews

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.DecideFullPaperFinalStatus(request, "head1"));
        }
    }
}
