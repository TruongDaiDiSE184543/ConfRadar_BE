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
    public class ConferenceStepServiceTechnicalCreationTest
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

        public ConferenceStepServiceTechnicalCreationTest()
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
        private IFormFile CreateMockImageFile(string fileName = "banner.jpg", string contentType = "image/jpeg", long length = 1024 * 1024)
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
        private CreateTechnicalConferenceBasicRequest CreateValidRequest()
        {
            return new CreateTechnicalConferenceBasicRequest
            {
                ConferenceName = "Test Technical Conference",
                IsResearchConference = false,
                TotalSlot = 100,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(32)),
                TicketSaleStart = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TicketSaleEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(29)),
                ConferenceCategoryId = "cat-123",
                CityId = "city-123",
                BannerImageFile = CreateMockImageFile(),
                targetAudienceTechnicalConference = "Software Developers and IT Professionals"
            };
        }

        /// <summary>
        /// Sets up common valid mocks for database repositories
        /// </summary>
        private void SetupValidMocks()
        {
            // Mock ConferenceCategoryRepository
            _mockUnitOfWork
                .Setup(u => u.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync("cat-123"))
                .ReturnsAsync(new ConferenceCategory { ConferenceCategoryId = "cat-123", ConferenceCategoryName = "Technology" });

            // Mock CityRepository
            _mockUnitOfWork
                .Setup(u => u.CityRepository.GetCityByIdAsync("city-123"))
                .ReturnsAsync(new City { CityId = "city-123", CityName = "Ho Chi Minh City" });

            // Mock ConferenceStatusRepository
            _mockUnitOfWork
                .Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });

            // Mock ObjectStorageFileService
            _mockObjectStorageFileService
                .Setup(f => f.IsValidImageFile(It.IsAny<IFormFile>()))
                .Returns(true);

            _mockObjectStorageFileService
                .Setup(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("uploads/banner.jpg");

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
                .Setup(u => u.ConferenceRepository.CreateConferenceAsync(It.IsAny<Conference>()))
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(u => u.TechnicalConferenceDetailRepository.CreateTechnicalAsync(It.IsAny<TechnicalConferenceDetail>()))
                .ReturnsAsync(1);

            // Mock transaction methods
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_ConferenceNameIsEmpty()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ConferenceName = ""; // Invalid: empty name

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            // Verify the exception message contains expected Vietnamese text
            exception.Message.Should().Contain("Tên hội nghị là bắt buộc");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_ConferenceNameIsWhitespace()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ConferenceName = "   "; // Invalid: whitespace only

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Tên hội nghị là bắt buộc");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_IsResearchConferenceIsTrue()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.IsResearchConference = true; // Invalid for technical conference creation

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Chức năng này dùng để tạo hội nghị kỹ thuật");
            exception.Message.Should().Contain("IsResearchConference' phải là false");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_TotalSlotIsZeroOrNegative()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.TotalSlot = 0; // Invalid: zero slots

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Tổng số vé phải là một số dương");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_DatesAreInvalid()
        {
            // ARRANGE
            var request = CreateValidRequest();
            // Invalid: start date is after end date
            DateOnly today = ExtensionHelper.GetVietnamDate();
            request.StartDate = today.AddDays(3);
            request.EndDate = today.AddDays(30);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Ngày tháng cung cấp không hợp lệ");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowNotFoundException_When_ConferenceCategoryDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.ConferenceCategoryId = "nonexistent-category";

            // Setup mock to return null for nonexistent category
            _mockUnitOfWork
                .Setup(u => u.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync("nonexistent-category"))
                .ReturnsAsync((ConferenceCategory)null);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Danh mục hội nghị với ID");
            exception.Message.Should().Contain("không tồn tại");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowNotFoundException_When_CityDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.CityId = "nonexistent-city";

            // Setup valid category but invalid city
            _mockUnitOfWork
                .Setup(u => u.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync("cat-123"))
                .ReturnsAsync(new ConferenceCategory { ConferenceCategoryId = "cat-123" });

            _mockUnitOfWork
                .Setup(u => u.CityRepository.GetCityByIdAsync("nonexistent-city"))
                .ReturnsAsync((City)null);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Thành phố với ID");
            exception.Message.Should().Contain("không tồn tại");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_BannerImageFileIsNull()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.BannerImageFile = null; // Invalid: no banner file
            SetupValidMocks();

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Ảnh bìa (banner) là bắt buộc");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_BannerImageFileIsInvalid()
        {
            // ARRANGE
            var request = CreateValidRequest();
            request.BannerImageFile = CreateMockImageFile("document.txt", "text/plain"); // Invalid file type

            // Setup valid dependencies but invalid file
            SetupValidMocks();
            _mockObjectStorageFileService
                .Setup(f => f.IsValidImageFile(It.IsAny<IFormFile>()))
                .Returns(false); // File validation fails

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Loại ảnh bìa không được hỗ trợ");

            // Verify file validation was called
            _mockObjectStorageFileService.Verify(
                f => f.IsValidImageFile(It.IsAny<IFormFile>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_ThrowBadRequestException_When_BannerImageFileIsTooLarge()
        {
            // ARRANGE
            var request = CreateValidRequest();
            // Create file larger than 5MB
            request.BannerImageFile = CreateMockImageFile("largefile.jpg", "image/jpeg", 6 * 1024 * 1024);

            // Setup valid dependencies
            SetupValidMocks();

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            exception.Message.Should().Contain("Kích thước tệp ảnh bìa không được vượt quá 5 MB");
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_CreateSuccessfully_When_AllValidInputsProvided()
        {
            // ARRANGE
            var request = CreateValidRequest();
            var userId = "user-123";

            // Setup all valid mocks
            SetupValidMocks();

            // Mock the GetConferenceBasicAsync method that gets called at the end
            var expectedResponse = new TechnicalConferenceBasicStepResponse
            {
                conferenceId = "conf-123",
                ConferenceName = request.ConferenceName,
                IsResearchConference = false,
                TotalSlot = request.TotalSlot,
                TargetAudience = request.targetAudienceTechnicalConference
            };

            // We need to mock the GetConferenceBasicAsync call that happens at the end
            var mockConference = new Conference
            {
                ConferenceId = "conf-123",
                ConferenceName = request.ConferenceName,
                IsResearchConference = false,
                TotalSlot = request.TotalSlot,
                CreatedBy = userId
            };

            var mockTechnicalDetail = new TechnicalConferenceDetail
            {
                ConferenceId = "conf-123",
                TargetAudience = request.targetAudienceTechnicalConference
            };

            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(mockConference);

            _mockUnitOfWork
                .Setup(u => u.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(It.IsAny<string>()))
                .ReturnsAsync(mockTechnicalDetail);

            // ACT
            var result = await _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.ConferenceName.Should().Be(request.ConferenceName);
            result.IsResearchConference.Should().BeFalse();
            result.TotalSlot.Should().Be(request.TotalSlot);
            result.TargetAudience.Should().Be(request.targetAudienceTechnicalConference);

            // Verify that all repository methods were called
            _mockUnitOfWork.Verify(u => u.ConferenceRepository.CreateConferenceAsync(It.IsAny<Conference>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.TechnicalConferenceDetailRepository.CreateTechnicalAsync(It.IsAny<TechnicalConferenceDetail>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);

            // Verify file operations were called
            _mockObjectStorageFileService.Verify(f => f.IsValidImageFile(It.IsAny<IFormFile>()), Times.Once);
            _mockObjectStorageFileService.Verify(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateTechnicalConferenceBasicAsync_Should_RollbackTransaction_When_ExceptionOccurs()
        {
            // ARRANGE
            var request = CreateValidRequest();
            SetupValidMocks();

            // Simulate an exception during conference creation
            _mockUnitOfWork
                .Setup(u => u.ConferenceRepository.CreateConferenceAsync(It.IsAny<Conference>()))
                .ThrowsAsync(new Exception("Database error"));

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.CreateTechnicalConferenceBasicAsync(request, "user-123")
            );

            // Verify rollback was called
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        #endregion
    }
}