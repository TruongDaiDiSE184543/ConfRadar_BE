using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationServiceTest
{
    public class ForgetPasswordTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;

        private readonly AuthService _authService;

        public ForgetPasswordTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockFirebaseAuth = new Mock<IFirebaseAuthService>();
            var mockObjectStorage = new Mock<IObjectStorageFileService>();

            var jwtSettings = Options.Create(new JwtSettings { SecretKey = "mock", ExpiresRefreshToken = 7 });
            var objectStorageSettings = Options.Create(new ObjectStorageSettings());

            _authService = new AuthService(
                mockPasswordHasher.Object,
                _mockEmailService.Object,
                _mockTokenService.Object,
                jwtSettings,
                _mockUnitOfWork.Object,
                mockObjectStorage.Object,
                objectStorageSettings,
                mockFirebaseAuth.Object,
                _mockTimeProviderService.Object
            );
        }

        // ---------------------------------------------------------
        // CASE 1: USER NOT FOUND
        // ---------------------------------------------------------
        [Fact]
        public async Task ForgetPassword_ShouldThrow_WhenUserNotFound()
        {
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail("test@gmail.com"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.ForgetPassword("test@gmail.com")
            );
        }

        // ---------------------------------------------------------
        // CASE 2: SUCCESS
        // ---------------------------------------------------------
        //[Fact]
        //public async Task ForgetPassword_ShouldUpdateUserAndSendEmail_WhenUserExists()
        //{
        //    var user = new User
        //    {
        //        UserId = "u1",
        //        Email = "test@gmail.com",
        //        FullName = "Test User"
        //    };

        //    var mockToken = "abc123";
        //    var mockVietnamTime = DateTime.UtcNow;

        //    _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail("test@gmail.com"))
        //        .ReturnsAsync(user);

        //    _mockTokenService.Setup(t => t.GenerateSecureRandomToken())
        //        .Returns(mockToken);

        //    _mockTimeProviderService.Setup(t => t.GetVietnamTime())
        //        .ReturnsAsync(mockVietnamTime);

        //    _mockUnitOfWork.Setup(u => u.UserRepository.UpdateUserAsync(It.IsAny<User>()))
        //        .ReturnsAsync(1);

        //    _mockEmailService
        //        .Setup(e => e.SendAuthenticationTemplateEmailAsync(
        //            It.IsAny<string>(),
        //            It.IsAny<string>(),
        //            It.IsAny<string>(),
        //            "Forget Password",
        //            "EmailForgetPassword.html"))
        //        .Returns(Task.CompletedTask);

        //    // ACT
        //    await _authService.ForgetPassword("test@gmail.com");

        //    // ASSERT
        //    Assert.Equal(mockToken, user.PasswordResetToken);
        //    Assert.Equal(mockVietnamTime, user.PasswordResetTokenExpiry);

        //    _mockUnitOfWork.Verify(u => u.UserRepository.UpdateUserAsync(user), Times.Once);
        //    _mockEmailService.Verify(e =>
        //        e.SendAuthenticationTemplateEmailAsync(
        //            "test@gmail.com",
        //            user.FullName,
        //            It.Is<string>(link => link.Contains(mockToken)),
        //            "Forget Password",
        //            "EmailForgetPassword.html"
        //        ), Times.Once);
        //}
    }

}
