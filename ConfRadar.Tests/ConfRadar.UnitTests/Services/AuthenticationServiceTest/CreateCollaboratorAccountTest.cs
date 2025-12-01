using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.User;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationServiceTest
{
    public class CreateCollaboratorAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEmailService> _mockEmail;
        private readonly Mock<ITimeProviderService> _mockTime;
        private readonly Mock<ITokenService> _mockToken;
        private readonly AuthService _authService;

        public CreateCollaboratorAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEmail = new Mock<IEmailService>();
            _mockTime = new Mock<ITimeProviderService>();
            _mockToken = new Mock<ITokenService>();

            var mockHasher = new Mock<IPasswordHasher>();
            var mockStorage = new Mock<IObjectStorageFileService>();
            var mockFirebase = new Mock<IFirebaseAuthService>();

            var jwt = Options.Create(new JwtSettings { SecretKey = "mock", ExpiresRefreshToken = 7 });
            var storageSetting = Options.Create(new ObjectStorageSettings { EndPoint = "mock" });

            _authService = new AuthService(
                mockHasher.Object,
                _mockEmail.Object,
                _mockToken.Object,
                jwt,
                _mockUnitOfWork.Object,
                mockStorage.Object,
                storageSetting,
                mockFirebase.Object,
                _mockTime.Object
            );
        }


        [Fact]
        public async Task ShouldCreateCollaborator_Success()
        {
            var request = new CreateCollaboratorAccountRequest
            {
                Email = "example@mail.com",
                FullName = "John Doe",
                OrganizationName = "Org",
                OrganizationDescription = "Desc"
            };

            _mockTime.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.UtcNow);

            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByName(request.FullName))
                .ReturnsAsync((User?)null);

            _mockToken.Setup(t => t.GenerateSecureRandomToken()).Returns("token123");

            _mockUnitOfWork.Setup(r => r.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role { RoleId = "COLLAB" });

            _mockUnitOfWork.Setup(r => r.UserRepository.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(1);

            var result = await _authService.CreateCollaboratorAccount(request);

            Assert.Equal(1, result);
            _mockEmail.Verify(e =>
                e.SendCreateAccountEmail(
                    request.Email,
                    request.FullName,
                    It.IsAny<string>(),
                    "Tạo tài khoản cho collaborator",
                    "EmailCreateAccount.html"),
                Times.Once);
        }


        [Fact]
        public async Task ShouldThrow_WhenEmailExists()
        {
            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync(new User());

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() =>
                _authService.CreateCollaboratorAccount(new CreateCollaboratorAccountRequest
                {
                    Email = "mail@mail.com",
                    FullName = "A"
                }));
        }


        [Fact]
        public async Task ShouldThrow_WhenFullNameExists()
        {
            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByName(It.IsAny<string>()))
                .ReturnsAsync(new User());

            await Assert.ThrowsAsync<ConfRadarAuthenticationException>(() =>
                _authService.CreateCollaboratorAccount(new CreateCollaboratorAccountRequest
                {
                    Email = "mail@mail.com",
                    FullName = "A"
                }));
        }


        [Fact]
        public async Task ShouldThrow_WhenCollabRoleNotFound()
        {
            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByName(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _mockUnitOfWork.Setup(r => r.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync((Role?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.CreateCollaboratorAccount(new CreateCollaboratorAccountRequest
                {
                    Email = "mail@mail.com",
                    FullName = "A"
                }));
        }
    }

}
