using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;


namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.TrackReviewDeadlines
{
    public class TrackReviewDeadlinesTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITimeProviderService> _mockTimeProviderService = new();
        private readonly PaperService _service;

        public TrackReviewDeadlinesTest()
        {
            _mockTimeProviderService.Setup(x => x.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            _service = new PaperService(
                _mockUnitOfWork.Object,
                Mock.Of<IMomoService>(),
                Mock.Of<ITokenService>(),
                Mock.Of<IOptions<ObjectStorageSettings>>(),
                Mock.Of<IObjectStorageFileService>(),
                Mock.Of<ITicketService>(),
                _mockTimeProviderService.Object,
                Mock.Of<INotificationService>(),
                Mock.Of<IConferenceStepService>(),
                Mock.Of<IEmailService>()
            );
        }

        [Fact]
        public async Task GetAssignedPapers_NoAssignment_ReturnsEmptyList()
        {
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByUserIdAndConferenceIdAsync("user1", null))
                .ReturnsAsync(new List<PaperReviewer>());

            var result = await _service.GetAssignedPapersDetailedAsync("user1", null);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

     

        [Fact]
        public async Task GetAssignedPapers_FullPaper_CanReviewAndDecideLogic()
        {
            var assignments = new List<PaperReviewer> { new() { PaperId = "p1", IsHeadReviewer = true } };
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByUserIdAndConferenceIdAsync("user1", null))
                .ReturnsAsync(assignments);

            var paper = new Paper
            {
                PaperId = "p1",
                Title = "Paper 1",
                Conference = new Conference { ConferenceName = "Conf 1" },
                PaperPhase = new PaperPhase { PhaseName = "FullPaperPhase" },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    ReviewEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                FullPaper = new FullPaper
                {
                    FullPaperId = "fp1",
                    FullPaperUrl = "url",
                    ReviewStatus = new ReviewStatus { Name = "Pending" }
                }
            };

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetDetailPaperFromListId(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Paper> { paper });

            _mockUnitOfWork.Setup(x => x.FullPaperReviewRepository.GetReviewsByUserAndPaperIdsAsync("user1", It.IsAny<List<string>>()))
                .ReturnsAsync(new List<FullPaperReview>());

            var result = await _service.GetAssignedPapersDetailedAsync("user1", null);

            Assert.Single(result);
            var dto = result[0];
            Assert.True(dto.FullPaperWork.CanReview);
            Assert.True(dto.FullPaperWork.CanDecide);
        }

        
    }
}
