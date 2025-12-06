using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.FullPaperReview;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class SubmitFullPaperReviewTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTime;
        private readonly PaperService _paperService;

        public SubmitFullPaperReviewTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTime = new Mock<ITimeProviderService>();

            // Các mock phụ để khởi tạo service
            var mockMomo = new Mock<IMomoService>();
            var mockToken = new Mock<ITokenService>();
            var mockFile = new Mock<IObjectStorageFileService>();
            var mockTicket = new Mock<ITicketService>();
            var mockNoti = new Mock<INotificationService>();
            var mockStep = new Mock<IConferenceStepService>();
            var options = Options.Create(new ObjectStorageSettings { EndPoint = "http://test.com" });

            _paperService = new PaperService(
                _mockUnitOfWork.Object, mockMomo.Object, mockToken.Object, options,
                mockFile.Object, mockTicket.Object, _mockTime.Object, mockNoti.Object, mockStep.Object
            );
        }

        private void SetupHappyPathMocks(string userId, string paperId, string fullPaperId, DateTime now)
        {
            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

            DateTime? todayDt = ExtensionHelper.GetVietnamTime();
            DateOnly? today = ExtensionHelper.GetVietnamDate();


            var paper = new Paper
            {
                PaperId = paperId,
                FullPaperId = fullPaperId,
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = today.Value.AddDays(-1),
                    ReviewEndDate = today.Value.AddDays(1)
                }
            };
            var fullPaper = new FullPaper { FullPaperId = fullPaperId, ReviewStatusId = "status-pending" };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(userId)).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(fullPaperId)).ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByFullPaperIdAsync(fullPaperId)).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(paperId)).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paperId)).ReturnsAsync(new PaperReviewer());
            _mockUnitOfWork.Setup(u => u.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync(fullPaperId, userId)).ReturnsAsync((FullPaperReview)null);

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync("Pending")).ReturnsAsync(new ReviewStatus { ReviewStatusId = "status-pending" });
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync("Accepted")).ReturnsAsync(new ReviewStatus { ReviewStatusId = "status-accepted" });
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Succeed_When_AllConditionsAreMet()
        {
            // ARRANGE
            var userId = "reviewer1";
            var request = new CreateFullPaperReviewRequest { FullPaperId = "fp1", reviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks(userId, "p1", "fp1", DateTime.Now);

            // ACT
            var result = await _paperService.SubmitReviewForFullPaper(request, userId);

            // ASSERT
            result.Should().NotBeNullOrEmpty();
            _mockUnitOfWork.Verify(u => u.FullPaperReviewRepository.CreateFullPaperReviewAsync(It.IsAny<FullPaperReview>()), Times.Once);
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Throw_When_RequestedStatusIsPending()
        {
            var request = new CreateFullPaperReviewRequest { reviewStatus = ReviewStatusEnum.Pending };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>())).ReturnsAsync(new User());
            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitReviewForFullPaper(request, "user1"));
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Throw_When_FullPaperNotFound()
        {
            var request = new CreateFullPaperReviewRequest { FullPaperId = "fp-not-found", reviewStatus = ReviewStatusEnum.Accepted };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>())).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp-not-found")).ReturnsAsync((FullPaper)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitReviewForFullPaper(request, "user1"));
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Throw_When_UserIsNotAssignedToPaper()
        {
            var request = new CreateFullPaperReviewRequest { FullPaperId = "fp1", reviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("reviewer1", "p1", "fp1", DateTime.Now);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user-not-assigned", "p1")).ReturnsAsync((PaperReviewer)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitReviewForFullPaper(request, "user-not-assigned"));
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Throw_When_ReviewDeadlineHasPassed()
        {
            var request = new CreateFullPaperReviewRequest { FullPaperId = "fp1", reviewStatus = ReviewStatusEnum.Accepted };
            // Set deadline in the past
            SetupHappyPathMocks("reviewer1", "p1", "fp1", DateTime.Now.AddDays(-10));

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitReviewForFullPaper(request, "reviewer1"));
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Throw_When_ReviewPeriodHasNotStarted()
        {
            var request = new CreateFullPaperReviewRequest { FullPaperId = "fp1", reviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("reviewer1", "p1", "fp1", DateTime.Now.AddDays(10));

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitReviewForFullPaper(request, "reviewer1"));
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Throw_When_UserAlreadySubmittedReview()
        {
            var request = new CreateFullPaperReviewRequest { FullPaperId = "fp1", reviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("reviewer1", "p1", "fp1", DateTime.Now);
            _mockUnitOfWork.Setup(u => u.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync("fp1", "reviewer1")).ReturnsAsync(new FullPaperReview());

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitReviewForFullPaper(request, "reviewer1"));
        }

        [Fact]
        public async Task SubmitReviewForFullPaper_Should_Throw_When_FullPaperIsNotInPendingStatus()
        {
            var request = new CreateFullPaperReviewRequest { FullPaperId = "fp1", reviewStatus = ReviewStatusEnum.Accepted };
            SetupHappyPathMocks("reviewer1", "p1", "fp1", DateTime.Now);    

            var fullPaper = new FullPaper { FullPaperId = "fp1", ReviewStatusId = "status-accepted" }; // Not pending
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp1")).ReturnsAsync(fullPaper);

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitReviewForFullPaper(request, "reviewer1"));
        }
    }
}
