using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Conference;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceManangment
{
    public class ApproveConferenceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceStatusService> _mockConferenceStatusService;
        private readonly Mock<IConferenceTimelineService> _mockConferenceTimelineService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ISystemConfigurationService> _mockSystemConfigurationService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly ConferenceService _conferenceService;

        public ApproveConferenceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConferenceStatusService = new Mock<IConferenceStatusService>();
            _mockConferenceTimelineService = new Mock<IConferenceTimelineService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();

            _mockSystemConfigurationService = new Mock<ISystemConfigurationService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();

            var objectStorageSettings = Options.Create(new AppSettingConfig.ObjectStorageSettings { EndPoint = "http://test-minio" });

            _conferenceService = new ConferenceService(
                _mockUnitOfWork.Object,
                _mockConferenceStatusService.Object,
                _mockConferenceTimelineService.Object,
                _mockObjectStorageFileService.Object,
                _mockTokenService.Object,
                _mockSystemConfigurationService.Object,
                objectStorageSettings,
                _mockTimeProviderService.Object,
                _mockNotificationService.Object
            );
        }

        private Conference CreateMockConference(string confId, string statusId, string creatorId)
        {
            return new Conference
            {
                ConferenceId = confId,
                ConferenceStatusId = statusId,
                CreatedBy = creatorId,
                ConferenceName = "Test Conf",
                CreatedByNavigation = new User
                {
                    UserId = creatorId,
                    FullName = "Test Creator",
                    FirebaseMobileFcmToken = "mobile-token",
                    FirebaseWebFcmToken = "web-token"
                }
            };
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldThrowBadRequest_WhenConferenceNotFound()
        {
            // Arrange
            string confId = "non-existent-id";
            var request = new ApproveConferenceRequest { IsApprove = true };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync((Conference)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ApproveConferenceAsync(confId, request));

            exception.Message.Should().Contain($"Không tìm thấy conf id {confId} này");
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldThrowBadRequest_WhenCreatorNotFound()
        {
            // Arrange
            string confId = "conf-1";
            var conference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = "user-1",
                CreatedByNavigation = null // Missing creator navigation
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ApproveConferenceAsync(confId, new ApproveConferenceRequest { IsApprove = true }));

            exception.Message.Should().Contain($"Không tìm thấy user tạo conference");
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldThrowBadRequest_WhenCurrentStatusNotFound()
        {
            // Arrange
            string confId = "conf-1";
            string currentStatusId = "status-pending";
            var conference = CreateMockConference(confId, currentStatusId, "user-1");

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);

            // Mock returning null for current status
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(currentStatusId))
                .ReturnsAsync((ConferenceStatus)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ApproveConferenceAsync(confId, new ApproveConferenceRequest { IsApprove = true }));

            exception.Message.Should().Contain("Không tìm thấy trạnng thái hiện tại của hội nghị");
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldThrowBadRequest_WhenTargetStatusNotFound()
        {
            // Arrange
            string confId = "conf-1";
            string currentStatusId = "status-pending";
            var conference = CreateMockConference(confId, currentStatusId, "user-1");
            var currentStatus = new ConferenceStatus { ConferenceStatusId = currentStatusId, ConferenceStatusName = "Pending" };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(currentStatusId))
                .ReturnsAsync(currentStatus);

            // Mock returning null for target status (Preparing)
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                .ReturnsAsync((ConferenceStatus)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ApproveConferenceAsync(confId, new ApproveConferenceRequest { IsApprove = true }));

            exception.Message.Should().Contain($"Không tìm thấy trạng thái");
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldThrowBadRequest_WhenStatusTransitionIsInvalid()
        {
            // Arrange
            string confId = "conf-1";
            var conference = CreateMockConference(confId, "status-deleted", "user-1"); // Current status is Deleted
            var currentStatus = new ConferenceStatus { ConferenceStatusId = "status-deleted", ConferenceStatusName = "Deleted" };
            var targetStatus = new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(conference.ConferenceStatusId)).ReturnsAsync(currentStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing")).ReturnsAsync(targetStatus);

            // Mock Status Service to return False for validation
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Deleted", "Preparing"))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ApproveConferenceAsync(confId, new ApproveConferenceRequest { IsApprove = true }));

            exception.Message.Should().Contain($"Chuyển trạng thái từ 'Deleted' sang 'Preparing' không hợp lệ");
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldThrowException_AndRollback_WhenDbUpdateFails()
        {
            // Arrange
            string confId = "conf-1";
            var conference = CreateMockConference(confId, "status-pending", "user-1");
            var currentStatus = new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" };
            var targetStatus = new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("status-pending")).ReturnsAsync(currentStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing")).ReturnsAsync(targetStatus);
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Pending", "Preparing")).ReturnsAsync(true);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));

            // Simulate DB failure
            _mockUnitOfWork.Setup(u => u.CommitAsync()).ThrowsAsync(new Exception("DB Error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _conferenceService.ApproveConferenceAsync(confId, new ApproveConferenceRequest { IsApprove = true }));

            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldThrowBadRequest_WhenRejectingButTargetStatusRejectedNotFound()
        {
            // Arrange
            string confId = "conf-1";
            var conference = CreateMockConference(confId, "status-pending", "user-1");
            var currentStatus = new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("status-pending")).ReturnsAsync(currentStatus);

            // Mock returning null for "Rejected" status
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Rejected"))
                .ReturnsAsync((ConferenceStatus)null);

            // Act & Assert (IsApprove = false)
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceService.ApproveConferenceAsync(confId, new ApproveConferenceRequest { IsApprove = false }));

            exception.Message.Should().Contain($"Không tìm thấy trạng thái");
        }

        [Fact]
        public async Task ApproveConferenceAsync_ShouldSuccess_WhenInputIsValid_AndNotificationsSent()
        {
            // Arrange
            string confId = "conf-1";
            string userId = "user-1";
            var conference = CreateMockConference(confId, "status-pending", userId);
            var currentStatus = new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" };
            var targetStatus = new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("status-pending")).ReturnsAsync(currentStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing")).ReturnsAsync(targetStatus);
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Pending", "Preparing")).ReturnsAsync(true);

            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.Now);

            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateNotificationAsync(It.IsAny<Notification>()))
                .ReturnsAsync(1); // Notification creation success

            // Act
            var result = await _conferenceService.ApproveConferenceAsync(confId, new ApproveConferenceRequest { IsApprove = true, Reason = "Looks good" });

            // Assert
            result.Should().BeTrue();

            // Verify status update
            conference.ConferenceStatusId.Should().Be("status-preparing");
            _mockUnitOfWork.Verify(u => u.ConferenceRepository.UpdateConferenceAsync(conference), Times.Once);

            // Verify timeline creation
            _mockConferenceTimelineService.Verify(t => t.CreateConferenceTimelineAsync(It.Is<ConferenceTimeline>(
                ct => ct.PreviousStatusId == "status-pending" && ct.AfterwardStatusId == "status-preparing" && ct.Reason == "Looks good"
            )), Times.Once);

            // Verify Transaction commit
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);

            // Verify Notification
            _mockUnitOfWork.Verify(u => u.NotificationRepository.CreateNotificationAsync(It.Is<Notification>(
                n => n.UserId == userId && n.Title == "Kết quả duyệt hội nghị" && n.Message.Contains("đã được xét duyệt")
            )), Times.Once);


        }

        [Fact]
        public async Task ApproveConferenceAsync_Approve_ShouldUpdateStatusToPreparing_AndNotify()
        {
            // ARRANGE
            var confId = "conf-1";
            var creatorId = "user-creator";
            var request = new ApproveConferenceRequest { IsApprove = true, Reason = "Good job" };

            var pendingId = "status-pending";
            var preparingId = "status-preparing";

            var conference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = creatorId,
                ConferenceName = "Test Conf",
                ConferenceStatusId = pendingId,
                CreatedByNavigation = new User { UserId = creatorId, FullName = "Creator", FirebaseMobileFcmToken = "token" }
            };

            // Mock Data
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);

            // Mock Status
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(pendingId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = pendingId, ConferenceStatusName = "Pending" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = preparingId, ConferenceStatusName = "Preparing" });

            // Mock Validation Transition
            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Pending", "Preparing")).ReturnsAsync(true);

            // Mock Notification creation
            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateNotificationAsync(It.IsAny<Notification>())).ReturnsAsync(1);

            // ACT
            var result = await _conferenceService.ApproveConferenceAsync(confId, request);

            // ASSERT
            result.Should().BeTrue();
            conference.ConferenceStatusId.Should().Be(preparingId); // Trạng thái phải đổi

            // Kiểm tra có gửi thông báo không
            _mockNotificationService.Verify(n => n.SendMobilePushAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveConferenceAsync_Reject_ShouldUpdateStatusToRejected()
        {
            // ARRANGE
            var confId = "conf-1";
            var request = new ApproveConferenceRequest { IsApprove = false, Reason = "Bad content" };
            var pendingId = "status-pending";
            var rejectedId = "status-rejected";

            var conference = new Conference
            {
                ConferenceId = confId,
                ConferenceStatusId = pendingId,
                CreatedByNavigation = new User { UserId = "u1" }
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(pendingId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = pendingId, ConferenceStatusName = "Pending" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Rejected"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = rejectedId, ConferenceStatusName = "Rejected" });

            _mockConferenceStatusService.Setup(s => s.IsStatusTransitionValidAsync("Pending", "Rejected")).ReturnsAsync(true);

            // Mock Notification creation
            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateNotificationAsync(It.IsAny<Notification>())).ReturnsAsync(1);

            // ACT
            var result = await _conferenceService.ApproveConferenceAsync(confId, request);

            // ASSERT
            result.Should().BeTrue();
            conference.ConferenceStatusId.Should().Be(rejectedId);
        }
    }
}