using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Services;
using Moq;

namespace ConfRadar.UnitTests.Services.FeedbackAndReportServiceTest.ReportTest
{
    public class GetReportFeedbackTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ReportService _service;

        public GetReportFeedbackTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new ReportService(_unitOfWorkMock.Object, Mock.Of<ITimeProviderService>());
        }

        [Fact]
        public async Task GetReportFeedBackByReportId_ShouldThrow_WhenReportNotFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync((Report)null);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetReportFeedBackByReportId("R1")
            );
        }

        [Fact]
        public async Task GetReportFeedBackByReportId_ShouldThrow_WhenReportHasNoFeedback()
        {
            // Arrange
            var report = new Report { ReportId = "R1", HasResolve = false };
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync(report);

            _unitOfWorkMock.Setup(r => r.ReportFeedbackRepository.GetReportFeedbackByIdAsync("R1"))
                           .ReturnsAsync((ReportFeedback)null);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetReportFeedBackByReportId("R1")
            );
        }

        [Fact]
        public async Task GetReportFeedBackByReportId_ShouldReturnFeedback_WhenExists()
        {
            // Arrange
            var report = new Report { ReportId = "R1", HasResolve = true };
            var feedback = new ReportFeedback
            {
                ReportId = "R1",
                ReportSubject = "Spam",
                Reason = "Test Reason",
                AdminId = "Admin1",
                Admin = new User { UserId = "Admin1", FullName = "Admin Name", Email = "admin@test.com" }
            };

            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync(report);

            _unitOfWorkMock.Setup(r => r.ReportFeedbackRepository.GetReportFeedbackByIdAsync("R1"))
                           .ReturnsAsync(feedback);

            // Act
            var result = await _service.GetReportFeedBackByReportId("R1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("R1", result.ReportId);
            Assert.Equal("Spam", result.ReportSubject);
            Assert.Equal("Test Reason", result.Reason);
            Assert.Equal("Admin1", result.AdminId);
            Assert.NotNull(result.Admin);
            Assert.Equal("Admin1", result.Admin.UserId);
            Assert.Equal("Admin Name", result.Admin.UserName);
        }

        [Fact]
        public async Task GetReportFeedBackByReportId_ShouldReturnFeedback_WhenAdminIsNull()
        {
            // Arrange
            var report = new Report { ReportId = "R1", HasResolve = true };
            var feedback = new ReportFeedback
            {
                ReportId = "R1",
                ReportSubject = "Spam",
                Reason = "Test Reason",
                AdminId = "Admin1",
                Admin = null
            };

            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync(report);

            _unitOfWorkMock.Setup(r => r.ReportFeedbackRepository.GetReportFeedbackByIdAsync("R1"))
                           .ReturnsAsync(feedback);

            // Act
            var result = await _service.GetReportFeedBackByReportId("R1");

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Admin);
        }
    }

}
