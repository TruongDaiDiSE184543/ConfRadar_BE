using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.ReviewContract;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.TestContractService
{
    public class ContractServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IEmailService> _mockEmailService;

        private readonly ContractService _contractService;

        private readonly ObjectStorageSettings _objectStorageSettings = new() { EndPoint = "https://mockstorage.com/" };

        public ContractServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockEmailService = new Mock<IEmailService>();

            _contractService = new ContractService(
                _mockUnitOfWork.Object,
                _mockTokenService.Object,
                Options.Create(_objectStorageSettings),
                _mockObjectStorageFileService.Object,
                _mockTimeProviderService.Object,
                _mockPasswordHasher.Object,
                _mockEmailService.Object
            );
        }

        [Fact]
        public async Task ShouldThrow_WhenExternalReviewerRoleNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync((Role?)null);

            var request = new CreateReviewerContractRequest { ConferenceId = "conf1", ReviewerId = "user1" };
            await Assert.ThrowsAsync<NotFoundException>(() => _contractService.CreateReviewerContract(request));
        }

        [Fact]
        public async Task ShouldThrow_WhenConferenceNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role { RoleId = "role1" });

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Conference?)null);

            var request = new CreateReviewerContractRequest { ConferenceId = "conf1", ReviewerId = "user1" };
            await Assert.ThrowsAsync<NotFoundException>(() => _contractService.CreateReviewerContract(request));
        }

        [Fact]
        public async Task ShouldThrow_WhenReviewerNotFound()
        {
            _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(It.IsAny<string>()))
                .ReturnsAsync(new Role { RoleId = "role1" });

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference { ConferenceId = "conf1", EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) });

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var request = new CreateReviewerContractRequest { ConferenceId = "conf1", ReviewerId = "user1" };
            await Assert.ThrowsAsync<NotFoundException>(() => _contractService.CreateReviewerContract(request));
        }

        //[Fact]
        //public async Task ShouldCreateReviewerContract_WhenValidRequest()
        //{
        //    var now = DateOnly.FromDateTime(DateTime.Now);
        //    var timeNow = DateTime.Now;

        //    var reviewer = new User { UserId = "user1", FullName = "Reviewer 1" };
        //    var conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf 1", EndDate = now.AddDays(1) };
        //    var externalRole = new Role { RoleId = "role1" };

        //    _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(now);
        //    _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(timeNow);

        //    _mockUnitOfWork.Setup(u => u.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ExternalReviewer.GetDescription()))
        //        .ReturnsAsync(externalRole);
        //    _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf1"))
        //        .ReturnsAsync(conference);
        //    _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("user1"))
        //        .ReturnsAsync(reviewer);
        //    _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("user1", "conf1"))
        //        .ReturnsAsync((ReviewerContract?)null);

        //    _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.CreateReviewerContractAsync(It.IsAny<ReviewerContract>()))
        //        .ReturnsAsync(1);

        //    var request = new CreateReviewerContractRequest { ConferenceId = "conf1", ReviewerId = "user1", SignDay = now, Wage = 100 };

        //    var result = await _contractService.CreateReviewerContract(request);

        //    Assert.Equal(1, result);
        //    _mockUnitOfWork.Verify(u => u.ReviewerContractRepository.CreateReviewerContractAsync(It.IsAny<ReviewerContract>()), Times.Once);
        //}
    }

}
