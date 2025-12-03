using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.UnitTests.Services.FeedbackAndReportServiceTest.ReportTest
{
    public class GetUnresolvedReportsTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITimeProviderService> _timeProviderMock;
        private readonly ReportService _service;

        public GetUnresolvedReportsTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _timeProviderMock = new Mock<ITimeProviderService>();

            _service = new ReportService(_unitOfWorkMock.Object, _timeProviderMock.Object);
        }

        [Fact]
        public async Task GetUnresolvedReportsAsync_ShouldReturnEmptyList_WhenRepoReturnsEmpty()
        {
            // Arrange
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetUnresolvedReportsAsync())
                           .ReturnsAsync(new List<Report>());

            // Act
            var result = await _service.GetUnresolvedReportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUnresolvedReportsAsync_ShouldReturnMappedList_WhenRepoReturnsReports()
        {
            // Arrange
            var reports = new List<Report>
        {
            new Report
            {
                ReportId = "R1",
                ReportSubject = "Spam",
                Reason = "Test Reason",
                Description = "Test Desc",
                CreatedAt = DateTime.UtcNow,
                UserId = "U1",
                User = new User
                {
                    UserId = "U1",
                    FullName = "User Name",
                    Email = "user@test.com"
                }
            }
        };

            _unitOfWorkMock.Setup(r => r.ReportRepository.GetUnresolvedReportsAsync())
                           .ReturnsAsync(reports);

            // Act
            var result = await _service.GetUnresolvedReportsAsync();

            // Assert
            Assert.Single(result);
            var report = result[0];

            Assert.Equal("R1", report.ReportId);
            Assert.Equal("Spam", report.ReportSubject);
            Assert.Equal("Test Reason", report.Reason);
            Assert.Equal("Test Desc", report.Description);
            Assert.Equal("U1", report.UserId);

            // User mapping
            Assert.NotNull(report.User);
            Assert.Equal("U1", report.User.UserId);
            Assert.Equal("User Name", report.User.FullName);
            Assert.Equal("user@test.com", report.User.Email);
        }

        [Fact]
        public async Task GetUnresolvedReportsAsync_ShouldCallRepositoryOnce()
        {
            // Arrange
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetUnresolvedReportsAsync())
                           .ReturnsAsync(new List<Report>());

            // Act
            await _service.GetUnresolvedReportsAsync();

            // Assert
            _unitOfWorkMock.Verify(r => r.ReportRepository.GetUnresolvedReportsAsync(), Times.Once);
        }
    }

}
