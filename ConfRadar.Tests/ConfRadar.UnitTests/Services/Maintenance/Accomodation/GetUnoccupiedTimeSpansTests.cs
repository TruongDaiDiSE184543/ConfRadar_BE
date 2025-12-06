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
    public class GetUnoccupiedTimeSpansTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IConferenceSessionRepository> _mockSessionRepo;
        private readonly RoomService _roomService;

        public GetUnoccupiedTimeSpansTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockSessionRepo = new Mock<IConferenceSessionRepository>();

            _mockUnitOfWork.Setup(u => u.RoomRepository).Returns(_mockRoomRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository).Returns(_mockSessionRepo.Object);

            _roomService = new RoomService(_mockUnitOfWork.Object);
        }

        private ConferenceSession CreateSession(DateOnly date, TimeOnly start, TimeOnly end) =>
            new ConferenceSession { StartTime = date.ToDateTime(start), EndTime = date.ToDateTime(end) };

        [Fact]
        public async Task GetUnoccupiedTimeSpans_Should_ReturnFullDay_When_NoSessions()
        {
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOnDateAsync(roomId, date)).ReturnsAsync(new List<ConferenceSession>());

            var result = await _roomService.GetUnoccupiedTimeSpansInRoomOnDateAsync(roomId, date);

            result.Should().HaveCount(1);
            result[0].StartTime.Should().Be(new TimeOnly(6, 0));
            result[0].EndTime.Should().Be(new TimeOnly(23, 59, 59));
        }

        [Fact]
        public async Task GetUnoccupiedTimeSpans_Should_ReturnTwoGaps_ForOneSessionInMiddle()
        {
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            var sessions = new List<ConferenceSession> { CreateSession(date, new TimeOnly(10, 0), new TimeOnly(12, 0)) };

            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOnDateAsync(roomId, date)).ReturnsAsync(sessions);

            var result = await _roomService.GetUnoccupiedTimeSpansInRoomOnDateAsync(roomId, date);

            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(new { StartTime = new TimeOnly(6, 0), EndTime = new TimeOnly(10, 0) });
            result[1].Should().BeEquivalentTo(new { StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(23, 59, 59) });
        }

        [Fact]
        public async Task GetUnoccupiedTimeSpans_Should_ReturnMultipleGaps_ForMultipleSessions()
        {
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            var sessions = new List<ConferenceSession>
            {
                CreateSession(date, new TimeOnly(9, 0), new TimeOnly(10, 0)),
                CreateSession(date, new TimeOnly(14, 0), new TimeOnly(15, 30))
            };

            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOnDateAsync(roomId, date)).ReturnsAsync(sessions);

            var result = await _roomService.GetUnoccupiedTimeSpansInRoomOnDateAsync(roomId, date);

            result.Should().HaveCount(3);
            result[0].EndTime.Should().Be(new TimeOnly(9, 0)); // Before
            result[1].Should().BeEquivalentTo(new { StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(14, 0) }); // Between
            result[2].StartTime.Should().Be(new TimeOnly(15, 30)); // After
        }

        [Fact]
        public async Task GetUnoccupiedTimeSpans_Should_NotCreateGap_ForBackToBackSessions()
        {
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            var sessions = new List<ConferenceSession>
            {
                CreateSession(date, new TimeOnly(9, 0), new TimeOnly(10, 0)),
                CreateSession(date, new TimeOnly(10, 0), new TimeOnly(11, 0)) // Tiếp nối nhau
            };

            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOnDateAsync(roomId, date)).ReturnsAsync(sessions);

            var result = await _roomService.GetUnoccupiedTimeSpansInRoomOnDateAsync(roomId, date);

            result.Should().HaveCount(2); // Chỉ có gap trước 9:00 và sau 11:00
            result[0].EndTime.Should().Be(new TimeOnly(9, 0));
            result[1].StartTime.Should().Be(new TimeOnly(11, 0));
        }

        [Fact]
        public async Task GetUnoccupiedTimeSpans_Should_Throw_When_RoomNotFound()
        {
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync("not-found")).ReturnsAsync((Room)null);
            await Assert.ThrowsAsync<NotFoundException>(() => _roomService.GetUnoccupiedTimeSpansInRoomOnDateAsync("not-found", new DateOnly()));
        }
    }
}
