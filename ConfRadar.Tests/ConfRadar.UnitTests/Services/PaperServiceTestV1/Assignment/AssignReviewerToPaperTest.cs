using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.Assignment
{
    public class AssignReviewerToPaperTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly PaperAssignmentService _service;

        public AssignReviewerToPaperTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();

            _service = new PaperAssignmentService(
                _mockUnitOfWork.Object,
                _mockTimeProviderService.Object,
                _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenUserNotFound()
        {
            var request = new AssignReviewerToPaperRequest { UserId = "U1", PaperId = "P1" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.AssignReviewerToPaper(request));
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenPaperNotFound()
        {
            var request = new AssignReviewerToPaperRequest { UserId = "U1", PaperId = "P1" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync((Paper)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.AssignReviewerToPaper(request));
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenRolesNotExist()
        {
            var request = new AssignReviewerToPaperRequest { UserId = "U1", PaperId = "P1" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync((Role)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.AssignReviewerToPaper(request));
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenUserNotReviewerRole()
        {
            var request = new AssignReviewerToPaperRequest
            {
                UserId = "U1",
                PaperId = "P1",
                IsHeadReviewer = false
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1" });

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1", RoleName = "Local Reviewer" });

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2", RoleName = "External Reviewer" });

            // **CRITICAL FIX**: Mock PaperAuthorRepository để tránh NullReferenceException
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync((PaperAuthor)null); // User không phải author

            // User không có reviewer roles
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("U1"))
                .ReturnsAsync(new List<UserRole>
                {
                    new UserRole { UserId = "U1", RoleId = "R_OTHER" } // Role khác
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.AssignReviewerToPaper(request));

            Assert.Contains("does not have Local Reviewer or External Reviewer role", exception.Message);
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenUserIsAuthor()
        {
            var request = new AssignReviewerToPaperRequest { UserId = "U1", PaperId = "P1" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2" });
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync(new PaperAuthor()); // user is author
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("U1"))
                .ReturnsAsync(new List<UserRole> { new UserRole { RoleId = "R1" } });

            await Assert.ThrowsAsync<BadRequestException>(() => _service.AssignReviewerToPaper(request));
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenUserAlreadyReviewer()
        {
            var request = new AssignReviewerToPaperRequest { UserId = "U1", PaperId = "P1" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2" });
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync((PaperAuthor)null);
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("U1"))
                .ReturnsAsync(new List<UserRole> { new UserRole { RoleId = "R1" } });
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("U1", "P1"))
                .ReturnsAsync(new PaperReviewer()); // already reviewer

            await Assert.ThrowsAsync<BadRequestException>(() => _service.AssignReviewerToPaper(request));
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldThrow_WhenHeadReviewerExists()
        {
            var request = new AssignReviewerToPaperRequest { UserId = "U1", PaperId = "P1", IsHeadReviewer = true };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2" });
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync((PaperAuthor)null);
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("U1"))
                .ReturnsAsync(new List<UserRole> { new UserRole { RoleId = "R1" } });
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("U1", "P1"))
                .ReturnsAsync((PaperReviewer)null);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetHeadReviewersByPaperIdAsync("P1"))
                .ReturnsAsync(new List<PaperReviewer> { new PaperReviewer() }); // head already exists

            await Assert.ThrowsAsync<BadRequestException>(() => _service.AssignReviewerToPaper(request));
        }

        [Fact]
        public async Task AssignReviewerToPaper_ShouldReturnSuccess_WhenValid()
        {
            var request = new AssignReviewerToPaperRequest { UserId = "U1", PaperId = "P1", IsHeadReviewer = true };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("U1"))
                .ReturnsAsync(new User { UserId = "U1", FirebaseMobileFcmToken = "token1", FirebaseWebFcmToken = "token2" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("P1"))
                .ReturnsAsync(new Paper { PaperId = "P1", Title = "Paper 1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("Local Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R1" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName("External Reviewer"))
                .ReturnsAsync(new Role { RoleId = "R2" });
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorByIdAsync("U1", "P1"))
                .ReturnsAsync((PaperAuthor)null);
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("U1"))
                .ReturnsAsync(new List<UserRole> { new UserRole { RoleId = "R1" } });
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("U1", "P1"))
                .ReturnsAsync((PaperReviewer)null);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetHeadReviewersByPaperIdAsync("P1"))
                .ReturnsAsync(new List<PaperReviewer>());
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.CreatePaperReviewerAsync(It.IsAny<PaperReviewer>()))
                .ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateNotificationAsync(It.IsAny<Notification>()))
                .ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.UtcNow);
            _mockNotificationService.Setup(n => n.SendMobilePushAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockNotificationService.Setup(n => n.SendWebPushAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _service.AssignReviewerToPaper(request);

            Assert.Contains("successfully assigned", result);
        }
    }
}
