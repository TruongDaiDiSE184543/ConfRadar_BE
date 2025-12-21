using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.Track_Conference_Status
{
    public class ChangeConferenceStatusTest
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceStatusService> _mockConferenceStatusService;
        private readonly Mock<IConferenceTimelineService> _mockConferenceTimelineService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ISystemConfigurationService> _mockSystemConfigurationService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<INotificationService> _mockNotificationService;

        private readonly ConferenceService _conferenceService;
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ChangeConferenceStatusTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConferenceStatusService = new Mock<IConferenceStatusService>();
            _mockConferenceTimelineService = new Mock<IConferenceTimelineService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockSystemConfigurationService = new Mock<ISystemConfigurationService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();

            _objectStorageSettings = new AppSettingConfig.ObjectStorageSettings();

            _conferenceService = new ConferenceService(
                _mockUnitOfWork.Object,
                _mockConferenceStatusService.Object,
                _mockConferenceTimelineService.Object,
                _mockObjectStorageFileService.Object,
                _mockTokenService.Object,
                _mockSystemConfigurationService.Object,
                Options.Create(_objectStorageSettings),
                _mockTimeProviderService.Object,
                _mockNotificationService.Object
            );
        }

        #endregion

        #region Helper Methods

        private void SetupStatusMocks()
        {
            // Define statuses
            var statuses = new List<ConferenceStatus>
            {
                new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" },
                new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" },
                new ConferenceStatus { ConferenceStatusId = "status-deleted", ConferenceStatusName = "Deleted" },
                new ConferenceStatus { ConferenceStatusId = "status-rejected", ConferenceStatusName = "Rejected" },
                new ConferenceStatus { ConferenceStatusId = "status-disabled", ConferenceStatusName = "Disabled" },
                new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" },
                new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" },
                new ConferenceStatus { ConferenceStatusId = "status-ready", ConferenceStatusName = "Ready" },
                new ConferenceStatus { ConferenceStatusId = "status-completed", ConferenceStatusName = "Completed" }
            };

            // Mock GetByName
            foreach (var status in statuses)
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(status.ConferenceStatusName))
                    .ReturnsAsync(status);
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName(status.ConferenceStatusName)) // Handling potential non-async call if any
                    .ReturnsAsync(status);
            }

            // Mock GetById
            foreach (var status in statuses)
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(status.ConferenceStatusId))
                    .ReturnsAsync(status);
            }
        }

        private void SetupConference(string confId, string userId, string statusId, bool isInternal = true)
        {
            var conference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                ConferenceStatusId = statusId,
                IsInternalHosted = isInternal
            };
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);
        }

        #endregion

        #region Basic Validations

        [Fact]
        public async Task ChangeConferenceStatus_ConferenceNotFound_ThrowsBadRequest()
        {
            SetupStatusMocks();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-1")).ReturnsAsync((Conference)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-ready"));
            ex.Message.Should().Contain("Không tìm thấy hội nghị");
        }

        [Fact]
        public async Task ChangeConferenceStatus_UserNotCreator_ThrowsBadRequest()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "creator", "status-draft");

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ChangeConferenceStatus("other-user", "conf-1", "status-ready"));
            ex.Message.Should().Contain("Chỉ có nguời tạo ra conference mới thay đổi được trạng thái");
        }

        [Fact]
        public async Task ChangeConferenceStatus_NewStatusNotFound_ThrowsBadRequest()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-draft");
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("invalid-status")).ReturnsAsync((ConferenceStatus)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "invalid-status"));
            ex.Message.Should().Contain("Không tìm thấy conference status");
        }

        #endregion

        #region Disabled Status Tests (4 Cases)

        [Fact]
        public async Task ChangeConferenceStatus_ToDisabled_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-ready");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-disabled"));
            ex.Message.Should().Contain("Không thể sử dụng với disabled status ở đây");
        }

        [Fact]
        public async Task ChangeConferenceStatus_FromDisabled_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-disabled");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-ready"));
            ex.Message.Should().Contain("Không thể sử dụng với disabled status ở đây");
        }

        [Fact]
        public async Task ChangeConferenceStatus_FromDisabledToDisabled_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-disabled");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-disabled"));
            ex.Message.Should().Contain("Không thể sử dụng với disabled status ở đây");
        }

        [Fact]
        public async Task ChangeConferenceStatus_ToDisabled_FromPending_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-pending");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-disabled"));
            ex.Message.Should().Contain("Không thể sử dụng với disabled status ở đây");
        }

        #endregion

        #region OnHold Status Tests (4 Cases)

        [Fact]
        public async Task ChangeConferenceStatus_PreparingToOnHold_Success()
        {
            // Arrange
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-preparing");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Preparing", "OnHold")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            // Act
            var result = await _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-onhold");

            // Assert
            result.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.ConferenceRepository.UpdateConferenceAsync(It.Is<Conference>(c => c.ConferenceStatusId == "status-onhold")), Times.Once);
        }

        [Fact]
        public async Task ChangeConferenceStatus_ReadyToOnHold_Success()
        {
            // Arrange
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-ready");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Ready", "OnHold")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            // Act
            var result = await _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-onhold");

            // Assert
            result.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.ConferenceRepository.UpdateConferenceAsync(It.Is<Conference>(c => c.ConferenceStatusId == "status-onhold")), Times.Once);
        }

      

        [Fact]
        public async Task ChangeConferenceStatus_OnHoldToCompleted_InvalidTransition_ThrowsException()
        {
            // Arrange
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-onhold");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("OnHold", "Completed")).ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AggregateException>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-completed"));
            
            ex.InnerExceptions[0].Message.Should().Contain("không hợp lệ");
        }

        [Fact]
        public async Task ChangeConferenceStatus_OnHoldToReady_TimelinesOutdated_ThrowsBadRequest()
        {
            // Arrange
            var userId = "user-1";
            var confId = "conf-1";
            var onHoldId = "status-onhold";
            var readyId = "status-ready";
            var today = new DateOnly(2025, 1, 15);
            var onHoldStartDate = new DateOnly(2025, 1, 1); // 14 days ago

            SetupStatusMocks();
            var conference = new Conference 
            { 
                ConferenceId = confId, 
                CreatedBy = userId, 
                ConferenceStatusId = onHoldId,
                IsInternalHosted = true,
                // Invalid date: StartDate falls between onHoldStartDate and today
                StartDate = new DateOnly(2025, 1, 10) 
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("OnHold", "Ready")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(today);
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(today.ToDateTime(new TimeOnly(0, 0)));

            // Mock Last Transition Ready -> OnHold
            var timelineEntry = new ConferenceTimeline { ChangeDate = onHoldStartDate };
            _mockUnitOfWork.Setup(u => u.ConferenceTimelineRepository.GetLastTransitionConferenceTimelineByConfIdAndStatusIdAsync(confId, readyId, onHoldId))
                .ReturnsAsync(timelineEntry);

            // Mock dependencies for ValidateConferenceTimelineAsync
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(confId)).ReturnsAsync(new List<ConferencePrice>());
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(confId)).ReturnsAsync(new List<ConferenceSession>());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AggregateException>(() =>
                _conferenceService.ChangeConferenceStatus(userId, confId, readyId));
            
            ex.InnerExceptions[0].Message.Should().Contain("Các mốc thời gian sau đã bị lỗi thời");
            ex.InnerExceptions[0].Message.Should().Contain("Ngày bắt đầu hội nghị");
        }

        [Fact]
        public async Task ChangeConferenceStatus_OnHoldToReady_TimelinesValid_Success()
        {
            // Arrange
            var userId = "user-1";
            var confId = "conf-1";
            var onHoldId = "status-onhold";
            var readyId = "status-ready";
            var today = new DateOnly(2025, 1, 15);
            var onHoldStartDate = new DateOnly(2025, 1, 1);

            SetupStatusMocks();
            var conference = new Conference 
            { 
                ConferenceId = confId, 
                CreatedBy = userId, 
                ConferenceStatusId = onHoldId,
                IsInternalHosted = true,
                // Valid date: StartDate is in future
                StartDate = new DateOnly(2025, 2, 1) 
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("OnHold", "Ready")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(today);
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(today.ToDateTime(new TimeOnly(0, 0)));

            var timelineEntry = new ConferenceTimeline { ChangeDate = onHoldStartDate };
            _mockUnitOfWork.Setup(u => u.ConferenceTimelineRepository.GetLastTransitionConferenceTimelineByConfIdAndStatusIdAsync(confId, readyId, onHoldId))
                .ReturnsAsync(timelineEntry);

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(confId)).ReturnsAsync(new List<ConferencePrice>());
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(confId)).ReturnsAsync(new List<ConferenceSession>());
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _conferenceService.ChangeConferenceStatus(userId, confId, readyId);

            // Assert
            result.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.ConferenceRepository.UpdateConferenceAsync(It.Is<Conference>(c => c.ConferenceStatusId == readyId)), Times.Once);
        }

        #endregion

        #region Pending Status Tests

        [Fact]
        public async Task ChangeConferenceStatus_PendingToDeleted_Success()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-pending");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Pending", "Deleted")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            var result = await _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-deleted");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeConferenceStatus_PendingToDraft_Success()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-pending");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Pending", "Draft")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            var result = await _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-draft");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeConferenceStatus_PendingToPreparing_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-pending");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-preparing"));
            ex.Message.Should().Contain("approve lên preparing");
        }

        [Fact]
        public async Task ChangeConferenceStatus_PendingToReady_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-pending");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-ready"));
            ex.Message.Should().Contain("approve lên preparing");
        }

        #endregion

        #region Draft Status Tests

        [Fact]
        public async Task ChangeConferenceStatus_DraftToDeleted_Success()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-draft");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Draft", "Deleted")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            var result = await _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-deleted");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeConferenceStatus_DraftToPending_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-draft");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-pending"));
            ex.Message.Should().Contain("chỉ có thể chuyển sang delete");
        }

        [Fact]
        public async Task ChangeConferenceStatus_DraftToPreparing_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-draft");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-preparing"));
            ex.Message.Should().Contain("chỉ có thể chuyển sang delete");
        }

        #endregion

        #region Rejected Status Tests

        [Fact]
        public async Task ChangeConferenceStatus_RejectedToDraft_Success()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-rejected");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Rejected", "Draft")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            var result = await _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-draft");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeConferenceStatus_RejectedToDeleted_Success()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-rejected");
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Rejected", "Deleted")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            var result = await _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-deleted");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeConferenceStatus_RejectedToPending_ThrowsException()
        {
            SetupStatusMocks();
            SetupConference("conf-1", "user-1", "status-rejected");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-pending"));
            ex.Message.Should().Contain("chỉ có thể đổi lên draft");
        }

        #endregion

        #region External Hosted (Collaborator) Tests

        [Fact]
        public async Task ChangeConferenceStatus_ExternalDeletedToAny_ThrowsException()
        {
            // This tests the logic: if (conference.IsInternalHosted != true && conference.ConferenceStatusId == deleteStatus.ConferenceStatusId)
            SetupStatusMocks();
            // Setup an external conference that is ALREADY deleted
            SetupConference("conf-1", "user-1", "status-deleted", isInternal: false);

            // Attempt to change to Draft
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ChangeConferenceStatus("user-1", "conf-1", "status-draft"));
            
            ex.Message.Should().Contain("Hội nghị được liên kết không thể chuyển sang trạng thái bị xoá");
        }

        #endregion
    }
}