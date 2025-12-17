using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.ConferenceTest
{
    public class GetTechnicalConferenceDetailTest
    {
        private readonly Mock<IUnitOfWork> uow;
        private readonly ConferenceService _conferenceService;

        public GetTechnicalConferenceDetailTest()
        {
            uow = new Mock<IUnitOfWork>();
            _conferenceService = new ConferenceService(
                uow.Object,
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

        private Conference CreateConference(string conferenceId, string status = "Ready", bool isResearch = false)
        {
            return new Conference
            {
                ConferenceId = conferenceId,
                ConferenceName = "Test Conference",
                Description = "Test Desc",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                ConferenceStatus = new ConferenceStatus
                {
                    ConferenceStatusId = "status1",
                    ConferenceStatusName = status
                },
                IsResearchConference = isResearch,
                CreatedByNavigation = new User { FullName = "Creator Name" },
                TechnicalConferenceDetail = new TechnicalConferenceDetail
                {
                    ConferenceId = conferenceId,
                    TargetAudience = "Developers"
                },
                ConferenceSessions = new List<ConferenceSession>
                {
                    new ConferenceSession
                    {
                        ConferenceSessionId = "s1",
                        SessionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        StartTime = DateTime.UtcNow.Date.AddHours(9)
                    },
                    new ConferenceSession
                    {
                        ConferenceSessionId = "s2",
                        SessionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        StartTime = DateTime.UtcNow.Date.AddHours(14)
                    }
                }
            };
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ValidConference_ReturnsDetail()
        { // Arrange
            var conferenceId = "conf1";
            var conference = CreateConference(conferenceId);

            // Mock ConferenceRepository
            uow.Setup(x => x.ConferenceRepository.GetTechnicalIncludedById(
                    It.IsAny<string>(), It.IsAny<string?>()
                ))
               .ReturnsAsync(conference);

            // Mock ConferenceStatusRepository
            uow.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByName(It.IsAny<string>()))
               .ReturnsAsync(new ConferenceStatus
               {
                   ConferenceStatusId = "status1",
                   ConferenceStatusName = "Ready"
               });

            // Act
            var result = await _conferenceService.GetTechnicalConferenceDetailAsync(conferenceId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(conferenceId, result.ConferenceId);
            Assert.Equal("Test Conference", result.ConferenceName);
            Assert.Equal("Developers", result.TargetAudience);
            Assert.NotNull(result.Sessions);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ConferenceNotFound_ThrowsNotFoundException()
        {// Arrange
            var conferenceId = "nonexistent";

            // Mock ConferenceRepository trả về null
            uow.Setup(x => x.ConferenceRepository.GetTechnicalIncludedById(
                    It.IsAny<string>(), It.IsAny<string?>()
                ))
               .ReturnsAsync((Conference)null);

            // Mock ConferenceStatusRepository trả về một status hợp lệ
            uow.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByName(It.IsAny<string>()))
               .ReturnsAsync(new ConferenceStatus
               {
                   ConferenceStatusId = "status1",
                   ConferenceStatusName = "Ready"
               });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _conferenceService.GetTechnicalConferenceDetailAsync(conferenceId, null)
            );

            Assert.Contains(conferenceId, ex.Message);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_IsResearchConference_ThrowsException()
        {
            // Arrange
            var conferenceId = "conf2";
            var conference = CreateConference(conferenceId, isResearch: true);

            // Mock ConferenceRepository trả về conference research
            uow.Setup(x => x.ConferenceRepository.GetTechnicalIncludedById(
                    It.IsAny<string>(), It.IsAny<string?>()
                ))
               .ReturnsAsync(conference);

            // Mock ConferenceStatusRepository trả về status hợp lệ
            uow.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByName(It.IsAny<string>()))
               .ReturnsAsync(new ConferenceStatus
               {
                   ConferenceStatusId = "status1",
                   ConferenceStatusName = "Ready"
               });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.GetTechnicalConferenceDetailAsync(conferenceId, null)
            );

            Assert.Contains("tech", ex.Message);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_InvalidStatus_ThrowsBadRequestException()
        {
            // Arrange
            var conferenceId = "conf3";
            var conference = CreateConference(conferenceId, status: "Pending");

            // Mock ConferenceRepository trả về conference có status Pending
            uow.Setup(x => x.ConferenceRepository.GetTechnicalIncludedById(
                    It.IsAny<string>(), It.IsAny<string?>()
                ))
               .ReturnsAsync(conference);

            // Mock ConferenceStatusRepository trả về status Ready (service cần gọi)
            uow.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByName(It.IsAny<string>()))
               .ReturnsAsync(new ConferenceStatus
               {
                   ConferenceStatusId = "status1",
                   ConferenceStatusName = "Ready"
               });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.GetTechnicalConferenceDetailAsync(conferenceId, null)
            );

            Assert.Contains("trạng thái không khả dụng", ex.Message);
        }
    }

}

      