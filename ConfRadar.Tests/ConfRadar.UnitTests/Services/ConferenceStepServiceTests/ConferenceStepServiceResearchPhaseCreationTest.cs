using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceResearchPhaseCreationTest : ConferenceStepService
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;

        public ConferenceStepServiceResearchPhaseCreationTest() : this(
            new Mock<IUnitOfWork>(),
            new Mock<IObjectStorageFileService>(),
            new Mock<ITokenService>(),
            new Mock<IConferenceService>(),
            new Mock<ITimeProviderService>())
        {
        }

        private ConferenceStepServiceResearchPhaseCreationTest(
            Mock<IUnitOfWork> mockUow,
            Mock<IObjectStorageFileService> mockOs,
            Mock<ITokenService> mockTs,
            Mock<IConferenceService> mockCs,
            Mock<ITimeProviderService> mockTp)
            : base(mockUow.Object, mockOs.Object, mockTs.Object, 
                   Options.Create(new AppSettingConfig.ObjectStorageSettings
                   {
                       EndPoint = "https://test-storage.com/",
                       AccessKey = "test-access-key",
                       SecretKey = "test-secret-key",
                       Secure = true
                   }),
                   mockCs.Object, mockTp.Object)
        {
            _mockUnitOfWork = mockUow;
            _mockObjectStorageFileService = mockOs;
            _mockTokenService = mockTs;
            _mockConferenceService = mockCs;
            _mockTimeProviderService = mockTp;
        }

        #endregion

        #region Helper Methods

        private CreateResearchConferencePhasesRequest CreateValidRequest()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            return new CreateResearchConferencePhasesRequest
            {
                Phases = new List<CreateResearchConferencePhaseItemRequest>
                {
                    new CreateResearchConferencePhaseItemRequest
                    {
                        RegistrationStartDate = today.AddDays(1),
                        RegistrationEndDate = today.AddDays(30),
                        AbstractDecideStatusStart = today.AddDays(31),
                        AbstractDecideStatusEnd = today.AddDays(35),
                        FullPaperStartDate = today.AddDays(36),
                        FullPaperEndDate = today.AddDays(60),
                        ReviewStartDate = today.AddDays(61),
                        ReviewEndDate = today.AddDays(75),
                        FullPaperDecideStatusStart = today.AddDays(76),
                        FullPaperDecideStatusEnd = today.AddDays(80),
                        ReviseStartDate = today.AddDays(81),
                        ReviseEndDate = today.AddDays(95),
                        RevisionPaperDecideStatusStart = today.AddDays(96),
                        RevisionPaperDecideStatusEnd = today.AddDays(100),
                        CameraReadyStartDate = today.AddDays(101),
                        CameraReadyEndDate = today.AddDays(115),
                        AuthorPaymentStart = today.AddDays(116),
                        AuthorPaymentEnd = today.AddDays(120),
                        RevisionRoundDeadlines = new List<CreateRevisionRoundDeadlineRequest>
                        {
                            new CreateRevisionRoundDeadlineRequest
                            {
                                StartSubmissionDate = today.AddDays(81),
                                EndSubmissionDate = today.AddDays(88)
                            },
                            new CreateRevisionRoundDeadlineRequest
                            {
                                StartSubmissionDate = today.AddDays(89),
                                EndSubmissionDate = today.AddDays(95)
                            }
                        }
                    },
                    new CreateResearchConferencePhaseItemRequest
                    {
                        RegistrationStartDate = today.AddDays(125),
                        RegistrationEndDate = today.AddDays(155),
                        AbstractDecideStatusStart = today.AddDays(160),
                        AbstractDecideStatusEnd = today.AddDays(165),
                        FullPaperStartDate = today.AddDays(170),
                        FullPaperEndDate = today.AddDays(180),
                        ReviewStartDate = today.AddDays(190),
                        ReviewEndDate = today.AddDays(200),
                        FullPaperDecideStatusStart = today.AddDays(202),
                        FullPaperDecideStatusEnd = today.AddDays(210),
                        ReviseStartDate = today.AddDays(215),
                        ReviseEndDate = today.AddDays(240),
                        RevisionPaperDecideStatusStart = today.AddDays(241),
                        RevisionPaperDecideStatusEnd = today.AddDays(245),
                        CameraReadyStartDate = today.AddDays(246),
                        CameraReadyEndDate = today.AddDays(260),
                        AuthorPaymentStart = today.AddDays(261),
                        AuthorPaymentEnd = today.AddDays(270),
                        RevisionRoundDeadlines = new List<CreateRevisionRoundDeadlineRequest>
                        {
                            new CreateRevisionRoundDeadlineRequest
                            {
                                StartSubmissionDate = today.AddDays(220),
                                EndSubmissionDate = today.AddDays(229)
                            },
                            new CreateRevisionRoundDeadlineRequest
                            {
                                StartSubmissionDate = today.AddDays(230),
                                EndSubmissionDate = today.AddDays(234)
                            }
                        }
                    }
                }
            };
        }

        private void SetupValidMocks(bool isEditable = true)
        {
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = "Test Research Conference",
                IsResearchConference = true,
                CreatedBy = "user-123",
                ConferenceStatusId = "status-preparing",
                StartDate = DateOnly.FromDateTime(DateTime.Now).AddDays(300),
                EndDate = DateOnly.FromDateTime(DateTime.Now).AddDays(310),
                TicketSaleStart = DateOnly.FromDateTime(DateTime.Now).AddDays(1),
                TicketSaleEnd = DateOnly.FromDateTime(DateTime.Now).AddDays(290)
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            _mockUnitOfWork
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("conf-123"))
                .ReturnsAsync(new ResearchConferenceDetail { ConferenceId = "conf-123", RevisionAttemptAllowed = 2, NumberPaperAccept = 100 });

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
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });
            }
            else
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusName = "Published" });
            }

            _mockTimeProviderService
                .Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            _mockUnitOfWork
                .Setup(u => u.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(It.IsAny<ResearchConferencePhase>()))
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(u => u.RevisionRoundDeadlineRepository.CreateCsAsync(It.IsAny<RevisionRoundDeadline>()))
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(u => u.ResearchConferencePhaseRepository.GetResearchPhaseByConfId("conf-123"))
                .ReturnsAsync(new List<ResearchConferencePhase>());

            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region CreateResearchConferencePhaseAsync Tests

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_PhasesListIsNull()
        {
            var request = new CreateResearchConferencePhasesRequest { Phases = null };
            SetupValidMocks();

            await Assert.ThrowsAsync<BadRequestException>(
                () => this.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
        {
            var request = CreateValidRequest();
            SetupValidMocks();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf")).ReturnsAsync((Conference)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => this.CreateResearchConferencePhaseAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_CreateSuccessfully_When_AllValidInputsProvided()
        {
            var request = CreateValidRequest();
            SetupValidMocks();

            var result = await this.CreateResearchConferencePhaseAsync("conf-123", request, "user-123");

            result.Should().NotBeNull();
            result.CreatedPhaseIds.Should().HaveCount(2);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        #endregion

        #region CreateNextResearchPhaseAsync Tests

        [Fact]
        public async Task CreateNextResearchPhaseAsync_Should_ThrowBadRequestException_When_DataInvalid()
        {
            SetupValidMocks();
            await Assert.ThrowsAsync<BadRequestException>(
                () => this.CreateNextResearchPhaseAsync("conf-123", null, "user-123")
            );
        }

        [Fact]
        public async Task CreateNextResearchPhaseAsync_Should_ThrowNotFoundException_When_ConferenceNotFound()
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("C1")).ReturnsAsync((Conference)null);
            var request = new CreateNextResearchPhaseRequest { NewPhase = new CreateResearchConferencePhaseItemRequest(), AuthorConferencePriceIds = new List<string> { "P1" } };

            await Assert.ThrowsAsync<NotFoundException>(
                () => this.CreateNextResearchPhaseAsync("C1", request, "U1")
            );
        }

        [Fact]
        public async Task CreateNextResearchPhaseAsync_Should_ThrowBadRequestException_When_NoLastPhase()
        {
            SetupValidMocks();
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetResearchConferencePhaseLastByConferenceIdAsync("conf-123"))
                .ReturnsAsync((ResearchConferencePhase)null);
            
            var request = new CreateNextResearchPhaseRequest 
            { 
                NewPhase = CreateValidRequest().Phases[0], 
                AuthorConferencePriceIds = new List<string> { "P1" } 
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => this.CreateNextResearchPhaseAsync("conf-123", request, "user-123")
            );
            Assert.Contains("Hội nghị chưa có phase nào", ex.Message);
        }

        [Fact]
        public async Task CreateNextResearchPhaseAsync_Should_ThrowBadRequestException_When_TimelineOverlap()
        {
            SetupValidMocks();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var lastPhase = new ResearchConferencePhase { AuthorPaymentEnd = today.AddDays(10) };
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetResearchConferencePhaseLastByConferenceIdAsync("conf-123"))
                .ReturnsAsync(lastPhase);
            
            var newPhase = CreateValidRequest().Phases[0];
            newPhase.RegistrationStartDate = today.AddDays(5); // Overlap with lastPhase.AuthorPaymentEnd (10)

            var request = new CreateNextResearchPhaseRequest 
            { 
                NewPhase = newPhase, 
                AuthorConferencePriceIds = new List<string> { "P1" } 
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => this.CreateNextResearchPhaseAsync("conf-123", request, "user-123")
            );
            Assert.Contains("phải sau ngày kết thúc của phase cuối cùng", ex.Message);
        }

        #endregion
    }
}