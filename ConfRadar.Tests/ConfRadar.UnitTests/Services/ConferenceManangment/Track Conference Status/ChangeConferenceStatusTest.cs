using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Conference;
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
            // Initialize Mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConferenceStatusService = new Mock<IConferenceStatusService>();
            _mockConferenceTimelineService = new Mock<IConferenceTimelineService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockSystemConfigurationService = new Mock<ISystemConfigurationService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();

            _objectStorageSettings = new AppSettingConfig.ObjectStorageSettings();

            // Initialize Service
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

        private Conference CreateConference(string confId, string userId, string statusId, string statusName)
        {
            return new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                ConferenceName = "Test Conference",
                Description = "Test Description",
                ConferenceStatusId = statusId,
                ConferenceStatus = new ConferenceStatus
                {
                    ConferenceStatusId = statusId,
                    ConferenceStatusName = statusName
                },
                IsInternalHosted = true // Set internal to skip contract checks for simplicity
            };
        }

        private void SetupStatusMocks(string preparingId, string onHoldId, string pendingId, string draftId, string deletedId, string rejectedId, string disabledId)
        {
            // Setup GetById
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(preparingId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = preparingId, ConferenceStatusName = "Preparing" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(onHoldId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = onHoldId, ConferenceStatusName = "OnHold" });

            // Setup GetByName (Used extensively in validation logic)
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = pendingId, ConferenceStatusName = "Pending" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Draft"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = draftId, ConferenceStatusName = "Draft" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Deleted"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = deletedId, ConferenceStatusName = "Deleted" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Rejected"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = rejectedId, ConferenceStatusName = "Rejected" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Disabled"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = disabledId, ConferenceStatusName = "Disabled" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("OnHold"))
               .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = onHoldId, ConferenceStatusName = "OnHold" });
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task ChangeConferenceStatus_Should_UpdateStatusAndCreateTimeline_When_TransitionIsValid()
        {
            // ARRANGE
            var userId = "user-123";
            var confId = "conf-ABC";

            var preparingId = "status-preparing";
            var onHoldId = "status-onhold";

            // Setup status dictionaries (Pending, Draft, etc.) to satisfy validation logic
            SetupStatusMocks(preparingId, onHoldId, "status-pending", "status-draft", "status-deleted", "status-rejected", "status-disabled");

            var conference = CreateConference(confId, userId, preparingId, "Preparing");
            var newStatusEntity = new ConferenceStatus { ConferenceStatusId = onHoldId, ConferenceStatusName = "OnHold" };

            // Mock getting conference
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);

            // Mock transaction
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Mock transition validation
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Preparing", "OnHold"))
                .ReturnsAsync(true);

            // Mock time
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            // ACT
            var result = await _conferenceService.ChangeConferenceStatus(userId, confId, onHoldId, "Testing Change Status");

            // ASSERT
            result.Should().BeTrue();

            // Verify conference status updated
            conference.ConferenceStatusId.Should().Be(onHoldId);

            // Verify repository update called
            _mockUnitOfWork.Verify(u => u.ConferenceRepository.UpdateConferenceAsync(conference), Times.Once);

            // Verify timeline created
            _mockConferenceTimelineService.Verify(t => t.CreateConferenceTimelineAsync(It.Is<ConferenceTimeline>(
                tl => tl.ConferenceId == confId &&
                      tl.PreviousStatusId == preparingId &&
                      tl.AfterwardStatusId == onHoldId
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangeConferenceStatus_Should_ThrowBadRequestException_When_UserIsNotCreator()
        {
            // ARRANGE
            var userId = "hacker-user";
            var confId = "conf-ABC";
            var conference = CreateConference(confId, "creator-user", "status-prep", "Preparing");

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceService.ChangeConferenceStatus(userId, confId, "status-new")
            );

            exception.Message.Should().Contain("Chỉ có nguời tạo ra conference mới thay đổi được trạng thái");
        }

        [Fact]
        public async Task ChangeConferenceStatus_Should_ThrowException_When_TryingToUseDisabledStatus()
        {
            // ARRANGE
            var userId = "user-123";
            var confId = "conf-ABC";
            var disabledId = "status-disabled";

            SetupStatusMocks("status-prep", "status-onhold", "status-pending", "status-draft", "status-deleted", "status-rejected", disabledId);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(disabledId))
       .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = disabledId, ConferenceStatusName = "Disabled" });

            var conference = CreateConference(confId, userId, "status-prep", "Preparing");

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _conferenceService.ChangeConferenceStatus(userId, confId, disabledId)
            );

            exception.Message.Should().Contain("Không thể sử dụng với disabled status ở đây");
        }

        [Fact]
        public async Task ChangeConferenceStatus_Should_ThrowException_When_PendingToReady_WithoutApproval()
        {
            // ARRANGE
            var userId = "user-123";
            var confId = "conf-ABC";
            var pendingId = "status-pending";
            var readyId = "status-ready"; // Trying to jump to Ready

            SetupStatusMocks("status-prep", "status-onhold", pendingId, "status-draft", "status-deleted", "status-rejected", "status-disabled");

            // Mock Ready status explicitly for GetById
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(readyId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = readyId, ConferenceStatusName = "Ready" });

            var conference = CreateConference(confId, userId, pendingId, "Pending");

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _conferenceService.ChangeConferenceStatus(userId, confId, readyId)
            );

            // The logic says: from pending can only go delete or back to draft. Otherwise throw exception.
            exception.Message.Should().Contain("Conference cần Organizer approve lên preparing trước");
        }

        [Fact]
        public async Task ChangeConferenceStatus_Should_ThrowBadRequestException_When_TransitionIsInvalid()
        {
            // ARRANGE
            var userId = "user-123";
            var confId = "conf-ABC";
            var preparingId = "status-preparing";
            var completedId = "status-completed"; // Invalid jump: Preparing -> Completed

            SetupStatusMocks(preparingId, "status-onhold", "status-pending", "status-draft", "status-deleted", "status-rejected", "status-disabled");


            var completedStatus = new ConferenceStatus { ConferenceStatusId = completedId, ConferenceStatusName = "Completed" };

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(completedId))
                .ReturnsAsync(completedStatus);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Completed"))
                .ReturnsAsync(completedStatus);

            // Mock Completed status
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(completedId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = completedId, ConferenceStatusName = "Completed" });

            var conference = CreateConference(confId, userId, preparingId, "Preparing");

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);

            // Mock Transition Validity as FALSE
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Preparing", "Completed"))
                .ReturnsAsync(false);

            // ACT & ASSERT
            // Note: The exception comes from private method UpdateConferenceStatusAsync which is called by ChangeConferenceStatus
            var exception = await Assert.ThrowsAsync<AggregateException>(
                () => _conferenceService.ChangeConferenceStatus(userId, confId, completedId)
            );

            exception.Message.Should().Contain("không hợp lệ");
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }

        #endregion
    }
}