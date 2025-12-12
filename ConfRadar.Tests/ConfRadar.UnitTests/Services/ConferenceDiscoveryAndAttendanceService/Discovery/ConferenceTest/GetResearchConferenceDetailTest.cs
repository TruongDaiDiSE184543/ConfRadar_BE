using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using MockQueryable;
using Moq;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.ConferenceTest
{
    public class GetResearchConferenceDetailTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IResearchConferenceDetailRepository> _mockResearchDetailRepo;
        private readonly Mock<IResearchConferencePhaseRepository> _mockResearchPhaseRepo;
        private readonly Mock<IRankingFileUrlRepository> _mockRankingFileUrlRepo;
        private readonly Mock<IMaterialDownloadRepository> _mockMaterialDownloadRepo;
        private readonly Mock<IRankingReferenceUrlRepository> _mockRankingReferenceUrlRepo;
        private readonly Mock<IConferenceSessionRepository> _mockSessionRepo;

        // Mock service trong constructor
        private readonly Mock<IConferenceStatusService> _mockStatusService;
        private readonly Mock<IConferenceTimelineService> _mockTimelineService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ISystemConfigurationService> _mockSystemConfigService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly Mock<INotificationService> _mockNotificationService;

        private readonly ConferenceService _service;

        public GetResearchConferenceDetailTest()
        {
            // ==== Mock repo level ====
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockResearchDetailRepo = new Mock<IResearchConferenceDetailRepository>();
            _mockResearchPhaseRepo = new Mock<IResearchConferencePhaseRepository>();
            _mockRankingFileUrlRepo = new Mock<IRankingFileUrlRepository>();
            _mockMaterialDownloadRepo = new Mock<IMaterialDownloadRepository>();
            _mockRankingReferenceUrlRepo = new Mock<IRankingReferenceUrlRepository>();
            _mockSessionRepo = new Mock<IConferenceSessionRepository>();

            // Gán repo vào UnitOfWork
            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
            _mockUnitOfWork.Setup(u => u.ResearchConferenceDetailRepository).Returns(_mockResearchDetailRepo.Object);
            _mockUnitOfWork.Setup(u => u.ResearchConferencePhaseRepository).Returns(_mockResearchPhaseRepo.Object);
            _mockUnitOfWork.Setup(u => u.RankingFileUrlRepository).Returns(_mockRankingFileUrlRepo.Object);
            _mockUnitOfWork.Setup(u => u.MaterialDownloadRepository).Returns(_mockMaterialDownloadRepo.Object);
            _mockUnitOfWork.Setup(u => u.RankingReferenceUrlRepository).Returns(_mockRankingReferenceUrlRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository).Returns(_mockSessionRepo.Object);

            // ==== Mock service level ====
            _mockStatusService = new Mock<IConferenceStatusService>();
            _mockTimelineService = new Mock<IConferenceTimelineService>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockSystemConfigService = new Mock<ISystemConfigurationService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();

            // ==== Mock IOptions ====
            var mockObjectStorageSetting = Options.Create(new AppSettingConfig.ObjectStorageSettings
            {
                AccessKey = "key",
                SecretKey = "secret",
                EndPoint = "localhost",
            });

            // ==== Inject vào service ====
            _service = new ConferenceService(
                _mockUnitOfWork.Object,
                _mockStatusService.Object,
                _mockTimelineService.Object,
                _mockObjectStorageFileService.Object,
                _mockTokenService.Object,
                _mockSystemConfigService.Object,
                mockObjectStorageSetting,
                _mockTimeProviderService.Object,
                _mockNotificationService.Object
            );
        }

        private Conference GetSampleConference(bool isResearch = true)
        {
            return new Conference
            {
                ConferenceId = "C1",
                ConferenceName = "Test Conf",
                IsResearchConference = isResearch,
                CreatedByNavigation = new User
                {
                    FullName = "Creator",
                    Organization = new Organization { OrganizationName = "Org1" }
                }
            };
        }

        // =====================================================================
        // 1. Conference not found → throw NotFoundException
        // =====================================================================
        [Fact]
        public async Task GetResearchConferenceDetailAsync_ShouldThrowNotFound_WhenConferenceNotFound()
        {
            // Arrange
            var mockList = new List<Conference>().AsQueryable().BuildMock();
            _mockConferenceRepo.Setup(r => r.GetAllConferences()).Returns(mockList);

            // Act
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetResearchConferenceDetailAsync("C1", null));

            // Assert
            Assert.Contains("không tìm thấy", ex.Message, StringComparison.OrdinalIgnoreCase);
        }


        // =====================================================================
        // 2. Conference is not research → throw
        // =====================================================================
        [Fact]
        public async Task GetResearchConferenceDetailAsync_ShouldThrow_WhenConferenceIsNotResearch()
        {
            // Arrange
            var conf = GetSampleConference(isResearch: false);
            var mockList = new List<Conference> { conf }.AsQueryable().BuildMock();

            _mockConferenceRepo.Setup(r => r.GetAllConferences()).Returns(mockList);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetResearchConferenceDetailAsync("C1", null));
        }


        // =====================================================================
        // 3. userId != null → phải gọi TicketRepository
        // =====================================================================
        [Fact]
        public async Task GetResearchConferenceDetailAsync_ShouldCallTicketRepo_WhenUserIdProvided()
        {
            // Arrange
            var conf = GetSampleConference();
            var mockList = new List<Conference> { conf }.AsQueryable().BuildMock();
            _mockConferenceRepo.Setup(r => r.GetAllConferences()).Returns(mockList);

            _mockTicketRepo.Setup(t => t.GetTicketByUserIdAndConferenceId("U1", "C1"))
                           .ReturnsAsync(new Ticket { TicketId = "T1", PricePhaseId = "PP1" });

            // Act
            var result = await _service.GetResearchConferenceDetailAsync("C1", "U1");

            // Assert
            _mockTicketRepo.Verify(t => t.GetTicketByUserIdAndConferenceId("U1", "C1"), Times.Once);
            Assert.Equal("T1", result.purchasedInfo.ticketId);
        }


        // =====================================================================
        // 4. userId == null → không được gọi TicketRepo
        // =====================================================================
        [Fact]
        public async Task GetResearchConferenceDetailAsync_ShouldNotCallTicketRepo_WhenUserIdIsNull()
        {
            // Arrange
            var conf = GetSampleConference();
            var mockList = new List<Conference> { conf }.AsQueryable().BuildMock();
            _mockConferenceRepo.Setup(r => r.GetAllConferences()).Returns(mockList);

            // Act
            await _service.GetResearchConferenceDetailAsync("C1", null);

            // Assert
            _mockTicketRepo.Verify(
                r => r.GetTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }


        // =====================================================================
        // 5. Conference tồn tại → trả đúng mapped result
        // =====================================================================
        [Fact]
        public async Task GetResearchConferenceDetailAsync_ShouldReturnMappedResult_WhenConferenceExists()
        {
            // Arrange
            var conf = GetSampleConference();
            var mockList = new List<Conference> { conf }.AsQueryable().BuildMock();
            _mockConferenceRepo.Setup(r => r.GetAllConferences()).Returns(mockList);

            //_mockResearchDetailRepo.Setup(r => r.GetResearchConferenceDetailByConferenceIdAsync("C1"))
            //    .ReturnsAsync(new ResearchConferenceDetail { PaperFormat = "PDF" });

            _mockResearchPhaseRepo.Setup(r => r.GetResearchPhaseByConfId("C1"))
                .ReturnsAsync(new List<ResearchConferencePhase>
                {
                new ResearchConferencePhase { ResearchConferencePhaseId = "Phase 1" }
                });

            _mockRankingFileUrlRepo.Setup(r => r.GetRankingFileUrlsByConferenceIdAsync("C1"))
                .ReturnsAsync(new List<RankingFileUrl>());

            _mockMaterialDownloadRepo.Setup(r => r.GetMaterialsByConferenceIdAsync("C1"))
                .ReturnsAsync(new List<MaterialDownload>());

            _mockRankingReferenceUrlRepo.Setup(r => r.GetRankingReferenceUrlsByConferenceIdAsync("C1"))
                .ReturnsAsync(new List<RankingReferenceUrl>());

            _mockSessionRepo.Setup(r => r.GetSessionsByConferenceIdAsync("C1"))
                .ReturnsAsync(new List<ConferenceSession>());

            // Act
            var result = await _service.GetResearchConferenceDetailAsync("C1", null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("C1", result.ConferenceId);
            Assert.Equal("PDF", result.PaperFormat);
            Assert.NotNull(result.ResearchPhase);
            Assert.Single(result.ResearchPhase);
        }


        // =====================================================================
        // 6. Null research fields → vẫn trả về object đầy đủ
        // =====================================================================
        [Fact]
        public async Task GetResearchConferenceDetailAsync_ShouldReturnEvenWhenRelatedDataIsNull()
        {
            // Arrange
            var conf = GetSampleConference();
            var mockList = new List<Conference> { conf }.AsQueryable().BuildMock();
            _mockConferenceRepo.Setup(r => r.GetAllConferences()).Returns(mockList);

            _mockResearchDetailRepo.Setup(r => r.GetResearchConferenceDetailByConferenceIdAsync("C1"))
                .ReturnsAsync((ResearchConferenceDetail?)null);

            _mockResearchPhaseRepo.Setup(r => r.GetResearchPhaseByConfId("C1"))
                .ReturnsAsync((List<ResearchConferencePhase>?)null);

            _mockRankingFileUrlRepo.Setup(r => r.GetRankingFileUrlsByConferenceIdAsync("C1"))
                .ReturnsAsync((List<RankingFileUrl>?)null);

            _mockMaterialDownloadRepo.Setup(r => r.GetMaterialsByConferenceIdAsync("C1"))
                .ReturnsAsync((List<MaterialDownload>?)null);

            _mockRankingReferenceUrlRepo.Setup(r => r.GetRankingReferenceUrlsByConferenceIdAsync("C1"))
                .ReturnsAsync((List<RankingReferenceUrl>?)null);

            _mockSessionRepo.Setup(r => r.GetSessionsByConferenceIdAsync("C1"))
                .ReturnsAsync((List<ConferenceSession>?)null);

            // Act
            var result = await _service.GetResearchConferenceDetailAsync("C1", null);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.PaperFormat);
            Assert.Null(result.ResearchPhase);
            Assert.Null(result.RankingFileUrls);
        }
    }
}