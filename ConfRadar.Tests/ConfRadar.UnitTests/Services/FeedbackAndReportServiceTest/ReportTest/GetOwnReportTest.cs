using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Services;
using Moq;

namespace ConfRadar.UnitTests.Services.FeedbackAndReportServiceTest.ReportTest
{
    public class GetOwnReportTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITimeProviderService> _timeProviderMock;
        private readonly ReportService _service;

        public GetOwnReportTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _timeProviderMock = new Mock<ITimeProviderService>();

            _service = new ReportService(_unitOfWorkMock.Object, _timeProviderMock.Object);
        }

        [Fact]
        public async Task GetReportsByUserIdAsync_ShouldReturnEmptyList_WhenRepoReturnsEmpty()
        {
            // Arrange
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportsByUserIdAsync("U1"))
                           .ReturnsAsync(new List<Report>());

            // Act
            var result = await _service.GetReportsByUserIdAsync("U1");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetReportsByUserIdAsync_ShouldReturnMappedList_WhenRepoReturnsReports()
        {
            // Arrange
            var reports = new List<Report>
        {
            new Report
            {
                ReportId = "R1",
                ReportSubject = "Subject 1",
                Reason = "Reason 1",
                Description = "Desc 1",
                HasResolve = true,
                CreatedAt = DateTime.UtcNow,
                UserId = "U1",
                User = new User
                {
                    UserId = "U1",
                    FullName = "User Name",
                    Email = "user@test.com"
                },
                ReportFeedback = new ReportFeedback
                {
                    ReportId = "R1",
                    ReportSubject = "SubFb",
                    Reason = "ReasonFb",
                    AdminId = "A1",
                    Admin = new User
                    {
                        UserId = "A1",
                        FullName = "Admin Name",
                        Email = "admin@test.com"
                    }
                }
            }
        };

            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportsByUserIdAsync("U1"))
                           .ReturnsAsync(reports);

            // Act
            var result = await _service.GetReportsByUserIdAsync("U1");

            // Assert
            Assert.Single(result);
            var report = result[0];

            Assert.Equal("R1", report.ReportId);
            Assert.Equal("Subject 1", report.ReportSubject);
            Assert.Equal("Reason 1", report.Reason);
            Assert.Equal("Desc 1", report.Description);
            Assert.True(report.HasResolve);

            // Check user mapping
            Assert.NotNull(report.User);
            Assert.Equal("U1", report.User.UserId);
            Assert.Equal("User Name", report.User.FullName);

            // Check feedback mapping
            Assert.NotNull(report.ReportFeedback);
            Assert.Equal("R1", report.ReportFeedback.ReportId);
            Assert.Equal("SubFb", report.ReportFeedback.ReportSubject);
            Assert.Equal("A1", report.ReportFeedback.AdminId);

            // Check admin mapping
            Assert.NotNull(report.ReportFeedback.Admin);
            Assert.Equal("A1", report.ReportFeedback.Admin.UserId);
            Assert.Equal("Admin Name", report.ReportFeedback.Admin.FullName);
        }

        [Fact]
        public async Task GetReportsByUserIdAsync_ShouldCallRepositoryOnce()
        {
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportsByUserIdAsync("U1"))
                           .ReturnsAsync(new List<Report>());

            await _service.GetReportsByUserIdAsync("U1");

            _unitOfWorkMock.Verify(
                r => r.ReportRepository.GetReportsByUserIdAsync("U1"),
                Times.Once
            );
        }
    }
}
