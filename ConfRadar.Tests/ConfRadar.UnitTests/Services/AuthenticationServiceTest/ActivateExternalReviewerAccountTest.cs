using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationServiceTest
{
    public class ActivateExternalReviewerAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AuthService _authService;

        public ActivateExternalReviewerAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockEmailService = new Mock<IEmailService>();
            var mockTokenService = new Mock<ITokenService>();
            var mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            var mockFirebaseAuthService = new Mock<IFirebaseAuthService>();
            var mockTimeProviderService = new Mock<ITimeProviderService>();
            var jwtSettings = Options.Create(new JwtSettings { SecretKey = "mock", ExpiresRefreshToken = 7 });
            var objectStorageSettings = Options.Create(new ObjectStorageSettings { EndPoint = "https://mockstorage.com" });

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

        // ---------------------------------------
        // SUCCESS CASE
        // ---------------------------------------
        [Fact]
        public async Task ShouldActivateExternalReviewer_WhenEverythingValid()
        {
            var role = new Role { RoleId = "role1", RoleName = "External Reviewer" };
            var user = new User { UserId = "u1", FullName = "John", };
            var contractList = new List<ReviewerContract> { new ReviewerContract() };
            var userRole = new UserRole { UserId = "u1", RoleId = "role1", IsActive = false };

            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(role);

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u1"))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetReviewerContractsByUserIdAsync("u1"))
                .ReturnsAsync(contractList);

            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetUserRoleByUserAndRole("u1", "role1"))
                .ReturnsAsync(userRole);

            _mockUnitOfWork.Setup(u => u.UserRoleRepository.UpdateUserRole(userRole))
                .ReturnsAsync(1);

            var result = await _authService.ActivateExternalReviewerAccount("u1");

            Assert.Equal(1, result);
            Assert.True(userRole.IsActive);
            _mockUnitOfWork.Verify(u => u.UserRoleRepository.UpdateUserRole(userRole), Times.Once);
        }

        // ---------------------------------------
        // ERROR CASES
        // ---------------------------------------

        [Fact]
        public async Task ShouldThrow_WhenRoleNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync((Role?)null);

            await Assert.ThrowsAsync<Exception>(() =>
                _authService.ActivateExternalReviewerAccount("u1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenUserNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role());

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u1"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.ActivateExternalReviewerAccount("u1"));
        }

        //[Fact]
        //public async Task ShouldThrow_WhenNoReviewerContracts()
        //{
        //    _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
        //        .ReturnsAsync(new Role());

        //    _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u1"))
        //        .ReturnsAsync(new User { UserId = "u1", FullName = "John" });

        //    _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetReviewerContractsByUserIdAsync("u1"))
        //        .ReturnsAsync(new List<ReviewerContract>()); // empty

        //    await Assert.ThrowsAsync<BadRequestException>(() =>
        //        _authService.ActivateExternalReviewerAccount("u1"));
        //}

        [Fact]
        public async Task ShouldThrow_WhenUserRoleNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role { RoleId = "r1" });

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u1"))
                .ReturnsAsync(new User { UserId = "u1", FullName = "John" });

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetReviewerContractsByUserIdAsync("u1"))
                .ReturnsAsync(new List<ReviewerContract> { new ReviewerContract() });

            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetUserRoleByUserAndRole("u1", "r1"))
                .ReturnsAsync((UserRole?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.ActivateExternalReviewerAccount("u1"));
        }

        [Fact]
        public async Task ShouldThrow_WhenRoleAlreadyActive()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role { RoleId = "r1" });

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u1"))
                .ReturnsAsync(new User { UserId = "u1", FullName = "John" });

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetReviewerContractsByUserIdAsync("u1"))
                .ReturnsAsync(new List<ReviewerContract> { new ReviewerContract() });

            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetUserRoleByUserAndRole("u1", "r1"))
                .ReturnsAsync(new UserRole { UserId = "u1", RoleId = "r1", IsActive = true });

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _authService.ActivateExternalReviewerAccount("u1"));
        }
    }

}
