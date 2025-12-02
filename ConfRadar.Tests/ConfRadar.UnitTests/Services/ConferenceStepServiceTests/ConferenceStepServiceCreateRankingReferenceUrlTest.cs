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
using Minio.Exceptions;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceCreateRankingReferenceUrlTest
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

        public ConferenceStepServiceCreateRankingReferenceUrlTest()
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
        private CreateRankingReferenceUrlRequest CreateValidRequest()
        {
            return new CreateRankingReferenceUrlRequest
            {
                ReferenceUrl = "https://www.core.edu.au/conference-portal?search=AI&by=all&source=CORE2023"
            };
        }

        /// <summary>
        /// Sets up common valid mocks for database repositories
        /// </summary>
        private void SetupValidMocks(bool isNotDeletedOrCancelled = true)
        {
            // Mock ConferenceRepository - to verify conference exists and is research conference
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = "Test Research Conference",
                IsResearchConference = true,
                CreatedBy = "user-123",
                ConferenceStatusId = isNotDeletedOrCancelled ? "status-active" : "status-deleted"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // Mock conference status for NotDeleteAndCancel method
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Deleted"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-deleted", ConferenceStatusName = "Deleted" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Cancelled"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-cancelled", ConferenceStatusName = "Cancelled" });

            // Mock TokenService
            _mockTokenService
                .Setup(t => t.GenerateSecureRandomToken())
                .Returns("random-token-123");

            // Mock TimeProviderService
            _mockTimeProviderService
                .Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            // Mock Repository Create methods
            _mockUnitOfWork
                .Setup(u => u.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(It.IsAny<RankingReferenceUrl>()))
                .ReturnsAsync(1);

            // Mock transaction methods
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_ReferenceUrlIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = null; // Invalid: required field
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_ReferenceUrlIsEmpty()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = ""; // Invalid: empty string
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_ReferenceUrlIsWhitespace()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = "   "; // Invalid: whitespace only
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }


        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_ReferenceUrlIsInvalidFormat()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = "not-a-valid-url"; // Invalid: not a proper URL format
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
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
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_ConferenceIsNotResearchConference()
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
                CreatedBy = "user-123",
                ConferenceStatusId = "status-active"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_UserIsNotConferenceCreator()
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
                ConferenceStatusId = "status-active"
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("conf-123"))
                .ReturnsAsync(mockConference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_CreateSuccessfully_When_AllValidInputsProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var conferenceId = "conf-123";
            var userId = "user-123";

            // Setup all valid mocks
            SetupValidMocks();

            // Mock the response that gets returned at the end
            _mockUnitOfWork
                .Setup(u => u.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingReferenceUrl
                {
                    ReferenceUrlId = "reference-123",
                    ReferenceUrl = request.ReferenceUrl,
                    ConferenceId = conferenceId,
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingReferenceUrlAsync(conferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.ReferenceUrl.Should().Be(request.ReferenceUrl);

            // Verify that all repository methods were called
            _mockUnitOfWork.Verify(u => u.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(It.IsAny<RankingReferenceUrl>()), Times.Once);
        }

       
        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_ConferenceIsDeletedOrCancelled()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks(isNotDeletedOrCancelled: false); // Conference is deleted or cancelled

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_AcceptValidHttpsUrl()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = "https://dblp.org/db/conf/icse/index.html";
            SetupValidMocks();

            // Mock response
            _mockUnitOfWork
                .Setup(u => u.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingReferenceUrl
                {
                    ReferenceUrlId = "reference-123",
                    ReferenceUrl = request.ReferenceUrl,
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.ReferenceUrl.Should().Be("https://dblp.org/db/conf/icse/index.html");

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(It.IsAny<RankingReferenceUrl>()), Times.Once);
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_AcceptValidHttpUrl()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = "http://portal.core.edu.au/conf-ranks/";
            SetupValidMocks();

            // Mock response
            _mockUnitOfWork
                .Setup(u => u.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingReferenceUrl
                {
                    ReferenceUrlId = "reference-123",
                    ReferenceUrl = request.ReferenceUrl,
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.ReferenceUrl.Should().Be("http://portal.core.edu.au/conf-ranks/");

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(It.IsAny<RankingReferenceUrl>()), Times.Once);
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_AcceptUrlWithQueryParameters()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = "https://scholar.google.com/citations?view_op=top_venues&hl=en&vq=eng_computerscienceartificialintelligence";
            SetupValidMocks();

            // Mock response
            _mockUnitOfWork
                .Setup(u => u.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingReferenceUrl
                {
                    ReferenceUrlId = "reference-123",
                    ReferenceUrl = request.ReferenceUrl,
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.ReferenceUrl.Should().Contain("scholar.google.com");
            result.ReferenceUrl.Should().Contain("view_op=top_venues");

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(It.IsAny<RankingReferenceUrl>()), Times.Once);
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_AcceptUrlWithFragment()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = "https://www.conference-ranking.org/Research.aspx?t=1&c=2#AI";
            SetupValidMocks();

            // Mock response
            _mockUnitOfWork
                .Setup(u => u.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingReferenceUrl
                {
                    ReferenceUrlId = "reference-123",
                    ReferenceUrl = request.ReferenceUrl,
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.ReferenceUrl.Should().Contain("#AI");

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(It.IsAny<RankingReferenceUrl>()), Times.Once);
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_ThrowBadRequestException_When_ReferenceUrlHasInvalidProtocol()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ReferenceUrl = "ftp://example.com/ranking.html"; // Invalid: ftp protocol
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingReferenceUrlAsync_Should_HandleLongValidUrl()
        {
            // ARRANGE
            var request = CreateValidRequest();
            // Create a long but valid URL (under 1000 chars)
            var longPath = string.Join("/", Enumerable.Repeat("conference", 30));
            request.ReferenceUrl = $"https://www.example-ranking-portal.com/{longPath}?search=AI&category=computer-science&year=2024&format=json&limit=100";
            
            // Ensure it's under the limit
            if (request.ReferenceUrl.Length >= 1000)
            {
                request.ReferenceUrl = request.ReferenceUrl.Substring(0, 999);
            }
            
            SetupValidMocks();

            // Mock response
            _mockUnitOfWork
                .Setup(u => u.RankingReferenceUrlRepository.GetRankingReferenceUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingReferenceUrl
                {
                    ReferenceUrlId = "reference-123",
                    ReferenceUrl = request.ReferenceUrl,
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingReferenceUrlAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.ReferenceUrl.Should().StartWith("https://www.example-ranking-portal.com");

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.RankingReferenceUrlRepository.CreateRankingReferenceUrlAsync(It.IsAny<RankingReferenceUrl>()), Times.Once);
        }

        #endregion
    }
}