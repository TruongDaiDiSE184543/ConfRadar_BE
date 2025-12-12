using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.TrackPaperStatus
{
    public class GetSubmittedPaperForCustomerTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConferenceStepService> _mockConferenceStepService;
        private readonly Mock<ITicketService> _mockTicketService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly PaperService _paperService;

        public GetSubmittedPaperForCustomerTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockConferenceStepService = new Mock<IConferenceStepService>();
            _mockTicketService = new Mock<ITicketService>();
            _mockEmailService = new Mock<IEmailService>(); 

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
                _mockConferenceStepService.Object,
                _mockEmailService.Object
            );
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetSubmittedPaper_ShouldReturnEmpty_WhenNoPapers()
        {
            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPapersByUserIdAsync("user1"))
                .ReturnsAsync(new List<Paper>());

            var result = await _paperService.GetSubmittedPaper("user1", null);

            Assert.Empty(result);
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetSubmittedPaper_ShouldFilterByConferenceId()
        {
            var list = new List<Paper>
            {
                new Paper { PaperId = "p1", ConferenceId = "c1" },
                new Paper { PaperId = "p2", ConferenceId = "c2" }
            };

            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPapersByUserIdAsync("user1"))
                .ReturnsAsync(list);

            var result = await _paperService.GetSubmittedPaper("user1", "c1");

            Assert.Single(result);
            Assert.Equal("p1", result[0].PaperId);
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetSubmittedPaper_ShouldMapBasicFieldsCorrectly()
        {
            var papers = new List<Paper>
            {
                new Paper
                {
                    PaperId = "p1",
                    Title = "Test title",
                    Description = "Test description",
                    ConferenceId = "c1",
                    Conference = new Conference
                    {
                        ConferenceName = "Conf",
                        Description = "Desc",
                        City = new City { CityName = "HCM" }
                    },
                    Abstract = new Abstract
                    {
                        AbstractId = "a1",
                        Title = "A title",
                        Description = "A desc"
                    },
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPapersByUserIdAsync("user"))
                .ReturnsAsync(papers);

            var result = await _paperService.GetSubmittedPaper("user", null);

            Assert.Single(result);
            Assert.Equal("p1", result[0].PaperId);
            Assert.Equal("Test title", result[0].PaperTitle);
            Assert.Equal("Conf", result[0].ConferenceName);
            Assert.Equal("HCM", result[0].CityName);
            Assert.NotNull(result[0].Abstract);
            Assert.Equal("a1", result[0].Abstract.AbstractId);
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetSubmittedPaper_ShouldHandleNullNestedObjects()
        {
            var papers = new List<Paper>
            {
                new Paper
                {
                    PaperId = "p1",
                    Conference = null,
                    Abstract = null,
                    FullPaper = null,
                    RevisionPaper = null,
                    CameraReady = null
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPapersByUserIdAsync("user"))
                .ReturnsAsync(papers);

            var result = await _paperService.GetSubmittedPaper("user", null);

            Assert.Single(result);
            Assert.Null(result[0].ConferenceName);
            Assert.Null(result[0].Abstract);
            Assert.Null(result[0].FullPaper);
            Assert.Null(result[0].RevisionPaper);
            Assert.Null(result[0].CameraReady);
        }

        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetSubmittedPaper_ShouldMapFullNestedObjectsCorrectly()
        {
            var papers = new List<Paper>
            {
                new Paper
                {
                    PaperId = "paperX",
                    ConferenceId = "confX",
                    Conference = new Conference
                    {
                        ConferenceName = "BigConf",
                        City = new City { CityName = "Hanoi" }
                    },
                    Abstract = new Abstract
                    {
                        AbstractId = "abs1",
                        Title = "Abs Title"
                    },
                    FullPaper = new FullPaper
                    {
                        FullPaperId = "fp1",
                        Title = "Full title"
                    },
                    RevisionPaper = new RevisionPaper
                    {
                        RevisionPaperId = "rev1",
                        RevisionRound = 2,
                        RevisionRoundDeadline = new RevisionRoundDeadline
                        {
                            RoundNumber = 5
                        }
                    },
                    CameraReady = new CameraReady
                    {
                        CameraReadyId = "cr1",
                        Title = "CR Title"
                    }
                }
            };

            _mockUnitOfWork.Setup(u => u.PaperAuthorRepository.GetPapersByUserIdAsync("u"))
                .ReturnsAsync(papers);

            var result = await _paperService.GetSubmittedPaper("u", null);

            Assert.Single(result);
            var dto = result[0];

            Assert.Equal("paperX", dto.PaperId);
            Assert.Equal("BigConf", dto.ConferenceName);
            Assert.Equal("abs1", dto.Abstract?.AbstractId);
            Assert.Equal("fp1", dto.FullPaper?.FullPaperId);
            Assert.Equal(2, dto.RevisionPaper?.RevisionRound);
            Assert.Equal(5, dto.RevisionPaper?.RevisionRoundDeadlineRoundNumber);
            Assert.Equal("cr1", dto.CameraReady?.CameraReadyId);
        }
    }
}
