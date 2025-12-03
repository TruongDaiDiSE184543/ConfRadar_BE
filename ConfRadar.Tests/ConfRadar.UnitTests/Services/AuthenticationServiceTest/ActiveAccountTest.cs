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
    public class ActivateAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AuthService _authService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;

        public ActivateAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockEmailService = new Mock<IEmailService>();
            var mockTokenService = new Mock<ITokenService>();
            var mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            var mockFirebaseAuthService = new Mock<IFirebaseAuthService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
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
                _mockTimeProviderService.Object
            );
        }

        [Fact]
        public async Task ShouldActivateAccount_WhenUserExists()
        {
            var user = new User { UserId = "user1", IsActive = false };
            var now = DateTime.UtcNow;
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("user1"))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(u => u.UserRepository.UpdateUserAsync(user))
                .ReturnsAsync(1);
            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
            .ReturnsAsync(now);
            _mockUnitOfWork.Setup(u => u.UserSuspendHistoryRepository
        .GetCurrentUserSuspendHistoryByUser("user1"))
        .ReturnsAsync(new List<UserSuspendHistory>());

            var user1 = new UserActiveAccountRequest()
            {
                UserId = "user1",

            };
            var result = await _authService.ActivateAccount(user1);

            Assert.Equal(1, result);
            Assert.True(user.IsActive);
            //Assert.Null(user.CurrentSuspendReason);
            //Assert.Null(user.CurrentSuspendedAt);
            _mockUnitOfWork.Verify(u => u.UserRepository.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task ShouldThrow_WhenUserNotFound()
        {
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("user1"))
                .ReturnsAsync((User?)null);
            var user1 = new UserActiveAccountRequest()
            {
                UserId = "user1",

            };
            await Assert.ThrowsAsync<BadRequestException>(() => _authService.ActivateAccount(user1));
        }
    }

}
