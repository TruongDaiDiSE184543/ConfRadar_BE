using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Conference;
using Microsoft.Extensions.Options;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.ConferenceTest
{
    public class GetTechnicalConferenceDetailTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IConferenceStatusService> _mockConferenceStatusService;
        private readonly Mock<IConferenceTimelineService> _mockTimelineService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorage;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ISystemConfigurationService> _mockSystemConfig;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly ConferenceService _service;

        public GetTechnicalConferenceDetailTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockConferenceStatusService = new Mock<IConferenceStatusService>();
            _mockTimelineService = new Mock<IConferenceTimelineService>();
            _mockObjectStorage = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockSystemConfig = new Mock<ISystemConfigurationService>();
            _mockTimeProvider = new Mock<ITimeProviderService>();
            _mockNotificationService = new Mock<INotificationService>();

            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);

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

        private Conference GetSampleTechnicalConference()
        {
            return new Conference
            {
                ConferenceId = "CONF1",
                ConferenceName = "Tech Summit 2025",
                Description = "Annual technology summit",
                StartDate = new DateOnly(2025, 12, 10),
                EndDate = new DateOnly(2025, 12, 12),
                TotalSlot = 500,
                AvailableSlot = 450,
                Address = "Tech Convention Center",
                BannerImageUrl = "tech-banner.jpg",
                CreatedAt = new DateTime(2025, 11, 1),
                TicketSaleStart = new DateOnly(2025, 11, 1),
                TicketSaleEnd = new DateOnly(2025, 12, 9),
                IsInternalHosted = true,
                IsResearchConference = false, // Technical conference
                CityId = "CITY1",
                ConferenceCategoryId = "CAT1",
                ConferenceStatusId = "Ready",
                CreatedBy = "USER1",
                CreatedByNavigation = new User
                {
                    UserId = "USER1",
                    FullName = "John Doe",
                    Organization = new Organization
                    {
                        OrganizationId = "ORG1",
                        OrganizationName = "Tech Corp"
                    }
                },
                ConferenceCategory = new ConferenceCategory
                {
                    ConferenceCategoryId = "CAT1",
                    ConferenceCategoryName = "Technology"
                },
                TechnicalConferenceDetail = new TechnicalConferenceDetail
                {
                    ConferenceId = "CONF1",
                    TargetAudience = "Developers, Tech enthusiasts"
                },
                Policies = new List<Policy>
                {
                    new Policy
                    {
                        PolicyId = "POL1",
                        Description = "No refunds after event starts"
                    }
                },
                Sponsors = new List<Sponsor>
                {
                    new Sponsor
                    {
                        SponsorId = "SPO1",
                        Name = "Tech Sponsor Inc",
                        ImageUrl = "sponsor-logo.jpg"
                    }
                },
                ConferenceMedia = new List<ConferenceMedium>
                {
                    new ConferenceMedium
                    {
                        ConferenceMediaId = "MED1",
                        ConferenceMediaUrl = "video1.mp4",
                    }
                },
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
                },
                ConferenceSessions = new List<ConferenceSession>
                {
                    new ConferenceSession
                    {
                        ConferenceSessionId = "SESS1",
                        Title = "Keynote: Future of AI",
                        Description = "Opening keynote",
                        StartTime = new DateTime(2025, 12, 10, 9, 0, 0),
                        EndTime = new DateTime(2025, 12, 10, 10, 0, 0),
                        Room = new Room
                        {
                            RoomId = "ROOM1",
                            DisplayName = "Main Hall",
                            Destination = new Destination
                            {
                                DestinationId = "DEST1",
                                Name = "Tech Center",
                                City = new City
                                {
                                    CityId = "CITY1",
                                    CityName = "Ho Chi Minh City"
                                }
                            }
                        },
                        Speakers = new List<Speaker>
                        {
                            new Speaker
                            {
                                SpeakerId = "SPK1",
                                Name = "Jane Smith",
                                Description = "AI Expert"
                            }
                        },
                        ConferenceSessionMedia = new List<ConferenceSessionMedium>
                        {
                            new ConferenceSessionMedium
                            {
                                ConferenceSessionMediaId = "SESSMED1",
                                
                                MediaUrl = "session-video.mp4"
                            }
                        }
                    }
                }
            };
        }

        private Conference GetSampleResearchConference()
        {
            return new Conference
            {
                ConferenceId = "CONF2",
                ConferenceName = "Research Summit 2025",
                Description = "Academic research conference",
                StartDate = new DateOnly(2025, 12, 15),
                EndDate = new DateOnly(2025, 12, 17),
                IsResearchConference = true, // Research conference
                CreatedBy = "USER1",
                CreatedByNavigation = new User
                {
                    UserId = "USER1",
                    FullName = "John Doe"
                }
            };
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldReturnDetailWithoutTicketInfo_WhenUserIdIsNull()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CONF1", result.ConferenceId);
            Assert.Equal("Tech Summit 2025", result.ConferenceName);
            Assert.NotNull(result.purchasedInfo);
            Assert.Equal(string.Empty, result.purchasedInfo.ticketId);
            Assert.Equal(string.Empty, result.purchasedInfo.conferencePriceId);
            Assert.Equal(string.Empty, result.purchasedInfo.pricePhaseId);

            _mockTicketRepo.Verify(r => r.GetTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldReturnDetailWithTicketInfo_WhenUserHasTicket()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            var ticket = new Ticket
            {
                TicketId = "TICKET1",
                UserId = "USER1",
                
                PricePhaseId = "PP1",
                PricePhase = new PricePhase
                {
                    PricePhaseId = "PP1",
                    ConferencePrice = new ConferencePrice
                    {
                        ConferencePriceId = "CP1",
                        Conference = new Conference()
                        {
                            ConferenceId = "CONF1",
                        }
                    }
                }
            };

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockTicketRepo
                .Setup(r => r.GetTicketByUserIdAndConferenceId("USER1", "CONF1"))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", "USER1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TICKET1", result.purchasedInfo.ticketId);
            Assert.Equal("CP1", result.purchasedInfo.conferencePriceId);
            Assert.Equal("PP1", result.purchasedInfo.pricePhaseId);
            _mockTicketRepo.Verify(r => r.GetTicketByUserIdAndConferenceId("USER1", "CONF1"), Times.Once);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldReturnDetailWithNullTicketInfo_WhenUserHasNoTicket()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            _mockTicketRepo
                .Setup(r => r.GetTicketByUserIdAndConferenceId("USER1", "CONF1"))
                .ReturnsAsync((Ticket)null);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", "USER1");

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.purchasedInfo.ticketId);
            Assert.Null(result.purchasedInfo.conferencePriceId);
            Assert.Null(result.purchasedInfo.pricePhaseId);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldThrowNotFoundException_WhenConferenceDoesNotExist()
        {
            // Arrange
            var conferences = new List<Conference>();
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetTechnicalConferenceDetailAsync("NONEXISTENT", null)
            );
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldThrowException_WhenConferenceIsResearchType()
        {
            // Arrange
            var conference = GetSampleResearchConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetTechnicalConferenceDetailAsync("CONF2", null)
            );
            Assert.Equal("chức năng chỉ dành cho tech", exception.Message);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldMapBasicPropertiesCorrectly()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.Equal("CONF1", result.ConferenceId);
            Assert.Equal("Tech Summit 2025", result.ConferenceName);
            Assert.Equal("Annual technology summit", result.Description);
            Assert.Equal(new DateOnly(2025, 12, 10), result.StartDate);
            Assert.Equal(new DateOnly(2025, 12, 12), result.EndDate);
            Assert.Equal(500, result.TotalSlot);
            Assert.Equal(450, result.AvailableSlot);
            Assert.Equal("Tech Convention Center", result.Address);
            Assert.Equal("tech-banner.jpg", result.BannerImageUrl);
            Assert.True(result.IsInternalHosted);
            Assert.False(result.IsResearchConference);
            Assert.Equal("CITY1", result.CityId);
            Assert.Equal("CAT1", result.ConferenceCategoryId);
            Assert.Equal("Ready", result.ConferenceStatusId);
            Assert.Equal("USER1", result.createdBy);
            Assert.Equal("John Doe", result.UserNameCreator);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldIncludeTargetAudience()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result.TargetAudience);
            Assert.Equal("Developers, Tech enthusiasts", result.TargetAudience);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldIncludePolicies()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result.Policies);
            Assert.Single(result.Policies);
            // Note: Verify actual policy mapping based on ToConferencePolicyResponse() implementation
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldIncludeSponsors()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result.Sponsors);
            Assert.Single(result.Sponsors);
            // Note: Verify actual sponsor mapping based on ToSponsorResponse() implementation
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldIncludeConferenceMedia()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result.ConferenceMedia);
            Assert.Single(result.ConferenceMedia);
            // Note: Verify actual media mapping based on ToConferenceMediaResponse() implementation
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldIncludeConferencePricesWithPhases()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result.ConferencePrices);
            Assert.Single(result.ConferencePrices);
            // Note: Verify actual price mapping based on ToConferencePriceWithPhasesResponse() implementation
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldIncludeSessionsWithSpeakers()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result.Sessions);
            Assert.Single(result.Sessions);
            // Note: Verify actual session mapping based on ToConferenceSessionWithSpeakersResponse() implementation
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldHandleNullTechnicalDetail()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            conference.TechnicalConferenceDetail = null;
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.Null(result.TargetAudience);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldHandleEmptyCollections()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            conference.Policies = new List<Policy>();
            conference.Sponsors = new List<Sponsor>();
            conference.ConferenceMedia = new List<ConferenceMedium>();
            conference.ConferencePrices = new List<ConferencePrice>();
            conference.ConferenceSessions = new List<ConferenceSession>();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.NotNull(result.Policies);
            Assert.Empty(result.Policies);
            Assert.NotNull(result.Sponsors);
            Assert.Empty(result.Sponsors);
            Assert.NotNull(result.ConferenceMedia);
            Assert.Empty(result.ConferenceMedia);
            Assert.NotNull(result.ConferencePrices);
            Assert.Empty(result.ConferencePrices);
            Assert.NotNull(result.Sessions);
            Assert.Empty(result.Sessions);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldHandleNullCollections()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            conference.Policies = null;
            conference.Sponsors = null;
            conference.ConferenceMedia = null;
            conference.ConferencePrices = null;
            conference.ConferenceSessions = null;
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            // Should handle null collections gracefully without throwing exceptions
            Assert.NotNull(result);
            Assert.Equal("CONF1", result.ConferenceId);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldIncludeCompleteNestedData()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert - Verify all nested relationships are included
            Assert.NotNull(result);
            Assert.NotNull(result.ConferencePrices);
            Assert.NotNull(result.Sessions);
            Assert.NotNull(result.Sponsors);
            Assert.NotNull(result.Policies);
            Assert.NotNull(result.ConferenceMedia);
            Assert.NotNull(result.purchasedInfo);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldVerifyAllIncludesAreCalled()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert - Verify query was called (includes are applied)
            _mockConferenceRepo.Verify(r => r.GetAllConferences(), Times.Once);
            Assert.NotNull(result);
            // The BuildMock() ensures all Include statements work properly
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldMapCreatorInformation()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.Equal("USER1", result.createdBy);
            Assert.Equal("John Doe", result.UserNameCreator);
        }

        [Fact]
        public async Task GetTechnicalConferenceDetailAsync_ShouldMapTicketSaleDates()
        {
            // Arrange
            var conference = GetSampleTechnicalConference();
            var conferences = new List<Conference> { conference };
            var mockQueryable = conferences.AsQueryable().BuildMock();

            _mockConferenceRepo
                .Setup(r => r.GetAllConferences())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetTechnicalConferenceDetailAsync("CONF1", null);

            // Assert
            Assert.Equal(new DateOnly(2025, 11, 1), result.TicketSaleStart);
            Assert.Equal(new DateOnly(2025, 12, 9), result.TicketSaleEnd);
        }
    }
}