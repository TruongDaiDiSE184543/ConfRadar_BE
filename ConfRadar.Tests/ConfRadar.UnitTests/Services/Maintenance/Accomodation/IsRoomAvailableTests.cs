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
    public class IsRoomAvailableTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IConferenceSessionRepository> _mockSessionRepo;
        private readonly RoomService _roomService;

        public IsRoomAvailableTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockSessionRepo = new Mock<IConferenceSessionRepository>();

            _mockUnitOfWork.Setup(u => u.RoomRepository).Returns(_mockRoomRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository).Returns(_mockSessionRepo.Object);

            _roomService = new RoomService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task IsRoomAvailable_Should_ReturnTrue_When_NoSessionsExist()
        {
            // ARRANGE
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOverlappingTimeAsync(roomId, date, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ConferenceSession>());

            // ACT
            var result = await _roomService.IsRoomAvailable(roomId, date, new TimeOnly(9, 0), new TimeOnly(10, 0));

            // ASSERT
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsRoomAvailable_Should_ReturnFalse_When_NewSlotIsInsideExistingSession()
        {
            // ARRANGE: Session có sẵn [9:00 - 12:00]. Kiểm tra [10:00 - 11:00]
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOverlappingTimeAsync(roomId, date, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ConferenceSession> { new ConferenceSession() }); // Giả lập có 1 session chồng chéo

            // ACT
            var result = await _roomService.IsRoomAvailable(roomId, date, new TimeOnly(10, 0), new TimeOnly(11, 0));

            // ASSERT
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRoomAvailable_Should_ReturnFalse_When_NewSlotOverlapsStartOfExisting()
        {
            // ARRANGE: Session có sẵn [10:00 - 12:00]. Kiểm tra [9:00 - 10:30]
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOverlappingTimeAsync(roomId, date, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ConferenceSession> { new ConferenceSession() });

            // ACT
            var result = await _roomService.IsRoomAvailable(roomId, date, new TimeOnly(9, 0), new TimeOnly(10, 30));

            // ASSERT
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRoomAvailable_Should_ReturnFalse_When_NewSlotOverlapsEndOfExisting()
        {
            // ARRANGE: Session có sẵn [9:00 - 11:00]. Kiểm tra [10:30 - 12:00]
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOverlappingTimeAsync(roomId, date, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ConferenceSession> { new ConferenceSession() });

            // ACT
            var result = await _roomService.IsRoomAvailable(roomId, date, new TimeOnly(10, 30), new TimeOnly(12, 0));

            // ASSERT
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRoomAvailable_Should_ReturnTrue_ForAdjacentSessions()
        {
            // ARRANGE: Session có sẵn [9:00 - 10:00]. Kiểm tra [10:00 - 11:00]
            var roomId = "room1";
            var date = new DateOnly(2023, 1, 1);
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(new Room());
            _mockSessionRepo.Setup(r => r.GetSessionsByRoomIdOverlappingTimeAsync(roomId, date, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ConferenceSession>()); // Không có chồng chéo

            // ACT
            var result = await _roomService.IsRoomAvailable(roomId, date, new TimeOnly(10, 0), new TimeOnly(11, 0));

            // ASSERT
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsRoomAvailable_Should_Throw_When_RoomNotFound()
        {
            _mockRoomRepo.Setup(r => r.GetRoomByIdAsync("not-found")).ReturnsAsync((Room)null);
            await Assert.ThrowsAsync<NotFoundException>(() => _roomService.IsRoomAvailable("not-found", new DateOnly(), new TimeOnly(9, 0), new TimeOnly(10, 0)));
        }
    }
}
