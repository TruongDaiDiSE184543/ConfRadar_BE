using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.FullPaperTest
{
    public class SubmitFullPaperTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITimeProviderService> _mockTimeProvider = new();
        private readonly PaperService _service;

        public SubmitFullPaperTest()
        {
            // Các service khác mock trống vì không liên quan exception
            _service = new PaperService(
                _mockUnitOfWork.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                _mockTimeProvider.Object,
                null!,
                null!,
                null!
            );
        }

        [Fact]
        public async Task SubmitFullPaper_PaperNotFound_ThrowsBadRequestException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Paper?)null);

            var request = new CreateFullPaperRequest { PaperId = "paper1" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.SubmitFullPaper(request, "user1"));
            Assert.Contains("Không thấy paper", ex.Message);
        }
        [Fact]
        public async Task SubmitFullPaper_PaperNotInFullPaperPhase_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "draft", // không phải "fullpaper"
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                ResearchConferencePhase = new ResearchConferencePhase { FullPaperEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new CreateFullPaperRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.SubmitFullPaper(request, "user1"));
            Assert.Contains("Không trong trạng thái full paper", ex.Message);
        }

        [Fact]
        public async Task SubmitFullPaper_FullPaperAlreadyExists_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                FullPaperId = "existingFullPaperId", // đã tồn tại
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "fullpaper",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                ResearchConferencePhase = new ResearchConferencePhase { FullPaperEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new CreateFullPaperRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.SubmitFullPaper(request, "user1"));
            Assert.Contains("Full paper file đã tồn tại", ex.Message);
        }

        [Fact]
        public async Task SubmitFullPaper_ConferenceNotReady_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "notready" }
                },
                PaperPhaseId = "fullpaper",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                ResearchConferencePhase = new ResearchConferencePhase { FullPaperEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new CreateFullPaperRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.SubmitFullPaper(request, "user1"));
            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task SubmitFullPaper_NotRootAuthor_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "fullpaper",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user2" } }, // user1 không phải root
                ResearchConferencePhase = new ResearchConferencePhase { FullPaperEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new CreateFullPaperRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.SubmitFullPaper(request, "user1"));
            Assert.Contains("Bạn không sỡ hữu bài báo", ex.Message);
        }

        [Fact]
        public async Task SubmitFullPaper_FullPaperDeadlinePassed_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "fullpaper",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                ResearchConferencePhase = new ResearchConferencePhase { FullPaperEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) }
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateFullPaperRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.SubmitFullPaper(request, "user1"));
            Assert.Contains("Hạn chót giai đoạn fullpaper", ex.Message);
        }
    }
}
