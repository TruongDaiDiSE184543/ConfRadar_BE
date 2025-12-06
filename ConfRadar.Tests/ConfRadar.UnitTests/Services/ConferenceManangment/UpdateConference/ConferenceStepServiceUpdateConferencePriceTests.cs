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
using Minio.Exceptions;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.UpdateConference
{
    public class ConferenceStepServiceUpdateConferencePriceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceUpdateConferencePriceTests()
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

        // Phương thức helper để thiết lập các mock chung cho một hội nghị hợp lệ và có thể chỉnh sửa
        private void SetupValidAndEditableConferenceMocks(string priceId, string confId, string userId, ConferencePrice existingPrice, Conference parentConference)
        {
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId))
                           .ReturnsAsync(existingPrice);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                           .ReturnsAsync(parentConference);

            // === BỔ SUNG MOCKS CHO EnsureConferenceIsEditable ===
            // Thiết lập các đối tượng trạng thái
            var preparingStatus = new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" };
            var draftStatus = new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" };
            var onHoldStatus = new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" };

            // Mock các lệnh gọi GetConferenceStatusByName...
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription()))
                           .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Preparing.GetDescription()))
                           .ReturnsAsync(preparingStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Draft.GetDescription()))
                           .ReturnsAsync(draftStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.OnHold.GetDescription()))
                           .ReturnsAsync(onHoldStatus);

            // Mock lệnh gọi GetConferenceStatusByIdAsync với trạng thái hiện tại của hội nghị
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(parentConference.ConferenceStatusId))
                           .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = parentConference.ConferenceStatusId, ConferenceStatusName = "Preparing" });
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_YeuCauHopLe_NenCapNhatGiaThanhCong()
        {
            // SẮP ĐẶT (ARRANGE)
            var priceId = "price-1";
            var confId = "conf-1";
            var userId = "user-1";

            var existingPrice = new ConferencePrice
            {
                ConferencePriceId = priceId,
                ConferenceId = confId,
                TicketName = "Vé Sớm",
                TicketPrice = 100,
                TotalSlot = 50,
                AvailableSlot = 50 // Chưa có vé nào được bán
            };

            var parentConference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                ConferenceStatusId = "status-preparing", // Trạng thái có thể chỉnh sửa
                TotalSlot = 200,
                IsInternalHosted = true // Hội nghị nội bộ
            };

            var request = new UpdateConferencePriceRequest
            {
                TicketName = "Vé Tiêu Chuẩn",
                TicketPrice = 150,
                TicketDescription = "Một mô tả mới",
                TotalSlot = 60 // Tăng số lượng vé
            };

            SetupValidAndEditableConferenceMocks(priceId, confId, userId, existingPrice, parentConference);

            // Mock không có vé nào được bán cho loại vé này
            _mockUnitOfWork.Setup(u => u.TicketRepository.GetTicketCountByConferencePriceIdAsync(priceId))
                           .ReturnsAsync(0);

            // Mock không có loại vé nào khác trùng tên mới
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesByConferenceIdAsync(confId))
                           .ReturnsAsync(new List<ConferencePrice> { existingPrice });

            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(priceId))
                           .ReturnsAsync(new List<PricePhase>());

            // HÀNH ĐỘNG (ACT)
            var result = await _conferenceStepService.UpdateConferencePriceAsync(priceId, request, userId);

            // KHẲNG ĐỊNH (ASSERT)
            result.Should().NotBeNull();
            result.TicketName.Should().Be(request.TicketName);
            result.TicketPrice.Should().Be(request.TicketPrice);
            result.TicketDescription.Should().Be(request.TicketDescription);

            // Xác minh rằng số lượng vé đã được cập nhật chính xác
            _mockUnitOfWork.Verify(u => u.ConferencePriceRepository.UpdateConferencePriceAsync(
                It.Is<ConferencePrice>(p => p.TotalSlot == 60 && p.AvailableSlot == 60)), Times.Once);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_HoiNghiKhongOTrangThaiChinhSua_NenNemRaNgoaiLeBadRequest()
        {
            // SẮP ĐẶT (ARRANGE)
            var priceId = "price-1";
            var confId = "conf-1";
            var userId = "user-1";

            var existingPrice = new ConferencePrice { ConferencePriceId = priceId, ConferenceId = confId };
            var parentConference = new Conference { ConferenceId = confId, CreatedBy = userId, ConferenceStatusId = "status-ready" }; // Trạng thái "Ready" -> không thể chỉnh sửa

            // Thiết lập mock cho trạng thái không thể chỉnh sửa
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId)).ReturnsAsync(existingPrice);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(parentConference);

            // === BỔ SUNG MOCKS CHO EnsureConferenceIsEditable (quan trọng) ===
            var readyStatus = new ConferenceStatus { ConferenceStatusId = "status-ready", ConferenceStatusName = "Ready" };
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("status-ready"))
                           .ReturnsAsync(readyStatus);
            // Vẫn cần mock các trạng thái khác để logic chạy qua
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                           .ReturnsAsync(new ConferenceStatus());
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName(It.IsAny<string>()))
                           .ReturnsAsync(new ConferenceStatus());

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.UpdateConferencePriceAsync(priceId, new UpdateConferencePriceRequest(), userId)
            );
            ex.Message.Should().Contain($"Hội nghị đang ở trạng thái 'Ready' và không thể chỉnh sửa.");
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_HoiNghiLienKetDangPreparing_NenNemRaNgoaiLeBadRequest()
        {
            // SẮP ĐẶT (ARRANGE)
            var priceId = "price-1";
            var confId = "conf-1";
            var userId = "user-1";

            var existingPrice = new ConferencePrice { ConferencePriceId = priceId, ConferenceId = confId };
            var parentConference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId,
                ConferenceStatusId = "status-preparing", // Trạng thái Preparing
                IsInternalHosted = false // Hội nghị liên kết (external)
            };

            var request = new UpdateConferencePriceRequest { TicketName = "Tên mới" };

            // Sử dụng helper để mock các trạng thái
            SetupValidAndEditableConferenceMocks(priceId, confId, userId, existingPrice, parentConference);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.UpdateConferencePriceAsync(priceId, request, userId)
            );
            ex.Message.Should().Contain("Bạn không thể cập nhật các thông tin cốt lõi");
        }

        // ... (Các unit test khác như PriceNotFound, UserNotCreator, v.v. giữ nguyên và dịch sang tiếng Việt) ...

        [Fact]
        public async Task UpdateConferencePriceAsync_GiaKhongTonTai_NenNemRaNgoaiLeNotFound()
        {
            // SẮP ĐẶT (ARRANGE)
            var priceId = "gia-khong-ton-tai";
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId))
                           .ReturnsAsync((ConferencePrice)null);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.UpdateConferencePriceAsync(priceId, new UpdateConferencePriceRequest(), "user-1")
            );
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_NguoiDungKhongPhaiNguoiTao_NenNemRaNgoaiLeForbidden()
        {
            // SẮP ĐẶT (ARRANGE)
            var priceId = "price-1";
            var confId = "conf-1";
            var creatorId = "nguoi-tao";
            var otherUserId = "nguoi-dung-khac";

            var existingPrice = new ConferencePrice { ConferencePriceId = priceId, ConferenceId = confId };
            var parentConference = new Conference { ConferenceId = confId, CreatedBy = creatorId, ConferenceStatusId = "status-preparing" };

            SetupValidAndEditableConferenceMocks(priceId, confId, creatorId, existingPrice, parentConference);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            await Assert.ThrowsAsync<ForbiddenException>(
                () => _conferenceStepService.UpdateConferencePriceAsync(priceId, new UpdateConferencePriceRequest(), otherUserId)
            );
        }
    }
}
