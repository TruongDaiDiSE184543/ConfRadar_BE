using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.ReviewandDecidePaper
{
    public class DecideRevisionStatusTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly PaperService _service;
        public DecideRevisionStatusTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockNotificationService = new Mock<INotificationService>();

            _service = new PaperService(
                _mockUnitOfWork.Object,
                null, null, null, null, null,
                _mockTimeProviderService.Object,
                _mockNotificationService.Object,
                null,
                _mockEmailService.Object
            );
        }

        [Fact]
        public async Task GlobalStatusPending_ThrowsBadRequestException()
        {
            // Arrange
            var revisePhase = new PaperPhase { PaperPhaseId = "revise" };
            var cameraReadyPhase = new PaperPhase { PaperPhaseId = "camera_ready" };
            var readyStatus = new ConferenceStatus { ConferenceStatusId = "ready" };

            var pendingStatus = new GlobalStatus
            {
                GlobalStatusId = "pending",
                Name = GlobalStatusEnum.Pending.GetDescription()
            };
            var acceptedStatus = new GlobalStatus
            {
                GlobalStatusId = "accepted",
                Name = GlobalStatusEnum.Accepted.GetDescription()
            };
            var rejectedStatus = new GlobalStatus
            {
                GlobalStatusId = "rejected",
                Name = GlobalStatusEnum.Rejected.GetDescription()
            };

            // Mock các repository ngay từ đầu method
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription()))
                .ReturnsAsync(revisePhase);

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription()))
                .ReturnsAsync(cameraReadyPhase);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingStatus);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(acceptedStatus);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                .ReturnsAsync(rejectedStatus);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            var request = new UpdateRevisionStatusRequest
            {
                PaperId = "p1",
                RevisionPaperId = "r1",
                GlobalStatus = GlobalStatusEnum.Pending // Điểm cần test
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("Không thể chuyển pending", ex.Message);
        }

        [Fact]
        public async Task PaperNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var revisePhase = new PaperPhase { PaperPhaseId = "revise" };
            var cameraReadyPhase = new PaperPhase { PaperPhaseId = "camera_ready" };
            var readyStatus = new ConferenceStatus { ConferenceStatusId = "ready" };

            var pendingStatus = new GlobalStatus
            {
                GlobalStatusId = "pending",
                Name = GlobalStatusEnum.Pending.GetDescription()
            };
            var acceptedStatus = new GlobalStatus
            {
                GlobalStatusId = "accepted",
                Name = GlobalStatusEnum.Accepted.GetDescription()
            };
            var rejectedStatus = new GlobalStatus
            {
                GlobalStatusId = "rejected",
                Name = GlobalStatusEnum.Rejected.GetDescription()
            };

            // Mock các repository đầu method
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription()))
                .ReturnsAsync(revisePhase);

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription()))
                .ReturnsAsync(cameraReadyPhase);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingStatus);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(acceptedStatus);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                .ReturnsAsync(rejectedStatus);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            // Mock paper not found - điểm cần test
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Paper?)null);

            var request = new UpdateRevisionStatusRequest
            {
                PaperId = "p1",
                RevisionPaperId = "r1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("Không tìm thấy paper", ex.Message);
        }

        [Fact]
        public async Task ConferenceNotReady_ThrowsBadRequestException()
        {
            // Arrange
            var readyStatus = new ConferenceStatus { ConferenceStatusId = "ready" };
            var revisePhase = new PaperPhase { PaperPhaseId = "revise" };
            var cameraReadyPhase = new PaperPhase { PaperPhaseId = "camera_ready" };

            // Mock GlobalStatus
            var pendingStatus = new GlobalStatus
            {
                GlobalStatusId = "pending",
                Name = GlobalStatusEnum.Pending.GetDescription()
            };
            var acceptedStatus = new GlobalStatus
            {
                GlobalStatusId = "accepted",
                Name = GlobalStatusEnum.Accepted.GetDescription()
            };
            var rejectedStatus = new GlobalStatus
            {
                GlobalStatusId = "rejected",
                Name = GlobalStatusEnum.Rejected.GetDescription()
            };

            var revisionPaper = new RevisionPaper
            {
                RevisionPaperId = "r1",
                GlobalStatusId = "pending",
                RevisionPaperSubmissions = new List<RevisionPaperSubmission>(),
                RevisionRoundDeadlineId = null
            };

            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "revise",
                ConferenceId = "conf1",
                RevisionPaperId = "r1", // Foreign key
                Conference = new Conference
                {
                    ConferenceId = "conf1",
                    ConferenceName = "Test Conference",
                    ConferenceStatusId = "not_ready", // Khác với ready - điểm cần test
                    ConferenceStatus = new ConferenceStatus
                    {
                        ConferenceStatusId = "not_ready"
                    },
                    ResearchConferenceDetail = new ResearchConferenceDetail
                    {
                        RevisionAttemptAllowed = 3
                    }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RevisionPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RevisionPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                    CameraReadyStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                    CameraReadyEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
                },
                PaperAuthors = new List<PaperAuthor>
        {
            new PaperAuthor
            {
                IsRootAuthor = true,
                UserId = "user1",
                User = new User
                {
                    UserId = "user1",
                    FullName = "User One",
                    Email = "user1@test.com"
                }
            }
        }
            };

            var request = new UpdateRevisionStatusRequest
            {
                PaperId = "paper1",
                RevisionPaperId = "r1",
                GlobalStatus = GlobalStatusEnum.Accepted,
                Reason = "Test reason"
            };

            // Mock PaperPhaseRepository
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription()))
                .ReturnsAsync(revisePhase);

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription()))
                .ReturnsAsync(cameraReadyPhase);

            // Mock GlobalStatusRepository
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingStatus);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(acceptedStatus);

            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                .ReturnsAsync(rejectedStatus);

            // Mock ConferenceStatusRepository
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            // Mock PaperRepository
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("paper1"))
                .ReturnsAsync(paper);

            // Mock RevisionPaperRepository - quan trọng!
            _mockUnitOfWork.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync("r1"))
                .ReturnsAsync(revisionPaper);

            // Mock TimeProvider
            _mockTimeProviderService.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.UtcNow);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("Hội nghị chưa ready", ex.Message);
        }

        [Fact]
        public async Task RevisionPaperNotFound_ThrowsNotFoundException()
        {

            var revisePhase = new PaperPhase { PaperPhaseId = "revise" };
            var cameraReadyPhase = new PaperPhase { PaperPhaseId = "camera_ready" };
            var readyStatus = new ConferenceStatus { ConferenceStatusId = "ready" };

            var pendingStatus = new GlobalStatus { GlobalStatusId = "pending" };
            var acceptedStatus = new GlobalStatus { GlobalStatusId = "accepted" };
            var rejectedStatus = new GlobalStatus { GlobalStatusId = "rejected" };

            // Mock common dependencies
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription()))
                .ReturnsAsync(revisePhase);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription()))
                .ReturnsAsync(cameraReadyPhase);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingStatus);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(acceptedStatus);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                .ReturnsAsync(rejectedStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "revise",
                ConferenceId = "conf1",
                Conference = new Conference
                {
                    ConferenceId = "conf1",
                    ConferenceName = "Test Conference",
                    ConferenceStatusId = "ready",
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    ResearchConferenceDetail = new ResearchConferenceDetail { RevisionAttemptAllowed = 2 }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RevisionPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RevisionPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                RevisionPaperId = "r1",
                PaperAuthors = new List<PaperAuthor>
        {
            new PaperAuthor
            {
                IsRootAuthor = true,
                UserId = "user1",
                User = new User
                {
                    UserId = "user1",
                    FullName = "User One",
                    Email = "user1@test.com"
                }
            }
        }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            // Mock TimeProvider - quan trọng vì check date range
            _mockTimeProviderService.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.UtcNow);

            // Mock revision paper not found - điểm cần test
            _mockUnitOfWork.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((RevisionPaper?)null);

            var request = new UpdateRevisionStatusRequest
            {
                PaperId = "p1",
                RevisionPaperId = "r1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("Không tìm thấy  revision paper", ex.Message);
        }

        [Fact]
        public async Task UserNotHeadReviewer_ThrowsBadRequestException()
        {
            // Arrange
            var revisePhase = new PaperPhase { PaperPhaseId = "revise" };
            var cameraReadyPhase = new PaperPhase { PaperPhaseId = "camera_ready" };
            var readyStatus = new ConferenceStatus { ConferenceStatusId = "ready" };

            var pendingStatus = new GlobalStatus { GlobalStatusId = "pending" };
            var acceptedStatus = new GlobalStatus { GlobalStatusId = "accepted" };
            var rejectedStatus = new GlobalStatus { GlobalStatusId = "rejected" };

            // Mock common dependencies
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription()))
                .ReturnsAsync(revisePhase);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription()))
                .ReturnsAsync(cameraReadyPhase);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(pendingStatus);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(acceptedStatus);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                .ReturnsAsync(rejectedStatus);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "revise",
                ConferenceId = "conf1",
                Conference = new Conference
                {
                    ConferenceId = "conf1",
                    ConferenceName = "Test Conference",
                    ConferenceStatusId = "ready",
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    ResearchConferenceDetail = new ResearchConferenceDetail { RevisionAttemptAllowed = 2 }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RevisionPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RevisionPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                RevisionPaperId = "r1",
                PaperAuthors = new List<PaperAuthor>
        {
            new PaperAuthor
            {
                IsRootAuthor = true,
                UserId = "user1",
                User = new User
                {
                    UserId = "user1",
                    FullName = "User One",
                    Email = "user1@test.com"
                }
            }
        }
            };

            var revisionPaper = new RevisionPaper
            {
                RevisionPaperId = "r1",
                RevisionPaperSubmissions = new List<RevisionPaperSubmission> { new RevisionPaperSubmission() },
                RevisionRoundDeadlineId = null
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            _mockUnitOfWork.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(revisionPaper);

            // Mock TimeProvider
            _mockTimeProviderService.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.UtcNow);

            // Mock paper reviewer không phải head reviewer - điểm cần test
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PaperReviewer
                {
                    IsHeadReviewer = false, // Không phải head reviewer
                    UserId = "user1",
                    PaperId = "p1"
                });

            var request = new UpdateRevisionStatusRequest
            {
                PaperId = "p1",
                RevisionPaperId = "r1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("Bạn không phải head reviewer", ex.Message);
        }
        [Fact]
        public async Task DecideReviseStatus_ActiveResearchPhaseNull_ThrowsNotFoundException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "revise",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" }
                },
                ResearchConferencePhase = null, // case 4
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1", User = new User { UserId = "user1", FullName = "User One", Email = "user1@test.com" } } }
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);

            var request = new UpdateRevisionStatusRequest { PaperId = "paper1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Accepted };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("giai đoạn", ex.Message);
        }

        [Fact]
        public async Task DecideReviseStatus_DateNotInDecidePhase_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "revise",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RevisionPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                    RevisionPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
                },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1", User = new User { UserId = "user1", FullName = "User One", Email = "user1@test.com" } } }
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateRevisionStatusRequest { PaperId = "paper1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Accepted };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("Giai đoạn quyết định revise", ex.Message);
        }

        [Fact]
        public async Task DecideReviseStatus_PaperNotInRevisePhase_ThrowsBadRequestException()
        {
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperPhaseId = "fullpaper", // case 6
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" }
                },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RevisionPaperDecideStatusStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RevisionPaperDecideStatusEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                PaperAuthors = new List<PaperAuthor> { new() { IsRootAuthor = true, UserId = "user1", User = new User { UserId = "user1", FullName = "User One", Email = "user1@test.com" } } }
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "revise" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "accepted" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "ready" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(paper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            var request = new UpdateRevisionStatusRequest { PaperId = "paper1", RevisionPaperId = "rev1", GlobalStatus = GlobalStatusEnum.Accepted };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DecideReviseStatus(request, "user1"));

            Assert.Contains("Paper không trong giai đoạn revise", ex.Message);
        }

    }
}
