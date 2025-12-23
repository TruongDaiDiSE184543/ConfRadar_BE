using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.UpdateConference
{
    public class ConferenceServiceActivateNextPhaseTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;

        // Không cần mock các service khác vì chúng không được sử dụng trong hàm này
        private readonly Mock<IConferenceStatusService> _mockStatusService;
        private readonly Mock<IConferenceTimelineService> _mockTimelineService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorage;
        private readonly Mock<ITokenService> _mockToken;
        private readonly Mock<ISystemConfigurationService> _mockSysConfig;
        private readonly Mock<INotificationService> _mockNotificationService;

        private readonly ConferenceService _service;

        public ConferenceServiceActivateNextPhaseTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProvider = new Mock<ITimeProviderService>();

            _mockStatusService = new Mock<IConferenceStatusService>();
            _mockTimelineService = new Mock<IConferenceTimelineService>();
            _mockObjectStorage = new Mock<IObjectStorageFileService>();
            _mockToken = new Mock<ITokenService>();
            _mockSysConfig = new Mock<ISystemConfigurationService>();
            _mockNotificationService = new Mock<INotificationService>();

            // Khởi tạo ConferenceService với tất cả các dependencies, dù một số không được dùng
            _service = new ConferenceService(
                _mockUnitOfWork.Object,
                _mockStatusService.Object,
                _mockTimelineService.Object,
                _mockObjectStorage.Object,
                _mockToken.Object,
                _mockSysConfig.Object,
                Microsoft.Extensions.Options.Options.Create(new AppSettingConfig.ObjectStorageSettings()),
                _mockTimeProvider.Object,
                _mockNotificationService.Object
            );
        }

        /// <summary>
        /// Phương thức helper để thiết lập dữ liệu mock mặc định cho trường hợp thành công.
        /// </summary>
        private void SetupDefaultMocks(string confId, string userId, DateOnly today)
        {
            // Mock Conference
            var conference = new Conference { ConferenceId = confId, CreatedBy = userId, IsResearchConference = true };
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);

            // Mock Research Detail
            var researchDetail = new ResearchConferenceDetail { ConferenceId = confId ,RevisionAttemptAllowed =1};
            _mockUnitOfWork.Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(confId)).ReturnsAsync(researchDetail);

            // Mock Active and Next Phase
            var activePhase = new ResearchConferencePhase { PhaseOrder = 1, AuthorPaymentEnd = today.AddDays(-1) }; // Đã kết thúc
            var nextPhase = new ResearchConferencePhase
            {
                ResearchConferencePhaseId = "next-phase-id",
                PhaseOrder = 2,
                IsActive = false, // Chưa active
                RegistrationStartDate = today, // Hợp lệ để kích hoạt
                RegistrationEndDate = today.AddDays(5),
                AuthorPaymentStart = today.AddDays(6),
                AuthorPaymentEnd = today.AddDays(10),
                RevisionRoundDeadlines = new List<RevisionRoundDeadline> { new RevisionRoundDeadline ()}
            };

            _mockUnitOfWork.Setup(u => u.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(It.IsAny<string>())).ReturnsAsync(new List<RevisionRoundDeadline> { new RevisionRoundDeadline()});
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(confId)).ReturnsAsync(activePhase);
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetResearchConferencePhaseByOrderAndConferenceIdAsync(confId, 2)).ReturnsAsync(nextPhase);

            // Mock Author Tickets (còn vé)
            var authorTickets = new List<ConferencePrice> { new ConferencePrice { AvailableSlot = 5 } };
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetNumberOfIsAuthorByConferenceId(confId)).ReturnsAsync(authorTickets);

            // Mock Time
            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(today);

            // Mock PricePhase (đã có price phase cho next phase)
            var pricePhases = new List<PricePhase> { new PricePhase { ResearchConferencePhaseId = "next-phase-id" } };
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhaseByconferenceIdThatIsAuthor(confId)).ReturnsAsync(pricePhases);

            // Mock Transaction
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task ActivateNextPhase_WithValidConditions_ShouldSucceedAndReturnTrue()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-1";
            var userId = "user-1";
            var today = new DateOnly(2025, 10, 20);
            SetupDefaultMocks(confId, userId, today);

            // HÀNH ĐỘNG (ACT)
            var result = await _service.ActivateNextPhase(confId, userId);

            // KHẲNG ĐỊNH (ASSERT)
            result.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(It.Is<ResearchConferencePhase>(p => p.IsActive == true)), Times.Once); // Xác minh next phase được active
            _mockUnitOfWork.Verify(u => u.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(It.Is<ResearchConferencePhase>(p => p.IsActive == false)), Times.Once); // Xác minh active phase bị de-active
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ActivateNextPhase_WhenConferenceNotFound_ShouldThrowNotFoundException()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-not-found";
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync((Conference)null);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ActivateNextPhase(confId, "user-1"));
            ex.Message.Should().Contain($"Không tìm thấy hội nghị với ID {confId}");
        }

        [Fact]
        public async Task ActivateNextPhase_WhenUserIsNotCreator_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-1";
            var creatorId = "creator-1";
            var anotherUserId = "user-2";
            var conference = new Conference { ConferenceId = confId, CreatedBy = creatorId };
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.ActivateNextPhase(confId, anotherUserId));
            ex.Message.Should().Be("Bạn không có quyền kích hoạt phase waitlist cho hội nghị này.");
        }

        [Fact]
        public async Task ActivateNextPhase_WhenConferenceIsNotResearchType_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-1";
            var userId = "user-1";
            var conference = new Conference { ConferenceId = confId, CreatedBy = userId, IsResearchConference = false };
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.ActivateNextPhase(confId, userId));
            ex.Message.Should().Be("Chức năng này chỉ dành cho hội nghị nghiên cứu.");
        }

        [Fact]
        public async Task ActivateNextPhase_WhenNextPhaseIsAlreadyActive_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-1";
            var userId = "user-1";
            var today = new DateOnly(2025, 10, 20);
            SetupDefaultMocks(confId, userId, today);

            // Ghi đè mock để next phase đã active
            var nextPhase = new ResearchConferencePhase { IsActive = true, PhaseOrder = 2 };
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetResearchConferencePhaseByOrderAndConferenceIdAsync(confId, 2)).ReturnsAsync(nextPhase);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.ActivateNextPhase(confId, userId));
            ex.Message.Should().Contain("đã kích hoạt truớc đó");
        }

        [Fact]
        public async Task ActivateNextPhase_WhenAllAuthorTicketsAreSold_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-1";
            var userId = "user-1";
            var today = new DateOnly(2025, 10, 20);
            SetupDefaultMocks(confId, userId, today);

            // Ghi đè mock vé tác giả đã bán hết
            var authorTickets = new List<ConferencePrice> { new ConferencePrice { AvailableSlot = 0 } };
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetNumberOfIsAuthorByConferenceId(confId)).ReturnsAsync(authorTickets);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.ActivateNextPhase(confId, userId));
            ex.Message.Should().Contain("đã được bán hết");
        }

        [Fact]
        public async Task ActivateNextPhase_WhenCurrentPhaseHasNotEnded_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-1";
            var userId = "user-1";
            var today = new DateOnly(2025, 10, 20);
            SetupDefaultMocks(confId, userId, today);

            // Ghi đè mock để phase hiện tại chưa kết thúc
            var activePhase = new ResearchConferencePhase { PhaseOrder = 1, AuthorPaymentEnd = today.AddDays(1) };
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(confId)).ReturnsAsync(activePhase);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.ActivateNextPhase(confId, userId));
            ex.Message.Should().Contain("khi phase hiện tại chưa kết thúc");
        }

       

        [Fact]
        public async Task ActivateNextPhase_WhenNoPricePhaseForNextPhase_ShouldThrowBadRequestException()
        {
            // SẮP ĐẶT (ARRANGE)
            var confId = "conf-1";
            var userId = "user-1";
            var today = new DateOnly(2025, 10, 20);
            SetupDefaultMocks(confId, userId, today);

            // Ghi đè mock để không có PricePhase nào cho phase tiếp theo
            var pricePhases = new List<PricePhase> { new PricePhase { ResearchConferencePhaseId = "some-other-phase-id" } };
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.GetPricePhaseByconferenceIdThatIsAuthor(confId)).ReturnsAsync(pricePhases);

            // HÀNH ĐỘNG & KHẲNG ĐỊNH (ACT & ASSERT)
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.ActivateNextPhase(confId, userId));
            ex.Message.Should().Contain("Vui lòng tạo ít nhất một 'Giai đoạn bán vé'");
        }
    }
}
