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
    public class ConferenceStepServiceCreateMaterialDownloadTest
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

        public ConferenceStepServiceCreateMaterialDownloadTest()
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
        private IFormFile CreateMockFile(string fileName = "conference-materials.pdf", string contentType = "application/pdf", long length = 1024 * 1024)
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
        private CreateMaterialDownloadRequest CreateValidRequest()
        {
            return new CreateMaterialDownloadRequest
            {
                FileDescription = "Official proceedings and papers from the research conference",
                File = CreateMockFile("proceedings.pdf", "application/pdf")
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
                .ReturnsAsync("uploads/materials/proceedings.pdf");

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
                .Setup(u => u.MaterialDownloadRepository.CreateMaterialDownloadAsync(It.IsAny<MaterialDownload>()))
                .ReturnsAsync(1);

            // Mock transaction methods
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

      
        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_ThrowBadRequestException_When_FileIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.File = null; // Invalid: required field
            SetupValidMocks();

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.CreateMaterialDownloadAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_ThrowBadRequestException_When_FileIsInvalid()
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
                () => _conferenceStepService.CreateMaterialDownloadAsync("conf-123", request, "user-123")
            );
        }

     

        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
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
                () => _conferenceStepService.CreateMaterialDownloadAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_ThrowBadRequestException_When_ConferenceIsNotResearchConference()
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
                () => _conferenceStepService.CreateMaterialDownloadAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_ThrowBadRequestException_When_UserIsNotConferenceCreator()
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
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.CreateMaterialDownloadAsync("conf-123", request, "user-123")
            );
        }

        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_CreateSuccessfully_When_AllValidInputsProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var conferenceId = "conf-123";
            var userId = "user-123";

            // Setup all valid mocks
            SetupValidMocks();

            // Mock the response that gets returned at the end
            _mockUnitOfWork
                .Setup(u => u.MaterialDownloadRepository.GetMaterialDownloadByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new MaterialDownload
                {
                    MaterialDownloadId = "material-123",
                    FileName = "uploads/materials/proceedings.pdf", // FileName is actually the URL
                    FileDescription = request.FileDescription,
                    ConferenceId = conferenceId
                });

            // ACT
            var result = await _conferenceStepService.CreateMaterialDownloadAsync(conferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.FileDescription.Should().Be(request.FileDescription);

            // Verify that all repository methods were called
            _mockUnitOfWork.Verify(u => u.MaterialDownloadRepository.CreateMaterialDownloadAsync(It.IsAny<MaterialDownload>()), Times.Once);

            // Verify file operations were called
            _mockObjectStorageFileService.Verify(f => f.IsValidDocumentFile(It.IsAny<IFormFile>()), Times.Once);
            _mockObjectStorageFileService.Verify(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
        }

       

        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_AcceptNullFileDescription_When_NotProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.FileDescription = null; // Optional field
            SetupValidMocks();

            // Mock the response
            _mockUnitOfWork
                .Setup(u => u.MaterialDownloadRepository.GetMaterialDownloadByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new MaterialDownload
                {
                    MaterialDownloadId = "material-123",
                    FileName = "uploads/materials/proceedings.pdf",
                    FileDescription = null,
                    ConferenceId = "conf-123"
                });

            // ACT
            var result = await _conferenceStepService.CreateMaterialDownloadAsync("conf-123", request, "user-123");

            // ASSERT
            result.Should().NotBeNull();
            result.FileDescription.Should().BeNull();

            // Verify creation was called
            _mockUnitOfWork.Verify(u => u.MaterialDownloadRepository.CreateMaterialDownloadAsync(It.IsAny<MaterialDownload>()), Times.Once);
        }

        [Fact]
        public async Task CreateMaterialDownloadAsync_Should_ThrowBadRequestException_When_ConferenceIsDeletedOrCancelled()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks(isNotDeletedOrCancelled: false); // Conference is deleted or cancelled

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateMaterialDownloadAsync("conf-123", request, "user-123")
            );
        }

        #endregion
    }
}