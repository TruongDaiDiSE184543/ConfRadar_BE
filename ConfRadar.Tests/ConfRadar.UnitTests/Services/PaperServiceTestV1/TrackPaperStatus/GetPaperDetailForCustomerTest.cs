using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pipelines.Sockets.Unofficial.Threading.MutexSlim;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.TrackPaperStatus
{
    public class GetPaperDetailForCustomerTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConferenceStepService> _mockConferenceStepService;
        private readonly Mock<ITicketService> _mockTicketService;

        private readonly PaperService _paperService;

        public GetPaperDetailForCustomerTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockConferenceStepService = new Mock<IConferenceStepService>();
            _mockTicketService = new Mock<ITicketService>();

            var objStorage = Options.Create(new AppSettingConfig.ObjectStorageSettings
            {
                EndPoint = "https://mock.com"
            });

            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                Mock.Of<IMomoService>(),
                _mockTokenService.Object,
                objStorage,
                _mockObjectStorageFileService.Object,
                _mockTicketService.Object,
                _mockTimeProviderService.Object,
                _mockNotificationService.Object,
                _mockConferenceStepService.Object
            );
        }

        // ---------------------------------------------------------------

        [Fact]
        public async Task GetPaperDetail_ShouldThrow_WhenPaperNotFound()
        {
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdWithPhaseAsync("p1"))
                   .ReturnsAsync((Paper)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _paperService.getPaperDetail("p1", "u1"));
        }

        // ---------------------------------------------------------------

        [Fact]
        public async Task GetPaperDetail_ShouldThrow_WhenUserNotAuthor()
        {
            var paper = new Paper { PaperId = "p1", ConferenceId = "c1" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdWithPhaseAsync("p1"))
                   .ReturnsAsync(paper);

            // Authors NOT including u1
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync("p1"))
                    .ReturnsAsync(new List<PaperAuthor>
                    {
                        new PaperAuthor { PaperId = "p1", UserId = "other" }
                    });

            await Assert.ThrowsAsync<Exception>(() =>
                _paperService.getPaperDetail("p1", "u1"));
        }

        // ---------------------------------------------------------------

        [Fact]
        public async Task GetPaperDetail_ShouldThrow_WhenResearchPhaseNotFound()
        {
            var paper = new Paper { PaperId = "p1", ConferenceId = "c1" };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdWithPhaseAsync("p1"))
                    .ReturnsAsync(paper);

            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync("p1"))
                    .ReturnsAsync(new List<PaperAuthor>
                    {
                        new PaperAuthor { PaperId = "p1", UserId = "u1" }
                    });

            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository
                .GetResearchConferencePhaseByPaperId("p1"))
                .ReturnsAsync((ResearchConferencePhase)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _paperService.getPaperDetail("p1", "u1"));
        }

        // ---------------------------------------------------------------

        [Fact]
        public async Task GetPaperDetail_ShouldReturnDetailCorrectly()
        {
            // ARRANGE -------------------------------------
            var paper = new Paper
            {
                PaperId = "p1",
                Title = "Paper Title",
                ConferenceId = "c1",
                PaperPhase = new PaperPhase { PaperPhaseId = "phase1", PhaseName = "Abstract" },
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    ResearchConferencePhaseId = "rcp1",
                    FullPaperStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    FullPaperEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))

                }
            };

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdWithPhaseAsync("p1"))
                    .ReturnsAsync(paper);

            // Authors
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync("p1"))
                    .ReturnsAsync(new List<PaperAuthor>
                    {
                        new PaperAuthor { PaperId = "p1", UserId = "u1", IsRootAuthor = true },
                        new PaperAuthor { PaperId = "p1", UserId = "u2", IsRootAuthor = false }
                    });

            // Root author
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u1"))
                    .ReturnsAsync(new User { UserId = "u1", FullName = "Root Author" });

            // Co-author
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("u2"))
                    .ReturnsAsync(new User { UserId = "u2", FullName = "Co Author" });

            // Research phase
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository.GetResearchConferencePhaseByPaperId("p1"))
                    .ReturnsAsync(paper.ResearchConferencePhase);

            // Revision deadlines
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository
                .GetRevisionRoundDeadlinesByPhaseIdAsync("rcp1"))
                .ReturnsAsync(new List<RevisionRoundDeadline>
                {
                    new RevisionRoundDeadline
                    {
                        RevisionRoundDeadlineId = "rd1",
                        RoundNumber = 1,
                        StartSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        EndSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                        ResearchConferencePhaseId = "rcp1"
                    }
                });

            // Conference Basic Info
            _mockConferenceStepService.Setup(s => s.GetResearchConferenceBasicAsync("c1"))
            .ReturnsAsync(new ResearchConferenceBasicStepResponse
    {
        ConferenceName = "Conference X",
        conferenceId = "c1"
        // Các field khác nếu cần mock
        });

            // ACT ------------------------------------------
            var result = await _paperService.getPaperDetail("p1", "u1");

            // ASSERT ---------------------------------------
            Assert.NotNull(result);
            Assert.Equal("p1", result.PaperId);
            Assert.Equal("Paper Title", result.Title);

            Assert.NotNull(result.RootAuthor);
            Assert.Equal("u1", result.RootAuthor.userId);

            Assert.Single(result.CoAuthors);
            Assert.Equal("u2", result.CoAuthors[0].userId);

            Assert.NotNull(result.ResearchPhase);
            Assert.Equal("rcp1", result.ResearchPhase.ResearchConferencePhaseId);

            Assert.NotNull(result.revisionDeadline);
            Assert.Single(result.revisionDeadline);
            Assert.Equal(1, result.revisionDeadline[0].RoundNumber);
        }
        [Fact]
        public async Task GetPaperDetail_ShouldReturnCorrectDetail_WhenDataValid()
        {
            // Arrange
            var paperId = "P1";
            var userId = "U1";

            // Conference
            var conference = new Conference
            {
                ConferenceId = "C1",
                ConferenceName = "Test Conf",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                CreatedAt = DateTime.UtcNow,
                BannerImageUrl = "banner.png",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusName = "Open" },
                CreatedByNavigation = new User { FullName = "Root Author Name" }
            };

            // Paper
            var paper = new Paper
            {
                PaperId = paperId,
                Title = "Paper Title",
                Description = "Paper Desc",
                ConferenceId = "C1",
                CreatedAt = DateTime.UtcNow,
                AbstractId = "A1",
                FullPaperId = "F1",
                RevisionPaperId = "R1",
                PaperPhase = new PaperPhase { PaperPhaseId = "PP1", PhaseName = "Abstract" },
                ResearchConferencePhase = new ResearchConferencePhase { ResearchConferencePhaseId = "RCP1" }
            };

            // Authors
            var rootAuthor = new PaperAuthor { PaperId = paperId, UserId = "U1", IsRootAuthor = true };
            var coAuthor = new PaperAuthor { PaperId = paperId, UserId = "U2", IsRootAuthor = false };

            _mockUnitOfWork.Setup(r => r.PaperRepository.GetPaperByIdWithPhaseAsync(paperId)).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(r => r.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId))
                .ReturnsAsync(new List<PaperAuthor> { rootAuthor, coAuthor });
            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByUserId("U1")).ReturnsAsync(new User { UserId = "U1", FullName = "Root Author Name" });
            _mockUnitOfWork.Setup(r => r.UserRepository.GetUserByUserId("U2")).ReturnsAsync(new User { UserId = "U2", FullName = "Co Author Name" });
            _mockUnitOfWork.Setup(r => r.ConferenceRepository.GetConferenceByIdAsync("C1")).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(r => r.ResearchConferencePhaseRepository.GetResearchConferencePhaseByPaperId(paperId))
                .ReturnsAsync(paper.ResearchConferencePhase);
            _mockUnitOfWork.Setup(r => r.ResearchConferencePhaseRepository.GetRevisionRoundDeadlinesByPhaseIdAsync("RCP1"))
                .ReturnsAsync(new List<RevisionRoundDeadline>
                {
            new RevisionRoundDeadline { RevisionRoundDeadlineId = "RD1", RoundNumber = 1, StartSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow), EndSubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
                });
            _mockUnitOfWork.Setup(r => r.AbstractRepository.GetAbstractByIdAsync("A1")).ReturnsAsync(new Abstract { AbstractId = "A1", Title = "Abs Title", AbstractUrl = "abs.pdf" });
            _mockUnitOfWork.Setup(r => r.FullPaperRepository.GetFullPaperByIdAsync("F1")).ReturnsAsync(new FullPaper { FullPaperId = "F1", Title = "Full Paper Title", FullPaperUrl = "full.pdf" });
            _mockUnitOfWork.Setup(r => r.RevisionPaperRepository.GetDetailRevisionPaper("R1")).ReturnsAsync(new RevisionPaper { RevisionPaperId = "R1", RevisionRound = 1 });

            _mockConferenceStepService.Setup(s => s.GetResearchConferenceBasicAsync("C1"))
                .ReturnsAsync(new ResearchConferenceBasicStepResponse { conferenceId = "C1", ConferenceName = "Test Conf", statusName = "Open", creatorUserName = "Root Author Name" });

            // Act
            var result = await _paperService.getPaperDetail(paperId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P1", result.PaperId);
            Assert.Equal("C1", result.researchConferenceInfo.conferenceId);
            Assert.Equal("Test Conf", result.researchConferenceInfo.ConferenceName);
            Assert.Equal("Open", result.researchConferenceInfo.statusName);
            Assert.Equal("Root Author Name", result.researchConferenceInfo.creatorUserName);
            Assert.Equal("U1", result.RootAuthor.userId);
            Assert.Single(result.CoAuthors);
            Assert.Equal("U2", result.CoAuthors[0].userId);
            Assert.NotNull(result.RevisionPaper);
            Assert.Single(result.revisionDeadline);
        }



    }

}
