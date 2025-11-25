using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.AuthenticationService
{
    public class SuspendAccountTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly AuthService _authService;

        public SuspendAccountTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEmailService = new Mock<IEmailService>();
            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockTokenService = new Mock<ITokenService>();
            var mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            var mockFirebaseAuth = new Mock<IFirebaseAuthService>();
            var mockTime = new Mock<ITimeProviderService>();

            var jwtSettings = Options.Create(new JwtSettings { SecretKey = "mock123", ExpiresRefreshToken = 7 });
            var objSettings = Options.Create(new ObjectStorageSettings());

            _authService = new AuthService(
                mockPasswordHasher.Object,
                _mockEmailService.Object,
                mockTokenService.Object,
                jwtSettings,
                _mockUnitOfWork.Object,
                mockObjectStorageFileService.Object,
                objSettings,
                mockFirebaseAuth.Object,
                mockTime.Object
            );
        }

        // ========================================================
        // 1. ROLE KHÔNG TỒN TẠI
        // ========================================================
        [Fact]
        public async Task ShouldThrow_WhenAdminOrOrganizerRoleNotFound()
        {
            _mockUnitOfWork.Setup(x => x.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync((Role?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _authService.SuspendAccount("user1"));
        }

        // ========================================================
        // 2. USER KHÔNG TỒN TẠI
        // ========================================================
        [Fact]
        public async Task ShouldThrow_WhenUserNotFound()
        {
            _mockUnitOfWork.Setup(x => x.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role { RoleId = "r1" });

            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<BadRequestException>(() => _authService.SuspendAccount("user1"));
        }

        // ========================================================
        // 3. USER LÀ ADMIN HOẶC ORGANIZER
        // ========================================================
        [Fact]
        public async Task ShouldThrow_WhenUserIsAdminOrOrganizer()
        {
            var adminRole = new Role { RoleId = "admin" };

        }
    }
}
