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
using Microsoft.IdentityModel.Tokens;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceResearchDetailCreationTest
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

        public ConferenceStepServiceResearchDetailCreationTest()
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
        private CreateResearchConferenceDetailRequest CreateValidRequest()
        {
            return new CreateResearchConferenceDetailRequest
            {
                PaperFormat = "ieee",
                NumberPaperAccept = 50,
                RevisionAttemptAllowed = 3,
                RankingDescription = "This conference is ranked A in computer science",
                AllowListener = true,
                RankValue = "A",
                RankYear = 2024,
                ReviewFee = 100.50m,
                RankingCategoryId = "ranking-cat-123"
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

            // Mock RankingCategoryRepository
            _mockUnitOfWork
                .Setup(u => u.RankingCategoryRepository.GetRankingCategoryByIdAsync("ranking-cat-123"))
                .ReturnsAsync(new RankingCategory { RankingCategoryId = "ranking-cat-123", RankName = "Computer Science" });

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
                .Setup(u => u.ResearchConferenceDetailRepository.CreateResearchConferenceDetailAsync(It.IsAny<ResearchConferenceDetail>()))
                .ReturnsAsync(0);

            // Mock transaction methods
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

      
        

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_NumberPaperAcceptIsZeroOrNegative()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();
            request.NumberPaperAccept = 0; // Invalid: zero papers

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_RevisionAttemptAllowedIsZeroOrNegative()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();
            request.RevisionAttemptAllowed = 0; // Invalid: zero revisions

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_RevisionAttemptAllowedIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();
            request.RevisionAttemptAllowed = null; // Invalid: required field

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_RankYearIsInvalid()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();
            request.RankYear = 1999; // Invalid: year before 2000

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_ReviewFeeIsNegative()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();
            request.ReviewFee = -10.50m; // Invalid: negative fee

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

    
        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
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
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_ConferenceIsNotResearchConference()
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
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_UserIsNotConferenceCreator()
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
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowNotFoundException_When_RankingCategoryDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();
            request.RankingCategoryId = "nonexistent-ranking";

            // Setup valid conference but invalid ranking category
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
                .Setup(u => u.RankingCategoryRepository.GetRankingCategoryByIdAsync("nonexistent-ranking"))
                .ReturnsAsync((RankingCategory)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_CreateSuccessfully_When_AllValidInputsProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var conferenceId = "conf-123";
            var userId = "user-123";

            // Setup all valid mocks
            SetupValidMocks();


            // Mock the GetResearchConferenceDetailAsync call that happens at the end
            var sequence = new MockSequence();
            _mockUnitOfWork.InSequence(sequence)
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId))
                .ReturnsAsync((ResearchConferenceDetail)null); // First call for validation check

            _mockUnitOfWork.InSequence(sequence)
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId))
                .ReturnsAsync(new ResearchConferenceDetail
                {
                    ConferenceId = conferenceId,
                    PaperFormat = request.PaperFormat,
                    NumberPaperAccept = request.NumberPaperAccept,
                    RevisionAttemptAllowed = request.RevisionAttemptAllowed.Value,
                    RankingDescription = request.RankingDescription,
                    AllowListener = request.AllowListener.Value,
                    RankValue = request.RankValue,
                    RankYear = request.RankYear,
                    ReviewFee = request.ReviewFee,
                    RankingCategoryId = request.RankingCategoryId
                }); // Second call for final response

            // ACT
            var result = await _conferenceStepService.CreateResearchConferenceDetailAsync(conferenceId,request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.ConferenceId.Should().Be(conferenceId);
            result.PaperFormat.Should().Be(request.PaperFormat);
            result.NumberPaperAccept.Should().Be(request.NumberPaperAccept);
            result.RevisionAttemptAllowed.Should().Be(request.RevisionAttemptAllowed);
            result.RankingDescription.Should().Be(request.RankingDescription);
            result.AllowListener.Should().Be(request.AllowListener);
            result.RankValue.Should().Be(request.RankValue);
            result.RankYear.Should().Be(request.RankYear);
            result.ReviewFee.Should().Be(request.ReviewFee);
            result.RankingCategoryId.Should().Be(request.RankingCategoryId);

            // Verify that all repository methods were called
            _mockUnitOfWork.Verify(u => u.ResearchConferenceDetailRepository.CreateResearchConferenceDetailAsync(It.IsAny<ResearchConferenceDetail>()), Times.Once);
        }

   
        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_ThrowBadRequestException_When_ConferenceDetailAlreadyExists()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();
            // Setup valid conference
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

            // Setup existing research conference detail
            _mockUnitOfWork
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("conf-123"))
                .ReturnsAsync(new ResearchConferenceDetail { ConferenceId = "conf-123" });

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateResearchConferenceDetailAsync_Should_AcceptZeroReviewFee_When_ConferenceIsFree()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReviewFee = 0; // Zero fee is valid (free conference)
            SetupValidMocks();

            // Mock the response
            var sequence = new MockSequence();
            _mockUnitOfWork.InSequence(sequence)
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("conf-123"))
                .ReturnsAsync((ResearchConferenceDetail)null); // First call for validation check

            _mockUnitOfWork.InSequence(sequence)
                .Setup(u => u.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync("conf-123"))
                .ReturnsAsync(new ResearchConferenceDetail
                {
                    ConferenceId = "conf-123",
                    PaperFormat = request.PaperFormat,
                    NumberPaperAccept = request.NumberPaperAccept,
                    RevisionAttemptAllowed = request.RevisionAttemptAllowed.Value,
                    RankingDescription = request.RankingDescription,
                    AllowListener = request.AllowListener.Value,
                    RankValue = request.RankValue,
                    RankYear = request.RankYear,
                    ReviewFee = 0,
                    RankingCategoryId = request.RankingCategoryId
                }); // Second call for final response

            // ACT
            var result = await _conferenceStepService.CreateResearchConferenceDetailAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.ReviewFee.Should().Be(0);

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.ResearchConferenceDetailRepository.CreateResearchConferenceDetailAsync(It.IsAny<ResearchConferenceDetail>()), Times.Once);
        }

        

        #endregion
    }
}