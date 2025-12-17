using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class DecideFullPaperStatusTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITimeProviderService> _mockTimeProvider = new();
        private readonly PaperService _service;

        public DecideFullPaperStatusTests()
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
        public async Task ReviewStatusPending_ThrowsBadRequestException()
        {
            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Pending
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Không thể chuyển qua status pending", ex.Message);
        }

        [Fact]
        public async Task StatusNotFound_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((ReviewStatus?)null);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Không thấy các trạng thái", ex.Message);
        }

        [Fact]
        public async Task PaperNotFound_ThrowsBadRequestException()
        {
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Paper?)null);

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Không tìm thấy paper", ex.Message);
        }

        [Fact]
        public async Task ConferenceNotReady_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "notready" } },
                PaperPhaseId = "fullpaper",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                FullPaperId = "fp1",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task ResearchConferencePhaseNull_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "fullpaper",
                ResearchConferencePhase = null,
                FullPaperId = "fp1",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };

            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("không tìm thấy giai đoạn", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FullPaperNull_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullpaper",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                FullPaperId = "fp1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((FullPaper?)null);
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Full paper với id", ex.Message);
        }

        [Fact]
        public async Task FullPaperIdMismatch_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullpaper",
                FullPaperId = "fp_real",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };
            var fullPaper = new FullPaper { FullPaperId = "fp_wrong", ReviewStatusId = "pending" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp_wrong",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("không khớp với fullpaper id", ex.Message);
        }

        [Fact]
        public async Task FullPaperNotPending_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullpaper",
                FullPaperId = "fp1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };
            var fullPaper = new FullPaper { FullPaperId = "fp1", ReviewStatusId = "accepted" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("không trong trạng thái pending", ex.Message);
        }

        [Fact]
        public async Task PaperPhaseNotFullPaper_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "revise",
                FullPaperId = "fp1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };
            var fullPaper = new FullPaper { FullPaperId = "fp1", ReviewStatusId = "pending" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Paper phase không đang trong giai đoạn full paper", ex.Message);
        }

        [Fact]
        public async Task PaperReviewerListEmpty_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullpaper",
                FullPaperId = "fp1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };
            var fullPaper = new FullPaper { FullPaperId = "fp1", ReviewStatusId = "pending" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<PaperReviewer>()); // empty
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Không tìm thấy danh sách paper reviewer", ex.Message);
        }

        [Fact]
        public async Task NotHeadReviewer_ThrowsNotFoundException()
        {
            var paperReviewer = new PaperReviewer { IsHeadReviewer = false, UserId = "user2" };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullpaper",
                FullPaperId = "fp1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };
            var fullPaper = new FullPaper { FullPaperId = "fp1", ReviewStatusId = "pending" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<PaperReviewer> { paperReviewer });
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Bạn không phải là head reviewer", ex.Message);
        }

        [Fact]
        public async Task FullPaperReviewsEmpty_ThrowsBadRequestException()
        {
            var paperReviewer = new PaperReviewer { IsHeadReviewer = true, UserId = "user1" };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullpaper",
                FullPaperId = "fp1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    FullPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    FullPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, User = new User { UserId = "user1" } } }
            };
            var fullPaper = new FullPaper { FullPaperId = "fp1", ReviewStatusId = "pending" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullpaper" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<PaperReviewer> { paperReviewer });
            _mockUnitOfWork.Setup(u => u.FullPaperReviewRepository.GetFullPaperReviewsByFullPaperIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<FullPaperReview>()); // empty
            _mockTimeProvider.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateFullPaperStatusRequest
            {
                PaperId = "p1",
                FullPaperId = "fp1",
                ReviewStatus = ReviewStatusEnum.Accepted
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideFullPaperFinalStatus(request, "user1"));

            Assert.Contains("Cần ít nhất 1 review từ các reviewer", ex.Message);
        }

    }
}
