using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationService
{
    public class ChangePasswordTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly AuthService _authService;

        private readonly JwtSettings _jwtSettings = new() { SecretKey = "mocksecret", ExpiresRefreshToken = 7 };
        private readonly ObjectStorageSettings _objectStorageSettings = new() { EndPoint = "https://mockstorage.com/" };

        public ChangePasswordTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();

            _authService = new AuthService(
                _mockPasswordHasher.Object,
                Mock.Of<IEmailService>(),
                Mock.Of<ITokenService>(),
                Options.Create(_jwtSettings),
                _mockUnitOfWork.Object,
                Mock.Of<IObjectStorageFileService>(),
                Options.Create(_objectStorageSettings),
                Mock.Of<IFirebaseAuthService>(),
                Mock.Of<ITimeProviderService>()
            );
        }

        [Fact]
        public async Task ShouldThrow_WhenUserNotFound()
        {
            string userId = "user123";
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(userId))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.ChangePassword("oldPass", "newPass", userId));
        }

        [Fact]
        public async Task ShouldThrow_WhenOldPasswordIsInvalid()
        {
            string userId = "user123";
            var user = new User { PasswordHash = "hashedPassword" };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(userId))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(h => h.Verify("wrongOldPass", user.PasswordHash))
                .Returns(false);

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() =>
                _authService.ChangePassword("wrongOldPass", "newPass", userId));
        }

        [Fact]
        public async Task ShouldUpdatePassword_WhenOldPasswordIsValid()
        {
            string userId = "user123";
            var user = new User { PasswordHash = "hashedOldPassword" };
            string newPassword = "newPass";
            string hashedNewPassword = "hashedNewPassword";

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(userId))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(h => h.Verify("oldPass", user.PasswordHash))
                .Returns(true);

            _mockPasswordHasher.Setup(h => h.Hash(newPassword))
                .Returns(hashedNewPassword);

            _mockUnitOfWork.Setup(u => u.UserRepository.UpdateUserAsync(user))
     .ReturnsAsync(1) // trả về int 1 để giả lập update thành công
     .Verifiable();

            await _authService.ChangePassword("oldPass", newPassword, userId);

            Assert.Equal(hashedNewPassword, user.PasswordHash);
            _mockUnitOfWork.Verify(u => u.UserRepository.UpdateUserAsync(user), Times.Once);
        }
    }

}
