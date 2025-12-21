using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;
using Xunit;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.Assignment
{
    public class AssignReviewerToPaperTest : PaperAssignmentService
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<INotificationService> _mockNotificationService;

        public AssignReviewerToPaperTest() : this(new Mock<IUnitOfWork>(), new Mock<ITimeProviderService>(), new Mock<INotificationService>())
        {
        }

        private AssignReviewerToPaperTest(Mock<IUnitOfWork> mockUow, Mock<ITimeProviderService> mockTp, Mock<INotificationService> mockNs)
            : base(mockUow.Object, mockTp.Object, mockNs.Object)
        {
            _mockUnitOfWork = mockUow;
            _mockTimeProviderService = mockTp;
            _mockNotificationService = mockNs;
        }

        [Fact]
        public async Task AssignReviewersToPaper_ShouldThrow_WhenNoHeadReviewer()
        {
            var request = new AssignReviewerToPaperRequest
            {
                PaperId = "P1",
                Reviewers = new List<ReviewerAssignment>
                {
                    new ReviewerAssignment { UserId = "U1", IsHeadReviewer = false }
                }
            };

            await Assert.ThrowsAsync<BadRequestException>(() => this.AssignReviewersToPaper(request));
        }

        [Fact]
        public async Task AssignReviewersToPaper_ShouldThrow_WhenPaperNotFound()
        {
            var request = new AssignReviewerToPaperRequest
            {
                PaperId = "P1",
                Reviewers = new List<ReviewerAssignment>
                {
                    new ReviewerAssignment { UserId = "U1", IsHeadReviewer = true }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync((Paper)null);

            await Assert.ThrowsAsync<BadRequestException>(() => this.AssignReviewersToPaper(request));
        }

        [Fact]
        public async Task AssignReviewersToPaper_ShouldThrow_WhenUserIsAuthor()
        {
            var request = new AssignReviewerToPaperRequest
            {
                PaperId = "P1",
                Reviewers = new List<ReviewerAssignment>
                {
                    new ReviewerAssignment { UserId = "U1", IsHeadReviewer = true }
                }
            };

            var user = new User { UserId = "U1", FullName = "Author User" };
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2" });
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync(new PaperAuthor());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => this.AssignReviewersToPaper(request));
            Assert.Contains("là tác giả", ex.Message);
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenUserNotReviewerRole()
        {
            var request = new AssignReviewerToPaperRequest
            {
                PaperId = "P1",
                Reviewers = new List<ReviewerAssignment>
                {
                    new ReviewerAssignment { UserId = "U1", IsHeadReviewer = true }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2" });
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1", FullName = "User" });
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync((PaperAuthor)null);
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("U1"))
                .ReturnsAsync(new List<UserRole>());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => this.AssignReviewersToPaper(request));
            Assert.Contains("không có vai trò 'Local Reviewer' hoặc 'External Reviewer'", ex.Message);
        }

        [Fact]
        public async Task AssignReviewersToPaper_ShouldReturnSuccess_WhenValid()
        {
            var request = new AssignReviewerToPaperRequest
            {
                PaperId = "P1",
                Reviewers = new List<ReviewerAssignment>
                {
                    new ReviewerAssignment { UserId = "U1", IsHeadReviewer = true }
                }
            };

            var user = new User
            {
                UserId = "U1",
                FullName = "Reviewer User",
                FirebaseMobileFcmToken = "token1",
                FirebaseWebFcmToken = "token2"
            };
            var paper = new Paper { PaperId = "P1", Title = "Paper 1", ConferenceId = "C1" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2" });
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync((PaperAuthor)null);
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("U1"))
                .ReturnsAsync(new List<UserRole> { new UserRole { RoleId = "R1" } });

            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync("P1"))
                .ReturnsAsync(new List<PaperReviewer>());
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.DeleteMultiplePaperReviewersAsync(It.IsAny<List<PaperReviewer>>()))
                .ReturnsAsync(0);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.CreateMultiplePaperReviewersAsync(It.IsAny<List<PaperReviewer>>()))
                .ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateMutipleNotificationAsync(It.IsAny<List<Notification>>()))
                .ReturnsAsync(1);

            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.UtcNow);

            var result = await this.AssignReviewersToPaper(request);

            Assert.Contains("thành công", result);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
