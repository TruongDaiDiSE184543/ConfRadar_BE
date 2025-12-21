using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.UpdateConference
{
    public class UpdateResearchSessionAsyncTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public UpdateResearchSessionAsyncTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConferenceService = new Mock<IConferenceService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            var objectStorageSettings = new AppSettingConfig.ObjectStorageSettings();

            _conferenceStepService = new ConferenceStepService(
                _mockUnitOfWork.Object,
                _mockObjectStorageFileService.Object,
                _mockTokenService.Object,
                Options.Create(objectStorageSettings),
                _mockConferenceService.Object,
                _mockTimeProviderService.Object
            );
        }

        private void SetupMocks(
            string sessionId, string confId, string userId,
            ConferenceSession session, Conference conference)
        {
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId))
                .ReturnsAsync(session);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId))
                .ReturnsAsync(session);
            
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<ConferenceSession>());

            // Status mocks
            var pendingStatus = new ConferenceStatus { ConferenceStatusId = "pending", ConferenceStatusName = "Pending" };
            var preparingStatus = new ConferenceStatus { ConferenceStatusId = "preparing", ConferenceStatusName = "Preparing" };
            var draftStatus = new ConferenceStatus { ConferenceStatusId = "draft", ConferenceStatusName = "Draft" };
            var onHoldStatus = new ConferenceStatus { ConferenceStatusId = "onhold", ConferenceStatusName = "OnHold" };
            var readyStatus = new ConferenceStatus { ConferenceStatusId = "ready", ConferenceStatusName = "Ready" };

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending")).ReturnsAsync(pendingStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing")).ReturnsAsync(preparingStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft")).ReturnsAsync(draftStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold")).ReturnsAsync(onHoldStatus);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("pending")).ReturnsAsync(pendingStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("preparing")).ReturnsAsync(preparingStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("draft")).ReturnsAsync(draftStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("onhold")).ReturnsAsync(onHoldStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("ready")).ReturnsAsync(readyStatus);

            if (!string.IsNullOrEmpty(session?.RoomId))
            {
                _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync(session.RoomId))
                    .ReturnsAsync(new Room { RoomId = session.RoomId });
            }
        }

        [Fact]
        public async Task UpdateResearchSessionAsync_WhenReadyStatus_OnlyUpdateRoomId_ShouldSucceed()
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsResearchConference = true,
                ConferenceStatusId = "ready",
                StartDate = date,
                EndDate = date.AddDays(1)
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1",
                RoomId = "room1",
                SessionDate = date,
                StartTime = date.ToDateTime(new TimeOnly(10, 0)),
                EndTime = date.ToDateTime(new TimeOnly(11, 0))
            };
            SetupMocks("sess1", "conf1", "user1", session, conf);
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync("room2")).ReturnsAsync(new Room { RoomId = "room2" });

            var request = new UpdateConferenceSessionRequest { RoomId = "room2" };

            var result = await _conferenceStepService.UpdateResearchSessionAsync("sess1", request, "user1");

            result.Should().NotBeNull();
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.UpdateConferenceSessionAsync(
                It.Is<ConferenceSession>(s => s.RoomId == "room2")), Times.Once);
        }

        [Fact]
        public async Task UpdateResearchSessionAsync_WhenReadyStatus_UpdateTitle_ShouldThrowBadRequestException()
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsResearchConference = true,
                ConferenceStatusId = "ready",
                StartDate = date,
                EndDate = date.AddDays(1)
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1",
                RoomId = "room1",
                SessionDate = date,
                StartTime = date.ToDateTime(new TimeOnly(10, 0)),
                EndTime = date.ToDateTime(new TimeOnly(11, 0))
            };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var request = new UpdateConferenceSessionRequest { Title = "New Title" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateResearchSessionAsync("sess1", request, "user1"));
            
            Assert.Contains("Hội nghị đang ở trạng thái 'Ready' và không thể chỉnh sửa", ex.Message);
        }

        [Fact]
        public async Task UpdateResearchSessionAsync_NotResearchConference_ThrowsBadRequestException()
        {
            var conf = new Conference { ConferenceId = "conf1", IsResearchConference = false };
            var session = new ConferenceSession { ConferenceSessionId = "sess1", ConferenceId = "conf1" };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateResearchSessionAsync("sess1", new UpdateConferenceSessionRequest(), "user1"));
            Assert.Contains("Chức năng này chỉ dành cho phiên của hội nghị nghiên c?u", ex.Message);
        }
    }
}