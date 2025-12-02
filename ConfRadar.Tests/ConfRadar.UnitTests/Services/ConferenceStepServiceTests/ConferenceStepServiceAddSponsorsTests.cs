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
    public class ConferenceStepServiceAddSponsorsTests
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceAddSponsorsTests()
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

        private AddConferenceSponsorsRequest CreateValidAddSponsorsRequest()
        {
            var mockFile = new Mock<IFormFile>();
            var content = "Fake Sponsor Image";
            var fileName = "sponsor.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            mockFile.Setup(_ => _.FileName).Returns(fileName);
            mockFile.Setup(_ => _.Length).Returns(ms.Length);
            mockFile.Setup(_ => _.ContentType).Returns("image/png");

            return new AddConferenceSponsorsRequest
            {
                Sponsors = new List<CreateSponsorRequest>
                {
                    new CreateSponsorRequest
                    {
                        Name = "Awesome Sponsor",
                        ImageFile = mockFile.Object
                    }
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
        public async Task AddConferenceSponsorsAsync_Should_Succeed_ForValidRequest()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSponsorsRequest();
            var userId = "user-123";
            var uploadedUrl = "http://example.com/sponsor.png";
            SetupValidMocks(conference);
            _mockObjectStorageFileService.Setup(s => s.IsValidImageFile(It.IsAny<IFormFile>())).Returns(true);
            _mockObjectStorageFileService.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                                         .ReturnsAsync(uploadedUrl);
            _mockUnitOfWork.Setup(u => u.SponsorRepository.CreateSponsorAsync(It.IsAny<Sponsor>()))
                           .Returns(Task.FromResult(1));

            // ACT
            var result = await _conferenceStepService.AddConferenceSponsorsAsync(conference.ConferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Awesome Sponsor");
            result[0].ImageUrl.Should().Be(uploadedUrl);

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockObjectStorageFileService.Verify(s => s.UploadFileAsync("sponsorimage", It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SponsorRepository.CreateSponsorAsync(It.IsAny<Sponsor>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task AddConferenceSponsorsAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidAddSponsorsRequest();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf"))
                           .ReturnsAsync((Conference)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.AddConferenceSponsorsAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task AddConferenceSponsorsAsync_Should_ThrowException_When_UserIsNotCreator()
        {
            // ARRANGE
            var conference = CreateTechnicalConference(userId: "creator-id");
            var request = CreateValidAddSponsorsRequest();
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceSponsorsAsync(conference.ConferenceId, request, "other-user-id")
            );
        }

        [Fact]
        public async Task AddConferenceSponsorsAsync_Should_ThrowBadRequestException_When_ImageFileIsInvalid()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();

            var mockInvalidFile = new Mock<IFormFile>();
            mockInvalidFile.Setup(f => f.ContentType).Returns("application/pdf");
            var invalidRequest = new AddConferenceSponsorsRequest
            {
                Sponsors = new List<CreateSponsorRequest>
                {
                    new CreateSponsorRequest
                    {
                        Name = "Invalid Sponsor",
                        ImageFile = mockInvalidFile.Object
                    }
                }
            };

            SetupValidMocks(conference);
            _mockObjectStorageFileService.Setup(s => s.IsValidImageFile(It.IsAny<IFormFile>())).Returns(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSponsorsAsync(conference.ConferenceId, invalidRequest, "user-123")
            );
        }

        #endregion
    }
}