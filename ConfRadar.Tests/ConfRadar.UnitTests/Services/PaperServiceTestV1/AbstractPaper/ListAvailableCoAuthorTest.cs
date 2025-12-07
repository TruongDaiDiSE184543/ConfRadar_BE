using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.AbstractPaper
{
    public class ListAvailableCoAuthorTest
    {

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly PaperService _paperService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;

        public ListAvailableCoAuthorTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockNotificationService = new Mock<INotificationService>();
            _objectStorageSettings = Options.Create(new ObjectStorageSettings { EndPoint = "https://mockstorage.com" });

            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                Mock.Of<IMomoService>(),
                _mockTokenService.Object,
                _objectStorageSettings,
                _mockObjectStorageFileService.Object,
                Mock.Of<ITicketService>(),
                _mockTimeProviderService.Object,
                _mockNotificationService.Object,
                Mock.Of<IConferenceStepService>()
            );
        }



        [Fact]
        public async Task GetAvailableCoAuthor_ShouldThrow_WhenRolesNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>())).ReturnsAsync((Role)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _paperService.GetAvailableCoAuthorForInclude("conf1", "user1")
            );
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldReturnEmpty_WhenNoUsersAvailable()
        {
            var adminRole = new Role { RoleId = "admin" };
            var organizerRole = new Role { RoleId = "org" };
            var localReviewerRole = new Role { RoleId = "rev" };
            var collabRole = new Role { RoleId = "col" };
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(new List<User>());
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription()))
    .ReturnsAsync(new Role { RoleId = "rev" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription()))
                .ReturnsAsync(new Role { RoleId = "col" });
            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "user1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldExcludeCurrentUser()
        {
            var users = new List<User>
        {
            new User { UserId = "user1" },
            new User { UserId = "user2" }
        };

            var adminRole = new Role { RoleId = "admin" };
            var organizerRole = new Role { RoleId = "org" };
            var reviewerRole = new Role { RoleId = "rev" };
            var collabRole = new Role { RoleId = "col" };

            // Mock tất cả 4 role
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription()))
                .ReturnsAsync(reviewerRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription()))
                .ReturnsAsync(collabRole);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(users);

            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "user1");

            Assert.DoesNotContain(result, r => r.UserId == "user1");
            Assert.Contains(result, r => r.UserId == "user2");
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldExcludeSystemRole()
        {
            // 1. Chuẩn bị role
            var adminRoleId = "role-admin";
            var organizerRoleId = "role-organizer";
            var localReviewerRoleId = "role-reviewer";
            var collabRoleId = "role-collab";

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(new Role { RoleId = adminRoleId, RoleName = "Admin" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(new Role { RoleId = organizerRoleId, RoleName = "Conference Organizer" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription()))
                .ReturnsAsync(new Role { RoleId = localReviewerRoleId, RoleName = "Local Reviewer" });
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription()))
                .ReturnsAsync(new Role { RoleId = collabRoleId, RoleName = "Collaborator" });

            // 2. Tạo users (có admin, organizer và user hợp lệ)
            var users = new List<User>
    {
        new User
        {
            UserId = "u1",
            IsActive = true,
            IsEmailConfirmed = true,
            UserRoles = new List<UserRole> { new UserRole { RoleId = adminRoleId, IsActive = true } }
        },
        new User
        {
            UserId = "u2",
            IsActive = true,
            IsEmailConfirmed = true,
            UserRoles = new List<UserRole> { new UserRole { RoleId = organizerRoleId, IsActive = true } }
        },
        new User
        {
            UserId = "u3",
            IsActive = true,
            IsEmailConfirmed = true,
            UserRoles = new List<UserRole>() // hợp lệ
        }
    };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
    .ReturnsAsync(new List<User>
    {
        new User { UserId = "u3", IsActive = true, IsEmailConfirmed = true, UserRoles = new List<UserRole>() }
    });

            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "currentUserId");

            Assert.Single(result);
            Assert.Equal("u3", result[0].UserId);

           
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldExcludeInactiveOrUnconfirmed()
        {
            var adminRole = new Role { RoleId = "admin" };
            var organizerRole = new Role { RoleId = "org" };
            var reviewerRole = new Role { RoleId = "rev" };
            var collabRole = new Role { RoleId = "col" };

            // Mock tất cả 4 role
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription()))
                .ReturnsAsync(reviewerRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription()))
                .ReturnsAsync(collabRole);

            var users = new List<User>
        {
            new User { UserId = "u1", IsActive = false, IsEmailConfirmed = true },
            new User { UserId = "u2", IsActive = true, IsEmailConfirmed = false },
            new User { UserId = "u3", IsActive = true, IsEmailConfirmed = true }
        };

         
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(users);

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
    .ReturnsAsync(users.Where(u => (bool)u.IsActive && (bool)u.IsEmailConfirmed).ToList());

            // Act
            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "currentUserId");

            // Assert
            Assert.Single(result);
            Assert.Equal("u3", result[0].UserId);
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldReturnValidUsers()
        {
            var adminRole = new Role { RoleId = "admin" };
            var organizerRole = new Role { RoleId = "org" };
            var reviewerRole = new Role { RoleId = "rev" };
            var collabRole = new Role { RoleId = "col" };

            // Mock tất cả 4 role
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription()))
                .ReturnsAsync(reviewerRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription()))
                .ReturnsAsync(collabRole);
            var users = new List<User>
        {
            new User { UserId = "u1", IsActive = true, IsEmailConfirmed = true, UserRoles = new List<UserRole>() },
            new User { UserId = "u2", IsActive = true, IsEmailConfirmed = true, UserRoles = new List<UserRole>() }
        };

           
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(users);

            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "userX");

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.UserId == "u1");
            Assert.Contains(result, r => r.UserId == "u2");
        }
    }
}
