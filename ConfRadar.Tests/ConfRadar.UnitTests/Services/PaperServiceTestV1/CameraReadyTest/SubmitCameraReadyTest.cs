using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.CameraReadyTest
{
    public class SubmitCameraReadyTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITimeProviderService> _mockTimeProvider = new();
        private readonly PaperService _service;

        public SubmitCameraReadyTest()
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
        public async Task CreateCameraReady_StatusNotFound_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository).Returns(Mock.Of<IConferenceStatusRepository>());
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository).Returns(Mock.Of<IPaperPhaseRepository>());
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository).Returns(Mock.Of<IGlobalStatusRepository>());
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(Mock.Of<IPaperRepository>());

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((ConferenceStatus?)null);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((PaperPhase?)null);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync((GlobalStatus?)null);

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Không tìm thấy trạng thái", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_PaperNotFound_ThrowsBadRequestException()
        {
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository).Returns(Mock.Of<IConferenceStatusRepository>());
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository).Returns(Mock.Of<IPaperPhaseRepository>());
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository).Returns(Mock.Of<IGlobalStatusRepository>());
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(Mock.Of<IPaperRepository>());

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Paper?)null);

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Bài báo với id paper1 không tồn tại", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_PaperNotCameraReadyPhase_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "paper1", PaperPhaseId = "wrongPhase" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Bài báo phải trong giai đoạn camera ready", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_ConferenceNotReady_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "cameraReady",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "notReady" } }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_NoResearchConferencePhases_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "cameraReady",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" }, ResearchConferencePhases = null }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Không tìm thấy các giai đoạn", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_NoActivePhase_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "cameraReady",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    ResearchConferencePhases = new List<ResearchConferencePhase> { new() { IsActive = false } }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Không tìm thấy giai đoạn nào active", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_PaymentDeadlinePassed_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "cameraReady",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    ResearchConferencePhases = new List<ResearchConferencePhase>
                {
                    new() { IsActive = true, AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) }
                }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Hạn chót nộp camera ready + thanh toán", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_AlreadyHasCameraReady_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "cameraReady",
                CameraReadyId = "existing",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    ResearchConferencePhases = new List<ResearchConferencePhase> { new() { IsActive = true, AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("đã có camera ready", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_NotRootAuthor_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "cameraReady",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user2" } }, // user1 không phải root
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    ResearchConferencePhases = new List<ResearchConferencePhase> { new() { IsActive = true, AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("Bạn không có quyền sỡ hữu bài báo", ex.Message);
        }

        [Fact]
        public async Task CreateCameraReady_PaperNotAccepted_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "cameraReady",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    ResearchConferencePhases = new List<ResearchConferencePhase> { new() { IsActive = true, AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "cameraReady" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });

            _mockUnitOfWork.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((FullPaper?)null);
            _mockUnitOfWork.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((RevisionPaper?)null);

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateCameraReadyRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateCameraReady(request, "user1"));

            Assert.Contains("paper phải có revision hoặc fullpaper chấp nhận", ex.Message);
        }

    }
}
