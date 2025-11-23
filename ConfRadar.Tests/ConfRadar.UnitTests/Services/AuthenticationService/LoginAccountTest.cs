using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.User;
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

namespace ConfRadar.UnitTests.Services.AuthenticationService
{
    public class LoginAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<IFirebaseAuthService> _mockFirebaseAuthService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;

        private readonly AuthService _authService;

        private readonly ObjectStorageSettings _objectStorageSettings = new() { EndPoint = "https://mockstorage.com/" };
        private readonly JwtSettings _jwtSettings = new() { SecretKey = "mocksecret", ExpiresRefreshToken = 7 };

        public LoginAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockFirebaseAuthService = new Mock<IFirebaseAuthService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            _authService = new AuthService(
                _mockPasswordHasher.Object,
                _mockEmailService.Object,
                _mockTokenService.Object,
                Options.Create(_jwtSettings),
                _mockUnitOfWork.Object,
                _mockObjectStorageFileService.Object,
                Options.Create(_objectStorageSettings),
                _mockFirebaseAuthService.Object,
                _mockTimeProviderService.Object
            );
        }

        // ============================================
        // 1. USER NOT FOUND
        // ============================================
        [Fact]
        public async Task ShouldThrow_WhenUserNotFound()
        {
            var request = new LocalLoginUserRequest { Email = "a@a.com", Password = "123" };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() => _authService.LocalLogin(request));
        }

        // ============================================
        // 2. EMAIL NOT CONFIRMED
        // ============================================
        [Fact]
        public async Task ShouldThrow_WhenEmailNotConfirmed()
        {
            var request = new LocalLoginUserRequest { Email = "a@a.com", Password = "123" };

            var user = new User { IsEmailConfirmed = false };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync(user);

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() => _authService.LocalLogin(request));
        }

        // ============================================
        // 3. WRONG LOGIN PROVIDER
        // ============================================
        [Fact]
        public async Task ShouldThrow_WhenLoginProviderIsNotLocal()
        {
            var request = new LocalLoginUserRequest { Email = "a@a.com", Password = "123" };

            var user = new User
            {
                IsEmailConfirmed = true,
                LoginProvider = "Google"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync(user);

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() => _authService.LocalLogin(request));
        }

        // ============================================
        // 4. WRONG PASSWORD
        // ============================================
        [Fact]
        public async Task ShouldThrow_WhenPasswordInvalid()
        {
            var request = new LocalLoginUserRequest { Email = "a@a.com", Password = "invalid" };

            var user = new User
            {
                IsEmailConfirmed = true,
                LoginProvider = "Local",
                PasswordHash = "hashed"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() => _authService.LocalLogin(request));
        }

        // ============================================
        // 5. LOGIN SUCCESSFULLY
        // ============================================
        [Fact]
        public async Task ShouldLoginSuccessfully()
        {
            var request = new LocalLoginUserRequest
            {
                Email = "test@test.com",
                Password = "123456",
                FirebaseMobileFcmToken = "mobileFcm",
                FirebaseWebFcmToken = "webFcm"
            };

            var user = new User
            {
                UserId = "u123",
                Email = request.Email,
                PasswordHash = "hashed",
                IsEmailConfirmed = true,
                IsActive = true,
                LoginProvider = "Local"
            };

            var now = DateTime.UtcNow;

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(request.Email))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(p => p.Verify(request.Password, "hashed"))
                .Returns(true);

            _mockTokenService.Setup(t => t.GenerateAccessToken(user.UserId, user.Email, true))
                .ReturnsAsync("access123");

            _mockTokenService.Setup(t => t.GenerateSecureRandomToken())
                .Returns("refresh123");

            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(now);

            _mockUnitOfWork.Setup(u => u.UserRepository.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);


            _mockUnitOfWork.Setup(u => u.UserRefreshTokenRepository.CreateUserRefreshToken(It.IsAny<UserRefreshToken>()))
                .ReturnsAsync(1);

            var result = await _authService.LocalLogin(request);

            Assert.Equal("access123", result.AccessToken);
            Assert.Equal("refresh123", result.RefreshToken);
            Assert.Equal("mobileFcm", user.FirebaseMobileFcmToken);
            Assert.Equal("webFcm", user.FirebaseWebFcmToken);

            _mockUnitOfWork.Verify(u => u.UserRepository.UpdateUserAsync(It.IsAny<User>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.UserRefreshTokenRepository.CreateUserRefreshToken(It.IsAny<UserRefreshToken>()), Times.Once);
        }
    }

}
