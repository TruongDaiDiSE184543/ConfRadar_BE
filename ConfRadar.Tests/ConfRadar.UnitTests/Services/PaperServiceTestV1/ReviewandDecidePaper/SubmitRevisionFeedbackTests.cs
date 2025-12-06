using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfRadar.Services.Common.AppSettingConfig;
using ConfRadar.Services.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class SubmitRevisionFeedbackTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTime;

        // Mock các repo con
        private readonly Mock<IPaperRepository> _mockPaperRepo;
        private readonly Mock<IPaperReviewerRepository> _mockPaperReviewerRepo;
        private readonly Mock<IRevisionPaperSubmissionRepository> _mockRevisionSubmissionRepo;
        private readonly Mock<IRevisionSubmissionFeedbackRepository> _mockFeedbackRepo;

        private readonly PaperService _paperService;

        public SubmitRevisionFeedbackTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTime = new Mock<ITimeProviderService>();

            // Khởi tạo các mock repo con
            _mockPaperRepo = new Mock<IPaperRepository>();
            _mockPaperReviewerRepo = new Mock<IPaperReviewerRepository>();
            _mockRevisionSubmissionRepo = new Mock<IRevisionPaperSubmissionRepository>();
            _mockFeedbackRepo = new Mock<IRevisionSubmissionFeedbackRepository>();

            // Gắn mock repo con vào UnitOfWork
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(_mockPaperRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository).Returns(_mockPaperReviewerRepo.Object);
            _mockUnitOfWork.Setup(u => u.RevisionPaperSubmissionRepository).Returns(_mockRevisionSubmissionRepo.Object);
            _mockUnitOfWork.Setup(u => u.RevisionSubmissionFeedbackRepository).Returns(_mockFeedbackRepo.Object);

            // Các mock phụ để khởi tạo service
            var mockMomo = new Mock<IMomoService>();
            var mockToken = new Mock<ITokenService>();
            var mockFile = new Mock<IObjectStorageFileService>();
            var mockTicket = new Mock<ITicketService>();
            var mockNoti = new Mock<INotificationService>();
            var mockStep = new Mock<IConferenceStepService>();
            var options = Options.Create(new ObjectStorageSettings());

            _paperService = new PaperService(
                _mockUnitOfWork.Object, mockMomo.Object, mockToken.Object, options,
                mockFile.Object, mockTicket.Object, _mockTime.Object, mockNoti.Object, mockStep.Object
            );
        }

        private void SetupHappyPathMocks(string userId, string paperId, string submissionId, bool isHead)
        {
            var now = DateTime.Now;
            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

            // Mock Paper
            var paper = new Paper
            {
                PaperId = paperId,
                Conference = new Conference { ConferenceName = "Test Conf" },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    // Đang trong giai đoạn Revise
                    ReviseStartDate = DateOnly.FromDateTime(now.AddDays(-10)),
                    ReviseEndDate = DateOnly.FromDateTime(now.AddDays(10))
                }
            };
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync(paperId)).ReturnsAsync(paper);

            // Mock Revision Submission
            var submission = new RevisionPaperSubmission
            {
                RevisionPaperSubmissionId = submissionId,
                RevisionDeadlineRound = new RevisionRoundDeadline
                {
                    // Đang trong deadline của round này
                    StartSubmissionDate = DateOnly.FromDateTime(now.AddDays(-5)),
                    EndSubmissionDate = DateOnly.FromDateTime(now.AddDays(5))
                }
            };
            _mockRevisionSubmissionRepo.Setup(r => r.GetRevisionPaperSubmissionByIdAsync(submissionId)).ReturnsAsync(submission);

            // Mock Paper Reviewer (quyền hạn)
            _mockPaperReviewerRepo.Setup(r => r.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paperId))
                .ReturnsAsync(new PaperReviewer { IsHeadReviewer = isHead });

            // Mock hàm create
            _mockFeedbackRepo.Setup(r => r.CreateMultipleFeedbacksAsync(It.IsAny<List<RevisionSubmissionFeedback>>())).ReturnsAsync(1);
        }

        [Fact]
        public async Task CreateRevisionSubmissionFeedBack_Should_Succeed_When_UserIsHeadReviewer_And_WithinDeadlines()
        {
            // ARRANGE
            var userId = "head1";
            var request = new CreateRevisionPaperSubmissionFeedback
            {
                PaperId = "p1",
                RevisionPaperSubmissionId = "sub1",
                Feedbacks = new List<RevisionPaperSubmissionFeedbackRequest>
                {
                    new RevisionPaperSubmissionFeedbackRequest { Feedback = "Point 1 needs clarification.", SortOrder = 1 }
                }
            };
            SetupHappyPathMocks(userId, "p1", "sub1", true);

            // ACT
            var result = await _paperService.CreateRevisionSubmissionFeedBack(request, userId);

            // ASSERT
            result.Should().BeGreaterThan(0);
            _mockFeedbackRepo.Verify(r => r.CreateMultipleFeedbacksAsync(
                It.Is<List<RevisionSubmissionFeedback>>(list =>
                    list.Count == 1 &&
                    list[0].Feedback == "Point 1 needs clarification." &&
                    list[0].UserId == userId
                )), Times.Once);
        }

        [Fact]
        public async Task CreateRevisionSubmissionFeedBack_Should_Throw_When_UserIsNotHeadReviewer()
        {
            var userId = "normal-reviewer";
            var request = new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "sub1", Feedbacks = new List<RevisionPaperSubmissionFeedbackRequest>() };
            SetupHappyPathMocks(userId, "p1", "sub1", false); // isHead = false

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _paperService.CreateRevisionSubmissionFeedBack(request, userId));
            ex.Message.Should().Contain("Chức năng này dành cho head reviewer");
        }

        [Fact]
        public async Task CreateRevisionSubmissionFeedBack_Should_Throw_When_OutsideRevisePhase()
        {
            var userId = "head1";
            var request = new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "sub1", Feedbacks = new List<RevisionPaperSubmissionFeedbackRequest>() };
            SetupHappyPathMocks(userId, "p1", "sub1", true);

            // Override ngày tháng để nằm ngoài Phase
            var paper = await _mockPaperRepo.Object.GetPaperByIdAsync("p1");
            paper.ResearchConferencePhase.ReviseStartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            paper.ResearchConferencePhase.ReviseEndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _paperService.CreateRevisionSubmissionFeedBack(request, userId));
            ex.Message.Should().Contain("Giai đoạn gửi feedback revise diễn ra từ");
        }

        [Fact]
        public async Task CreateRevisionSubmissionFeedBack_Should_Throw_When_OutsideSubmissionDeadline()
        {
            var userId = "head1";
            var request = new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "sub1", Feedbacks = new List<RevisionPaperSubmissionFeedbackRequest>() };
            SetupHappyPathMocks(userId, "p1", "sub1", true);

            // Override ngày tháng để nằm ngoài Deadline của Round
            var submission = await _mockRevisionSubmissionRepo.Object.GetRevisionPaperSubmissionByIdAsync("sub1");
            submission.RevisionDeadlineRound.StartSubmissionDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
            submission.RevisionDeadlineRound.EndSubmissionDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _paperService.CreateRevisionSubmissionFeedBack(request, userId));
            ex.Message.Should().Contain("Deadline cho tương tác qua lại nằm từ");
        }

        [Fact]
        public async Task CreateRevisionSubmissionFeedBack_Should_Throw_When_PaperNotFound()
        {
            var userId = "head1";
            var request = new CreateRevisionPaperSubmissionFeedback { PaperId = "p-not-found", RevisionPaperSubmissionId = "sub1", Feedbacks = new List<RevisionPaperSubmissionFeedbackRequest>() };
            _mockPaperRepo.Setup(r => r.GetPaperByIdAsync("p-not-found")).ReturnsAsync((Paper)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.CreateRevisionSubmissionFeedBack(request, userId));
        }

        [Fact]
        public async Task CreateRevisionSubmissionFeedBack_Should_Throw_When_SubmissionNotFound()
        {
            var userId = "head1";
            var request = new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "sub-not-found", Feedbacks = new List<RevisionPaperSubmissionFeedbackRequest>() };
            SetupHappyPathMocks(userId, "p1", "sub1", true); // Setup với sub1
            _mockRevisionSubmissionRepo.Setup(r => r.GetRevisionPaperSubmissionByIdAsync("sub-not-found")).ReturnsAsync((RevisionPaperSubmission)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.CreateRevisionSubmissionFeedBack(request, userId));
        }

        [Fact]
        public async Task CreateRevisionSubmissionFeedBack_Should_Throw_When_ReviewerNotAssignedToPaper()
        {
            var userId = "head1";
            var request = new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "sub1", Feedbacks = new List<RevisionPaperSubmissionFeedbackRequest>() };
            SetupHappyPathMocks(userId, "p1", "sub1", true);

            // Mock user này không được assign vào paper p1
            _mockPaperReviewerRepo.Setup(r => r.GetPaperReviewersByPaperIdAndUserIdAsync("other-user", "p1")).ReturnsAsync((PaperReviewer)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.CreateRevisionSubmissionFeedBack(request, "other-user"));
        }
    }
}
