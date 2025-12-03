using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(new List<User>());

            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "user1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldExcludeCurrentUser()
        {
            var adminRole = new Role { RoleId = "admin" };
            var organizerRole = new Role { RoleId = "org" };
            var users = new List<User>
        {
            new User { UserId = "user1" },
            new User { UserId = "user2" }
        };

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(users);

            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "user1");

            Assert.DoesNotContain(result, r => r.UserId == "user1");
            Assert.Contains(result, r => r.UserId == "user2");
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldExcludeAdminOrganizer()
        {
            var adminRoleId = "role-admin";
            var organizerRoleId = "role-organizer";

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
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
    .ReturnsAsync(new Role { RoleId = adminRoleId, RoleName = "Admin" });

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(new Role { RoleId = organizerRoleId, RoleName = "Conference Organizer" });

            // Mock repo: chỉ trả users không có role admin/organizer
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(users.Where(u => !u.UserRoles.Any(ur => ur.RoleId == adminRoleId || ur.RoleId == organizerRoleId)).ToList());

            // Act
            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "currentUserId");

            // Assert
            Assert.DoesNotContain(result, u => u.UserId == "u1");
            Assert.DoesNotContain(result, u => u.UserId == "u2");
            Assert.Contains(result, u => u.UserId == "u3");
        }

        [Fact]
        public async Task GetAvailableCoAuthor_ShouldExcludeInactiveOrUnconfirmed()
        {
            var adminRole = new Role { RoleId = "admin" };
            var organizerRole = new Role { RoleId = "org" };
            var users = new List<User>
        {
            new User { UserId = "u1", IsActive = false, IsEmailConfirmed = true },
            new User { UserId = "u2", IsActive = true, IsEmailConfirmed = false },
            new User { UserId = "u3", IsActive = true, IsEmailConfirmed = true }
        };

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
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
            var users = new List<User>
        {
            new User { UserId = "u1", IsActive = true, IsEmailConfirmed = true, UserRoles = new List<UserRole>() },
            new User { UserId = "u2", IsActive = true, IsEmailConfirmed = true, UserRoles = new List<UserRole>() }
        };

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription()))
                .ReturnsAsync(adminRole);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription()))
                .ReturnsAsync(organizerRole);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetAvailableCoAuthorForInclude("conf1", It.IsAny<List<string>>()))
                .ReturnsAsync(users);

            var result = await _paperService.GetAvailableCoAuthorForInclude("conf1", "userX");

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.UserId == "u1");
            Assert.Contains(result, r => r.UserId == "u2");
        }
    }
}
