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
    public class ConferenceStepServiceAddMediaTests
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceAddMediaTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConferenceService = new Mock<IConferenceService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            var objectStorageSettings = new AppSettingConfig.ObjectStorageSettings();

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

        private Conference CreateTechnicalConference(string userId = "user-123")
        {
            return new Conference
            {
                ConferenceId = "conf-tech-123",
                CreatedBy = userId,
                IsResearchConference = false,
                ConferenceStatus = new ConferenceStatus { ConferenceStatusName = "Preparing" }
            };
        }

        private AddConferenceMediaRequest CreateValidAddMediaRequest(string contentType = "image/jpeg")
        {
            var mockFile = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            mockFile.Setup(_ => _.FileName).Returns(fileName);
            mockFile.Setup(_ => _.Length).Returns(ms.Length);
            mockFile.Setup(_ => _.ContentType).Returns(contentType);

            return new AddConferenceMediaRequest
            {
                Media = new List<CreateConferenceMediaRequest>
                {
                    new CreateConferenceMediaRequest { MediaFile = mockFile.Object }
                }
            };
        }

        private void SetupValidMocks(Conference conference)
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(conference.ConferenceId))
                           .ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task AddConferenceMediaAsync_Should_Succeed_ForValidRequest()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddMediaRequest();
            var userId = "user-123";
            var uploadedUrl = "http://example.com/media.jpg";
            SetupValidMocks(conference);

            _mockObjectStorageFileService.Setup(s => s.IsValidImageFile(It.IsAny<IFormFile>())).Returns(true);
            _mockObjectStorageFileService.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                                         .ReturnsAsync(uploadedUrl);
            _mockUnitOfWork.Setup(u => u.ConferenceMediaRepository.CreateConferenceMediaAsync(It.IsAny<ConferenceMedium>()))
                           .Returns(Task.FromResult(1));


            // ACT
            var result = await _conferenceStepService.AddConferenceMediaAsync(conference.ConferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].MediaUrl.Should().Be(uploadedUrl);

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockObjectStorageFileService.Verify(s => s.UploadFileAsync("conferencemedia", It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.ConferenceMediaRepository.CreateConferenceMediaAsync(It.IsAny<ConferenceMedium>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task AddConferenceMediaAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidAddMediaRequest();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf"))
                           .ReturnsAsync((Conference)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.AddConferenceMediaAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task AddConferenceMediaAsync_Should_ThrowException_When_UserIsNotCreator()
        {
            // ARRANGE
            var conference = CreateTechnicalConference(userId: "creator-id");
            var request = CreateValidAddMediaRequest();
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceMediaAsync(conference.ConferenceId, request, "other-user-id")
            );
        }

        [Fact]
        public async Task AddConferenceMediaAsync_Should_ThrowNullReferenceException_When_RequestIsNull()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _conferenceStepService.AddConferenceMediaAsync(conference.ConferenceId, null, "user-123")
            );
        }

        [Fact]
        public async Task AddConferenceMediaAsync_Should_ThrowArgumentNullException_When_MediaListIsNull()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var nullMediaRequest = new AddConferenceMediaRequest { Media = null };
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _conferenceStepService.AddConferenceMediaAsync(conference.ConferenceId, nullMediaRequest, "user-123")
            );
        }

        [Fact]
        public async Task AddConferenceMediaAsync_Should_ThrowException_When_MediaListIsEmpty()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var emptyMediaRequest = new AddConferenceMediaRequest { Media = new List<CreateConferenceMediaRequest>() };
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceMediaAsync(conference.ConferenceId, emptyMediaRequest, "user-123")
            );
        }

        [Fact]
        public async Task AddConferenceMediaAsync_Should_ThrowNullReferenceException_When_MediaFileIsNull()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var requestWithNullFile = new AddConferenceMediaRequest { Media = new List<CreateConferenceMediaRequest> { } };
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceMediaAsync(conference.ConferenceId, requestWithNullFile, "user-123")
            );
        }

        [Fact]
        public async Task AddConferenceMediaAsync_Should_RollbackTransaction_When_FileTypeIsInvalid()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddMediaRequest("application/zip");
            SetupValidMocks(conference);
            _mockObjectStorageFileService.Setup(s => s.IsValidImageFile(It.IsAny<IFormFile>())).Returns(false);
            _mockObjectStorageFileService.Setup(s => s.IsValidVideoFile(It.IsAny<IFormFile>())).Returns(false);

            // ACT
            var result = await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceMediaAsync(conference.ConferenceId, request, "user-123")) ;

            // ASSERT
            result.Message.Should().Contain("Không hỗ trợ định ");
        }

        #endregion
    }
}
