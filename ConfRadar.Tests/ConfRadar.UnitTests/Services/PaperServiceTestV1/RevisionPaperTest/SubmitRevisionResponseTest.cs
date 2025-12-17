using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.RevisionPaperTest
{
    public class SubmitRevisionResponseTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<ITimeProviderService> _mockTimeProviderService = new();
        private readonly PaperService _service;

        public SubmitRevisionResponseTest()
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
        public async Task CreateRevisionResponse_StatusOrPhaseNull_ThrowsNotFoundException()
        {
            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>())).ReturnsAsync((ConferenceStatus?)null);
            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync((PaperPhase?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse(), "user1"));

            Assert.Contains("Không tìm thấy trạng thái", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_ResponsesEmpty_ThrowsBadRequestException()
        {
            SetupBasicStatusPhaseMocks();

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { Responses = new List<RevisionPaperSubmissionFeedbackResponse>() }, "user1"));

            Assert.Contains("Responses không được để trống", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_PaperNotFound_ThrowsNotFoundException()
        {
            SetupBasicStatusPhaseMocks();

            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync((Paper?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Không tìm thấy paper  id", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_PaperNotRevisePhase_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", PaperPhaseId = "otherPhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Bài báo phải trong giai đoạn revise", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_ConferenceNotReady_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "notReadyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_ResearchPhaseNull_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = null };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Không tìm thấy giai đoạn cho hội nghị", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_ReviseEndDateExpired_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) } };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Giai đoạn gửi response revise diễn ra hạn chót", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_RevisionSubmissionNotFound_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync((RevisionPaperSubmission?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", RevisionPaperSubmissionId = "s1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Không tìm thấy revision submission id", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_RevisionDeadlineNull_ThrowsNotFoundException()
        {
            var paper = new Paper { PaperId = "p1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionSubmission = new RevisionPaperSubmission { RevisionPaperSubmissionId = "s1", RevisionDeadlineRound = null };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync(revisionSubmission);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", RevisionPaperSubmissionId = "s1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Không tìm thấy revision deadline", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_DeadlineExpired_ThrowsBadRequestException()
        {
            var paper = new Paper { PaperId = "p1", PaperPhaseId = "revisePhase", Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } }, ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            var revisionSubmission = new RevisionPaperSubmission { RevisionPaperSubmissionId = "s1", RevisionDeadlineRound = new RevisionRoundDeadline { EndSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) } };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync(revisionSubmission);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", RevisionPaperSubmissionId = "s1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Deadline cho lần tương tác nằm hạn chót", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_UserNotRootAuthor_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "revisePhase",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } },
                ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = false, UserId = "other" } }
            };
            var revisionSubmission = new RevisionPaperSubmission { RevisionPaperSubmissionId = "s1", RevisionDeadlineRound = new RevisionRoundDeadline { EndSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync(revisionSubmission);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse { PaperId = "p1", RevisionPaperSubmissionId = "s1", Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() } }, "user1"));

            Assert.Contains("Bạn không sỡ hữu bài báo này", ex.Message);
        }

        [Fact]
        public async Task CreateRevisionResponse_FeedbackNotFound_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "revisePhase",
                Conference = new Conference { ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "readyStatus" } },
                ResearchConferencePhase = new ResearchConferencePhase { ReviseEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1" } }
            };
            var revisionSubmission = new RevisionPaperSubmission { RevisionPaperSubmissionId = "s1", RevisionDeadlineRound = new RevisionRoundDeadline { EndSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } };
            SetupBasicStatusPhaseMocks();
            _mockUnitOfWork.Setup(x => x.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(x => x.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync("s1")).ReturnsAsync(revisionSubmission);
            _mockUnitOfWork.Setup(x => x.RevisionSubmissionFeedbackRepository.GetFeedbackByIdAsync(It.IsAny<string>())).ReturnsAsync((RevisionSubmissionFeedback?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateRevisionSubmissionResponse(new CreateRevisionPaperSubmissionResponse
                {
                    PaperId = "p1",
                    RevisionPaperSubmissionId = "s1",
                    Responses = new List<RevisionPaperSubmissionFeedbackResponse> { new() { RevisionSubmissionFeedbackId = "f1", Response = "response" } }
                }, "user1"));

            Assert.Contains("Không tìm thấy paper id f1", ex.Message);
        }

        // Helper mock status/phase cơ bản
        private void SetupBasicStatusPhaseMocks()
        {
            _mockUnitOfWork.Setup(x => x.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "readyStatus" });
            _mockUnitOfWork.Setup(x => x.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revisePhase" });
        }
    }
}
