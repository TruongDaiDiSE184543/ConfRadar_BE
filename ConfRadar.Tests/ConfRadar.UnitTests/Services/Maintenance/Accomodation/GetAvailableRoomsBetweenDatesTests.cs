using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Repositories;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.UnitTests.Services.Maintenance.Accomodation
{
    public class GetAvailableRoomsBetweenDatesTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IConferenceSessionRepository> _mockSessionRepo;
        private readonly RoomService _roomService;

        public GetAvailableRoomsBetweenDatesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockSessionRepo = new Mock<IConferenceSessionRepository>();

            _mockUnitOfWork.Setup(u => u.RoomRepository).Returns(_mockRoomRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository).Returns(_mockSessionRepo.Object);

            _roomService = new RoomService(_mockUnitOfWork.Object);
        }

        private ConferenceSession CreateSession(string roomId, DateOnly date, TimeOnly start, TimeOnly end) =>
            new ConferenceSession { RoomId = roomId, SessionDate = date, StartTime = date.ToDateTime(start), EndTime = date.ToDateTime(end) };

        [Fact]
        public async Task GetAvailableRooms_Should_CorrectlyIdentify_FullyAvailableAndPartiallyAvailableRooms()
        {
            // ARRANGE
            var startDate = new DateOnly(2023, 1, 1);
            var endDate = new DateOnly(2023, 1, 1);
            var rooms = new List<Room>
            {
                new Room { RoomId = "roomA", Destination = new Destination { City = new City() } },
                new Room { RoomId = "roomB", Destination = new Destination { City = new City() } }
            };
            var sessions = new List<ConferenceSession>
            {
                CreateSession("roomA", startDate, new TimeOnly(10, 0), new TimeOnly(12, 0)) // Room A bận
            };

            _mockRoomRepo.Setup(r => r.GetAllRoomsAsync(null, null)).ReturnsAsync(rooms);
            _mockSessionRepo.Setup(r => r.GetSessionsInDateRangeAsync(It.IsAny<List<string>>(), startDate, endDate)).ReturnsAsync(sessions);

            // ACT
            var result = await _roomService.GetAvailableRoomsBetweenDates(startDate, endDate);

            // ASSERT
            result.Should().HaveCount(2);

            var roomA = result.FirstOrDefault(r => r.RoomId == "roomA");
            roomA.IsAvailableWholeday.Should().BeFalse();
            roomA.AvailableTimeSpans.Should().HaveCount(2);

            var roomB = result.FirstOrDefault(r => r.RoomId == "roomB");
            roomB.IsAvailableWholeday.Should().BeTrue();
            roomB.AvailableTimeSpans.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAvailableRooms_Should_HandleMultipleDaysCorrectly()
        {
            var startDate = new DateOnly(2023, 1, 1);
            var endDate = new DateOnly(2023, 1, 2);
            var rooms = new List<Room> { new Room { RoomId = "roomA", Destination = new Destination { City = new City() } } };
            var sessions = new List<ConferenceSession>
            {
                CreateSession("roomA", startDate, new TimeOnly(10, 0), new TimeOnly(12, 0)) // Ngày 1 bận
                // Ngày 2 rảnh
            };

            _mockRoomRepo.Setup(r => r.GetAllRoomsAsync(null, null)).ReturnsAsync(rooms);
            _mockSessionRepo.Setup(r => r.GetSessionsInDateRangeAsync(It.IsAny<List<string>>(), startDate, endDate)).ReturnsAsync(sessions);

            var result = await _roomService.GetAvailableRoomsBetweenDates(startDate, endDate);

            result.Should().HaveCount(2); // 1 entry cho ngày 1, 1 entry cho ngày 2
            result.First(r => r.Date == startDate).IsAvailableWholeday.Should().BeFalse();
            result.First(r => r.Date == endDate).IsAvailableWholeday.Should().BeTrue();
        }

        [Fact]
        public async Task GetAvailableRooms_Should_ReturnEmptyList_When_NoRoomsMatchFilter()
        {
            var startDate = new DateOnly(2023, 1, 1);
            var endDate = new DateOnly(2023, 1, 1);

            _mockRoomRepo.Setup(r => r.GetAllRoomsAsync("city-not-found", null)).ReturnsAsync(new List<Room>());

            var result = await _roomService.GetAvailableRoomsBetweenDates(startDate, endDate, cityId: "city-not-found");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableRooms_Should_Throw_When_DateRangeExceeds7Days()
        {
            var startDate = new DateOnly(2023, 1, 1);
            var endDate = startDate.AddDays(8);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _roomService.GetAvailableRoomsBetweenDates(startDate, endDate));
            ex.Message.Should().Contain("Khoảng cách không thể vượt quá 7 ngày.");
        }
    }
}
