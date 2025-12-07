using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using MockQueryable.Moq;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.ConferenceTest
{
    public class GetConferencesWithPricesTest
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

        public GetConferencesWithPricesTest()
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

        private List<Conference> GetSampleConferencesWithPrices()
        {
            return new List<Conference>
            {
                new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "AI Conference 2025",
                    ConferenceStatusId = "READY",
                    CreatedAt = new DateTime(2025, 11, 20),
                    Description = "Artificial Intelligence summit",
                    StartDate = new DateOnly(2025, 12, 10),
                    EndDate = new DateOnly(2025, 12, 12),
                    TotalSlot = 500,
                    AvailableSlot = 450,
                    Address = "Ho Chi Minh City Convention Center",
                    BannerImageUrl = "ai-banner.jpg",
                    TicketSaleStart = new DateOnly(2025, 11, 1),
                    TicketSaleEnd = new DateOnly(2025, 12, 9),
                    IsInternalHosted = true,
                    IsResearchConference = true,
                    CityId = "CITY1",
                    ConferenceCategoryId = "CAT1",
                    ConferencePrices = new List<ConferencePrice>
                    {
                        new ConferencePrice
                        {
                            ConferencePriceId = "CP1",
                            TicketPrice = 1000000,
                            TicketName = "Standard Ticket",
                            TicketDescription = "Standard access",
                            IsAuthor = false,
                            TotalSlot = 300,
                            AvailableSlot = 280,
                            PricePhases = new List<PricePhase>
                            {
                                new PricePhase
                                {
                                    PricePhaseId = "PP1",
                                    PhaseName = "Early Bird",
                                    StartDate = new DateOnly(2025, 11, 1),
                                    EndDate = new DateOnly(2025, 11, 20),
                                    ApplyPercent = 20,
                                    TotalSlot = 100,
                                    AvailableSlot = 90,
                                    RefundPolicies = new List<RefundPolicy>
                                    {
                                        new RefundPolicy
                                        {
                                            RefundPolicyId = "RP1",
                                            PercentRefund = 90,
                                            PricePhaseId = "PP1",
                                            RefundDeadline = new DateOnly(2025, 11, 15),
                                            RefundOrder = 1
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new Conference
                {
                    ConferenceId = "C2",
                    ConferenceName = "Data Science Workshop",
                    ConferenceStatusId = "READY",
                    CreatedAt = new DateTime(2025, 11, 25),
                    Description = "Learn data science techniques",
                    StartDate = new DateOnly(2025, 12, 15),
                    EndDate = new DateOnly(2025, 12, 17),
                    TotalSlot = 200,
                    AvailableSlot = 180,
                    Address = "Hanoi Tech Park",
                    BannerImageUrl = "ds-banner.jpg",
                    TicketSaleStart = new DateOnly(2025, 11, 5),
                    TicketSaleEnd = new DateOnly(2025, 12, 14),
                    IsInternalHosted = false,
                    IsResearchConference = false,
                    CityId = "CITY2",
                    ConferenceCategoryId = "CAT2",
                    ConferencePrices = new List<ConferencePrice>()
                },
                new Conference
                {
                    ConferenceId = "C3",
                    ConferenceName = "ML Summit",
                    ConferenceStatusId = "DRAFT",
                    CreatedAt = new DateTime(2025, 11, 28),
                    Description = "Machine Learning event",
                    StartDate = new DateOnly(2025, 12, 20),
                    EndDate = new DateOnly(2025, 12, 22),
                    TotalSlot = 300,
                    AvailableSlot = 300,
                    Address = "Da Nang Innovation Hub",
                    BannerImageUrl = "ml-banner.jpg",
                    CityId = "CITY1",
                    ConferenceCategoryId = "CAT1",
                    ConferencePrices = new List<ConferencePrice>()
                }
            };
        }

        // Setup repository mock với BuildMockDbSet để hỗ trợ async operations
        private void SetupConferenceRepoMock(List<Conference> conferences)
        {
            // Mock GetConferencesWithPrice để filter theo statusId
            _mockConferenceRepo
                .Setup(r => r.GetConferencesWithPrice(It.IsAny<string>()))
                .Returns<string>(statusId =>
                {
                    // Filter conferences theo statusId trước khi return
                    var filtered = conferences
                        .Where(c => c.ConferenceStatusId == statusId)
                        .AsQueryable()
                        .BuildMockDbSet();
                    return filtered.Object;
                });

            _mockConferenceStatusRepo
                .Setup(r => r.GetConferenceStatusByName("Ready"))
                .ReturnsAsync(new ConferenceStatus
                {
                    ConferenceStatusId = "READY",
                    ConferenceStatusName = "Ready"
                });
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldReturnOnlyReadyConferences()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, c => Assert.Contains(c.ConferenceId, new[] { "C1", "C2" }));
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldFilterBySearchKeyword()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10, searchKeyword: "AI");

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("C1", result.Items.First().ConferenceId);
            Assert.Contains("AI", result.Items.First().ConferenceName);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldFilterBySearchKeywordInDescription()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10, searchKeyword: "data science");

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("C2", result.Items.First().ConferenceId);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldFilterByCityId()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10, cityId: "CITY1");

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("C1", result.Items.First().ConferenceId);
            Assert.Equal("CITY1", result.Items.First().CityId);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldFilterByStartDate()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10, startDate: new DateOnly(2025, 12, 15));

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("C2", result.Items.First().ConferenceId);
            Assert.True(result.Items.First().StartDate >= new DateOnly(2025, 12, 15));
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldFilterByEndDate()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10, endDate: new DateOnly(2025, 12, 12));

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("C1", result.Items.First().ConferenceId);
            Assert.True(result.Items.First().EndDate <= new DateOnly(2025, 12, 12));
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldApplyMultipleFilters()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(
                1, 10,
                searchKeyword: "AI",
                cityId: "CITY1",
                startDate: new DateOnly(2025, 12, 1));

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("C1", result.Items.First().ConferenceId);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldOrderByCreatedAtDescending()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("C2", result.Items[0].ConferenceId);
            Assert.Equal("C1", result.Items[1].ConferenceId);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldApplyPaginationCorrectly()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act - Page 2 với pageSize = 1
            var result = await _service.GetConferencesWithPricesAsync(2, 1);

            // Assert
            // Order: C2 (2025-11-25), C1 (2025-11-20) -> Page 2 sẽ là C1
            Assert.Single(result.Items);
            Assert.Equal("C1", result.Items.First().ConferenceId); // C1 ở page 2
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(1, result.PageSize);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldIncludeConferencePricesWithPhasesAndRefundPolicies()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10);

            // Assert
            var conferenceWithPrices = result.Items.First(c => c.ConferenceId == "C1");
            Assert.NotNull(conferenceWithPrices.ConferencePrices);
            Assert.Single(conferenceWithPrices.ConferencePrices);

            var conferencePrice = conferenceWithPrices.ConferencePrices.First();
            Assert.Equal("CP1", conferencePrice.ConferencePriceId);
            Assert.Equal(1000000, conferencePrice.TicketPrice);
            Assert.Equal("Standard Ticket", conferencePrice.TicketName);

            Assert.NotNull(conferencePrice.PricePhases);
            Assert.Single(conferencePrice.PricePhases);

            var pricePhase = conferencePrice.PricePhases.First();
            Assert.Equal("PP1", pricePhase.PricePhaseId);
            Assert.Equal("Early Bird", pricePhase.PhaseName);

            Assert.NotNull(pricePhase.RefundPolicies);
            Assert.Single(pricePhase.RefundPolicies);

            var refundPolicy = pricePhase.RefundPolicies.First();
            Assert.Equal("RP1", refundPolicy.RefundPolicyId);
            Assert.Equal(90, refundPolicy.PercentRefund);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldMapAllPropertiesCorrectly()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10);

            // Assert
            var conference = result.Items.First(c => c.ConferenceId == "C1");
            Assert.Equal("AI Conference 2025", conference.ConferenceName);
            Assert.Equal("Artificial Intelligence summit", conference.Description);
            Assert.Equal(new DateOnly(2025, 12, 10), conference.StartDate);
            Assert.Equal(new DateOnly(2025, 12, 12), conference.EndDate);
            Assert.Equal(500, conference.TotalSlot);
            Assert.Equal(450, conference.AvailableSlot);
            Assert.Equal("Ho Chi Minh City Convention Center", conference.Address);
            Assert.Equal("ai-banner.jpg", conference.BannerImageUrl);
            Assert.True(conference.IsInternalHosted);
            Assert.True(conference.IsResearchConference);
            Assert.Equal("CITY1", conference.CityId);
            Assert.Equal("CAT1", conference.ConferenceCategoryId);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldReturnEmptyWhenNoMatchingFilters()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10, searchKeyword: "NonExistentKeyword");

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldHandleConferenceWithoutPrices()
        {
            // Arrange
            var conferences = GetSampleConferencesWithPrices();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10);

            // Assert
            var conferenceWithoutPrices = result.Items.First(c => c.ConferenceId == "C2");
            Assert.NotNull(conferenceWithoutPrices.ConferencePrices);
            Assert.Empty(conferenceWithoutPrices.ConferencePrices);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldOrderRefundPoliciesByRefundOrder()
        {
            // Arrange
            var conferences = new List<Conference>
            {
                new Conference
                {
                    ConferenceId = "C1",
                    ConferenceName = "Test Conference",
                    ConferenceStatusId = "READY",
                    CreatedAt = new DateTime(2025, 11, 20),
                    StartDate = new DateOnly(2025, 12, 10),
                    EndDate = new DateOnly(2025, 12, 12),
                    CityId = "CITY1",
                    ConferencePrices = new List<ConferencePrice>
                    {
                        new ConferencePrice
                        {
                            ConferencePriceId = "CP1",
                            PricePhases = new List<PricePhase>
                            {
                                new PricePhase
                                {
                                    PricePhaseId = "PP1",
                                    RefundPolicies = new List<RefundPolicy>
                                    {
                                        new RefundPolicy { RefundPolicyId = "RP3", RefundOrder = 3, PercentRefund = 50 },
                                        new RefundPolicy { RefundPolicyId = "RP1", RefundOrder = 1, PercentRefund = 90 },
                                        new RefundPolicy { RefundPolicyId = "RP2", RefundOrder = 2, PercentRefund = 70 }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10);

            // Assert
            var refundPolicies = result.Items.First().ConferencePrices.First().PricePhases.First().RefundPolicies;
            Assert.Equal(3, refundPolicies.Count);
            Assert.Equal("RP1", refundPolicies[0].RefundPolicyId);
            Assert.Equal("RP2", refundPolicies[1].RefundPolicyId);
            Assert.Equal("RP3", refundPolicies[2].RefundPolicyId);
        }

        [Fact]
        public async Task GetConferencesWithPricesAsync_ShouldHandleEmptyDatabase()
        {
            // Arrange
            var conferences = new List<Conference>();
            SetupConferenceRepoMock(conferences);

            // Act
            var result = await _service.GetConferencesWithPricesAsync(1, 10);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
        }
    }
}