using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.FullPaperReview;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class SubmitFullPaperReviewTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITokenService> _mockTokenService = new();
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService = new();
        private readonly Mock<IOptions<ObjectStorageSettings>> _mockObjectStorageSettings = new();
        private readonly Mock<ITimeProviderService> _mockTimeProviderService = new();
        private readonly PaperService _service;

        public SubmitFullPaperReviewTests()
        {
            _mockObjectStorageSettings.Setup(x => x.Value).Returns(new ObjectStorageSettings { EndPoint = "https://fakeendpoint/" });
            _mockTimeProviderService.Setup(x => x.GetVietnamTime()).ReturnsAsync(DateTime.UtcNow);
            _mockTimeProviderService.Setup(x => x.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            _service = new PaperService(
                _mockUnitOfWork.Object,
                Mock.Of<IMomoService>(),
                _mockTokenService.Object,
                _mockObjectStorageSettings.Object,
                _mockObjectStorageFileService.Object,
                Mock.Of<ITicketService>(),
                _mockTimeProviderService.Object,
                Mock.Of<INotificationService>(),
                Mock.Of<IConferenceStepService>(),
                Mock.Of<IEmailService>()
            );
        }
        [Fact]
        public async Task SubmitReview_StatusOrPhaseNull_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>())).ReturnsAsync((ConferenceStatus?)null);
            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>())).ReturnsAsync((PaperPhase?)null);
            _mockUnitOfWork.Setup(x => x.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>())).ReturnsAsync((ReviewStatus?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("Không tìm thấy trạng thái", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_UserNotExist_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("User với id user1 không tồn tại", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_ReviewPending_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Pending }, "user1"));

            Assert.Contains("Không thể chuyển pending", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_FullPaperNotExist_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1")).ReturnsAsync((FullPaper?)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("Full paper với id 1 không tồn tại", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_PaperNotExist_ThrowsBadRequestException()
        {
            var fullPaper = new FullPaper { FullPaperId = "1", ReviewStatusId = "pendingStatus" };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1")).ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1")).ReturnsAsync((Paper?)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("Bài báo với full paper ID 1 không tồn tại", ex.Message);
        }

        // Helper để mock các status/phase cơ bản
        private void SetupBasicStatusPhaseMocks()
        {
            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "readyStatus" });

            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "fullPaperPhase" });

            _mockUnitOfWork.Setup(x => x.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "pendingStatus" });
        }
        [Fact]
        public async Task SubmitReview_ConferenceNotReady_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();

            var fullPaper = new FullPaper { FullPaperId = "1", ReviewStatusId = "pendingStatus" };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullPaperPhase",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "notReadyStatus" } },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    ReviewEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };

            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1")).ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_ActivePhaseNull_ThrowsNotFoundException()
        {
            SetupBasicStatusPhaseMocks();

            var fullPaper = new FullPaper { FullPaperId = "1", ReviewStatusId = "pendingStatus" };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullPaperPhase",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } },
                ResearchConferencePhase = null
            };

            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1")).ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("Không tìm thấy các giai đoạn", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_UserNotReviewer_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();

            var fullPaper = new FullPaper { FullPaperId = "1", ReviewStatusId = "pendingStatus" };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullPaperPhase",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    ReviewEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };

            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1")).ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user1", "p1")).ReturnsAsync((PaperReviewer?)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("không tìm thấy trong danh sách gán reviewer", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_PaperNotFullPaperPhase_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();

            var fullPaper = new FullPaper { FullPaperId = "1", ReviewStatusId = "pendingStatus" };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "otherPhase",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    ReviewEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };

            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1")).ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user1", "p1")).ReturnsAsync(new PaperReviewer());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("Bài báo không trong trạng thái full paper", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_ReviewAlreadySubmitted_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();

            var fullPaper = new FullPaper { FullPaperId = "1", ReviewStatusId = "pendingStatus" };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullPaperPhase",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    ReviewEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };

            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1")).ReturnsAsync(new User());
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1")).ReturnsAsync(fullPaper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user1", "p1")).ReturnsAsync(new PaperReviewer());
            _mockUnitOfWork.Setup(x => x.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync("1", "user1"))
                .ReturnsAsync(new FullPaperReview());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(new CreateFullPaperReviewRequest { FullPaperId = "1", reviewStatus = ReviewStatusEnum.Accepted }, "user1"));

            Assert.Contains("Bạn đã gửi full paper review rồi", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_ReviewDateOutOfRange_ThrowsBadRequestException()
        {
            // Arrange
            SetupBasicStatusPhaseMocks();

            var readyStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" };
            var fullPaperPhase = new PaperPhase { PaperPhaseId = "fullPaperPhase" };
            var pendingReviewStatus = new ReviewStatus { ReviewStatusId = "pendingStatus" };
            var acceptedReviewStatus = new ReviewStatus { ReviewStatusId = "acceptedStatus" };

            var reviewStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)); // Trong tương lai
            var reviewEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));

            var fullPaper = new FullPaper
            {
                FullPaperId = "1",
                ReviewStatusId = "pendingStatus",
            };

            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullPaperPhase",
                FullPaperId = "1",
                ConferenceId = "conf1",
                Conference = new Conference
                {
                    ConferenceId = "conf1",
                    ConferenceName = "Test Conference",
                    ConferenceStatusId = "readyStatus",
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = reviewStartDate, // Bắt đầu ngày mai
                    ReviewEndDate = reviewEndDate
                },
                PaperAuthors = new List<PaperAuthor>
        {
            new PaperAuthor
            {
                IsRootAuthor = true,
                UserId = "rootUser",
                User = new User
                {
                    UserId = "rootUser",
                    FullName = "Root User",
                    Email = "root@test.com"
                }
            }
        }
            };

            // Mock ConferenceStatusRepository
            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            // Mock PaperPhaseRepository
            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription()))
                .ReturnsAsync(fullPaperPhase);

            // Mock ReviewStatusRepository
            _mockUnitOfWork.Setup(x => x.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingReviewStatus);

            _mockUnitOfWork.Setup(x => x.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(acceptedReviewStatus);

            // Mock UserRepository
            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1"))
                .ReturnsAsync(new User
                {
                    UserId = "user1",
                    FullName = "Test User",
                    Email = "user1@test.com"
                });

            // Mock FullPaperRepository
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1"))
                .ReturnsAsync(fullPaper);

            // Mock PaperRepository
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1"))
                .ReturnsAsync(paper);

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1"))
                .ReturnsAsync(paper);

            // Mock PaperReviewerRepository
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user1", "p1"))
                .ReturnsAsync(new PaperReviewer
                {
                    UserId = "user1",
                    PaperId = "p1"
                });

            // Mock FullPaperReviewRepository - chưa có review trước đó
            _mockUnitOfWork.Setup(x => x.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync("1", "user1"))
                .ReturnsAsync((FullPaperReview)null);

            // Mock TimeProvider - ngày hiện tại (trước ReviewStartDate) - điểm cần test
            _mockTimeProviderService.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow)); // Hôm nay, trước reviewStartDate

            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.UtcNow);

            var request = new CreateFullPaperReviewRequest
            {
                FullPaperId = "1",
                reviewStatus = ReviewStatusEnum.Accepted,
                Note = "Test note"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(request, "user1"));

            Assert.Contains("Giai đoạn gửi full paper review nằm từ", ex.Message);
        }

        [Fact]
        public async Task SubmitReview_FullPaperNotPending_ThrowsBadRequestException()
        {
            // Arrange
            SetupBasicStatusPhaseMocks();

            var readyStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" };
            var fullPaperPhase = new PaperPhase { PaperPhaseId = "fullPaperPhase" };
            var pendingReviewStatus = new ReviewStatus { ReviewStatusId = "pendingStatus" };
            var acceptedReviewStatus = new ReviewStatus { ReviewStatusId = "acceptedStatus" };

            var fullPaper = new FullPaper
            {
                FullPaperId = "1",
                ReviewStatusId = "otherStatus", // Không phải pending - điểm cần test
            };

            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "fullPaperPhase",
                FullPaperId = "1",
                ConferenceId = "conf1",
                Conference = new Conference
                {
                    ConferenceId = "conf1",
                    ConferenceName = "Test Conference",
                    ConferenceStatusId = "readyStatus",
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ReviewStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    ReviewEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                PaperAuthors = new List<PaperAuthor>
        {
            new PaperAuthor
            {
                IsRootAuthor = true,
                UserId = "rootUser",
                User = new User
                {
                    UserId = "rootUser",
                    FullName = "Root User",
                    Email = "root@test.com"
                }
            }
        }
            };

            // Mock ConferenceStatusRepository
            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            // Mock PaperPhaseRepository
            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription()))
                .ReturnsAsync(fullPaperPhase);

            // Mock ReviewStatusRepository - 2 calls
            _mockUnitOfWork.Setup(x => x.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingReviewStatus);

            _mockUnitOfWork.Setup(x => x.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(acceptedReviewStatus);

            // Mock UserRepository
            _mockUnitOfWork.Setup(x => x.UserRepository.GetUserByUserId("user1"))
                .ReturnsAsync(new User
                {
                    UserId = "user1",
                    FullName = "Test User",
                    Email = "user1@test.com"
                });

            // Mock FullPaperRepository
            _mockUnitOfWork.Setup(x => x.FullPaperRepository.GetFullPaperByIdAsync("1"))
                .ReturnsAsync(fullPaper);

            // Mock PaperRepository - 2 calls quan trọng
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByFullPaperIdAsync("1"))
                .ReturnsAsync(paper);

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1"))
                .ReturnsAsync(paper);

            // Mock PaperReviewerRepository
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user1", "p1"))
                .ReturnsAsync(new PaperReviewer
                {
                    UserId = "user1",
                    PaperId = "p1"
                });

            // Mock FullPaperReviewRepository - check existing review
            _mockUnitOfWork.Setup(x => x.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync("1", "user1"))
                .ReturnsAsync((FullPaperReview)null);

            // Mock TimeProvider
            _mockTimeProviderService.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.UtcNow);

            var request = new CreateFullPaperReviewRequest
            {
                FullPaperId = "1",
                reviewStatus = ReviewStatusEnum.Accepted,
                Note = "Test note"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.SubmitReviewForFullPaper(request, "user1"));

            Assert.Contains("Full paper phải trong trạng thái pending", ex.Message);
        }

       

       

    }
}
