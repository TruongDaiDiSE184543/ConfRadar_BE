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
using ConfRadar.Repositories.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceAddResearchSessionsTests
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

        public ConferenceStepServiceAddResearchSessionsTests()
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
        /// Creates a mock image file for testing
        /// </summary>
        private IFormFile CreateMockImageFile(string fileName = "session-media.jpg", string contentType = "image/jpeg", long length = 1024 * 1024)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(length);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
            return mockFile.Object;
        }

        /// <summary>
        /// Creates a valid request object for testing
        /// </summary>
        private AddResearchSessionsRequest CreateValidRequest()
        {
            var today = ExtensionHelper.GetVietnamDate();
            
            return new AddResearchSessionsRequest
            {
                Sessions = new List<CreateResearchSessionRequest>
                {
                    new CreateResearchSessionRequest
                    {
                        Title = "Opening Keynote",
                        Description = "Welcome and overview of the research conference",
                        StartTime = new TimeOnly(9, 0, 0),
                        EndTime = new TimeOnly(10, 0, 0),
                        Date = today.AddDays(25),
                        RoomId = "room-123",
                        SessionMedias = new List<CreateConferenceSessionMediaRequest>
                        {
                            new CreateConferenceSessionMediaRequest
                            {
                                MediaFile = CreateMockImageFile("keynote-slide.pdf", "application/pdf")
                            }
                        }
                    },
                    new CreateResearchSessionRequest
                    {
                        Title = "Research Paper Presentations",
                        Description = "Presentations of accepted research papers",
                        StartTime = new TimeOnly(10, 30, 0),
                        EndTime = new TimeOnly(12, 0, 0),
                        Date = today.AddDays(30),
                        RoomId = "room-456",
                        SessionMedias = new List<CreateConferenceSessionMediaRequest>
                        {
                            new CreateConferenceSessionMediaRequest
                            {
                                MediaFile = CreateMockImageFile("research-slides.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")
                            }
                        }
                    },
                     new CreateResearchSessionRequest
                    {
                        Title = "Ending and rewarding",
                        Description = "Best papers featuring for furthur nomination",
                        StartTime = new TimeOnly(10, 30, 0),
                        EndTime = new TimeOnly(12, 0, 0),
                        Date = today.AddDays(35),
                        RoomId = "room-456",
                        SessionMedias = new List<CreateConferenceSessionMediaRequest>
                        {
                            new CreateConferenceSessionMediaRequest
                            {
                                MediaFile = CreateMockImageFile("research-slides.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")
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
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(25)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(35)),
                ConferenceStatusId = "status-preparing"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // Mock RoomRepository
            _mockUnitOfWork
                .Setup(u => u.RoomRepository.GetRoomByIdAsync("room-123"))
                .ReturnsAsync(new Room { RoomId = "room-123", DisplayName = "Main Hall", Number = "A101" });

            _mockUnitOfWork
                .Setup(u => u.RoomRepository.GetRoomByIdAsync("room-456"))
                .ReturnsAsync(new Room { RoomId = "room-456", DisplayName = "Conference Room B", Number = "B201" });
            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionRepository.GetSessionsByConferenceIdAsync("con-123"))
                .ReturnsAsync(new List<ConferenceSession>());

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" });
            // Mock conference status for EnsureConferenceIsEditable method
            if (isEditable)
            {
                
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });
            }
            else
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusName = "Published" });
            }

            // Mock ObjectStorageFileService
            _mockObjectStorageFileService
                .Setup(f => f.IsValidImageFile(It.IsAny<IFormFile>()))
                .Returns(true);

            _mockObjectStorageFileService
                .Setup(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("uploads/session-media.jpg");

            // Mock TokenService
            _mockTokenService
                .Setup(t => t.GenerateSecureRandomToken())
                .Returns("random-token-123");

            // Mock TimeProviderService
            _mockTimeProviderService
                .Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            // Mock ConferenceSessionRepository for checkEachDateHasConferenceSession method
            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionRepository.GetSessionsByConferenceIdAsync("conf-123"))
                .ReturnsAsync(new List<ConferenceSession>()); // Return empty list (no existing sessions)

            // Mock ConferenceSessionRepository for ValidateSessionTimeAvailability method
            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<ConferenceSession>()); // Return empty list (no conflicting sessions)

            // Mock ConferenceSessionRepository for GetSessionWithDetailsAsync method (used for response mapping)
            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionRepository.GetSessionWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((string sessionId) => new ConferenceSession{});

            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync("room-124", ExtensionHelper.GetVietnamDate().AddDays(25)))
                .ReturnsAsync(new List<ConferenceSession>());

            // Mock Repository Create methods
            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()))
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(It.IsAny<ConferenceSessionMedium>()))
                .ReturnsAsync(1);

            // Mock transaction methods
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionsListIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions = null; // Invalid: null sessions
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionsListIsEmpty()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions = new List<CreateResearchSessionRequest>(); // Invalid: empty list
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionTitleIsEmpty()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].Title = ""; // Invalid: empty title
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionTitleIsWhitespace()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].Title = "   "; // Invalid: whitespace only
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

      

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_StartTimeIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].StartTime = null; // Invalid: required field
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_EndTimeIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].EndTime = null; // Invalid: required field
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_DateIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].Date = null; // Invalid: required field
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_StartTimeIsAfterEndTime()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].StartTime = new TimeOnly(12, 0, 0);
            request.Sessions[0].EndTime = new TimeOnly(10, 0, 0); // Invalid: end before start
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionDateIsBeforeConferenceStart()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].Date = DateOnly.FromDateTime(DateTime.Now.AddDays(20)); // Before conference start
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionDateIsAfterConferenceEnd()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].Date = DateOnly.FromDateTime(DateTime.Now.AddDays(40)); // After conference end
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
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
                () => _conferenceStepService.AddResearchSessionsAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_ConferenceIsNotResearchConference()
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
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_UserIsNotConferenceCreator()
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
                CreatedBy = "other-user-456", // Different user
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(25)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(35))
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowNotFoundException_When_RoomDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].RoomId = "nonexistent-room";

            // Setup valid conference but nonexistent room
            SetupValidMocks();
            _mockUnitOfWork
                .Setup(u => u.RoomRepository.GetRoomByIdAsync("nonexistent-room"))
                .ReturnsAsync((Room)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionMediaFileIsInvalid()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].SessionMedias[0].MediaFile = CreateMockImageFile("invalid.exe", "application/x-executable");

            // Setup valid mocks but invalid file validation
            SetupValidMocks();
            _mockObjectStorageFileService
                .Setup(f => f.IsValidImageFile(It.IsAny<IFormFile>()))
                .Returns(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_ThrowBadRequestException_When_SessionMediaFileIsTooLarge()
        {
            // ARRANGE
            var request = CreateValidRequest();
            // Create file larger than 5MB
            request.Sessions[0].SessionMedias[0].MediaFile = CreateMockImageFile("largefile.jpg", "image/jpeg", 6 * 1024 * 1024);
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_CreateSuccessfully_When_AllValidInputsProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var conferenceId = "conf-123";
            var userId = "user-123";

            // Setup all valid mocks
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.AddResearchSessionsAsync(conferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ResearchSessionWithMediaResponse>>();
            result.Should().HaveCount(3); // Two sessions created

            // Verify that all repository methods were called correct number of times
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()), Times.Exactly(3));
            _mockUnitOfWork.Verify(u => u.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(It.IsAny<ConferenceSessionMedium>()), Times.Exactly(3)); // One media per session
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);

            // Verify file operations were called for each media file
            _mockObjectStorageFileService.Verify(f => f.IsValidImageFile(It.IsAny<IFormFile>()), Times.Exactly(3));
            _mockObjectStorageFileService.Verify(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_RollbackTransaction_When_ExceptionOccurs()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Simulate an exception during session creation
            _mockUnitOfWork
                .Setup(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()))
                .ThrowsAsync(new Exception("Database error"));

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123")
            );

            // Verify rollback was called
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_AcceptNullSessionMedias_When_NotProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].SessionMedias = null; // No media files
            request.Sessions[1].SessionMedias = null;
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            // Verify that session media creation was not called
            _mockUnitOfWork.Verify(u => u.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(It.IsAny<ConferenceSessionMedium>()), Times.Once);
            
            // Verify that sessions were still created
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()), Times.Exactly(3));
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_AcceptNullRoomId_When_NotProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].RoomId = null; // No room specified
            request.Sessions[1].RoomId = null;
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            // Verify that room validation was not called
            _mockUnitOfWork.Verify(u => u.RoomRepository.GetRoomByIdAsync(It.IsAny<string>()), Times.Once);
            
            // Verify that sessions were created without rooms
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()), Times.Exactly(3));
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_HandleEmptySessionMediasList_When_EmptyListProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.Sessions[0].SessionMedias = new List<CreateConferenceSessionMediaRequest>(); // Empty list
            request.Sessions[1].SessionMedias = new List<CreateConferenceSessionMediaRequest>();
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            // Verify that session media creation was not called
            _mockUnitOfWork.Verify(u => u.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(It.IsAny<ConferenceSessionMedium>()), Times.Once);
            
            // Verify that sessions were created
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()), Times.Exactly(3));
        }

        [Fact]
        public async Task AddResearchSessionsAsync_Should_CreateSessionsWithoutSpeakers_When_ResearchConference()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // ACT
            var result = await _conferenceStepService.AddResearchSessionsAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            // Verify that no speaker creation was attempted (research conferences don't have speakers in sessions)
            _mockUnitOfWork.Verify(u => u.SpeakerRepository.CreateSpeakerAsync(It.IsAny<Speaker>()), Times.Never);
            
            // Verify sessions were created correctly
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()), Times.Exactly(3));
        }

     

        #endregion
    }
}