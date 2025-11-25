using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.User;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationService
{
    public class CreateLocalReviewerAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly AuthService _authService;

        public CreateLocalReviewerAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTokenService = new Mock<ITokenService>();
            var mockObjectStorage = new Mock<IObjectStorageFileService>();
            var mockFirebaseAuth = new Mock<IFirebaseAuthService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            var jwt = Options.Create(new JwtSettings { SecretKey = "mock", ExpiresRefreshToken = 7 });
            var obj = Options.Create(new ObjectStorageSettings { EndPoint = "mock" });

            _authService = new AuthService(
                mockPasswordHasher.Object,
                _mockEmailService.Object,
                _mockTokenService.Object,
                jwt,
                _mockUnitOfWork.Object,
                mockObjectStorage.Object,
                obj,
                mockFirebaseAuth.Object,
                _mockTimeProviderService.Object
            );
        }

        // -----------------------------------------------------

        [Fact]
        public async Task ShouldCreateLocalReviewer_WhenValid()
        {
            var req = new CreateLocalReviewerAccountRequest
            {
                Email = "test@gmail.com",
                FullName = "User A"
            };

            var now = DateTime.UtcNow;
            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(now);

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(req.Email))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(req.FullName))
                .ReturnsAsync((User?)null);

            _mockTokenService.Setup(t => t.GenerateSecureRandomToken())
                .Returns("tokentest");

            var role = new Role
            {
                RoleId = "role-local"
            };

            _mockUnitOfWork.Setup(u => u.RoleRepository
                .GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription()))
                .ReturnsAsync(role);

            _mockUnitOfWork.Setup(u => u.UserRepository.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(1);

            var result = await _authService.CreateLocalReviewerAccount(req);

            Assert.Equal(1, result);
            _mockEmailService.Verify(e => e.SendCreateAccountEmail(
                req.Email,
                req.FullName,
                It.IsAny<string>(),
                "Tạo tài khoản cho local reviewer",
                "EmailChangePassword.html"
            ), Times.Once);
        }

        // -----------------------------------------------------

        [Fact]
        public async Task ShouldThrow_WhenEmailExists()
        {
            var req = new CreateLocalReviewerAccountRequest
            {
                Email = "test@gmail.com",
                FullName = "User A"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(req.Email))
                .ReturnsAsync(new User());

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() =>
                _authService.CreateLocalReviewerAccount(req)
            );
        }

        // -----------------------------------------------------

        [Fact]
        public async Task ShouldThrow_WhenFullNameExists()
        {
            var req = new CreateLocalReviewerAccountRequest
            {
                Email = "test@gmail.com",
                FullName = "User A"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(req.Email))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(req.FullName))
                .ReturnsAsync(new User());

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() =>
                _authService.CreateLocalReviewerAccount(req)
            );
        }

        // -----------------------------------------------------

        [Fact]
        public async Task ShouldThrow_WhenLocalReviewerRoleNotFound()
        {
            var req = new CreateLocalReviewerAccountRequest
            {
                Email = "test@gmail.com",
                FullName = "User A"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(req.Email))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(req.FullName))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(u => u.RoleRepository
                .GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription()))
                .ReturnsAsync((Role?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.CreateLocalReviewerAccount(req)
            );
        }
    }

}
