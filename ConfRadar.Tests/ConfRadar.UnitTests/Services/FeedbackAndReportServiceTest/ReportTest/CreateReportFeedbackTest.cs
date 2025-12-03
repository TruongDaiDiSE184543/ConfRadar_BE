using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Report;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.UnitTests.Services.FeedbackAndReportServiceTest.ReportTest
{
    public class CreateReportFeedbackTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ReportService _service;

        public CreateReportFeedbackTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new ReportService(_unitOfWorkMock.Object, Mock.Of<ITimeProviderService>());
        }

        [Fact]
        public async Task CreateReportFeedbackAsync_ShouldThrow_WhenReportNotFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync((Report)null);

            var request = new CreateReportFeedbackRequest
            {
                ReportSubject = "Spam",
                Reason = "Test reason"
            };

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateReportFeedbackAsync("R1", "Admin1", request)
            );
        }

        [Fact]
        public async Task CreateReportFeedbackAsync_ShouldThrow_WhenReportAlreadyResolved()
        {
            // Arrange
            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync(new Report { ReportId = "R1", HasResolve = true });

            var request = new CreateReportFeedbackRequest
            {
                ReportSubject = "Spam",
                Reason = "Test reason"
            };

            // Act + Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateReportFeedbackAsync("R1", "Admin1", request)
            );
        }

        [Fact]
        public async Task CreateReportFeedbackAsync_ShouldReturnFeedback_WhenSuccess()
        {
            // Arrange
            var report = new Report { ReportId = "R1", HasResolve = false };
            var feedback = new ReportFeedback
            {
                ReportId = "R1",
                ReportSubject = "Spam",
                Reason = "Test reason",
                AdminId = "Admin1"
            };

            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync(report);

            _unitOfWorkMock.Setup(r => r.ReportRepository.UpdateReportAsync(It.IsAny<Report>()))
                           .ReturnsAsync(1);

            _unitOfWorkMock.Setup(r => r.ReportFeedbackRepository.CreateReportFeedbackAsync(It.IsAny<ReportFeedback>()))
                           .ReturnsAsync(1);

            _unitOfWorkMock.Setup(r => r.ReportFeedbackRepository.GetReportFeedbackByIdAsync("R1"))
                           .ReturnsAsync(feedback);

            _unitOfWorkMock.Setup(r => r.CommitAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync()).Returns(Task.CompletedTask);

            var request = new CreateReportFeedbackRequest
            {
                ReportSubject = "Spam",
                Reason = "Test reason"
            };

            // Act
            var result = await _service.CreateReportFeedbackAsync("R1", "Admin1", request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("R1", result.ReportId);
            Assert.Equal("Spam", result.ReportSubject);
            Assert.Equal("Test reason", result.Reason);
            Assert.Equal("Admin1", result.AdminId);
        }

        [Fact]
        public async Task CreateReportFeedbackAsync_ShouldRollback_WhenExceptionOccurs()
        {
            // Arrange
            var report = new Report { ReportId = "R1", HasResolve = false };

            _unitOfWorkMock.Setup(r => r.ReportRepository.GetReportByIdAsync("R1"))
                           .ReturnsAsync(report);

            _unitOfWorkMock.Setup(r => r.ReportRepository.UpdateReportAsync(It.IsAny<Report>()))
                           .ThrowsAsync(new Exception("DB error"));

            _unitOfWorkMock.Setup(r => r.RollbackAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync()).Returns(Task.CompletedTask);

            var request = new CreateReportFeedbackRequest
            {
                ReportSubject = "Spam",
                Reason = "Test reason"
            };

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateReportFeedbackAsync("R1", "Admin1", request)
            );

            _unitOfWorkMock.Verify(r => r.RollbackAsync(), Times.Once);
        }
    }

}
