using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.User;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationService
{
    public class RegisterAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<IFirebaseAuthService> _mockFirebaseAuthService;
        private readonly AuthService _authService;

        private readonly ObjectStorageSettings _objectStorageSettings = new() { EndPoint = "https://mockstorage.com/" };
        private readonly JwtSettings _jwtSettings = new() { SecretKey = "mocksecret" };

        public RegisterAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockFirebaseAuthService = new Mock<IFirebaseAuthService>();

            _authService = new AuthService(
                _mockPasswordHasher.Object,
                _mockEmailService.Object,
                _mockTokenService.Object,
                Options.Create(_jwtSettings),
                _mockUnitOfWork.Object,
                _mockObjectStorageFileService.Object,
                Options.Create(_objectStorageSettings),
                _mockFirebaseAuthService.Object
            );
        }

        [Fact]
        public async Task ShouldThrow_WhenEmailExists()
        {
            var request = new CreateUserRequest { Email = "test@test.com", FullName = "John Doe", Password = "123456" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(new User());
            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() => _authService.RegisterAccount(request));
        }

        [Fact]
        public async Task ShouldThrow_WhenFullNameExists()
        {
            var request = new CreateUserRequest { Email = "test@test.com", FullName = "John Doe", Password = "123456" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(It.IsAny<string>())).ReturnsAsync(new User());
            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() => _authService.RegisterAccount(request));
        }

        [Fact]
        public async Task ShouldThrow_WhenAvatarContentTypeNull()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.ContentType).Returns((string)null);
            var request = new CreateUserRequest
            {
                Email = "test@test.com",
                FullName = "John Doe",
                Password = "123456",
                AvatarFile = mockFile.Object
            };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(It.IsAny<string>())).ReturnsAsync((User?)null);
            await Assert.ThrowsAsync<BadRequestException>(() => _authService.RegisterAccount(request));
        }

        [Fact]
        public async Task ShouldThrow_WhenAvatarInvalidContentType()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            var request = new CreateUserRequest
            {
                Email = "test@test.com",
                FullName = "John Doe",
                Password = "123456",
                AvatarFile = mockFile.Object
            };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(It.IsAny<string>())).ReturnsAsync((User?)null);
            await Assert.ThrowsAsync<BadRequestException>(() => _authService.RegisterAccount(request));
        }

        [Fact]
        public async Task ShouldCreateUser_WithAvatar()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("avatar.png");
            mockFile.Setup(f => f.ContentType).Returns("image/png");
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var request = new CreateUserRequest
            {
                Email = "test@test.com",
                FullName = "John Doe",
                Password = "123456",
                AvatarFile = mockFile.Object
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>())).ReturnsAsync(new Role { RoleId = Guid.NewGuid().ToString() });
            _mockObjectStorageFileService.Setup(o => o.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("avatar/avatar.png");
            _mockTokenService.Setup(t => t.GenerateSecureRandomToken()).Returns("token123");
            _mockPasswordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashedpassword");
            _mockUnitOfWork.Setup(u => u.UserRepository.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(1);

            var result = await _authService.RegisterAccount(request);

            Assert.Equal(1, result);
            _mockEmailService.Verify(e => e.SendAuthenticationTemplateEmailAsync(
                request.Email,
                request.FullName,
                It.Is<string>(s => s.Contains("token123")),
                "Confirm Email Registration",
                "EmailRegistrationConfirmation.html"
            ), Times.Once);
        }

        [Fact]
        public async Task ShouldCreateUser_WithoutAvatar()
        {
            var request = new CreateUserRequest
            {
                Email = "test@test.com",
                FullName = "John Doe",
                Password = "123456"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByName(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>())).ReturnsAsync(new Role { RoleId = Guid.NewGuid().ToString() });
            _mockTokenService.Setup(t => t.GenerateSecureRandomToken()).Returns("token123");
            _mockPasswordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashedpassword");
            _mockUnitOfWork.Setup(u => u.UserRepository.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(1);

            var result = await _authService.RegisterAccount(request);

            Assert.Equal(1, result);
            _mockEmailService.Verify(e => e.SendAuthenticationTemplateEmailAsync(
                request.Email,
                request.FullName,
                It.Is<string>(s => s.Contains("token123")),
                "Confirm Email Registration",
                "EmailRegistrationConfirmation.html"
            ), Times.Once);
        }
    }
}
