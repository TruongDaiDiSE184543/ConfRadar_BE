using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Abstract;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.AbstractPaper
{
    public class ListPendingAbstractTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly PaperService _paperService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;

        public ListPendingAbstractTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockNotificationService = new Mock<INotificationService>();
            _objectStorageSettings = Options.Create(new ObjectStorageSettings { EndPoint = "https://mockstorage.com" });

            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                Mock.Of<IMomoService>(),
                _mockTokenService.Object,
                _objectStorageSettings,
                _mockObjectStorageFileService.Object,
                Mock.Of<ITicketService>(),
                _mockTimeProviderService.Object,
                _mockNotificationService.Object,
                Mock.Of<IConferenceStepService>()
            );
        }

        [Fact]
        public async Task GetListPendingAbstract_ShouldReturnList_WhenPendingExists()
        {
            // Arrange
            var pendingStatus = new GlobalStatus { GlobalStatusId = "status1", Name = "Pending" };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingStatus);

            var pendingAbstracts = new List<PendingAbstractResponse>
    {
        new PendingAbstractResponse { AbstractId = "abs1", PaperId = "p1", ConferenceId = "conf1", PresenterId = "user1", PresenterName = "Alice" }
    };
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAllPendingAbstractsAsync("status1"))
                .ReturnsAsync(pendingAbstracts);


            // Act
            var result = await _paperService.GetListPendingAbstract("conf1");

            // Assert
            Assert.Single(result);
            Assert.Equal("abs1", result[0].AbstractId);
            Assert.Equal("Alice", result[0].PresenterName);
        }
        [Fact]
        public async Task GetListPendingAbstract_ShouldReturnFiltered_WhenConfIdProvided()
        {
            // Arrange
            var pendingStatus = new GlobalStatus { GlobalStatusId = "status1", Name = "Pending" };
            var abstracts = new List<PendingAbstractResponse>
        {
            new PendingAbstractResponse { AbstractId = "a1", ConferenceId = "conf1" },
            new PendingAbstractResponse { AbstractId = "a2", ConferenceId = "conf2" }
        };

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(pendingStatus);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAllPendingAbstractsAsync(pendingStatus.GlobalStatusId))
                .ReturnsAsync(abstracts);

            // Act
            var result = await _paperService.GetListPendingAbstract("conf1");

            // Assert
            Assert.Single(result);
            Assert.All(result, r => Assert.Equal("conf1", r.ConferenceId));
        }

        [Fact]
        public async Task GetListPendingAbstract_ShouldReturnAll_WhenConfIdNotProvided()
        {
            // Arrange
            var pendingStatus = new GlobalStatus { GlobalStatusId = "status1", Name = "Pending" };
            var abstracts = new List<PendingAbstractResponse>
        {
            new PendingAbstractResponse { AbstractId = "a1", ConferenceId = "conf1" },
            new PendingAbstractResponse { AbstractId = "a2", ConferenceId = "conf2" }
        };

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(pendingStatus);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAllPendingAbstractsAsync(pendingStatus.GlobalStatusId))
                .ReturnsAsync(abstracts);

            // Act
            var result = await _paperService.GetListPendingAbstract(null);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetListPendingAbstract_ShouldReturnEmpty_WhenNoPendingAbstracts()
        {
            // Arrange
            var pendingStatus = new GlobalStatus { GlobalStatusId = "status1", Name = "Pending" };
            var abstracts = new List<PendingAbstractResponse>();

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(pendingStatus);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAllPendingAbstractsAsync(pendingStatus.GlobalStatusId))
                .ReturnsAsync(abstracts);

            // Act
            var result = await _paperService.GetListPendingAbstract(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetListPendingAbstract_ShouldThrowNotFound_WhenPendingStatusNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync((GlobalStatus)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.GetListPendingAbstract(null));
        }

    }
}
