using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Report;
using Moq;

namespace ConfRadar.UnitTests.Services.FeedbackAndReportServiceTest.ReportTest
{
    public class CreateReportTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IReportRepository> _reportRepoMock;
        private readonly Mock<ITimeProviderService> _timeProviderMock;
        private readonly ReportService _service;
        public CreateReportTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _reportRepoMock = new Mock<IReportRepository>();
            _timeProviderMock = new Mock<ITimeProviderService>();

            _unitOfWorkMock.Setup(x => x.ReportRepository)
                           .Returns(_reportRepoMock.Object);

            _service = new ReportService(_unitOfWorkMock.Object, _timeProviderMock.Object);
        }
        // ---------------------------------------------------------
        // UTC01: CreateReportAsync thành công
        // ---------------------------------------------------------
        [Fact]
        public async Task CreateReportAsync_ShouldReturnReportResponse_WhenSuccess()
        {
            // Arrange
            var userId = "user123";
            var request = new CreateReportRequest
            {
                ReportSubject = "Spam",
                Reason = "Inappropriate content",
                Description = "User is spamming"
            };

            var timeNow = DateTime.UtcNow;
            _timeProviderMock.Setup(x => x.GetVietnamTime())
                             .ReturnsAsync(timeNow);

            Report? createdReport = null;

            _reportRepoMock.Setup(x => x.CreateReportAsync(It.IsAny<Report>()))
                           .Callback<Report>(r => createdReport = r)
                           .ReturnsAsync(1);

            _reportRepoMock.Setup(x => x.GetReportByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(() => createdReport);

            // Act
            var result = await _service.CreateReportAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.ReportSubject, result.ReportSubject);
            Assert.Equal(request.Reason, result.Reason);
            Assert.Equal(request.Description, result.Description);
            Assert.False(result.HasResolve);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(timeNow, result.CreatedAt);
        }





    }
}
