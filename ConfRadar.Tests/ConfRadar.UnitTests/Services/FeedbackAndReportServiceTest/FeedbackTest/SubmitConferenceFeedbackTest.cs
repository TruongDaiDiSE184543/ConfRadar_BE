using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Conference;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.UnitTests.Services.FeedbackAndReportServiceTest.FeedbackTest
{
    public class SubmitConferenceFeedbackTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceService _conferenceService;
        public SubmitConferenceFeedbackTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            _conferenceService = new ConferenceService(
                _mockUnitOfWork.Object,
                Mock.Of<IConferenceStatusService>(),
                Mock.Of<IConferenceTimelineService>(),
                Mock.Of<IObjectStorageFileService>(),
                Mock.Of<ITokenService>(),
                Mock.Of<ISystemConfigurationService>(),
                Options.Create(new AppSettingConfig.ObjectStorageSettings()),
                _mockTimeProviderService.Object,
                Mock.Of<INotificationService>()
            );
        }
        [Fact]
        public async Task SubmitConferenceFeedback_ShouldThrow_WhenSessionNotFound()
        {
            var request = new CreateConferenceFeedbackRequest { ConferenceSessionId = "S1", Rating = 5 };
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync("S1"))
                .ReturnsAsync((ConferenceSession)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.SubmitConferenceFeedback(request, "U1"));
        }
        [Fact]
        public async Task SubmitConferenceFeedback_ShouldThrow_WhenUserNotCheckedIn()
        {
            var request = new CreateConferenceFeedbackRequest { ConferenceSessionId = "S1", Rating = 5 };
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync("S1"))
                .ReturnsAsync(new ConferenceSession { ConferenceSessionId = "S1" });

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync("S1", "U1"))
                .ReturnsAsync((UserCheckIn)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.SubmitConferenceFeedback(request, "U1"));
        }
        [Fact]
        public async Task SubmitConferenceFeedback_ShouldThrow_WhenCheckInStatusNotFound()
        {
            var request = new CreateConferenceFeedbackRequest { ConferenceSessionId = "S1", Rating = 5 };
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync("S1"))
                .ReturnsAsync(new ConferenceSession { ConferenceSessionId = "S1" });

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync("S1", "U1"))
                .ReturnsAsync(new UserCheckIn { CheckinStatusId = "CS1" });

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((CheckinStatus)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _conferenceService.SubmitConferenceFeedback(request, "U1"));
        }
        [Fact]
        public async Task SubmitConferenceFeedback_ShouldThrow_WhenUserNotCheckedInStatus()
        {
            var request = new CreateConferenceFeedbackRequest { ConferenceSessionId = "S1", Rating = 5 };
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync("S1"))
                .ReturnsAsync(new ConferenceSession { ConferenceSessionId = "S1" });

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync("S1", "U1"))
                .ReturnsAsync(new UserCheckIn { CheckinStatusId = "CS2" });

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "CS1" });

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.SubmitConferenceFeedback(request, "U1"));
        }
        [Fact]
        public async Task SubmitConferenceFeedback_ShouldReturnRecordCount_WhenSuccess()
        {
            var request = new CreateConferenceFeedbackRequest { ConferenceSessionId = "S1", Rating = 5 };
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync("S1"))
                .ReturnsAsync(new ConferenceSession { ConferenceSessionId = "S1" });

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync("S1", "U1"))
                .ReturnsAsync(new UserCheckIn { CheckinStatusId = "CS1" });

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "CS1" });

            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.UtcNow);

            _mockUnitOfWork.Setup(u => u.ConferenceFeedbackRepository.CreateFeedbackAsync(It.IsAny<ConferenceFeedback>()))
                .ReturnsAsync(1);

            var result = await _conferenceService.SubmitConferenceFeedback(request, "U1");

            Assert.Equal(1, result);
        }

    }
}
