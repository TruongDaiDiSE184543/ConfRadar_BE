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

        private void SetupStatusMocks()
        {
            var pending = new ConferenceStatus { ConferenceStatusId = "pending", ConferenceStatusName = "Pending" };
            var preparing = new ConferenceStatus { ConferenceStatusId = "preparing", ConferenceStatusName = "Preparing" };
            var draft = new ConferenceStatus { ConferenceStatusId = "draft", ConferenceStatusName = "Draft" };
            var onHold = new ConferenceStatus { ConferenceStatusId = "onhold", ConferenceStatusName = "OnHold" };
            var ready = new ConferenceStatus { ConferenceStatusId = "ready", ConferenceStatusName = "Ready" };

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending")).ReturnsAsync(pending);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing")).ReturnsAsync(preparing);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft")).ReturnsAsync(draft);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold")).ReturnsAsync(onHold);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("pending")).ReturnsAsync(pending);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("preparing")).ReturnsAsync(preparing);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("draft")).ReturnsAsync(draft);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("onhold")).ReturnsAsync(onHold);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("ready")).ReturnsAsync(ready);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_PriceNotFound_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1"))
                .ReturnsAsync((ConferencePrice)null);

            await Assert.ThrowsAsync<NotFoundException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", new UpdateConferencePriceRequest(), "u1"));
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ConferenceNotFound_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1"))
                .ReturnsAsync(new ConferencePrice { ConferenceId = "c1" });
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1"))
                .ReturnsAsync((Conference)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", new UpdateConferencePriceRequest(), "u1"));
            Assert.Contains("hội nghị gốc", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_NotCreator_ThrowsException()
        {
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1" };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "creator" };
            
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);

            await Assert.ThrowsAsync<Exception>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", new UpdateConferencePriceRequest(), "u1"));
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ConferenceNotEditable_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1" };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "ready" }; // Ready status
            
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", new UpdateConferencePriceRequest(), "u1"));
            Assert.Contains("không thể chỉnh sửa", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_TechConference_UpdateIsPublish_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1" };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = false , IsInternalHosted = true };
            
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);

            var request = new UpdateConferencePriceRequest { IsPublish = true };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("Với hội nghị Kỹ thuật, không thể cập nhật thuộc tính 'IsPublish'", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ResearchConference_DetailNotFound_ThrowsInvalidOperationException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1" };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = true , IsInternalHosted = true };
            
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("c1"))
                .ReturnsAsync((ResearchConferenceDetail)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", new UpdateConferencePriceRequest(), "u1"));
            Assert.Contains("Không tìm thấy chi tiết nghiên cứu", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ResearchConference_NonAuthorPublish_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1", IsAuthor = false };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = true , IsInternalHosted = true };
            var detail = new ResearchConferenceDetail { ConferenceId = "c1" };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("c1")).ReturnsAsync(detail);

            var request = new UpdateConferencePriceRequest { IsPublish = true };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("Chỉ có vé dành cho tác giả mới có thể được cấu hình để xuất bản", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ResearchConference_PriceLowerThanSubmitFee_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1", IsAuthor = true };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = true , IsInternalHosted = true };
            var detail = new ResearchConferenceDetail { ConferenceId = "c1", SubmitPaperFee = 100 };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("c1")).ReturnsAsync(detail);

            var request = new UpdateConferencePriceRequest { TicketPrice = 50 };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("Giá vé không được thấp hơn phí nộp bài", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_NegativePrice_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1" };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing" ,IsInternalHosted = true};
            
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);

            var request = new UpdateConferencePriceRequest { TicketPrice = -10 };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("không được là số âm", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_TotalSlotZeroOrLess_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1" };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsInternalHosted = true };
            
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);

            var request = new UpdateConferencePriceRequest { TotalSlot = 0 };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("phải lớn hơn 0", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_DuplicateTicketName_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1", TicketName = "Old" };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = false ,IsInternalHosted = true};
            var otherPrice = new ConferencePrice { ConferencePriceId = "p2", ConferenceId = "c1", TicketName = "New" };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesByConferenceIdAsync("c1")).ReturnsAsync(new List<ConferencePrice> { price, otherPrice });

            var request = new UpdateConferencePriceRequest { TicketName = "New" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("đã tồn tại", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_TotalSlotLessThanSold_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1", TotalSlot = 10, AvailableSlot = 5 }; // Sold 5
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = false , IsInternalHosted = true };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);

            var request = new UpdateConferencePriceRequest { TotalSlot = 4 };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("vì đã có 5 vé được bán", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_TotalSlotExceedsConferenceCapacity_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1", TotalSlot = 10, AvailableSlot = 10 };
            var otherPrice = new ConferencePrice { ConferencePriceId = "p2", ConferenceId = "c1", TotalSlot = 90 }; // Total 100 used
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = false, TotalSlot = 100, IsInternalHosted = true };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesByConferenceIdAsync("c1")).ReturnsAsync(new List<ConferencePrice> { price, otherPrice });

            var request = new UpdateConferencePriceRequest { TotalSlot = 20 }; // 90 + 20 = 110 > 100

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("vượt quá giới hạn", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ResearchConference_AuthorSlotExceedsPaperAccept_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1", IsAuthor = true, TotalSlot = 10, AvailableSlot = 10 };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = true, TotalSlot = 200, IsInternalHosted = true };
            var detail = new ResearchConferenceDetail { ConferenceId = "c1", NumberPaperAccept = 15 };
            
            // Assume another author price exists
            var otherPrice = new ConferencePrice { ConferencePriceId = "p2", ConferenceId = "c1", IsAuthor = true, TotalSlot = 10 };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("c1")).ReturnsAsync(detail);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesByConferenceIdAsync("c1")).ReturnsAsync(new List<ConferencePrice> { price, otherPrice });

            var request = new UpdateConferencePriceRequest { TotalSlot = 10 }; // 10 (other) + 10 (new) = 20 > 15

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("vượt quá giới hạn 15 bài báo được chấp nhận", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ExternalTechConference_Preparing_UpdateCoreLogic_ThrowsBadRequestException()
        {
            SetupStatusMocks();
            var price = new ConferencePrice { ConferencePriceId = "p1", ConferenceId = "c1" };
            var conf = new Conference 
            { 
                ConferenceId = "c1", 
                CreatedBy = "u1", 
                ConferenceStatusId = "preparing", 
                IsInternalHosted = false, // External
                IsResearchConference = false // Tech
            };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);

            var request = new UpdateConferencePriceRequest { TicketName = "New Name" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1"));
            Assert.Contains("Bạn không thể cập nhật các thông tin cốt lõi", ex.Message);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_TechConference_SuccessfulUpdate()
        {
            SetupStatusMocks();
            var price = new ConferencePrice 
            { 
                ConferencePriceId = "p1", 
                ConferenceId = "c1", 
                TicketName = "Old", 
                TicketPrice = 100, 
                TotalSlot = 10, 
                AvailableSlot = 5 
            };
            var conf = new Conference 
            { 
                ConferenceId = "c1", 
                CreatedBy = "u1", 
                ConferenceStatusId = "preparing", 
                IsResearchConference = false, 
                TotalSlot = 100 ,
                IsInternalHosted = true
            };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesByConferenceIdAsync("c1")).ReturnsAsync(new List<ConferencePrice> { price });
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync("p1")).ReturnsAsync(new List<PricePhase>());

            var request = new UpdateConferencePriceRequest 
            { 
                TicketName = "New",
                TicketPrice = 150,
                TotalSlot = 20 // Increase by 10
            };

            var result = await _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1");

            result.Should().NotBeNull();
            result.TicketName.Should().Be("New");
            result.TicketPrice.Should().Be(150);
            
            // Verify model update
            _mockUnitOfWork.Verify(u => u.ConferencePriceRepository.UpdateConferencePriceAsync(
                It.Is<ConferencePrice>(p => 
                    p.TicketName == "New" && 
                    p.TicketPrice == 150 && 
                    p.TotalSlot == 20 && 
                    p.AvailableSlot == 15 // 5 (old available) + 10 (diff)
                )), Times.Once);
        }

        [Fact]
        public async Task UpdateConferencePriceAsync_ResearchConference_SuccessfulUpdate()
        {
            SetupStatusMocks();
            var price = new ConferencePrice 
            { 
                ConferencePriceId = "p1", 
                ConferenceId = "c1", 
                IsAuthor = true, 
                IsPublish = false 
            };
            var conf = new Conference { ConferenceId = "c1", CreatedBy = "u1", ConferenceStatusId = "preparing", IsResearchConference = true , IsInternalHosted = true };
            var detail = new ResearchConferenceDetail { ConferenceId = "c1", SubmitPaperFee = 50 };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync("p1")).ReturnsAsync(price);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("c1")).ReturnsAsync(conf);
            _mockUnitOfWork.Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("c1")).ReturnsAsync(detail);
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync("p1")).ReturnsAsync(new List<PricePhase>());

            var request = new UpdateConferencePriceRequest 
            { 
                IsPublish = true,
                TicketPrice = 100 // > 50
            };

            var result = await _conferenceStepService.UpdateConferencePriceAsync("p1", request, "u1");

            result.Should().NotBeNull();
            result.IsPublish.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.ConferencePriceRepository.UpdateConferencePriceAsync(
                It.Is<ConferencePrice>(p => p.IsPublish == true && p.TicketPrice == 100)), Times.Once);
        }
    }
}