using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.AbstractPaper
{
    public class SubmitAbstractPaperTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly PaperService _paperService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;

        public SubmitAbstractPaperTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockNotificationService = new Mock<INotificationService>();
            _objectStorageSettings = Options.Create(new ObjectStorageSettings { EndPoint = "https://mockstorage.com" });

            _paperService = new PaperService(
                _mockUnitOfWork.Object,
                Mock.Of<IMomoService>(),
                _mockTokenService.Object,
                _objectStorageSettings,
                _mockObjectStorageFileService.Object,
                Mock.Of<ITicketService>(),
                _mockTimeProviderService.Object,
                _mockNotificationService.Object,
                Mock.Of<IConferenceStepService>()
            );
        }

        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenPaperPhaseOrGlobalStatusNull()
        {
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync((GlobalStatus)null);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync((PaperPhase)null);

            var request = new CreateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.SubmitAbstract(request, "user1"));
        }
        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenPaperNotFound()
        {
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };

            // Mock GlobalStatusRepository
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(globalStatus);

            // Mock PaperPhaseRepository
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(paperPhase);

            // PaperRepository trả về null → trigger NotFoundException branch
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1"))
                .ReturnsAsync((Paper)null);

            var request = new CreateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.SubmitAbstract(request, "user1"));
        }

        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenActivePhaseNull()
        {
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var paper = new Paper { PaperId = "p1", ResearchConferencePhase = null, Conference = new Conference { ConferenceName = "Conf1" } };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1"))
                .ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
        .ReturnsAsync(paperPhase);
            var request = new CreateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.SubmitAbstract(request, "user1"));
        }
        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenOutsideRegistrationDate()
        {
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var now = new DateOnly(2025, 12, 2);
            var phase = new ResearchConferencePhase
            {
                RegistrationStartDate = now.AddDays(1),
                RegistrationEndDate = now.AddDays(2)
            };
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "phase1", // match mocked PaperPhase
                ResearchConferencePhase = phase,
                Conference = new Conference { ConferenceName = "Conf1" }
            };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1"))
                .ReturnsAsync(paper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(now);

            var request = new CreateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitAbstract(request, "user1"));
        }
        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenPaperPhaseMismatch()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var phase = new ResearchConferencePhase { RegistrationStartDate = now.AddDays(-1), RegistrationEndDate = now.AddDays(1) };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var paper = new Paper { PaperId = "p1", ResearchConferencePhase = phase, PaperPhaseId = "wrongPhase", Conference = new Conference { ConferenceName = "Conf1" }, PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } } };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(now);

            var request = new CreateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitAbstract(request, "user1"));
        }


        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenUserNotRootAuthor()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var phase = new ResearchConferencePhase { RegistrationStartDate = now.AddDays(-1), RegistrationEndDate = now.AddDays(1) };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var paper = new Paper { PaperId = "p1", ResearchConferencePhase = phase, PaperPhaseId = "phase1", Conference = new Conference { ConferenceName = "Conf1" }, PaperAuthors = new List<PaperAuthor>() };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(now);

            var request = new CreateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<NotFoundException>(() => _paperService.SubmitAbstract(request, "user1"));
        }

        // ví dụ test trường hợp thêm chính mình
        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenCoAuthorIsSelf()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var phase = new ResearchConferencePhase { RegistrationStartDate = now.AddDays(-1), RegistrationEndDate = now.AddDays(1) };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = phase,
                PaperPhaseId = "phase1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } }
            };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync("p1")).ReturnsAsync(new List<PaperReviewer>());
            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("user1", "conf1")).ReturnsAsync((ReviewerContract)null);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(now);

            var request = new CreateAbstractRequest { PaperId = "p1", CoAuthorId = new List<string> { "user1" } };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitAbstract(request, "user1"));
        }
        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenPaperAlreadyHasAbstract()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var phase = new ResearchConferencePhase { RegistrationStartDate = now.AddDays(-1), RegistrationEndDate = now.AddDays(1) };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = phase,
                PaperPhaseId = "phase1",
                AbstractId = "existingAbstract",
                Conference = new Conference { ConferenceName = "Conf1" },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } }
            };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(now);

            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync("p1"))
    .ReturnsAsync(new List<PaperReviewer>());
            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((ReviewerContract)null);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId(It.IsAny<string>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.Setup(u => u.AbstractRepository.CreateAbstractAsync(It.IsAny<Abstract>()))
                .ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateMutipleNotificationAsync(It.IsAny<List<Notification>>()))
                .ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.CreateMutiplePaperAuthorAsync(It.IsAny<List<PaperAuthor>>()))
                .ReturnsAsync(1);


            var request = new CreateAbstractRequest { PaperId = "p1" };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitAbstract(request, "user1"));
        }

        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenAbstractFileContentTypeNull()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var phase = new ResearchConferencePhase { RegistrationStartDate = now.AddDays(-1), RegistrationEndDate = now.AddDays(1) };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };

            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = phase,
                PaperPhaseId = "phase1",
                Conference = new Conference { ConferenceName = "Conf1" },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } }
            };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync("p1"))
    .ReturnsAsync(new List<PaperReviewer>());
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(now);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.ContentType).Returns((string)null); // content type null
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
            var request = new CreateAbstractRequest { PaperId = "p1", AbstractFile = mockFile.Object };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitAbstract(request, "user1"));
        }
        [Fact]
        public async Task SubmitAbstract_ShouldReturnFinalResult_WhenAllValid()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            var timeNow = DateTime.UtcNow;
            // Research conference phase
            var phase = new ResearchConferencePhase
            {
                RegistrationStartDate = dateNow.AddDays(-1),
                RegistrationEndDate = dateNow.AddDays(1)
            };
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            // Paper phase
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };

            // Paper with root author
            var paper = new Paper
            {
                PaperId = "p1",
                PaperPhaseId = "phase1",
                ConferenceId = "conf1",
                ResearchConferencePhase = phase,
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperAuthors = new List<PaperAuthor>
        {
            new PaperAuthor { UserId = "user1", IsRootAuthor = true }
        }
            };

            // Request with co-author
            var request = new CreateAbstractRequest
            {
                PaperId = "p1",
                Title = "Test Abstract",
                Description = "Desc",
                CoAuthorId = new List<string> { "co1" }
            };

            // Mock co-author user with Firebase tokens
            var coAuthorUser = new User
            {
                UserId = "co1",
                FirebaseMobileFcmToken = "token_mobile",
                FirebaseWebFcmToken = "token_web"
            };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
            .ReturnsAsync(globalStatus);
            // Mock UnitOfWork methods
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(paperPhase);

            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1"))
                .ReturnsAsync(paper);

            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync("conf1"))
                .ReturnsAsync(new List<PaperReviewer>());

            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("co1", "conf1"))
                .ReturnsAsync((ReviewerContract)null);

            _mockUnitOfWork.Setup(u => u.UserRepository.GetUserByUserId("co1"))
                .ReturnsAsync(coAuthorUser);

            _mockUnitOfWork.Setup(u => u.AbstractRepository.CreateAbstractAsync(It.IsAny<Abstract>()))
                .ReturnsAsync(1);

            _mockUnitOfWork.Setup(u => u.PaperRepository.UpdatePaperAsync(paper))
                .ReturnsAsync(1);

            _mockUnitOfWork.Setup(u => u.NotificationRepository.CreateMutipleNotificationAsync(It.IsAny<List<Notification>>()))
                .ReturnsAsync(1);

            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.CreateMutiplePaperAuthorAsync(It.IsAny<List<PaperAuthor>>()))
                .ReturnsAsync(1);

            // Mock TokenService
            _mockTokenService.Setup(t => t.GenerateSecureRandomToken()).Returns("token");

            // Mock ObjectStorageFileService
            _mockObjectStorageFileService.Setup(t => t.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>()
            )).ReturnsAsync("/abstracts/file.pdf");

            // Mock TimeProviderService
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(timeNow);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow);

            // Mock NotificationService
            _mockNotificationService
     .Setup(n => n.SendMobilePushAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
     .ReturnsAsync(true); // Moq có helper ReturnsAsync cho Task<bool>

            _mockNotificationService
                .Setup(n => n.SendWebPushAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _paperService.SubmitAbstract(request, "user1");

            // Assert
            Assert.Equal(4, result); // 4 repository calls: Abstract + Paper + Notification + PaperAuthor
            Assert.NotNull(paper.AbstractId);  // Paper.AbstractId updated
        }



        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenCoAuthorIsReviewer()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            var timeNow = DateTime.UtcNow;
            var phase = new ResearchConferencePhase { RegistrationStartDate = dateNow.AddDays(-1), RegistrationEndDate = dateNow.AddDays(1) };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = phase,
                PaperPhaseId = "phase1",
                ConferenceId = "conf1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } }
            };

            var reviewer = new PaperReviewer { UserId = "co1", PaperId = "p1" };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
        .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync("conf1")).ReturnsAsync(new List<PaperReviewer> { reviewer });
            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("co1", "conf1")).ReturnsAsync((ReviewerContract)null);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow);

            var request = new CreateAbstractRequest { PaperId = "p1", CoAuthorId = new List<string> { "co1" } };

            await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitAbstract(request, "user1"));
        }

        [Fact]
        public async Task SubmitAbstract_ShouldThrow_WhenCoAuthorHasActiveReviewerContract()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            var timeNow = DateTime.UtcNow;
            var phase = new ResearchConferencePhase { RegistrationStartDate = dateNow.AddDays(-1), RegistrationEndDate = dateNow.AddDays(1) };
            var paperPhase = new PaperPhase { PaperPhaseId = "phase1" };
            var globalStatus = new GlobalStatus { GlobalStatusId = "status1" };
            var paper = new Paper
            {
                PaperId = "p1",
                ResearchConferencePhase = phase,
                PaperPhaseId = "phase1",
                ConferenceId = "conf1",
                Conference = new Conference { ConferenceId = "conf1", ConferenceName = "Conf1" },
                PaperAuthors = new List<PaperAuthor> { new PaperAuthor { UserId = "user1", IsRootAuthor = true } }
            };

            var reviewerContract = new ReviewerContract
            {
                UserId = "co1",
                ConferenceId = "conf1",
                IsActive = true,
                User = new User { FullName = "Co Author 1" }
            };
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(globalStatus);
            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>())).ReturnsAsync(paperPhase);
            _mockUnitOfWork.Setup(u => u.PaperRepository.GetPaperByIdAsync("p1")).ReturnsAsync(paper);
            _mockUnitOfWork.Setup(u => u.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync("conf1")).ReturnsAsync(new List<PaperReviewer>());
            _mockUnitOfWork.Setup(u => u.ReviewerContractRepository.GetContractByUserAndConferenceAsync("co1", "conf1")).ReturnsAsync(reviewerContract);
            _mockTimeProviderService.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow);

            var request = new CreateAbstractRequest { PaperId = "p1", CoAuthorId = new List<string> { "co1" } };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _paperService.SubmitAbstract(request, "user1"));
            Assert.Contains("đang có hợp đồng review", ex.Message);
        }


    }

}
