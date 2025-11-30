using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.TicketTest
{


    public class GetTicketListByConferenceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITimeProviderService> _timeProviderServiceMock;
        private readonly TicketService _ticketService;

        public GetTicketListByConferenceTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _timeProviderServiceMock = new Mock<ITimeProviderService>();

            _ticketService = new TicketService(
                _unitOfWorkMock.Object,
                _timeProviderServiceMock.Object
            );
        }

        [Fact]
        public async Task GetTicketListByConferenceId_ShouldReturnMappedResponse()
        {
            // Arrange
            string conferenceId = "CONF123";

            var tickets = new List<Ticket>()
        {
            new Ticket
            {
                TicketId = "T001",
                UserId = "U001",
                IsRefunded = false,
                RegisteredDate = new DateOnly(2025, 01, 10),
                User = new User
                {
                    UserId = "U001",
                    FullName = "John Doe",
                    Email = "john@example.com",
                    AvatarUrl = "avatar.jpg"
                },
                PricePhase = new PricePhase
                {
                    ConferencePrice = new ConferencePrice
                    {
                        ConferenceId = conferenceId,
                        Conference = new Conference
                        {
                            ConferenceId = conferenceId,
                            ConferenceName = "Tech Conference 2025"
                        }
                    }
                }
            }
        };

            _unitOfWorkMock.Setup(u => u.TicketRepository.GetTicketListByConferenceId(conferenceId))
                .ReturnsAsync(tickets);

            // Act
            var result = await _ticketService.GetTicketListByConferenceId(conferenceId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var ticket = result[0];

            Assert.Equal("T001", ticket.TicketId);
            Assert.Equal("U001", ticket.UserId);
            Assert.False(ticket.IsRefunded);
            Assert.Equal("John Doe", ticket.UserName);
            Assert.Equal("john@example.com", ticket.Email);
            Assert.Equal("avatar.jpg", ticket.AvatarUrl);
            Assert.Equal(new DateOnly(2025, 01, 10), ticket.RegisteredDate);
            Assert.Equal(conferenceId, ticket.ConferenceId);
            Assert.Equal("Tech Conference 2025", ticket.ConferenceName);

            _unitOfWorkMock.Verify(u => u.TicketRepository.GetTicketListByConferenceId(conferenceId), Times.Once);
        }

        [Fact]
        public async Task GetTicketListByConferenceId_ShouldReturnEmptyList_WhenNoTickets()
        {
            // Arrange
            string conferenceId = "CONF_EMPTY";

            _unitOfWorkMock.Setup(u => u.TicketRepository.GetTicketListByConferenceId(conferenceId))
                .ReturnsAsync(new List<Ticket>());

            // Act
            var result = await _ticketService.GetTicketListByConferenceId(conferenceId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}