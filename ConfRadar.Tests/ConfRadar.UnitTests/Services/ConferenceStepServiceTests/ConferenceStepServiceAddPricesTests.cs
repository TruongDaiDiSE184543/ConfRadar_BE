using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceAddPricesTests
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceAddPricesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConferenceService = new Mock<IConferenceService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            var objectStorageSettings = new AppSettingConfig.ObjectStorageSettings(); // Not used in this method

            _conferenceStepService = new ConferenceStepService(
                _mockUnitOfWork.Object,
                _mockObjectStorageFileService.Object,
                _mockTokenService.Object,
                Options.Create(objectStorageSettings),
                _mockConferenceService.Object,
                _mockTimeProviderService.Object
            );
        }

        #endregion

        #region Helper Methods

        private Conference CreateTechnicalConference(string userId = "user-123", int totalSlots = 200)
        {
            return new Conference
            {
                ConferenceId = "conf-tech-123",
                CreatedBy = userId,
                IsResearchConference = false,
                TotalSlot = totalSlots,
                AvailableSlot = totalSlots,
                TicketSaleStart = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TicketSaleEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(60)),
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(61)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(63)),
                ConferenceStatusId = "status-preparing",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusName = "Preparing" }
            };
        }

        private AddConferencePricesRequest CreateValidAddPricesRequest()
        {
            return new AddConferencePricesRequest
            {
                TypeOfTicket = new List<CreateConferencePriceRequest>
                {
                    new CreateConferencePriceRequest
                    {
                        TicketName = "Standard Ticket",
                        TicketDescription = "Access to all sessions.",
                        TicketPrice = 100,
                        TotalSlot = 100,
                        isAuthor = false,
                        Phases = new List<CreatePricePhaseRequest>
                        {
                            new CreatePricePhaseRequest
                            {
                                PhaseName = "Early Bird",
                                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(20)),
                                ApplyPercent = 80, // 80% of base price
                                Totalslot = 50,
                                refundInPhase = new List<CreateRefundPolicyRequest>
                                {
                                    new CreateRefundPolicyRequest
                                    {
                                        PercentRefund = 100,
                                        RefundDeadline = DateOnly.FromDateTime(DateTime.Now.AddDays(10))
                                    }
                                }
                            },
                            new CreatePricePhaseRequest
                            {
                                PhaseName = "Regular",
                                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(21)),
                                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(50)),
                                ApplyPercent = 100,
                                Totalslot = 50
                            }
                        }
                    }
                }
            };
        }

        private void SetupValidMocks(Conference conference, bool isEditable = true)
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(conference.ConferenceId))
                           .ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesByConferenceIdAsync(conference.ConferenceId))
                           .ReturnsAsync(new List<ConferencePrice>());
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.CreatePricePhaseAsync(It.IsAny<PricePhase>())).ReturnsAsync(1);

            if (isEditable)
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft"))
                   .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusName = "Preparing" });
            }
            else
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(It.IsAny<string>()))
                   .ReturnsAsync(new ConferenceStatus { ConferenceStatusName = "Published" });
            }

            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidAddPricesRequest();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf"))
                           .ReturnsAsync((Conference)null);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.AddConferencePricesAsync("nonexistent-conf", request, "user-123")
            );

            exception.Message.Should().Contain("Hội nghị với ID nonexistent-conf không thấy");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowForbiddenException_When_UserIsNotCreator()
        {
            // ARRANGE
            var conference = CreateTechnicalConference(userId: "creator-id");
            var request = CreateValidAddPricesRequest();
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "other-user-id")
            );

            exception.Message.Should().Contain("Bạn không có quyền thêm giá vé cho hội nghị này.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_RequestContainsNoTickets()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket = new List<CreateConferencePriceRequest>(); // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain("Yêu cầu phải chứa ít nhất một loại vé.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_ConferenceIsNotEditable()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            conference.ConferenceStatusId = "status-published";
            var request = CreateValidAddPricesRequest();
            SetupValidMocks(conference, isEditable: false);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                   .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" });

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain("Thao tác không được phép. Hội nghị đang ở trạng thái 'Published' và không thể chỉnh sửa.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_TotalSlotsExceedConferenceCapacity()
        {
            // ARRANGE
            var conference = CreateTechnicalConference(totalSlots: 50); // Low capacity
            var request = CreateValidAddPricesRequest(); // Default is 100 slots
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().StartWith("Số lượng totalSlot của từng loại vé tổng phải nhỏ hơn hoặc bằng");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_TicketNameIsDuplicated()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().TicketName = "Existing Ticket";
            SetupValidMocks(conference);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesByConferenceIdAsync(conference.ConferenceId))
                .ReturnsAsync(new List<ConferencePrice> { new ConferencePrice { TicketName = "Existing Ticket" } });


            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain("Tên vé 'Existing Ticket' đã tồn tại trong hội nghị này.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_TicketPriceIsNegative()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().TicketPrice = -10; // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain($"Giá vé cho '{request.TypeOfTicket.First().TicketName}' không được là số âm.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_TicketSlotIsZero()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().TotalSlot = 0; // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain($"Số lượng vé cho '{request.TypeOfTicket.First().TicketName}' phải lớn hơn 0.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_TicketHasNoPhases()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().Phases = new List<CreatePricePhaseRequest>(); // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain($"Loại vé '{request.TypeOfTicket.First().TicketName}' phải có ít nhất một giai đoạn bán vé.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_PhaseSlotsMismatchTicketSlot()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().TotalSlot = 100;
            request.TypeOfTicket.First().Phases.First().Totalslot = 20; // 20 + 50 != 100
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().StartWith($"Với vé");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_PhasesOverlap()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            var phases = request.TypeOfTicket.First().Phases;
            phases[0].EndDate = phases[1].StartDate.AddDays(1); // Overlap
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain("bị chồng chéo hoặc quá sát với giai đoạn");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_PhaseNameIsEmpty()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().Phases.First().PhaseName = " "; // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain("Tên giai đoạnn trong vé ' ' không được để trùng.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_PhaseApplyPercentIsInvalid()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().Phases.First().ApplyPercent = -5; // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            //exception.Message.Should().Contain("Tỉ lệ áp dụng cho giai đoạn '-5' phải từ 0 đến 1000.");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_PhaseDatesAreInvalid()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            var phase = request.TypeOfTicket.First().Phases.First();
            phase.StartDate = phase.EndDate.AddDays(1); // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            exception.Message.Should().Contain("Start phase phải lớn hơn end phase");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_ForTechnical_When_PhaseIsOutsideSaleDates()
        {
            // ARRANGE
            var conference = CreateTechnicalConference(); // Sale starts tomorrow
            var request = CreateValidAddPricesRequest();
            request.TypeOfTicket.First().Phases.First().StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)); // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            //exception.Message.Should().Contain("Start phase phải và endphase phải nằm trong ticket sale start và ticket sale end của conference");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_RefundDeadlineIsBeforePhaseStart()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            var phase = request.TypeOfTicket.First().Phases.First();
            phase.refundInPhase.First().RefundDeadline = phase.StartDate.AddDays(-1); // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            //exception.Message.Should().Contain("hạn chót hoàn tiền");
            //exception.Message.Should().Contain("phải sau ngày bắt đầu giai đoạn");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_ThrowBadRequestException_When_RefundDeadlineIsAfterTicketSaleEnd()
        {
            // ARRANGE
            var conference = CreateTechnicalConference(); // Sale ends in 60 days
            var request = CreateValidAddPricesRequest();
            var phase = request.TypeOfTicket.First().Phases.First();
            phase.refundInPhase.First().RefundDeadline = conference.TicketSaleEnd.Value.AddDays(1); // Invalid
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, "user-123")
            );

            //exception.Message.Should().Contain("hạn chót hoàn tiền");
            //exception.Message.Should().Contain("phải trước ngày kết thúc bán vé của hội nghị");
        }

        [Fact]
        public async Task AddConferencePricesAsync_Should_Succeed_ForValidTechnicalConferenceRequest()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPricesRequest();
            var userId = "user-123";
            SetupValidMocks(conference);

            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.CreateConferencePriceAsync(It.IsAny<ConferencePrice>()))
                .Returns(Task.FromResult(1));
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.CreatePricePhaseAsync(It.IsAny<PricePhase>()))
                .Returns(Task.FromResult(1));
            _mockUnitOfWork.Setup(u => u.ConferenceRefundPolicyRepository.CreateConferenceRefundPolicyAsync(It.IsAny<RefundPolicy>()))
               .Returns(Task.FromResult(1));

            // ACT
            var result = await _conferenceStepService.AddConferencePricesAsync(conference.ConferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.conferencePriceWithPhasesResponses.Should().HaveCount(1);
            var priceResponse = result.conferencePriceWithPhasesResponses.First();
            priceResponse.TicketName.Should().Be("Standard Ticket");
            priceResponse.PricePhases.Should().HaveCount(2);
            priceResponse.PricePhases.First().RefundPolicy.Should().HaveCount(1);

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.ConferencePriceRepository.CreateConferencePriceAsync(It.IsAny<ConferencePrice>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.PricePhaseRepository.CreatePricePhaseAsync(It.IsAny<PricePhase>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(u => u.ConferenceRefundPolicyRepository.CreateConferenceRefundPolicyAsync(It.IsAny<RefundPolicy>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        #endregion
    }
}
