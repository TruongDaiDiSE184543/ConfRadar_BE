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
    public class ConferenceStepServiceAddPricePhaseForNextPhaseTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceAddPricePhaseForNextPhaseTests()
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
        /// Phương thức helper để tạo dữ liệu mock mặc định cho trường hợp thành công.
        /// </summary>
        private (ConferencePrice price, Conference conf, ResearchConferencePhase active, ResearchConferencePhase next) SetupDefaultMocks(string userId)
        {
            var confPriceId = "price-author-1";
            var confId = "conf-research-1";
            var today = new DateOnly(2025, 12, 1);

            var conferencePrice = new ConferencePrice
            {
                ConferencePriceId = confPriceId,
                ConferenceId = confId,
                IsAuthor = true, // Phải là vé tác giả
                AvailableSlot = 10 // Phải còn vé
            };

            var conference = new Conference
            {
                ConferenceId = confId,
                CreatedBy = userId // Phải là người tạo
            };

            var activePhase = new ResearchConferencePhase
            {
                PhaseOrder = 1,
                AuthorPaymentEnd = today.AddDays(-1) // Phase chính đã kết thúc hôm qua
            };

            var nextPhase = new ResearchConferencePhase
            {
                ResearchConferencePhaseId = "next-phase-id",
                PhaseOrder = 2,
                AuthorPaymentStart = today.AddDays(1), // Phase tiếp theo bắt đầu từ ngày mai
                AuthorPaymentEnd = today.AddDays(10)
            };

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(confPriceId)).ReturnsAsync(conferencePrice);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(confId)).ReturnsAsync(activePhase);
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetResearchConferencePhaseByOrderAndConferenceIdAsync(confId, 2)).ReturnsAsync(nextPhase);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(today);
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(confPriceId)).ReturnsAsync(new List<PricePhase>());

            // Mock transaction
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            return (conferencePrice, conference, activePhase, nextPhase);
        }

        [Fact]
        public async Task AddPricePhaseForNextPhase_WithValidRequest_ShouldSucceed()
        {
            // SẮP ĐẶT (ARRANGE)
            var userId = "user-1";
            var confPriceId = "price-author-1";
            var (_, _, _, nextPhase) = SetupDefaultMocks(userId);

            var request = new PhaseForWaitList
            {
                Phases = new List<CreatePricePhaseRequest>
                {
                    new CreatePricePhaseRequest
                    {
                        PhaseName = "Mở bán vé đợt 2",
                        StartDate = nextPhase.AuthorPaymentStart.Value,
                        EndDate = nextPhase.AuthorPaymentEnd.Value,
                        Totalslot = 5 // Số lượng vé hợp lệ
                    }
                }
            };

            // HÀNH ĐỘNG (ACT)
            var result = await _conferenceStepService.AddPricePhaseForNextPhase(confPriceId, request, userId);

            // KHẲNG ĐỊNH (ASSERT)
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().PhaseName.Should().Be("Mở bán vé đợt 2");

            _mockUnitOfWork.Verify(u => u.PricePhaseRepository.CreatePricePhaseAsync(It.IsAny<PricePhase>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AddPricePhaseForNextPhase_WhenTicketIsNotForAuthor_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var userId = "user-1";
            var confPriceId = "price-author-1";
            var (price, _, _, _) = SetupDefaultMocks(userId);
            price.IsAuthor = false; // Thay đổi điều kiện để gây lỗi

            var request = new PhaseForWaitList { Phases = new List<CreatePricePhaseRequest>() };

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceStepService.AddPricePhaseForNextPhase(confPriceId, request, userId)
            );
            ex.Message.Should().Be("Chức nang này chỉ dành để thêm giai đoạn cho loại vé 'isAuthor'.");
        }

        [Fact]
        public async Task AddPricePhaseForNextPhase_WhenNoAvailableSlots_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var userId = "user-1";
            var confPriceId = "price-author-1";
            var (price, _, _, _) = SetupDefaultMocks(userId);
            price.AvailableSlot = 0; // Thay đổi điều kiện để gây lỗi

            var request = new PhaseForWaitList { Phases = new List<CreatePricePhaseRequest>() };

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceStepService.AddPricePhaseForNextPhase(confPriceId, request, userId)
            );
            ex.Message.Should().Be("Không thể thêm giai đoạn mới vì loại vé này đã hết vé (available slot = 0).");
        }

        [Fact]
        public async Task AddPricePhaseForNextPhase_WhenCalledTooEarly_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var userId = "user-1";
            var confPriceId = "price-author-1";
            var (_, _, activePhase, _) = SetupDefaultMocks(userId);

            // Mock thời gian là trước khi phase chính kết thúc
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(activePhase.AuthorPaymentEnd.Value.AddDays(-1));

            var request = new PhaseForWaitList { Phases = new List<CreatePricePhaseRequest>() };

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceStepService.AddPricePhaseForNextPhase(confPriceId, request, userId)
            );
            ex.Message.Should().Contain("Chưa đến thời điểm hợp lệ.");
        }

        [Fact]
        public async Task AddPricePhaseForNextPhase_WhenNoNextPhaseExists_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var userId = "user-1";
            var confPriceId = "price-author-1";
            var confId = "conf-research-1";
            SetupDefaultMocks(userId);

            // Ghi đè mock để trả về null
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetResearchConferencePhaseByOrderAndConferenceIdAsync(confId, 2))
                           .ReturnsAsync((ResearchConferencePhase)null);

            var request = new PhaseForWaitList { Phases = new List<CreatePricePhaseRequest>() };

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceStepService.AddPricePhaseForNextPhase(confPriceId, request, userId)
            );
            ex.Message.Should().Be("Hội nghị không còn phase tiếp theo để thêm giai đoạn bán vé.");
        }

        [Fact]
        public async Task AddPricePhaseForNextPhase_WhenTotalSlotsExceedAvailable_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var userId = "user-1";
            var confPriceId = "price-author-1";
            var (price, _, _, _) = SetupDefaultMocks(userId); // price.AvailableSlot là 10

            var request = new PhaseForWaitList
            {
                Phases = new List<CreatePricePhaseRequest>
                {
                    new CreatePricePhaseRequest { Totalslot = 5 },
                    new CreatePricePhaseRequest { Totalslot = 6 } // Tổng là 11, lớn hơn 10
                }
            };

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceStepService.AddPricePhaseForNextPhase(confPriceId, request, userId)
            );
            ex.Message.Should().Contain("không được vượt quá số vé còn lại của loại vé này");
        }

        [Fact]
        public async Task AddPricePhaseForNextPhase_WhenDateIsOutOfNextPhaseBounds_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var userId = "user-1";
            var confPriceId = "price-author-1";
            var (_, _, _, nextPhase) = SetupDefaultMocks(userId);

            var request = new PhaseForWaitList
            {
                Phases = new List<CreatePricePhaseRequest>
                {
                    new CreatePricePhaseRequest
                    {
                        PhaseName = "Phase lỗi",
                        StartDate = nextPhase.AuthorPaymentStart.Value,
                        EndDate = nextPhase.AuthorPaymentEnd.Value.AddDays(1), // Ngày kết thúc nằm ngoài phạm vi
                        Totalslot = 5
                    }
                }
            };

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _conferenceStepService.AddPricePhaseForNextPhase(confPriceId, request, userId)
            );
            ex.Message.Should().Contain("phải nằm trong khoảng thời gian thanh toán của phase tiếp theo");
        }
    }
}
