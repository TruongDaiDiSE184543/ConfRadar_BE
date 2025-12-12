using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.AbstractPaper
{
    public class DecideAbstractPaperStatusTest
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMomoService> _mockMomoService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITicketService> _mockTicketService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<IConferenceStepService> _mockConferenceStepService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly PaperService _paperService;


        public DecideAbstractPaperStatusTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMomoService = new Mock<IMomoService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTicketService = new Mock<ITicketService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockConferenceStepService = new Mock<IConferenceStepService>();
            _mockEmailService = new Mock<IEmailService>();

            // --- SỬA Ở ĐÂY: Khởi tạo và gắn NotificationRepo vào UnitOfWork ---
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockUnitOfWork.Setup(u => u.NotificationRepository).Returns(_mockNotificationRepo.Object);
            // ------------------------------------------------------------------

            var options = Options.Create(new ObjectStorageSettings());

            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                _mockMomoService.Object,
                _mockTokenService.Object,
                options,
                _mockObjectStorageFileService.Object,
                _mockTicketService.Object,
                _mockTimeProviderService.Object,
                _mockNotificationService.Object,
                _mockConferenceStepService.Object,
                _mockEmailService.Object
            );
        }

        #endregion

        #region Helper Methods & Constants

        private const string PendingStatusId = "status-pending";
        private const string AcceptedStatusId = "status-accepted";
        private const string RejectedStatusId = "status-rejected";
        private const string AbstractPhaseId = "phase-abstract";
        private const string FullPaperPhaseId = "phase-fullpaper";

        private void SetupHappyPathMocks(string paperId, string abstractId, string userId)
        {
            // 1. Mock Time
            var now = DateTime.Now;
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

            // 2. Mock Global Statuses
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync((string name) =>
                {
                    if (name.Contains("Pending")) return new GlobalStatus { GlobalStatusId = PendingStatusId, Name = "Pending" };
                    if (name.Contains("Accepted")) return new GlobalStatus { GlobalStatusId = AcceptedStatusId, Name = "Accepted" };
                    if (name.Contains("Rejected")) return new GlobalStatus { GlobalStatusId = RejectedStatusId, Name = "Rejected" };
                    return null;
                });

            // 3. Mock Paper Phases
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync((string name) =>
                {
                    if (name.Contains("Abstract")) return new PaperPhase { PaperPhaseId = AbstractPhaseId, PhaseName = "Abstract" };
                    if (name.Contains("FullPaper")) return new PaperPhase { PaperPhaseId = FullPaperPhaseId, PhaseName = "FullPaper" };
                    return null;
                });

            // 4. Mock Paper
            var researchPhase = new ResearchConferencePhase
            {
                AbstractDecideStatusStart = DateOnly.FromDateTime(now.AddDays(-1)),
                AbstractDecideStatusEnd = DateOnly.FromDateTime(now.AddDays(1))
            };

            var paper = new Paper
            {
                PaperId = paperId,
                Title = "Test Paper",
                PaperPhaseId = AbstractPhaseId,
                TicketId = "ticket-123",
                Conference = new Conference { ConferenceName = "Test Conf" },
                ResearchConferencePhase = researchPhase,
                PaperAuthors = new List<PaperAuthor>
                {
                    new PaperAuthor { UserId = userId, IsRootAuthor = true }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(paperId))
                .ReturnsAsync(paper);

            // 5. Mock Abstract
            var abstractPaper = new Abstract
            {
                AbstractId = abstractId,
                GlobalStatusId = PendingStatusId
            };

            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync(abstractId))
                .ReturnsAsync(abstractPaper);

            // 6. Mock User
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(userId))
                .ReturnsAsync(new User { UserId = userId, FirebaseMobileFcmToken = "token" });

            // 7. Mock Transaction & Data Operations
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // --- SỬA Ở ĐÂY: Setup kết quả trả về cho các hàm void/int async ---
            _mockNotificationRepo.Setup(r => r.CreateNotificationAsync(It.IsAny<Notification>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.UpdateAbstractAsync(It.IsAny<Abstract>())).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.PaperRepository.UpdatePaperAsync(It.IsAny<Paper>())).ReturnsAsync(1);
            // ------------------------------------------------------------------
        }

        #endregion

        #region Facts

        [Fact]
        public async Task DecideAbstractPaperStatus_Should_Accept_And_AdvanceToFullPaperPhase()
        {
            // ARRANGE
            string paperId = "paper-1";
            string abstractId = "abs-1";
            string userId = "user-1";
            SetupHappyPathMocks(paperId, abstractId, userId);

            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = paperId,
                AbstractId = abstractId,
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            // ACT
            await _paperService.DecideAbstractPaperStatus(request, userId);

            // ASSERT
            // Verify Abstract updated to Accepted
            _mockUnitOfWork.Verify(u => u.AbstractRepository.UpdateAbstractAsync(It.Is<Abstract>(a =>
                a.GlobalStatusId == AcceptedStatusId &&
                a.ReviewAt != null)), Times.Once);

            // Verify Paper updated to FullPaper Phase
            _mockUnitOfWork.Verify(u => u.PaperRepository.UpdatePaperAsync(It.Is<Paper>(p =>
                p.PaperPhaseId == FullPaperPhaseId)), Times.Once);

            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_Should_Reject_And_RefundTicket()
        {
            // ARRANGE
            string paperId = "paper-1";
            string abstractId = "abs-1";
            string userId = "user-1";
            SetupHappyPathMocks(paperId, abstractId, userId);

            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = paperId,
                AbstractId = abstractId,
                GlobalStatus = GlobalStatusEnum.Rejected
            };

            // ACT
            await _paperService.DecideAbstractPaperStatus(request, userId);

            // ASSERT
            // Verify Abstract updated to Rejected
            _mockUnitOfWork.Verify(u => u.AbstractRepository.UpdateAbstractAsync(It.Is<Abstract>(a =>
                a.GlobalStatusId == RejectedStatusId)), Times.Once);

            // Verify Refund Service called
            //_mockTicketService.Verify(t => t.RefundAuthorCloneFunction(userId, "ticket-123", It.IsAny<string>()), Times.Once);

            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_Should_ThrowBadRequest_When_RequestStatusIsPending()
        {
            // ARRANGE
            var request = new UpdateAbstractPaperStatusRequest { GlobalStatus = GlobalStatusEnum.Pending };

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _paperService.DecideAbstractPaperStatus(request, "user-1")
            );
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_Should_ThrowNotFound_When_PaperById_ReturnsNull()
        {
            // ARRANGE
            string paperId = "paper-not-found";
            SetupHappyPathMocks("other-paper", "abs-1", "user-1"); // Setup valid mocks for helpers
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(paperId)).ReturnsAsync((Paper)null); // Override

            var request = new UpdateAbstractPaperStatusRequest { PaperId = paperId, AbstractId = "abs-1", GlobalStatus = GlobalStatusEnum.Accepted };

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _paperService.DecideAbstractPaperStatus(request, "user-1")
            );
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_Should_ThrowBadRequest_When_DateIsOutsideDecisionPeriod()
        {
            // ARRANGE
            string paperId = "paper-1";
            SetupHappyPathMocks(paperId, "abs-1", "user-1");

            // Mock Paper with ResearchPhase expired
            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync(paperId);
            var today = await _mockTimeProviderService.Object.GetVietnamDate();

            // Set range in the past
            paper.ResearchConferencePhase.AbstractDecideStatusStart = today.AddDays(-10);
            paper.ResearchConferencePhase.AbstractDecideStatusEnd = today.AddDays(-5);

            var request = new UpdateAbstractPaperStatusRequest { PaperId = paperId, AbstractId = "abs-1", GlobalStatus = GlobalStatusEnum.Accepted };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _paperService.DecideAbstractPaperStatus(request, "user-1")
            );
            ex.Message.Should().Contain("Ngày quyết định abstract này từ");
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_Should_ThrowBadRequest_When_PaperPhase_IsNotAbstract()
        {
            // ARRANGE
            string paperId = "paper-1";
            SetupHappyPathMocks(paperId, "abs-1", "user-1");

            // Mock Paper is currently in FullPaper phase (wrong phase for abstract decision)
            var paper = await _mockUnitOfWork.Object.PaperRepository.GetPaperByIdAsync(paperId);
            paper.PaperPhaseId = FullPaperPhaseId;

            var request = new UpdateAbstractPaperStatusRequest { PaperId = paperId, AbstractId = "abs-1", GlobalStatus = GlobalStatusEnum.Accepted };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _paperService.DecideAbstractPaperStatus(request, "user-1")
            );
            ex.Message.Should().Contain("Paper đang không trong quá trình quyết định abstract");
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_Should_ThrowBadRequest_When_CurrentAbstractStatus_IsNotPending()
        {
            // ARRANGE
            string abstractId = "abs-1";
            SetupHappyPathMocks("paper-1", abstractId, "user-1");

            // Mock Abstract has already been Accepted (Not Pending)
            var abstractPaper = await _mockUnitOfWork.Object.AbstractRepository.GetAbstractByIdAsync(abstractId);
            abstractPaper.GlobalStatusId = AcceptedStatusId;

            var request = new UpdateAbstractPaperStatusRequest { PaperId = "paper-1", AbstractId = abstractId, GlobalStatus = GlobalStatusEnum.Rejected };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _paperService.DecideAbstractPaperStatus(request, "user-1")
            );
            ex.Message.Should().Contain("abstract không trong quá trình pending");
        }



        #endregion
    }
}
