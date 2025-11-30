using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Conference;
using Microsoft.Extensions.Options;
using MockQueryable;
using MockQueryable.Moq;  // ← QUAN TRỌNG: Thêm dòng này
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.ConferenceTest
{
    public class GetAllConferencesPaginatedTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly Mock<IConferenceStatusRepository> _mockConferenceStatusRepo;
        private readonly Mock<IConferenceStatusService> _mockConferenceStatusService;
        private readonly Mock<IConferenceTimelineService> _mockTimelineService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorage;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ISystemConfigurationService> _mockSystemConfig;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly ConferenceService _service;

        public GetAllConferencesPaginatedTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();
            _mockConferenceStatusRepo = new Mock<IConferenceStatusRepository>();
            _mockConferenceStatusService = new Mock<IConferenceStatusService>();
            _mockTimelineService = new Mock<IConferenceTimelineService>();
            _mockObjectStorage = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockSystemConfig = new Mock<ISystemConfigurationService>();
            _mockTimeProvider = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();

            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository).Returns(_mockConferenceStatusRepo.Object);

            var objectStorageSettings = Options.Create(new AppSettingConfig.ObjectStorageSettings());

            _service = new ConferenceService(
                _mockUnitOfWork.Object,
                _mockConferenceStatusService.Object,
                _mockTimelineService.Object,
                _mockObjectStorage.Object,
                _mockTokenService.Object,
                _mockSystemConfig.Object,
                objectStorageSettings,
                _mockTimeProvider.Object,
                _mockNotificationService.Object
            );
        }

        private List<Conference> GetSampleConferences()
        {
            return new List<Conference>
            {
                new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "Conf 1",
                    ConferenceStatusId = "Ready",
                    CreatedAt = new DateTime(2025, 11, 24),
                    Description = "Desc 1",
                    StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(12)),
                    Address = "Address 1",
                    BannerImageUrl = "banner1.jpg",
                    IsInternalHosted = true,
                    IsResearchConference = false,
                    ConferenceCategoryId = "CAT1"
                },
                new Conference
                {
                    ConferenceId = "C2",
                    ConferenceName = "Conf 2",
                    ConferenceStatusId = "Ready",
                    CreatedAt = new DateTime(2025, 11, 26),
                    Description = "Desc 2",
                    StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(15)),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(17)),
                    Address = "Address 2",
                    BannerImageUrl = "banner2.jpg",
                    IsInternalHosted = false,
                    IsResearchConference = true,
                    ConferenceCategoryId = "CAT2"
                },
                new Conference
                {
                    ConferenceId = "C3",
                    ConferenceName = "Conf 3",
                    ConferenceStatusId = "Draft",
                    CreatedAt = new DateTime(2025, 11, 28),
                    Description = "Desc 3",
                    StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(20)),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(22)),
                    Address = "Address 3",
                    BannerImageUrl = "banner3.jpg",
                    IsInternalHosted = true,
                    IsResearchConference = false,
                    ConferenceCategoryId = "CAT3"
                },
            };
        }

        [Fact]
        public async Task GetAllConferencesPaginatedAsync_ShouldReturnOnlyReadyConferences()
        {
            // Arrange
            var page = 1;
            var pageSize = 10;

            var conferences = GetSampleConferences();
            var mockQueryable = conferences.AsQueryable().BuildMock();  // ← BuildMock()

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockConferenceStatusRepo
                .Setup(r => r.GetConferenceStatusByNameAsync("Ready"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "Ready", ConferenceStatusName = "Ready" });

            // Act
            var result = await _service.GetAllConferencesPaginatedAsync(page, pageSize);

            // Assert
            Assert.Equal(2, result.Items.Count); // C1, C2 are Ready
            Assert.All(result.Items, c => Assert.Contains(c.ConferenceId, new[] { "C1", "C2" }));
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
        }

        [Fact]
        public async Task GetAllConferencesPaginatedAsync_ShouldApplyPaginationCorrectly()
        {
            // Arrange
            var page = 2;
            var pageSize = 1;

            var conferences = GetSampleConferences();
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockConferenceStatusRepo
                .Setup(r => r.GetConferenceStatusByNameAsync("Ready"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "Ready", ConferenceStatusName = "Ready" });

            // Act
            var result = await _service.GetAllConferencesPaginatedAsync(page, pageSize);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("C2", result.Items.First().ConferenceId); // C2 is second in CreatedAt order
        }

        [Fact]
        public async Task GetAllConferencesPaginatedAsync_ShouldReturnEmptyWhenNoReadyConferences()
        {
            // Arrange
            var conferences = new List<Conference>
            {
                new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "Draft Conf",
                    ConferenceStatusId = "Draft",
                    CreatedAt = DateTime.Now,
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now)
                }
            };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockConferenceStatusRepo
                .Setup(r => r.GetConferenceStatusByNameAsync("Ready"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "Ready", ConferenceStatusName = "Ready" });

            // Act
            var result = await _service.GetAllConferencesPaginatedAsync(1, 10);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetAllConferencesPaginatedAsync_ShouldOrderByCreatedAtAscending()
        {
            // Arrange
            var conferences = new List<Conference>
            {
                new Conference
                {
                    ConferenceId = "C3",
                    ConferenceName = "Latest",
                    ConferenceStatusId = "Ready",
                    CreatedAt = new DateTime(2025, 11, 28),
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now)
                },
                new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "Oldest",
                    ConferenceStatusId = "Ready",
                    CreatedAt = new DateTime(2025, 11, 19),
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now)
                },
                new Conference
                {
                    ConferenceId = "C2",
                    ConferenceName = "Middle",
                    ConferenceStatusId = "Ready",
                    CreatedAt = new DateTime(2025, 11, 24),
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now)
                }
            };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockConferenceStatusRepo
                .Setup(r => r.GetConferenceStatusByNameAsync("Ready"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "Ready", ConferenceStatusName = "Ready" });

            // Act
            var result = await _service.GetAllConferencesPaginatedAsync(1, 10);

            // Assert
            Assert.Equal(3, result.Items.Count);
            Assert.Equal("C1", result.Items[0].ConferenceId); // oldest
            Assert.Equal("C2", result.Items[1].ConferenceId);
            Assert.Equal("C3", result.Items[2].ConferenceId); // latest
        }

        [Fact]
        public async Task GetAllConferencesPaginatedAsync_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var conferences = new List<Conference>
            {
                new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "Test Conference",
                    ConferenceStatusId = "Ready",
                    CreatedAt = DateTime.Now,
                    Description = "Test Description",
                    StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(12)),
                    Address = "Test Address",
                    BannerImageUrl = "test-banner.jpg",
                    IsInternalHosted = true,
                    IsResearchConference = false,
                    ConferenceCategoryId = "CAT123"
                }
            };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockConferenceStatusRepo
                .Setup(r => r.GetConferenceStatusByNameAsync("Ready"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "Ready", ConferenceStatusName = "Ready" });

            // Act
            var result = await _service.GetAllConferencesPaginatedAsync(1, 10);

            // Assert
            var dto = result.Items.First();
            Assert.Equal("C1", dto.ConferenceId);
            Assert.Equal("Test Conference", dto.ConferenceName);
            Assert.Equal("Test Description", dto.Description);
            Assert.Equal("Test Address", dto.Address);
            Assert.Equal("test-banner.jpg", dto.BannerImageUrl);
            Assert.True(dto.IsInternalHosted);
            Assert.False(dto.IsResearchConference);
            Assert.Equal("CAT123", dto.ConferenceCategoryId);
        }

        [Fact]
        public async Task GetAllConferencesPaginatedAsync_ShouldHandleLastPageCorrectly()
        {
            // Arrange
            var conferences = GetSampleConferences();
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockConferenceStatusRepo
                .Setup(r => r.GetConferenceStatusByNameAsync("Ready"))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "Ready", ConferenceStatusName = "Ready" });

            // Act - page 3, pageSize 1, but only 2 Ready conferences
            var result = await _service.GetAllConferencesPaginatedAsync(3, 1);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Page);
            Assert.Equal(1, result.PageSize);
        }
    }
}