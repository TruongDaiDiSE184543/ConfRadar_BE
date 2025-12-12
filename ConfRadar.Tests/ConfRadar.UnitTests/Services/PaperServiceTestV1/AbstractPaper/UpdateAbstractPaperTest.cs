using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Paper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.AbstractPaper
{
    public class UpdateAbstractPaperTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMomoService> _mockMomoService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IOptions<ObjectStorageSettings>> _mockObjectStorageSettings;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITicketService> _mockTicketService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConferenceStepService> _mockConferenceStepService;
        private readonly PaperService _paperService;
        private readonly Mock<IEmailService> _mockEmailService;
        public UpdateAbstractPaperTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMomoService = new Mock<IMomoService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageSettings = new Mock<IOptions<ObjectStorageSettings>>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTicketService = new Mock<ITicketService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockConferenceStepService = new Mock<IConferenceStepService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockObjectStorageSettings.SetupGet(x => x.Value)
    .Returns(new ObjectStorageSettings
    {
        EndPoint = "https://mock-storage/"
    });
            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                _mockMomoService.Object,
                _mockTokenService.Object,
                _mockObjectStorageSettings.Object,
                _mockObjectStorageFileService.Object,
                _mockTicketService.Object,
                _mockTimeProviderService.Object,
                _mockNotificationService.Object,
                _mockConferenceStepService.Object,
                _mockEmailService.Object
            );
        }

        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenPaperPhaseOrPendingStatusNull()
        {
            var request = new UpdateAbstractRequest { PaperId = "p1" };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync((GlobalStatus)null);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync((PaperPhase)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.UpdateAbstract(request, "user1"));
        }

        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenPaperNotFound()
        {
            var request = new UpdateAbstractRequest { PaperId = "p1" };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "status1" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "phase1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync((Paper)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.UpdateAbstract(request, "user1"));
        }

        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenPaperHasNoAbstract()
        {
            var request = new UpdateAbstractRequest { PaperId = "p1" };
            var paper = new Paper { PaperId = "p1", AbstractId = null };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "status1" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "phase1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.UpdateAbstract(request, "user1"));
        }

        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenAbstractNotPending()
        {
            var request = new UpdateAbstractRequest { PaperId = "p1" };
            var paper = new Paper { PaperId = "p1", AbstractId = "abs1" };
            var abstractPaper = new Abstract { AbstractId = "abs1", GlobalStatusId = "otherStatus" };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "pendingStatus" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "phase1" });
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1")).ReturnsAsync(abstractPaper);

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.UpdateAbstract(request, "user1"));
        }
        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenResearchConferencePhaseNull()
        {
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var abstractPaper = new Abstract { AbstractId = "abs1", GlobalStatusId = "status1" };

            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = null,  // null để test case này
                PaperPhaseId = "phase1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } },
                AbstractId = "abs1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" } // cần có
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1")).ReturnsAsync(abstractPaper);

            var request = new UpdateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.UpdateAbstract(request, "user1"));
        }

        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenCurrentDateOutsideRegistration()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var abstractPaper = new Abstract { AbstractId = "abs1", GlobalStatusId = "status1" };

            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RegistrationStartDate = dateNow.AddDays(1), // đặt start date sau ngày hiện tại
                    RegistrationEndDate = dateNow.AddDays(2)
                },
                PaperPhaseId = "phase1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } },
                AbstractId = "abs1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" } // phải có Conference
            };
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1")).ReturnsAsync(abstractPaper);

            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow);

            var request = new UpdateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.UpdateAbstract(request, "user1"));
        }

        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenPaperNotInAbstractPhase()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var abstractPaper = new Abstract { AbstractId = "abs1", GlobalStatusId = "status1" };
            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RegistrationStartDate = dateNow.AddDays(-1),
                    RegistrationEndDate = dateNow.AddDays(1)
                },
                PaperPhaseId = "wrongPhase", // không đúng phase
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } },
                AbstractId = "abs1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" } // thêm Conference
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1")).ReturnsAsync(abstractPaper);

            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow);

            var request = new UpdateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.UpdateAbstract(request, "user1"));
        }

        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenUserNotRootAuthor()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow); // ngày hiện tại
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var abstractPaper = new Abstract { AbstractId = "abs1", GlobalStatusId = "status1" };

            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "phase1",
                AbstractId = "abs1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user2", IsRootAuthor = true } }, // user1 không phải root
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RegistrationStartDate = dateNow, // bắt đầu đúng hôm nay
                    RegistrationEndDate = dateNow.AddDays(1) // kết thúc ngày mai
                },
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" }
            };

            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1")).ReturnsAsync(abstractPaper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow); // trả đúng dateNow

            var request = new UpdateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.UpdateAbstract(request, "user1"));
        }
        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenCoAuthorIsRootAuthor()
        {
            // Arrange
            var paper = new Paper
            {
                PaperId = "p1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } },
                AbstractId = "abs1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RegistrationStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RegistrationEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                },
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperPhaseId = "phase1"
            };

            var req = new UpdateAbstractRequest
            {
                PaperId = "p1",
                CoAuthorId = new List<string> { "user1" } // trùng root author
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1"))
    .ReturnsAsync(new Abstract { AbstractId = "abs1", GlobalStatusId = "status1" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "phase1" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "status1" });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.UpdateAbstract(req, "user1"));
        }


        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenCoAuthorIsReviewer()
        {
            // Arrange
            var paper = new Paper
            {
                PaperId = "p1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "root", IsRootAuthor = true } },
                AbstractId = "abs1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperPhaseId = "phase1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RegistrationStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RegistrationEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };

            var abstractPaper = new Abstract
            {
                AbstractId = "abs1",
                GlobalStatusId = "status1"
            };
            var req = new UpdateAbstractRequest
            {
                PaperId = "p1",
                CoAuthorId = new List<string> { "user2" }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "phase1" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "status1" });
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1")).ReturnsAsync(abstractPaper);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync("conf1"))
                .ReturnsAsync(new List<PaperReviewer> { new PaperReviewer { UserId = "user2" } });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.UpdateAbstract(req, "root"));
        }
        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenCoAuthorHasActiveReviewerContract()
        {
            // Arrange
            var paper = new Paper
            {
                PaperId = "p1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "root", IsRootAuthor = true } },
                AbstractId = "abs1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperPhaseId = "phase1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RegistrationStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RegistrationEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };
            var abstractPaper = new Abstract
            {
                AbstractId = "abs1",
                GlobalStatusId = "status1"
            };
            var req = new UpdateAbstractRequest
            {
                PaperId = "p1",
                CoAuthorId = new List<string> { "user3" }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "phase1" });
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1")).ReturnsAsync(abstractPaper);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "status1" });
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync("conf1"))
                .ReturnsAsync(new List<PaperReviewer>());

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("user3", "conf1"))
                .ReturnsAsync(new ReviewerContract { IsActive = true });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.UpdateAbstract(req, "root"));
        }
        [Fact]
        public async Task UpdateAbstract_ShouldThrow_WhenAbstractFileContentTypeIsNull()
        {
            // Arrange
            var paper = new Paper
            {
                PaperId = "p1",
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "root", IsRootAuthor = true } },
                AbstractId = "abs1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperPhaseId = "phase1",
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    RegistrationStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    RegistrationEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
                }
            };

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.ContentType).Returns((string?)null);

            var req = new UpdateAbstractRequest
            {
                PaperId = "p1",
                AbstractFile = mockFile.Object
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.AbstractRepository.GetAbstractByIdAsync("abs1"))
    .ReturnsAsync(new Abstract { AbstractId = "abs1", GlobalStatusId = "status1" });
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "phase1" });
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "status1" });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.UpdateAbstract(req, "root"));
        }


    }
}
