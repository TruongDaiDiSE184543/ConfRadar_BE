using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.User;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationServiceTest
{
    public class SuspendExternalReviewerAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly AuthService _authService;

        public SuspendExternalReviewerAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockEmailService = new Mock<IEmailService>();
            var mockTokenService = new Mock<ITokenService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            var mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            var mockFirebaseAuthService = new Mock<IFirebaseAuthService>();
            var mockTimeProviderService = new Mock<ITimeProviderService>();
            var jwtSettings = Options.Create(new JwtSettings { SecretKey = "mock", ExpiresRefreshToken = 7 });
            var objectStorageSettings = Options.Create(new ObjectStorageSettings { EndPoint = "https://mock" });

            _authService = new AuthService(
                mockPasswordHasher.Object,
                mockEmailService.Object,
                mockTokenService.Object,
                jwtSettings,
                _mockUnitOfWork.Object,
                mockObjectStorageFileService.Object,
                objectStorageSettings,
                mockFirebaseAuthService.Object,
                mockTimeProviderService.Object
            );
        }

        [Fact]
        public async Task ShouldSuspendReviewer_WhenAllValid()
        {
            var role = new Role { RoleId = "r1", RoleName = "External Reviewer" };
            var user = new User { UserId = "u1", FullName = "John", Email = "john@gmail.com" };
            var contracts = new List<ReviewerContract> { new ReviewerContract() };
            var userRole = new UserRole { UserId = "u1", RoleId = "r1", IsActive = true };

            _mockUnitOfWork.Setup(u => u.RoleRepository
                .GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(role);

            _mockUnitOfWork.Setup(u => u.UserRepository
                .GetUserByUserId("u1"))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository
                .GetReviewerContractsByUserIdAsync("u1"))
                .ReturnsAsync(contracts);

            _mockUnitOfWork.Setup(u => u.UserRoleRepository
                .GetUserRoleByUserAndRole("u1", "r1"))
                .ReturnsAsync(userRole);

            _mockUnitOfWork.Setup(u => u.UserRoleRepository
                .UpdateUserRole(userRole))
                .ReturnsAsync(1);
            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
        .ReturnsAsync(DateTime.UtcNow);
            _mockEmailService
    .Setup(e => e.SendSuspendTemplateEmailAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>()
    ))
    .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UserSuspendHistoryRepository
    .CreateSuspensionAsync(It.IsAny<UserSuspendHistory>()))
    .ReturnsAsync(1);


            var user1 = new UserSuspendRequest()
            {
                UserId = "u1",
                Reason = "siu"

            };

            var result = await _authService.SuspendExternalReviewerAccount(user1);

            Assert.True(result > 0);

            Assert.False(userRole.IsActive);
            _mockUnitOfWork.Verify(u => u.UserRoleRepository.UpdateUserRole(userRole), Times.Once);
        }

        [Fact]
        public async Task ShouldThrow_WhenRoleNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository
                .GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync((Role?)null);
            var user1 = new UserSuspendRequest()
            {
                UserId = "u1",
                Reason = "siu"

            };
            await Assert.ThrowsAsync<Exception>(() =>
                _authService.SuspendExternalReviewerAccount(user1));
        }

        [Fact]
        public async Task ShouldThrow_WhenUserNotFound()
        {
            var role = new Role { RoleId = "r1" };
            _mockUnitOfWork.Setup(u => u.RoleRepository
                .GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(role);

            _mockUnitOfWork.Setup(u => u.UserRepository
                .GetUserByUserId("u1"))
                .ReturnsAsync((User?)null);
            var user1 = new UserSuspendRequest()
            {
                UserId = "u1",
                Reason = "siu"

            };
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.SuspendExternalReviewerAccount(user1));
        }

        //[Fact]
        //public async Task ShouldThrow_WhenNoReviewerContracts()
        //{
        //    var role = new Role { RoleId = "r1" };
        //    var user = new User { UserId = "u1", FullName = "John" };

        //    _mockUnitOfWork.Setup(u => u.RoleRepository
        //        .GetRoleByRoleName(It.IsAny<string>()))
        //        .ReturnsAsync(role);

        //    _mockUnitOfWork.Setup(u => u.UserRepository
        //        .GetUserByUserId("u1"))
        //        .ReturnsAsync(user);

        //    _mockUnitOfWork.Setup(u => u.ReviewerContractRepository
        //        .GetReviewerContractsByUserIdAsync("u1"))
        //        .ReturnsAsync(new List<ReviewerContract>());

        //    await Assert.ThrowsAsync<BadRequestException>(() =>
        //        _authService.SuspendExternalReviewerAccount("u1"));
        //}

        [Fact]
        public async Task ShouldThrow_WhenUserRoleNotFound()
        {
            var role = new Role { RoleId = "r1" };
            var user = new User { UserId = "u1" };
            var contracts = new List<ReviewerContract> { new ReviewerContract() };

            _mockUnitOfWork.Setup(u => u.RoleRepository
                .GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(role);

            _mockUnitOfWork.Setup(u => u.UserRepository
                .GetUserByUserId("u1"))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository
                .GetReviewerContractsByUserIdAsync("u1"))
                .ReturnsAsync(contracts);

            _mockUnitOfWork.Setup(u => u.UserRoleRepository
                .GetUserRoleByUserAndRole("u1", "r1"))
                .ReturnsAsync((UserRole?)null);
            var user1 = new UserSuspendRequest()
            {
                UserId = "u1",
                Reason = "siu"

            };
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.SuspendExternalReviewerAccount(user1));
        }

        [Fact]
        public async Task ShouldThrow_WhenUserRoleAlreadyDisabled()
        {
            var role = new Role { RoleId = "r1" };
            var user = new User { UserId = "u1", FullName = "John" };
            var contracts = new List<ReviewerContract> { new ReviewerContract() };
            var userRole = new UserRole { UserId = "u1", RoleId = "r1", IsActive = false };

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(role);

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u1"))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetReviewerContractsByUserIdAsync("u1"))
                .ReturnsAsync(contracts);

            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetUserRoleByUserAndRole("u1", "r1"))
                .ReturnsAsync(userRole);
            var user1 = new UserSuspendRequest()
            {
                UserId = "u1",
                Reason = "siu"

            };
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _authService.SuspendExternalReviewerAccount(user1));
        }
    }

}
