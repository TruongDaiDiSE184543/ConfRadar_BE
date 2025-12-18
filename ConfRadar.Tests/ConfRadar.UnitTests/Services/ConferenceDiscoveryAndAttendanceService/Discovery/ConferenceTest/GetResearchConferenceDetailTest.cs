using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.ConferenceTest
{
    public class GetResearchConferenceDetailTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ConferenceService _service;

        public GetResearchConferenceDetailTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new ConferenceService(
                _mockUnitOfWork.Object,
                Mock.Of<IConferenceStatusService>(),
                Mock.Of<IConferenceTimelineService>(),
                Mock.Of<IObjectStorageFileService>(),
                Mock.Of<ITokenService>(),
                Mock.Of<ISystemConfigurationService>(),
                Options.Create(new AppSettingConfig.ObjectStorageSettings()),
                Mock.Of<ITimeProviderService>(),
                Mock.Of<INotificationService>()
            );
        }

        [Fact]
        public async Task GetResearchConferenceDetail_ConferenceNotFound_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(x => x.ConferenceRepository.GetResearchIncludedById("conf1"))
                .ReturnsAsync((Conference)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetResearchConferenceDetailAsync("conf1", null));

            Assert.Contains("không tìm thấy", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetResearchConferenceDetail_InvalidStatus_ThrowsBadRequestException()
        {
            var conference = new Conference
            {
                ConferenceId = "conf1",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusName = "Draft" },
                IsResearchConference = true
            };

            _mockUnitOfWork.Setup(x => x.ConferenceRepository.GetResearchIncludedById("conf1"))
                .ReturnsAsync(conference);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.GetResearchConferenceDetailAsync("conf1", null));

            Assert.Contains("không khả dụng", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetResearchConferenceDetail_NotResearchConference_ThrowsException()
        {
            var conference = new Conference
            {
                ConferenceId = "conf1",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusName = ConferenceStatusEnum.Ready.GetDescription() },
                IsResearchConference = false
            };

            _mockUnitOfWork.Setup(x => x.ConferenceRepository.GetResearchIncludedById("conf1"))
                .ReturnsAsync(conference);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetResearchConferenceDetailAsync("conf1", null));

            Assert.Contains("chức năng chỉ dành cho research", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetResearchConferenceDetail_ValidConference_ReturnsResponse()
        {
            var conference = new Conference
            {
                ConferenceId = "conf1",
                ConferenceName = "Conf 1",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusName = ConferenceStatusEnum.Ready.GetDescription() },
                IsResearchConference = true
            };

            _mockUnitOfWork.Setup(x => x.ConferenceRepository.GetResearchIncludedById("conf1"))
                .ReturnsAsync(conference);

            var response = await _service.GetResearchConferenceDetailAsync("conf1", null);

            Assert.NotNull(response);
            Assert.Equal("conf1", response.ConferenceId);
            Assert.Equal("Conf 1", response.ConferenceName);
        }
    }
}