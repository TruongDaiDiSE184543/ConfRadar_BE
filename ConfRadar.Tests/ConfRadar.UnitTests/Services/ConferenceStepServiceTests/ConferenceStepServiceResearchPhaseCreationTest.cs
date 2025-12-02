using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceResearchPhaseCreationTest
    {
        #region Fields and Constructor

        // Mock all the dependencies that ConferenceStepService needs
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;

        // The actual service we're testing (CONCRETE CLASS, not interface)
        private readonly ConferenceStepService _conferenceStepService;

        // Configuration settings
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferenceStepServiceResearchPhaseCreationTest()
        {
            // Initialize all mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConferenceService = new Mock<IConferenceService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            // Create the configuration object
            _objectStorageSettings = new AppSettingConfig.ObjectStorageSettings
            {
                EndPoint = "https://test-storage.com/",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                Secure = true
            };

            // Create the service instance with all dependencies
            _conferenceStepService = new ConferenceStepService(
                _mockUnitOfWork.Object,                    // Pass the .Object property
                _mockObjectStorageFileService.Object,
                _mockTokenService.Object,
                Options.Create(_objectStorageSettings),    // Wrap settings in Options<T>
                _mockConferenceService.Object,
                _mockTimeProviderService.Object
            );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a valid request object for testing
        /// </summary>
        private CreateResearchConferencePhasesRequest CreateValidRequest()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            return new CreateResearchConferencePhasesRequest
            {
                Phases = new List<CreateResearchConferencePhaseItemRequest>
                {
                    // Main phase
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
                        CameraReadyDecideStatusStart = today.AddDays(116),
                        CameraReadyDecideStatusEnd = today.AddDays(120),
                        IsWaitlist = false,
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
                    // Waitlist phase
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
                        ReviseEndDate = today.AddDays(230),
                        RevisionPaperDecideStatusStart = today.AddDays(235),
                        RevisionPaperDecideStatusEnd = today.AddDays(240),
                        CameraReadyStartDate = today.AddDays(242),
                        CameraReadyEndDate = today.AddDays(260),
                        CameraReadyDecideStatusStart = today.AddDays(261),
                        CameraReadyDecideStatusEnd = today.AddDays(270),
                        IsWaitlist = true,
                        RevisionRoundDeadlines = new List<CreateRevisionRoundDeadlineRequest>
                        {
                            new CreateRevisionRoundDeadlineRequest
                            {
                                StartSubmissionDate = today.AddDays(220),
                                EndSubmissionDate = today.AddDays(229)
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Sets up common valid mocks for database repositories
        /// </summary>
        private void SetupValidMocks(bool isEditable = true)
        {
            // Mock ConferenceRepository - to verify conference exists and is research conference
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = "Test Research Conference",
                IsResearchConference = true,
                CreatedBy = "user-123",
                ConferenceStatusId = "status-preparing"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // Mock ResearchConferenceDetailRepository - to verify detail exists
            _mockUnitOfWork
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("conf-123"))
                .ReturnsAsync(new ResearchConferenceDetail { ConferenceId = "conf-123" , RevisionAttemptAllowed = 2});

            // Mock conference status for EnsureConferenceIsEditable method
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

            // Mock TimeProviderService
            _mockTimeProviderService
                .Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            // Mock Repository Create methods
            _mockUnitOfWork
                .Setup(u => u.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(It.IsAny<ResearchConferencePhase>()))
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(u => u.RevisionRoundDeadlineRepository.CreateCsAsync(It.IsAny<RevisionRoundDeadline>()))
                .ReturnsAsync(1);

            // Mock existing phases check
            _mockUnitOfWork
                .Setup(u => u.ResearchConferencePhaseRepository.GetResearchPhaseByConfId("conf-123"))
                .ReturnsAsync(new List<ResearchConferencePhase>());

            // Mock transaction methods
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_PhasesListIsEmpty()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Phases = new List<CreateResearchConferencePhaseItemRequest>(); // Invalid: empty list
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_PhasesListIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Phases = null; // Invalid: null list
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_LessThanTwoPhasesProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Phases = new List<CreateResearchConferencePhaseItemRequest>
            {
                request.Phases[0] // Only one phase, need at least 2
            };
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_NoWaitlistPhaseProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            // Set both phases as non-waitlist
            request.Phases[0].IsWaitlist = false;
            request.Phases[1].IsWaitlist = false;
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_MultipleWaitlistPhasesProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            // Set both phases as waitlist
            request.Phases[0].IsWaitlist = true;
            request.Phases[1].IsWaitlist = true;
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }


        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_DateSequenceIsInvalid()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var today = DateOnly.FromDateTime(DateTime.Now);
            // Invalid: registration end date before start date
            request.Phases[0].RegistrationStartDate = today.AddDays(30);
            request.Phases[0].RegistrationEndDate = today.AddDays(1);
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_IsWaitlistIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Phases[0].IsWaitlist = null; // Invalid: required field
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Setup mock to return null for nonexistent conference
            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf"))
                .ReturnsAsync((Conference)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_ConferenceIsNotResearchConference()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Setup mock conference that is NOT a research conference
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = "Technical Conference",
                IsResearchConference = false, // Not a research conference
                CreatedBy = "user-123"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_UserIsNotConferenceCreator()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Setup mock conference created by different user
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = "Research Conference",
                IsResearchConference = true,
                CreatedBy = "other-user-456" // Different user
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_ResearchConferenceDetailDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Setup valid conference but no research conference detail
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = "Research Conference",
                IsResearchConference = true,
                CreatedBy = "user-123"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            _mockUnitOfWork
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("conf-123"))
                .ReturnsAsync((ResearchConferenceDetail)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_PhasesAlreadyExist()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Setup valid conference and detail but existing phases
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = "Research Conference",
                IsResearchConference = true,
                CreatedBy = "user-123"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            _mockUnitOfWork
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("conf-123"))
                .ReturnsAsync(new ResearchConferenceDetail { ConferenceId = "conf-123" });

            // Setup existing phases
            _mockUnitOfWork
                .Setup(u => u.ResearchConferencePhaseRepository.GetResearchPhaseByConfId("conf-123"))
                .ReturnsAsync(new List<ResearchConferencePhase>
                {
                    new ResearchConferencePhase { ResearchConferencePhaseId = "phase-123"}
                });

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_ThrowBadRequestException_When_RevisionRoundDeadlineDatesAreInvalid()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var today = DateOnly.FromDateTime(DateTime.Now);
            // Invalid: revision round end date before start date
            request.Phases[0].RevisionRoundDeadlines[0].StartSubmissionDate = today.AddDays(88);
            request.Phases[0].RevisionRoundDeadlines[0].EndSubmissionDate = today.AddDays(81);
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_CreateSuccessfully_When_AllValidInputsProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var conferenceId = "conf-123";
            var userId = "user-123";

            // Setup all valid mocks
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.CreateResearchConferencePhaseAsync(conferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.Message.Should().NotBeNullOrEmpty();
            result.CreatedPhaseIds.Should().NotBeNull();
            result.CreatedPhaseIds.Should().HaveCount(2); // Main phase + waitlist phase

            // Verify that all repository methods were called correct number of times
            _mockUnitOfWork.Verify(u => u.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(It.IsAny<ResearchConferencePhase>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(u => u.RevisionRoundDeadlineRepository.CreateCsAsync(It.IsAny<RevisionRoundDeadline>()), Times.Exactly(3)); // 2 + 1 revision rounds
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_RollbackTransaction_When_ExceptionOccurs()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Simulate an exception during phase creation
            _mockUnitOfWork
                .Setup(u => u.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(It.IsAny<ResearchConferencePhase>()))
                .ThrowsAsync(new Exception("Database error"));

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123")
            );

            // Verify rollback was called
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_CreatePhasesWithCorrectWaitlistFlags()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();

            // Verify that phases were created with correct waitlist flags
            _mockUnitOfWork.Verify(
                u => u.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(
                    It.Is<ResearchConferencePhase>(p => p.IsWaitlist == false)), // Main phase
                Times.Once);

            _mockUnitOfWork.Verify(
                u => u.ResearchConferencePhaseRepository.CreateResearchConferencePhaseAsync(
                    It.Is<ResearchConferencePhase>(p => p.IsWaitlist == true)), // Waitlist phase
                Times.Once);
        }

        [Fact]
        public async Task CreateResearchConferencePhaseAsync_Should_AcceptNullRevisionRoundDeadlinesForWaitlistPhase_When_NotProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Phases[1].RevisionRoundDeadlines = null;
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.CreateResearchConferencePhaseAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.CreatedPhaseIds.Should().HaveCount(2);

            // Verify that revision round deadlines were not created
            _mockUnitOfWork.Verify(u => u.RevisionRoundDeadlineRepository.CreateCsAsync(It.IsAny<RevisionRoundDeadline>()), Times.Exactly(2));
        }

        #endregion
    }
}