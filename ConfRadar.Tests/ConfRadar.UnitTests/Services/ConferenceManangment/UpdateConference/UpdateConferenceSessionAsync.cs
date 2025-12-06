using ConfRadar.Repositories.Models;
using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using FluentAssertions;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.UpdateConference
{
    public class ConferenceStepServiceUpdateConferenceSessionTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceUpdateConferenceSessionTests()
        {
            // Khởi tạo tất cả các mock
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

        /// <summary>
        /// Phương thức helper để thiết lập các mock chung cho một phiên và hội nghị hợp lệ.
        /// </summary>
        private void SetupMocksForSessionUpdate(
            string sessionId, string confId, string userId,
            ConferenceSession session, Conference conference,
            List<ConferenceSession> otherSessionsInRoom = null)
        {
            // Mock để lấy phiên và hội nghị
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId)).ReturnsAsync(session);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId)).ReturnsAsync(session);

            // Mock cho việc kiểm tra trùng lặp thời gian
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
                           .ReturnsAsync(otherSessionsInRoom ?? new List<ConferenceSession>());

            // === MOCKS CHO CÁC HELPER METHOD (EnsureConferenceIsEditable, ValidateUpdateForOnHoldConference) ===
            var preparingStatus = new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" };
            var draftStatus = new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" };
            var onHoldStatus = new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" };

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Preparing.GetDescription())).ReturnsAsync(preparingStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Draft.GetDescription())).ReturnsAsync(draftStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.OnHold.GetDescription())).ReturnsAsync(onHoldStatus);

            // Mock trạng thái hiện tại MỘT CÁCH TỔNG QUÁT. Các test case đặc biệt sẽ ghi đè mock này.
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(conference.ConferenceStatusId))
                           .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = conference.ConferenceStatusId, ConferenceStatusName = "Preparing" }); // Giả định mặc định là Preparing
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_WithValidRequest_ShouldUpdateSuccessfully()
        {
            // SẮP ĐẶT (ARRANGE)
            var sessionId = "session-1";
            var confId = "conf-1";
            var userId = "user-1";
            var roomId = "room-1";

            var existingSession = new ConferenceSession
            {
                ConferenceSessionId = sessionId,
                ConferenceId = confId,
                Title = "Tiêu đề cũ",
                Description = "Mô tả cũ",
                StartTime = new DateTime(2025, 12, 25, 9, 0, 0),
                EndTime = new DateTime(2025, 12, 25, 10, 0, 0),
                SessionDate = new DateOnly(2025, 12, 25),
                RoomId = roomId
            };

            var parentConference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                IsResearchConference = false, // Phải là hội nghị kỹ thuật
                IsInternalHosted = true,
                ConferenceStatusId = "status-preparing",
                StartDate = new DateOnly(2025, 12, 25),
                EndDate = new DateOnly(2025, 12, 26)
            };

            var request = new UpdateConferenceSessionRequest
            {
                Title = "Tiêu đề mới",
                Description = "Mô tả mới"
            };

            SetupMocksForSessionUpdate(sessionId, confId, userId, existingSession, parentConference);

            // HÀNH ĐỘNG (ACT)
            var result = await _conferenceStepService.UpdateConferenceSessionAsync(sessionId, request, userId);

            // KHẲNG ĐỊNH (ASSERT)
            result.Should().NotBeNull();
            result.Title.Should().Be(request.Title);
            result.Description.Should().Be(request.Description);

            // Xác minh rằng phương thức cập nhật trong repository đã được gọi một lần
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.UpdateConferenceSessionAsync(It.IsAny<ConferenceSession>()), Times.Once);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_WhenSessionNotFound_ShouldThrowNotFoundException()
        {
            // SẮP ĐẶT (ARRANGE)
            var sessionId = "phien-khong-ton-tai";
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId))
                           .ReturnsAsync((ConferenceSession)null);

            var request = new UpdateConferenceSessionRequest();

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.UpdateConferenceSessionAsync(sessionId, request, "user-1")
            );
            ex.Message.Should().Contain($"Không tìm thấyy phiên với ID {sessionId}");
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_WhenConferenceIsResearchType_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var sessionId = "session-1";
            var confId = "conf-1";
            var userId = "user-1";

            var existingSession = new ConferenceSession { ConferenceSessionId = sessionId, ConferenceId = confId };
            var parentConference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                IsResearchConference = true // Đây là điểm mấu chốt gây ra lỗi
            };

            // Chỉ cần mock 2 lệnh gọi đầu tiên là đủ để gây ra lỗi
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId)).ReturnsAsync(existingSession);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(parentConference);

            var request = new UpdateConferenceSessionRequest();

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.UpdateConferenceSessionAsync(sessionId, request, userId)
            );
            ex.Message.Should().Be("Chức năng này không dành cho phiên của hội nghị nghiên cứu.");
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_OnHoldExternalConferenceAndForbiddenFieldChange_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var sessionId = "session-1";
            var confId = "conf-1";
            var userId = "user-1";

            var existingSession = new ConferenceSession { ConferenceSessionId = sessionId, ConferenceId = confId, Title = "Tiêu đề cũ" };
            var parentConference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                IsResearchConference = false,
                IsInternalHosted = false, // Hội nghị liên kết (external)
                ConferenceStatusId = "status-onhold" // Đang tạm hoãn
            };

            var request = new UpdateConferenceSessionRequest
            {
                Title = "Tiêu đề mới" // Cố gắng thay đổi trường bị cấm
            };

            // Gọi helper mock chung
            SetupMocksForSessionUpdate(sessionId, confId, userId, existingSession, parentConference);

            // *** SỬA LỖI: Ghi đè mock từ helper để trả về đúng trạng thái "OnHold" ***
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("status-onhold"))
                           .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" });

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.UpdateConferenceSessionAsync(sessionId, request, userId)
            );
            ex.Message.Should().Be("Không thể thay đổi 'Tiêu đề phiên' khi hội nghị đang OnHold.");
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_WhenTimeConflictsWithAnotherSession_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var sessionId = "session-1";
            var confId = "conf-1";
            var userId = "user-1";
            var roomId = "room-1";

            var existingSession = new ConferenceSession { ConferenceSessionId = sessionId, ConferenceId = confId, RoomId = roomId };
            var parentConference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                IsResearchConference = false,
                IsInternalHosted = true,
                ConferenceStatusId = "status-preparing",
                StartDate = new DateOnly(2025, 12, 25),
                EndDate = new DateOnly(2025, 12, 26)
            };

            // Một phiên khác đã tồn tại trong cùng phòng, cùng ngày
            var conflictingSession = new ConferenceSession
            {
                ConferenceSessionId = "session-2",
                ConferenceId = confId,
                RoomId = roomId,
                SessionDate = new DateOnly(2025, 12, 25),
                StartTime = new DateTime(2025, 12, 25, 10, 0, 0),
                EndTime = new DateTime(2025, 12, 25, 11, 0, 0)
            };

            var request = new UpdateConferenceSessionRequest
            {
                Date = new DateOnly(2025, 12, 25),
                StartTime = new TimeOnly(10, 30, 0), // Thời gian này bị trùng
                EndTime = new TimeOnly(11, 30, 0)
            };

            // Mock có một phiên khác trong phòng
            SetupMocksForSessionUpdate(sessionId, confId, userId, existingSession, parentConference, new List<ConferenceSession> { conflictingSession });
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.UpdateConferenceSessionAsync(sessionId, request, userId)
            );
            ex.Message.Should().Contain("bị trùng lặp với một phiên đã có");
        }
    }
}
