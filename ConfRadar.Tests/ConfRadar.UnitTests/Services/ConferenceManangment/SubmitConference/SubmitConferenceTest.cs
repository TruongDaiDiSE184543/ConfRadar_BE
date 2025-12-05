using ConfRadar.Repositories.Models;
using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ConfRadar.Services.Exceptions;


namespace ConfRadar.UnitTests.Services.ConferenceManangment.SubmitConference
{
    public class SubmitConferenceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceStatusService> _mockStatusService;
        private readonly Mock<IConferenceTimelineService> _mockTimelineService;
        // Các mock phụ
        private readonly Mock<ITimeProviderService> _mockTimeProvider;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorage;
        private readonly Mock<ITokenService> _mockToken;
        private readonly Mock<ISystemConfigurationService> _mockSysConfig;

        private readonly ConferenceService _service;

        public SubmitConferenceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockStatusService = new Mock<IConferenceStatusService>();
            _mockTimelineService = new Mock<IConferenceTimelineService>();
            _mockTimeProvider = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>(); 
            _mockObjectStorage = new Mock<IObjectStorageFileService>();
            _mockToken = new Mock<ITokenService>();
            _mockSysConfig = new Mock<ISystemConfigurationService>();

            var options = Options.Create(new AppSettingConfig.ObjectStorageSettings());

            _service = new ConferenceService(
                _mockUnitOfWork.Object, _mockStatusService.Object, _mockTimelineService.Object,
                _mockObjectStorage.Object, _mockToken.Object, _mockSysConfig.Object,
                options, _mockTimeProvider.Object, _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task RequestOrganizerApproval_ValidDraft_ShouldChangeToPending()
        {
            // ARRANGE
            var confId = "conf-1";
            var userId = "user-1";
            var draftId = "status-draft";
            var pendingId = "status-pending";

            var conference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                ConferenceStatusId = draftId, // Bắt buộc phải là Draft
                ConferenceName = "My Conf"
            };

            // Mock DB
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(userId)).ReturnsAsync(new User { FullName = "Test User" });

            // Mock Status checks
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Draft"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = draftId, ConferenceStatusName = "Draft" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = pendingId, ConferenceStatusName = "Pending" });

            // Mock cho hàm UpdateStatus bên trong
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(draftId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = draftId, ConferenceStatusName = "Draft" });
            _mockStatusService.Setup(s => s.IsStatusTransitionValidAsync("Draft", "Pending")).ReturnsAsync(true);

            // ACT
            var result = await _service.RequestOrganizerApproval(confId, userId);

            // ASSERT
            result.Should().BeTrue();
            conference.ConferenceStatusId.Should().Be(pendingId);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task RequestOrganizerApproval_NotDraft_ShouldThrowException()
        {
            // ARRANGE
            var confId = "conf-1";
            var userId = "user-1";
            var rejectedId = "status-rejected";

            var conference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                ConferenceStatusId = rejectedId // Đang bị Rejected thì không được Submit lại
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(userId)).ReturnsAsync(new User());

            // Setup Draft status ID để so sánh
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Draft"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-draft" });

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.RequestOrganizerApproval(confId, userId));
            ex.Message.Should().Contain("phải đang là draft status");
        }
    }
}
