using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.RevisionPaperTest
{
    public class SubmitRevisionSubmissionTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITimeProviderService> _mockTimeProvider = new();
        private readonly PaperService _service;

        public SubmitRevisionSubmissionTest()
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
        public async Task CreateRevisionPaperSubmission_StatusNotFound_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository).Returns(Mock.Of<IPaperPhaseRepository>());
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository).Returns(Mock.Of<IGlobalStatusRepository>());
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository).Returns(Mock.Of<IConferenceStatusRepository>());
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(Mock.Of<IPaperRepository>());
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((PaperPhase?)null);

            var request = new CreateRevisionPaperSubmissionRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionPaperSubmission(request, "user1"));

            Assert.Contains("Không thấy trạng thái", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionPaperSubmission_PaperNotFound_ThrowsBadRequestException()
        {
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Paper?)null);

            var request = new CreateRevisionPaperSubmissionRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionPaperSubmission(request, "user1"));

            Assert.Contains("Paper id paper1 không tìm thấy", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionPaperSubmission_ConferenceNotReady_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "notready" } },
                PaperPhaseId = "revise",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new CreateRevisionPaperSubmissionRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionPaperSubmission(request, "user1"));

            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionPaperSubmission_ReviseDeadlinePassed_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "revise",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) }
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new CreateRevisionPaperSubmissionRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionPaperSubmission(request, "user1"));

            Assert.Contains("Hạn chót giai đoạn revise", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionPaperSubmission_NotRootAuthor_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "revise",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user2" } }, // user1 không phải root
                ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new CreateRevisionPaperSubmissionRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionPaperSubmission(request, "user1"));

            Assert.Contains("Bạn không sở hữu bài báo", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionPaperSubmission_ResearchPhaseNull_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" } },
                PaperPhaseId = "revise",
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } },
                ResearchConferencePhase = null // không tìm thấy giai đoạn
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new CreateRevisionPaperSubmissionRequest { PaperId = "paper1" };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionPaperSubmission(request, "user1"));

            Assert.Contains("Không tìm thấy giai đoạn cho hội nghị", ex.Message);
        }



    }
}
