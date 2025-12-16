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
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        // Các mock phụ
        private readonly Mock<IMomoService> _mockMomo;
        private readonly Mock<ITokenService> _mockToken;
        private readonly Mock<IObjectStorageFileService> _mockFile;
        private readonly Mock<ITicketService> _mockTicket;
        private readonly Mock<INotificationService> _mockNoti;
        private readonly Mock<IConferenceStepService> _mockStep;

        // --- 4 MOCK QUAN TRỌNG CHO TEST NÀY ---
        private readonly Mock<IPaperReviewerRepository> _mockAssignRepo;
        private readonly Mock<IPaperRepository> _mockPaperRepo;
        private readonly Mock<IFullPaperReviewRepository> _mockFullReviewRepo;
        private readonly Mock<ITimeProviderService> _mockTime;

        private readonly PaperService _paperService;
        private readonly Mock<IEmailService> _mockEmailService;
        public TrackReviewDeadlinesTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            // Khởi tạo các Mock quan trọng
            _mockAssignRepo = new Mock<IPaperReviewerRepository>();
            _mockPaperRepo = new Mock<IPaperRepository>();
            _mockFullReviewRepo = new Mock<IFullPaperReviewRepository>();
            _mockTime = new Mock<ITimeProviderService>();

            // Setup UnitOfWork trả về các Mock Repo
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository).Returns(_mockAssignRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(_mockPaperRepo.Object);
            _mockUnitOfWork.Setup(u => u.FullPaperReviewRepository).Returns(_mockFullReviewRepo.Object);
            // Cần thêm RevisionRepo nếu test sâu về Revision (để tránh null reference nếu code có gọi)
            //var mockRevReviewRepo = new Mock<IRevisionPaperReviewRepository>();
            //_mockUnitOfWork.Setup(u => u.RevisionPaperReviewRepository).Returns(mockRevReviewRepo.Object);

            // Các mock phụ khác
            _mockMomo = new Mock<IMomoService>();
            _mockToken = new Mock<ITokenService>();
            _mockFile = new Mock<IObjectStorageFileService>();
            _mockTicket = new Mock<ITicketService>();
            _mockNoti = new Mock<INotificationService>();
            _mockStep = new Mock<IConferenceStepService>();
            _mockEmailService = new Mock<IEmailService>();
            var options = Options.Create(new ObjectStorageSettings());

            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                _mockMomo.Object,
                _mockToken.Object,
                options,
                _mockFile.Object,
                _mockTicket.Object,
                _mockTime.Object, // Inject Time Mock
                _mockNoti.Object,
                _mockStep.Object,
                _mockEmailService.Object
            );
        }

        [Fact]
        public async Task GetAssignedPapersDetailed_Should_SetCanReviewTrue_When_WithinDeadline_And_NotReviewedYet()
        {
            // ARRANGE
            string userId = "reviewer-1";
            string paperId = "p1";
            var today = new DateOnly(2023, 10, 15); // Giả sử hôm nay là 15/10

            // 1. Mock Time: Hôm nay là 15/10
            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(today);

            // 2. Mock Assignment: User được gán bài p1
            _mockAssignRepo.Setup(r => r.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(new List<PaperReviewer> { new PaperReviewer { PaperId = paperId, IsHeadReviewer = false } });

            // 3. Mock Paper Detail (Quan trọng nhất): Cấu hình Deadline bao trùm ngày hôm nay
            var mockPaper = new Paper
            {
                PaperId = paperId,
                Title = "Test Paper",
                FullPaperId = "fp1",
                FullPaper = new FullPaper
                {
                    FullPaperId = "fp1",
                    ReviewStatus = new ReviewStatus { Name = "Pending" }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    // Deadline từ 10/10 đến 20/10 => Hôm nay 15/10 là TRONG HẠN
                    ReviewStartDate = new DateOnly(2023, 10, 10),
                    ReviewEndDate = new DateOnly(2023, 10, 20)
                }
            };

            _mockPaperRepo.Setup(r => r.GetDetailPaperFromListId(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Paper> { mockPaper });

            // 4. Mock Review Repo: User CHƯA review bài này (trả về list rỗng)
            _mockFullReviewRepo.Setup(r => r.GetReviewsByUserAndPaperIdsAsync(userId, It.IsAny<List<string>>()))
                .ReturnsAsync(new List<FullPaperReview>());

            // ACT
            var result = await _paperService.GetAssignedPapersDetailedAsync(userId, null);

            // ASSERT
            result.Should().HaveCount(1);
            var item = result.First().FullPaperWork;

            item.CanReview.Should().BeTrue("vì hôm nay (15/10) nằm trong khoảng review (10/10 - 20/10) và chưa review");
            item.IsMyReviewSubmitted.Should().BeFalse();
        }

        [Fact]
        public async Task GetAssignedPapersDetailed_Should_SetCanReviewFalse_When_DeadlineExpired()
        {
            // ARRANGE
            string userId = "reviewer-1";
            string paperId = "p1";
            var today = new DateOnly(2023, 10, 25); // Giả sử hôm nay là 25/10 (QUÁ HẠN)

            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(today);

            _mockAssignRepo.Setup(r => r.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, null))
                .ReturnsAsync(new List<PaperReviewer> { new PaperReviewer { PaperId = paperId } });

            var mockPaper = new Paper
            {
                PaperId = paperId,
                FullPaper = new FullPaper { FullPaperId = "fp1" },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    // Deadline đã kết thúc vào ngày 20/10
                    ReviewStartDate = new DateOnly(2023, 10, 10),
                    ReviewEndDate = new DateOnly(2023, 10, 20)
                }
            };

            _mockPaperRepo.Setup(r => r.GetDetailPaperFromListId(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Paper> { mockPaper });

            _mockFullReviewRepo.Setup(r => r.GetReviewsByUserAndPaperIdsAsync(userId, It.IsAny<List<string>>()))
                .ReturnsAsync(new List<FullPaperReview>());

            // ACT
            var result = await _paperService.GetAssignedPapersDetailedAsync(userId, null);

            // ASSERT
            result.First().FullPaperWork.CanReview.Should().BeFalse("vì hôm nay (25/10) đã quá hạn review (20/10)");
        }

        [Fact]
        public async Task GetAssignedPapersDetailed_Should_AllowHeadReviewer_ToDecide_When_InDecisionPhase()
        {
            // ARRANGE
            string userId = "head-reviewer";
            string paperId = "p1";
            var today = new DateOnly(2023, 11, 05); // Hôm nay là 05/11

            _mockTime.Setup(t => t.GetVietnamDate()).ReturnsAsync(today);

            // Mock: User này là HEAD REVIEWER
            _mockAssignRepo.Setup(r => r.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, null))
                .ReturnsAsync(new List<PaperReviewer> { new PaperReviewer { PaperId = paperId, IsHeadReviewer = true } });

            var mockPaper = new Paper
            {
                PaperId = paperId,
                FullPaper = new FullPaper
                {
                    FullPaperId = "fp1",
                    ReviewStatus = new ReviewStatus { Name = "Pending" } // Status phải là Pending mới được Decide
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    // Giai đoạn Decide từ 01/11 đến 10/11
                    FullPaperDecideStatusStart = new DateOnly(2023, 11, 01),
                    FullPaperDecideStatusEnd = new DateOnly(2023, 11, 10)
                }
            };

            _mockPaperRepo.Setup(r => r.GetDetailPaperFromListId(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Paper> { mockPaper });


            _mockFullReviewRepo.Setup(r => r.GetReviewsByUserAndPaperIdsAsync(userId, It.IsAny<List<string>>()))
       .ReturnsAsync(new List<FullPaperReview>());

            // Mock RevisionPaperReviewRepository để đảm bảo myRevisionReviews không bị null
            //_mockUnitOfWork.Setup(u => u.RevisionPaperReviewRepository.GetReviewsByUserAndPaperIdsAsync(userId, It.IsAny<List<string>>()))
            //.ReturnsAsync(new List<RevisionPaperReview>());

            // ACT
            var result = await _paperService.GetAssignedPapersDetailedAsync(userId, null);

            // ASSERT
            var item = result.First();
            item.IsHeadReviewer.Should().BeTrue();
            item.FullPaperWork.CanDecide.Should().BeTrue("vì user là Head, status là Pending và đang trong thời gian Decide");
        }
    }
}
