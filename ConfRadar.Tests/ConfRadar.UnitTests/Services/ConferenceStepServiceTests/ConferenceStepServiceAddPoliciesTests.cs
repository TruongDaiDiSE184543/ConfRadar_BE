using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    public class ConferenceStepServiceAddPoliciesTests
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceAddPoliciesTests()
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

        private AddConferencePoliciesRequest CreateValidAddPoliciesRequest()
        {
            return new AddConferencePoliciesRequest
            {
                Policies = new List<CreateConferencePolicyRequest>
                {
                    new CreateConferencePolicyRequest
                    {
                        PolicyName = "Refund Policy",
                        Description = "Details about refunds."
                    },
                    new CreateConferencePolicyRequest
                    {
                        PolicyName = "Code of Conduct",
                        Description = "Expected behavior for attendees."
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
        public async Task AddConferencePoliciesAsync_Should_Succeed_ForValidRequest()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddPoliciesRequest();
            var userId = "user-123";
            SetupValidMocks(conference);

            _mockUnitOfWork.Setup(u => u.ConferencePolicyRepository.CreateConferencePolicyAsync(It.IsAny<Policy>()))
                           .Returns(Task.FromResult(1));

            // ACT
            var result = await _conferenceStepService.AddConferencePoliciesAsync(conference.ConferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].PolicyName.Should().Be("Refund Policy");

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.ConferencePolicyRepository.CreateConferencePolicyAsync(It.IsAny<Policy>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task AddConferencePoliciesAsync_Should_ThrowNotFoundException_When_ConferenceDoesNotExist()
        {
            // ARRANGE
            var request = CreateValidAddPoliciesRequest();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf"))
                           .ReturnsAsync((Conference)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _conferenceStepService.AddConferencePoliciesAsync("nonexistent-conf", request, "user-123")
            );
        }

        [Fact]
        public async Task AddConferencePoliciesAsync_Should_ThrowException_When_UserIsNotCreator()
        {
            // ARRANGE
            var conference = CreateTechnicalConference(userId: "creator-id");
            var request = CreateValidAddPoliciesRequest();
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferencePoliciesAsync(conference.ConferenceId, request, "other-user-id")
            );
        }

        [Fact]
        public async Task AddConferencePoliciesAsync_Should_ThrowNullReferenceException_When_RequestIsNull()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            SetupValidMocks(conference);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _conferenceStepService.AddConferencePoliciesAsync(conference.ConferenceId, null, "user-123")
            );
        }

        #endregion
    }
}