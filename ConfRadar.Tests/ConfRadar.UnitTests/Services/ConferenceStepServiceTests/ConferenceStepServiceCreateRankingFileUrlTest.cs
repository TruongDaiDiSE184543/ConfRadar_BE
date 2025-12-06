using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceCreateRankingFileUrlTest
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

        public ConferenceStepServiceCreateRankingFileUrlTest()
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
        /// Creates a mock file for testing
        /// </summary>
        private IFormFile CreateMockFile(string fileName = "ranking-file.pdf", string contentType = "application/pdf", long length = 1024 * 1024)
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
        private CreateRankingFileUrlRequest CreateValidRequest()
        {
            return new CreateRankingFileUrlRequest
            {
                FileUrl = "dummy-url-will-be-overwritten.pdf", // Meaningless, gets overwritten with uploaded file URL
                File = CreateMockFile("conference-ranking.pdf", "application/pdf")
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

            // Mock ObjectStorageFileService
            _mockObjectStorageFileService
                .Setup(f => f.IsValidDocumentFile(It.IsAny<IFormFile>()))
                .Returns(true);



            _mockObjectStorageFileService
                .Setup(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("uploads/ranking/conference-ranking.pdf");

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
                .Setup(u => u.RankingFileUrlRepository.CreateRankingFileUrlAsync(It.IsAny<RankingFileUrl>()))
                .ReturnsAsync(1);

            // Mock transaction methods
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_ThrowBadRequestException_When_FileIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.File = null; // Invalid: required field
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_ThrowBadRequestException_When_ConferenceIsDeletedOrCancelled()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks(isNotDeletedOrCancelled: false); // Conference is deleted or cancelled

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_ThrowBadRequestException_When_FileIsInvalid()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.File = CreateMockFile("virus.exe", "application/x-executable");

            // Setup valid mocks but invalid file validation
            SetupValidMocks();
            _mockObjectStorageFileService
                .Setup(f => f.IsValidDocumentFile(It.IsAny<IFormFile>()))
                .Returns(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123")
            );
        }



        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidRequest();

            // Setup mock to return null for nonexistent conference
            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf"))
                .ReturnsAsync((Conference)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.CreateRankingFileUrlAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_ThrowBadRequestException_When_ConferenceIsNotResearchConference()
        {
            // ARRANGE
            var request = CreateValidRequest();

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
                () => _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_ThrowBadRequestException_When_UserIsNotConferenceCreator()
        {
            // ARRANGE
            var request = CreateValidRequest();

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
                () => _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123")
            );
        }


        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_AcceptNullFileUrl_When_NotProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.FileUrl = null; // Optional field
            SetupValidMocks();

            // Mock the response
            _mockUnitOfWork
                .Setup(u => u.RankingFileUrlRepository.GetRankingFileUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingFileUrl
                {
                    RankingFileUrlId = "ranking-123",
                    FileUrl = "uploads/ranking/conference-ranking.pdf",
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.FileUrl.Should().NotBeNull(); // Should have uploaded file URL

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.RankingFileUrlRepository.CreateRankingFileUrlAsync(It.IsAny<RankingFileUrl>()), Times.Once);
        }




        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_AcceptValidPdfFile()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.File = CreateMockFile("ranking-document.pdf", "application/pdf", 2 * 1024 * 1024); // 2MB PDF
            SetupValidMocks();

            // Mock response
            _mockUnitOfWork
                .Setup(u => u.RankingFileUrlRepository.GetRankingFileUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingFileUrl
                {
                    RankingFileUrlId = "ranking-123",
                    FileUrl = "uploads/ranking/ranking-document.pdf",
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123");



            // Verify file validation was called
            _mockObjectStorageFileService.Verify(f => f.IsValidDocumentFile(It.IsAny<IFormFile>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.RankingFileUrlRepository.CreateRankingFileUrlAsync(It.IsAny<RankingFileUrl>()), Times.Once);
        }

        [Fact]
        public async Task CreateRankingFileUrlAsync_Should_AcceptValidDocxFile()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.File = CreateMockFile("ranking-doc.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 1024 * 1024);
            SetupValidMocks();

            // Mock response
            _mockUnitOfWork
                .Setup(u => u.RankingFileUrlRepository.GetRankingFileUrlByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new RankingFileUrl
                {
                    RankingFileUrlId = "ranking-123",
                    FileUrl = "uploads/ranking/ranking-doc.docx",
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateRankingFileUrlAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();


            // Verify creation was called
            _mockObjectStorageFileService.Verify(f => f.IsValidDocumentFile(It.IsAny<IFormFile>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.RankingFileUrlRepository.CreateRankingFileUrlAsync(It.IsAny<RankingFileUrl>()), Times.Once);
        }

        #endregion
    }
}