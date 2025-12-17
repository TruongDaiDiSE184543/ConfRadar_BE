using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class SubmitRevisionFeedbackTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITimeProviderService> _mockTimeProviderService = new();
        private readonly PaperService _service;

        public SubmitRevisionFeedbackTests()
        {
            _mockTimeProviderService.Setup(x => x.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));
            _mockTimeProviderService.Setup(x => x.GetVietnamTime()).ReturnsAsync(DateTime.UtcNow);

            _service = new PaperService(
                _mockUnitOfWork.Object,
                Mock.Of<IMomoService>(),
                Mock.Of<ITokenService>(),
                Mock.Of<IOptions<ObjectStorageSettings>>(),
                Mock.Of<IObjectStorageFileService>(),
                Mock.Of<ITicketService>(),
                _mockTimeProviderService.Object,
                Mock.Of<INotificationService>(),
                Mock.Of<IConferenceStepService>(),
                Mock.Of<IEmailService>()
            );
        }
        [Fact]
        public async Task CreateRevisionFeedback_StatusOrPhaseNull_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(x => x.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync((GlobalStatus?)null);
            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>())).ReturnsAsync((ConferenceStatus?)null);
            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync((PaperPhase?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback(), "user1"));

            Assert.Contains("Không tìm thấy trạng thái", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_PaperNotFound_ThrowsNotFoundException()
        {
            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync((Paper?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Không tìm thấy paper với id", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_RevisionPaperIdNull_ThrowsNotFoundException()
        {
            SetupBasicStatusPhaseMocks();

            var paper = new Paper { PaperId = "p1", RevisionPaperId = null };
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Không tìm thấy mã revision", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_RevisionPaperNotFound_ThrowsNotFoundException()
        {
            SetupBasicStatusPhaseMocks();

            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1" };
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync((RevisionPaper?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Không tìm thấy revision với id r1", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_RevisionPaperNotPending_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "otherStatus" };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Chỉ có thể gửi revision feedback khi revision trong trạng thái pending", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_PaperNotRevisePhase_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "otherPhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Bài báo phải trong giai đoạn revise", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_ConferenceNotReady_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "notReadyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_ResearchPhaseNull_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = null };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Không tìm thấy giai đoạn cho hội nghị", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_DeadlineExpired_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1" }, "user1"));

            Assert.Contains("Giai đoạn gửi feedback revise diễn ra hạn chót", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_RevisionPaperSubmissionNotFound_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync(It.IsAny<string>())).ReturnsAsync((RevisionPaperSubmission?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "s1" }, "user1"));

            Assert.Contains("Không tìm thấy revision paper submission id", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_DeadlineRoundNull_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };
            var revisionSubmission = new RevisionPaperSubmission { RevisionPaperSubmissionId = "s1", RevisionDeadlineRound = null };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync(revisionSubmission);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "s1" }, "user1"));

            Assert.Contains("Không tìm thấy revision deadline", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_UserNotReviewer_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };
            var revisionSubmission = new RevisionPaperSubmission
            {
                RevisionPaperSubmissionId = "s1",
                // Giả sử EndSubmissionDate trực tiếp có trong RevisionPaperSubmission
                RevisionDeadlineRound = new RevisionRoundDeadline
                {
                    EndSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };


            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync(revisionSubmission);
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user1", "p1")).ReturnsAsync((PaperReviewer?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "s1" }, "user1"));

            Assert.Contains("Không tìm thấy user id user1", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionFeedback_UserNotHeadReviewer_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", RevisionPaperId = "r1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionPaper = new RevisionPaper { RevisionPaperId = "r1", GlobalStatusId = "pendingStatus" };
            var revisionSubmission = new RevisionPaperSubmission
            {
                RevisionPaperSubmissionId = "s1",
                // Giả sử EndSubmissionDate trực tiếp có trong RevisionPaperSubmission
                RevisionDeadlineRound = new RevisionRoundDeadline
                {
                    EndSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };
            var paperReviewer = new PaperReviewer { IsHeadReviewer = false };

            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1")).ReturnsAsync(revisionPaper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync(revisionSubmission);
            _mockUnitOfWork.Setup(x => x.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync("user1", "p1")).ReturnsAsync(paperReviewer);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionFeedBack(new CreateRevisionPaperSubmissionFeedback { PaperId = "p1", RevisionPaperSubmissionId = "s1" }, "user1"));

            Assert.Contains("Chức năng này dành cho head reviewer", ex.Message);
        }

        // Helper mock các status/phase cơ bản
        private void SetupBasicStatusPhaseMocks()
        {
            _mockUnitOfWork.Setup(x => x.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pendingStatus" });

            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "readyStatus" });

            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revisePhase" });
        }
    }
}
