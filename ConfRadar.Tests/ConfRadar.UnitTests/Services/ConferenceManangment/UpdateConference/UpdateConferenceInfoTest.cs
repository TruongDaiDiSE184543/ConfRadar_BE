using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.UpdateConference
{
    public class UpdateConferenceInfoTest
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorage;
        private readonly Mock<ITokenService> _mockToken;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;

        private readonly ConferenceStepService _service;
        private readonly AppSettingConfig.ObjectStorageSettings _options;

        public UpdateConferenceInfoTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockObjectStorage = new Mock<IObjectStorageFileService>();
            _mockToken = new Mock<ITokenService>();
            _mockConferenceService = new Mock<IConferenceService>();
            _mockTimeProvider = new Mock<ITimeProviderService>();

            _options = new AppSettingConfig.ObjectStorageSettings { EndPoint = "https://minio.com/" };

            _service = new ConferenceStepService(
                _mockUnitOfWork.Object,
                _mockObjectStorage.Object,
                _mockToken.Object,
                Options.Create(_options),
                _mockConferenceService.Object,
                _mockTimeProvider.Object
            );
        }

        #endregion

        #region Helper Methods (Setup Data)

        private void SetupStatusMocks(string draftId, string preparingId, string pendingId, string onHoldId)
        {
            // Setup GetByName
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Draft"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = draftId, ConferenceStatusName = "Draft" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = draftId, ConferenceStatusName = "Draft" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = preparingId, ConferenceStatusName = "Preparing" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Preparing"))
               .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = preparingId, ConferenceStatusName = "Preparing" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = pendingId, ConferenceStatusName = "Pending" });

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = onHoldId, ConferenceStatusName = "OnHold" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("OnHold"))
               .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = onHoldId, ConferenceStatusName = "OnHold" });

            // Setup GetById for EnsureConferenceIsEditable logic
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(draftId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = draftId, ConferenceStatusName = "Draft" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(preparingId))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = preparingId, ConferenceStatusName = "Preparing" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(pendingId))
               .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = pendingId, ConferenceStatusName = "Pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(onHoldId))
               .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = onHoldId, ConferenceStatusName = "OnHold" });
        }

        private Conference CreateConference(string confId, string userId, string statusId)
        {
            return new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                ConferenceStatusId = statusId,
                ConferenceName = "Original Name",
                Description = "Original Desc",
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(12)),
                TicketSaleStart = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TicketSaleEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                IsInternalHosted = true, // Default
                TotalSlot = 100,
                AvailableSlot = 100
            };
        }

        #endregion

        #region Test Cases

        // 1. Success Case: Happy Path (Draft Status)
        [Fact]
        public async Task UpdateConferenceBasicAsync_ValidDraft_ShouldUpdateSuccess()
        {
            // ARRANGE
            var confId = "conf-1";
            var userId = "user-1";
            var draftId = "status-draft";

            SetupStatusMocks(draftId, "status-prep", "status-pending", "status-onhold");

            var conference = CreateConference(confId, userId, draftId);
            var technicalDetail = new TechnicalConferenceDetail { ConferenceId = confId, TargetAudience = "Old Audience" };

            var request = new UpdateConferenceBasicRequest
            {
                ConferenceName = "New Name",
                TotalSlot = 200,
                targetaudience = "New Audience"
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(technicalDetail);
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId(userId)).ReturnsAsync(new List<UserRole>());

            // ACT
            var result = await _service.UpdateConferenceBasicAsync(confId, request, userId);

            // ASSERT
            conference.ConferenceName.Should().Be("New Name");
            conference.TotalSlot.Should().Be(200);
            conference.AvailableSlot.Should().Be(200);
            technicalDetail.TargetAudience.Should().Be("New Audience");

            _mockUnitOfWork.Verify(u => u.ConferenceRepository.UpdateConferenceAsync(conference), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        // 2. Exception: Không tìm thấy Conference
        [Fact]
        public async Task UpdateConferenceBasicAsync_ConferenceNotFound_ShouldThrowNotFoundException()
        {
            // ARRANGE
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("any-id")).ReturnsAsync((Conference)null);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateConferenceBasicAsync("any-id", new UpdateConferenceBasicRequest(), "user-1"));

            ex.Message.Should().Contain("không tìm thấy"); // Copy đúng chuỗi từ code gốc
        }

        // 3. Exception: Không phải Owner (Forbidden)
        [Fact]
        public async Task UpdateConferenceBasicAsync_UserNotCreator_ShouldThrowForbiddenException()
        {
            // ARRANGE
            var confId = "conf-1";
            var conference = CreateConference(confId, "creator-id", "status-draft");

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(new TechnicalConferenceDetail());

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateConferenceBasicAsync(confId, new UpdateConferenceBasicRequest(), "hacker-id"));

            ex.Message.Should().Contain("không có quyền");
        }

        // 4. Exception: Trạng thái không hợp lệ (Ví dụ: Pending hoặc Ready/Completed)
        [Fact]
        public async Task UpdateConferenceBasicAsync_InvalidStatus_ShouldThrowBadRequestException()
        {
            // ARRANGE
            var confId = "conf-1";
            var pendingId = "status-pending";
            SetupStatusMocks("status-draft", "status-prep", pendingId, "status-onhold");

            var conference = CreateConference(confId, "user-1", pendingId); // Status là Pending (không nằm trong list cho phép sửa)

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(new TechnicalConferenceDetail());

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateConferenceBasicAsync(confId, new UpdateConferenceBasicRequest(), "user-1"));

            ex.Message.Should().Contain("Thao tác không được phép"); // Message từ EnsureConferenceIsEditable
        }

        // 5. Exception: Logic Collaborator không được sửa khi Preparing (Quan trọng)
        [Fact]
        public async Task UpdateConferenceBasicAsync_ExternalHosted_AtPreparing_ShouldThrowBadRequestException()
        {
            // ARRANGE
            var confId = "conf-1";
            var preparingId = "status-prep";
            SetupStatusMocks("status-draft", preparingId, "status-pending", "status-onhold");

            var conference = CreateConference(confId, "user-1", preparingId);
            conference.IsInternalHosted = false; // Đây là External Hosted (Collaborator)

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(new TechnicalConferenceDetail());

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateConferenceBasicAsync(confId, new UpdateConferenceBasicRequest(), "user-1"));

            // Check logic: restrictExternalHostedAtPreaparingStatus = true
            ex.Message.Should().Contain("Bạn không thể cập nhật các thông tin cốt lõi");
        }

        // 6. Exception: Logic OnHold - Cố tình sửa các trường bị cấm
        [Fact]
        public async Task UpdateConferenceBasicAsync_OnHold_UpdateRestrictedFields_ShouldThrowBadRequestException()
        {
            // ARRANGE
            var confId = "conf-1";
            var onHoldId = "status-onhold";

            // 1. Lấy đúng string từ Enum để đảm bảo khớp 100% (Tránh lỗi: "OnHold" vs "On Hold")
            string onHoldName = ConferenceStatusEnum.OnHold.GetDescription();

            // Setup Mock cho Status
            SetupStatusMocks("status-draft", "status-prep", "status-pending", onHoldId);

            // 2. Tạo Conference
            var conference = CreateConference(confId, "user-1", onHoldId);

            // --- [QUAN TRỌNG 1] --- 
            // Gán Navigation Property thủ công. 
            // Trong hàm ValidateUpdateForOnHoldConference, code check: conference.ConferenceStatus?.ConferenceStatusName
            // Nếu bạn không gán dòng dưới này, conference.ConferenceStatus là NULL => Code return luôn => Action không chạy.
            conference.ConferenceStatus = new ConferenceStatus
            {
                ConferenceStatusId = onHoldId,
                ConferenceStatusName = onHoldName // Phải khớp với Enum Description
            };

            // --- [QUAN TRỌNG 2] ---
            // Logic OnHold chỉ áp dụng cho External Hosted (Collaborator).
            // Nếu để true => Code return luôn => Action không chạy.
            conference.IsInternalHosted = false;

            // --- [QUAN TRỌNG 3] ---
            // Dữ liệu Request phải KHÁC dữ liệu cũ để trigger điều kiện if trong Action
            var request = new UpdateConferenceBasicRequest
            {
                ConferenceName = "New Name Forbidden" // Khác với "Original Name" trong hàm CreateConference
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(new TechnicalConferenceDetail());

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateConferenceBasicAsync(confId, request, "user-1"));

            ex.Message.Should().Contain("Không thể thay đổi 'Tên hội nghị'");
        }
        // 7. Success: Logic OnHold - Sửa ngày tháng (Được phép)
        [Fact]
        public async Task UpdateConferenceBasicAsync_OnHold_UpdateAllowedFields_ShouldSuccess()
        {
            // ARRANGE
            var confId = "conf-1";
            var onHoldId = "status-onhold";
            SetupStatusMocks("status-draft", "status-prep", "status-pending", onHoldId);

            var conference = CreateConference(confId, "user-1", onHoldId);
            conference.IsInternalHosted = false;

            // Request sửa ngày (Được phép)
            var newStart = DateOnly.FromDateTime(DateTime.Now.AddDays(20));
            var newEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(22));
            var request = new UpdateConferenceBasicRequest
            {
                StartDate = newStart,
                EndDate = newEnd
                // Không sửa Name/Desc...
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(new TechnicalConferenceDetail());
            _mockUnitOfWork.Setup(u => u.UserRoleRepository.GetMutipleUserRolesByUserId("user-1")).ReturnsAsync(new List<UserRole>());

            // ACT
            await _service.UpdateConferenceBasicAsync(confId, request, "user-1");

            // ASSERT
            conference.StartDate.Should().Be(newStart);
            conference.EndDate.Should().Be(newEnd);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        // 8. Exception: Ngày tháng không hợp lệ (Logic IsValidConferenceAndTicketSaleDates)
        [Fact]
        public async Task UpdateConferenceBasicAsync_InvalidDateLogic_ShouldThrowBadRequestException()
        {
            // ARRANGE
            var confId = "conf-1";
            var draftId = "status-draft";
            SetupStatusMocks(draftId, "status-prep", "status-pending", "status-onhold");

            var conference = CreateConference(confId, "user-1", draftId);
            conference.IsInternalHosted = false;
            // Request: Ngày bán vé > Ngày kết thúc hội nghị (Vô lý)
            var request = new UpdateConferenceBasicRequest
            {
                TicketSaleStart = DateOnly.FromDateTime(DateTime.Now.AddDays(20)),
                TicketSaleEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(25)),
                // StartDate conference cũ là Day 10 -> Lỗi
            };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(new TechnicalConferenceDetail());

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateConferenceBasicAsync(confId, request, "user-1"));

            ex.Message.Should().Contain("Ngày tháng cung cấp không hợp lệ");
        }

        // 9. Exception: File ảnh không hợp lệ
        [Fact]
        public async Task UpdateConferenceBasicAsync_InvalidImageFile_ShouldThrowBadRequestException()
        {
            // ARRANGE
            var confId = "conf-1";
            var draftId = "status-draft";
            SetupStatusMocks(draftId, "status-prep", "status-pending", "status-onhold");

            var conference = CreateConference(confId, "user-1", draftId);

            var mockFile = new Mock<IFormFile>();
            var request = new UpdateConferenceBasicRequest { BannerImageFile = mockFile.Object };

            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(confId)).ReturnsAsync(new TechnicalConferenceDetail());

            // Mock File Service trả về False
            _mockObjectStorage.Setup(s => s.IsValidImageFile(It.IsAny<IFormFile>())).Returns(false);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateConferenceBasicAsync(confId, request, "user-1"));

            ex.Message.Should().Contain("Định dạng ảnh bìa không được hỗ trợ");
        }

        #endregion
    }
}
