using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ViewAssignedPaper
{
    public class ViewAssignedPapersTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        // Các mock phụ khác để thỏa mãn Constructor của PaperService
        private readonly Mock<IMomoService> _mockMomoService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockFileService;
        private readonly Mock<ITicketService> _mockTicketService;
        private readonly Mock<ITimeProviderService> _mockTimeService;
        private readonly Mock<INotificationService> _mockNotiService;
        private readonly Mock<IConferenceStepService> _mockStepService;

        // Mock Repo quan trọng nhất cho test này
        private readonly Mock<IPaperReviewerRepository> _mockPaperReviewerRepo;

        private readonly PaperService _paperService;

        public ViewAssignedPapersTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPaperReviewerRepo = new Mock<IPaperReviewerRepository>();

            // Setup UnitOfWork trả về Mock Repo
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository).Returns(_mockPaperReviewerRepo.Object);

            // Khởi tạo các mock phụ (ko quan trọng logic test này nhưng cần để new Service)
            _mockMomoService = new Mock<IMomoService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockFileService = new Mock<IObjectStorageFileService>();
            _mockTicketService = new Mock<ITicketService>();
            _mockTimeService = new Mock<ITimeProviderService>();
            _mockNotiService = new Mock<INotificationService>();
            _mockStepService = new Mock<IConferenceStepService>();
            var options = Options.Create(new ObjectStorageSettings());

            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                _mockMomoService.Object,
                _mockTokenService.Object,
                options,
                _mockFileService.Object,
                _mockTicketService.Object,
                _mockTimeService.Object,
                _mockNotiService.Object,
                _mockStepService.Object
            );
        }

        [Fact]
        public async Task GetAllAssignedPapers_Should_ReturnList_When_DataExists()
        {
            // ARRANGE
            string userId = "reviewer-1";
            string confId = "conf-1";

            // Tạo data giả: 2 PaperReviewer chứa 2 Paper
            var mockData = new List<PaperReviewer>
            {
                new PaperReviewer
                {
                    UserId = userId,
                    PaperId = "p1",
                    Paper = new Paper
                    {
                        PaperId = "p1",
                        Title = "Paper One",
                        PaperPhase = new PaperPhase { PhaseName = "Abstract" }
                    }
                },
                new PaperReviewer
                {
                    UserId = userId,
                    PaperId = "p2",
                    Paper = new Paper
                    {
                        PaperId = "p2",
                        Title = "Paper Two",
                        PaperPhase = new PaperPhase { PhaseName = "FullPaper" }
                    }
                }
            };

            // Setup Mock Repo trả về list trên
            _mockPaperReviewerRepo
                .Setup(r => r.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, confId))
                .ReturnsAsync(mockData);

            // ACT
            var result = await _paperService.GetAllAssignedPapersToAReviewer(userId, confId);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            // Check data item 1
            result[0].Paper.PaperId.Should().Be("p1");
            result[0].phaseName.Should().Be("Abstract");

            // Check data item 2
            result[1].Paper.PaperId.Should().Be("p2");
            result[1].phaseName.Should().Be("FullPaper");
        }

        [Fact]
        public async Task GetAllAssignedPapers_Should_ReturnEmpty_When_NoAssignments()
        {
            // ARRANGE
            string userId = "reviewer-lazy";
            string confId = "conf-1";

            // Setup Mock Repo trả về list rỗng
            _mockPaperReviewerRepo
                .Setup(r => r.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, confId))
                .ReturnsAsync(new List<PaperReviewer>());

            // ACT
            var result = await _paperService.GetAllAssignedPapersToAReviewer(userId, confId);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAssignedPapers_Should_FilterOut_NullPapers()
        {
            // ARRANGE
            string userId = "reviewer-1";
            string confId = "conf-1";

            // Data giả: 1 cái có Paper, 1 cái Paper bị null (dữ liệu lỗi)
            var mockData = new List<PaperReviewer>
            {
                new PaperReviewer
                {
                    Paper = new Paper { PaperId = "p1", Title = "Valid Paper" }
                },
                new PaperReviewer
                {
                    Paper = null // Cái này sẽ bị code Service lọc bỏ
                }
            };

            _mockPaperReviewerRepo
                .Setup(r => r.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, confId))
                .ReturnsAsync(mockData);

            // ACT
            var result = await _paperService.GetAllAssignedPapersToAReviewer(userId, confId);

            // ASSERT
            result.Should().HaveCount(1); // Chỉ còn 1 cái hợp lệ
            result[0].Paper.Title.Should().Be("Valid Paper");
        }
    }
}
