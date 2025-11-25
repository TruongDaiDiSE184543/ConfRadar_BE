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
    public class UpdateProfileTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly ObjectStorageSettings _objectStorageSettings;

        private readonly AuthService _authService;

        public UpdateProfileTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _objectStorageSettings = new ObjectStorageSettings { EndPoint = "https://mockstorage.com/" };

            _authService = new AuthService(
                passwordHasher: Mock.Of<IPasswordHasher>(),
                emailService: Mock.Of<IEmailService>(),
                tokenService: _mockTokenService.Object,
                jwtSettings: Options.Create(new JwtSettings { SecretKey = "mocksecret" }),
                unitOfWork: _mockUnitOfWork.Object,
                objectStorageFileService: _mockObjectStorageFileService.Object,
                objectStorageSettings: Options.Create(_objectStorageSettings),
                firebaseAuthService: Mock.Of<IFirebaseAuthService>(),
                timeProviderService: Mock.Of<ITimeProviderService>()
            );
        }

        [Fact]
        public async Task ShouldThrow_WhenUserNotFound()
        {
            var request = new ProfileUpdateRequest { FullName = "Test" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() => _authService.UpdateProfile(request, "user123"));
        }

        [Fact]
        public async Task ShouldUpdateProfile_WhenValidRequestWithoutAvatar()
        {
            var request = new ProfileUpdateRequest
            {
                FullName = "New Name",
                BioDescription = "Bio",
                BirthDay = DateOnly.Parse("1990-01-01"),
                PhoneNumber = "0123456789"
            };

            var user = new User { UserId = "user123" };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("user123"))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(u => u.UserRepository.UpdateUserAsync(user))
                .ReturnsAsync(1)
                .Verifiable();

            var result = await _authService.UpdateProfile(request, "user123");

            Assert.Equal(1, result);
            Assert.Equal("New Name", user.FullName);
            Assert.Equal("Bio", user.BioDescription);
            Assert.Equal(DateOnly.Parse("1990-01-01"), user.BirthDay);
            Assert.Equal("0123456789", user.PhoneNumber);
            _mockUnitOfWork.Verify(u => u.UserRepository.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task ShouldUploadAvatarAndUpdateProfile_WhenAvatarProvided()
        {
            var avatarMock = new Mock<IFormFile>();
            avatarMock.Setup(f => f.ContentType).Returns("image/png");
            avatarMock.Setup(f => f.FileName).Returns("avatar.png");
            avatarMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            var request = new ProfileUpdateRequest
            {
                FullName = "New Name",
                AvatarFile = avatarMock.Object
            };

            var user = new User { UserId = "user123" };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("user123"))
                .ReturnsAsync(user);

            _mockTokenService.Setup(t => t.GenerateSecureRandomToken())
                .Returns("randomToken");

            _mockObjectStorageFileService.Setup(o => o.UploadFileAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("uploaded/avatar.png");

            _mockUnitOfWork.Setup(u => u.UserRepository.UpdateUserAsync(user))
                .ReturnsAsync(1)
                .Verifiable();

            var result = await _authService.UpdateProfile(request, "user123");

            Assert.Equal(1, result);
            Assert.Equal("https://mockstorage.com/uploaded/avatar.png", user.AvatarUrl);
            _mockUnitOfWork.Verify(u => u.UserRepository.UpdateUserAsync(user), Times.Once);
        }
    }
}
