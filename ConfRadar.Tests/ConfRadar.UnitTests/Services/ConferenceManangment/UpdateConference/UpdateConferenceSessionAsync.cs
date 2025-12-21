using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.UpdateConference
{
    public class ConferenceStepServiceUpdateConferenceSessionTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceUpdateConferenceSessionTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConferenceService = new Mock<IConferenceService>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();

            var objectStorageSettings = new AppSettingConfig.ObjectStorageSettings();

            _conferenceStepService = new ConferenceStepService(
                _mockUnitOfWork.Object,
                _mockObjectStorageFileService.Object,
                _mockTokenService.Object,
                Options.Create(objectStorageSettings),
                _mockConferenceService.Object,
                _mockTimeProviderService.Object
            );
        }

        private void SetupMocks(
            string sessionId, string confId, string userId,
            ConferenceSession session, Conference conference,
            List<ConferenceSession> otherSessions = null)
        {
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId))
                .ReturnsAsync(session);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId))
                .ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId))
                .ReturnsAsync(session);
            
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(otherSessions ?? new List<ConferenceSession>());

            // Status mocks
            var pending = new ConferenceStatus { ConferenceStatusId = "pending", ConferenceStatusName = "Pending" };
            var preparing = new ConferenceStatus { ConferenceStatusId = "preparing", ConferenceStatusName = "Preparing" };
            var draft = new ConferenceStatus { ConferenceStatusId = "draft", ConferenceStatusName = "Draft" };
            var onHold = new ConferenceStatus { ConferenceStatusId = "onhold", ConferenceStatusName = "OnHold" };
            var deleted = new ConferenceStatus { ConferenceStatusId = "deleted", ConferenceStatusName = "Deleted" };

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending")).ReturnsAsync(pending);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing")).ReturnsAsync(preparing);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft")).ReturnsAsync(draft);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold")).ReturnsAsync(onHold);

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("pending")).ReturnsAsync(pending);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("preparing")).ReturnsAsync(preparing);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("draft")).ReturnsAsync(draft);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("onhold")).ReturnsAsync(onHold);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync("deleted")).ReturnsAsync(deleted);

            if (!string.IsNullOrEmpty(session?.RoomId))
            {
                _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync(session.RoomId))
                    .ReturnsAsync(new Room { RoomId = session.RoomId });
            }
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_SessionNotFound_ThrowsNotFoundException()
        {
            SetupMocks("sess1", "conf1", "user1", null, null);
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", new UpdateConferenceSessionRequest(), "user1"));
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_ResearchConference_ThrowsBadRequestException()
        {
            var conf = new Conference { ConferenceId = "conf1", IsResearchConference = true };
            var session = new ConferenceSession { ConferenceSessionId = "sess1", ConferenceId = "conf1" };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", new UpdateConferenceSessionRequest(), "user1"));
            Assert.Contains("không dành cho phiên của hội nghị nghiên cứu", ex.Message);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_OnHold_TitleChanged_ThrowsBadRequestException()
        {
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                IsResearchConference = false, 
                ConferenceStatusId = "onhold", 
                CreatedBy = "user1" 
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1", 
                Title = "Old Title" 
            };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var request = new UpdateConferenceSessionRequest { Title = "New Title" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", request, "user1"));
            Assert.Contains("Không thể thay đổi 'Tiêu đề phiên' khi hội nghị đang OnHold", ex.Message);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_NotCreator_ThrowsException()
        {
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "creator",
                ConferenceStatusId = "preparing" // Added valid status
            };
            var session = new ConferenceSession { ConferenceSessionId = "sess1", ConferenceId = "conf1" };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var ex = await Assert.ThrowsAsync<Exception>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", new UpdateConferenceSessionRequest(), "user1")); // user1 != creator
            Assert.Contains("Bạn không có quyền cập nhật phiên này", ex.Message);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_ExternalHosted_Preparing_ThrowsBadRequestException()
        {
            // EnsureConferenceIsEditable(conference, true) checks:
            // if (conference.IsInternalHosted != true && conference.ConferenceStatusId == preparing.ConferenceStatusId) -> Throw
            
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsInternalHosted = false, // External
                ConferenceStatusId = "preparing"
            };
            var session = new ConferenceSession { ConferenceSessionId = "sess1", ConferenceId = "conf1" };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", new UpdateConferenceSessionRequest(), "user1"));
            Assert.Contains("không thể cập nhật các thông tin cốt lõi", ex.Message);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_InternalHosted_RoomIdRequired_ThrowsBadRequestException()
        {
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsInternalHosted = true,
                ConferenceStatusId = "preparing",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1",
                RoomId = null,
                SessionDate = DateOnly.FromDateTime(DateTime.Now),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var request = new UpdateConferenceSessionRequest { RoomId = "" }; // Trying to set null/empty

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", request, "user1"));
            Assert.Contains("Hội nghị Technical nội bộ bắt buộc phiên", ex.Message);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_RoomIdNotFound_ThrowsNotFoundException()
        {
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsInternalHosted = true,
                ConferenceStatusId = "preparing",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1",
                RoomId = "room1",
                SessionDate = DateOnly.FromDateTime(DateTime.Now),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
            SetupMocks("sess1", "conf1", "user1", session, conf);
            
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync("newRoom")).ReturnsAsync((Room)null);

            var request = new UpdateConferenceSessionRequest { RoomId = "newRoom" };

            await Assert.ThrowsAsync<NotFoundException>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", request, "user1"));
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_TimeOverlap_ThrowsBadRequestException()
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsInternalHosted = true,
                ConferenceStatusId = "preparing",
                StartDate = date,
                EndDate = date.AddDays(1)
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1",
                RoomId = "room1",
                SessionDate = date,
                StartTime = date.ToDateTime(new TimeOnly(10, 0)),
                EndTime = date.ToDateTime(new TimeOnly(11, 0))
            };

            var otherSession = new ConferenceSession
            {
                ConferenceSessionId = "sess2",
                StartTime = date.ToDateTime(new TimeOnly(9, 30)),
                EndTime = date.ToDateTime(new TimeOnly(10, 30)) // Overlaps with requested 10:00 start
            };

            SetupMocks("sess1", "conf1", "user1", session, conf, new List<ConferenceSession> { otherSession });
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync("room1")).ReturnsAsync(new Room());

            var request = new UpdateConferenceSessionRequest 
            { 
                RoomId = "room1", // Explicitly setting room trigger validation
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0)
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _conferenceStepService.UpdateConferenceSessionAsync("sess1", request, "user1"));
            Assert.Contains("bị trùng lặp", ex.Message);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_ExternalHosted_ForcesRoomIdNull_Success()
        {
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsInternalHosted = false,
                ConferenceStatusId = "draft", // Not preparing to pass EnsureConferenceIsEditable
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1",
                RoomId = "someRoom", // Should be cleared
                SessionDate = DateOnly.FromDateTime(DateTime.Now),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
            SetupMocks("sess1", "conf1", "user1", session, conf);

            var request = new UpdateConferenceSessionRequest { RoomId = "newRoom" }; // Requesting a room

            await _conferenceStepService.UpdateConferenceSessionAsync("sess1", request, "user1");

            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.UpdateConferenceSessionAsync(
                It.Is<ConferenceSession>(s => s.RoomId == null)), Times.Once);
        }

        [Fact]
        public async Task UpdateConferenceSessionAsync_InternalHosted_UpdatesSuccessfully()
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var conf = new Conference 
            { 
                ConferenceId = "conf1", 
                CreatedBy = "user1",
                IsInternalHosted = true,
                ConferenceStatusId = "preparing",
                StartDate = date,
                EndDate = date.AddDays(1)
            };
            var session = new ConferenceSession 
            { 
                ConferenceSessionId = "sess1", 
                ConferenceId = "conf1",
                RoomId = "room1",
                SessionDate = date,
                StartTime = date.ToDateTime(new TimeOnly(10, 0)),
                EndTime = date.ToDateTime(new TimeOnly(11, 0))
            };
            SetupMocks("sess1", "conf1", "user1", session, conf);
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync("room2")).ReturnsAsync(new Room { RoomId = "room2" });

            var request = new UpdateConferenceSessionRequest 
            { 
                RoomId = "room2",
                Title = "New Title",
                StartTime = new TimeOnly(12, 0),
                EndTime = new TimeOnly(13, 0)
            };

            var result = await _conferenceStepService.UpdateConferenceSessionAsync("sess1", request, "user1");

            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.UpdateConferenceSessionAsync(
                It.Is<ConferenceSession>(s => 
                    s.RoomId == "room2" && 
                    s.Title == "New Title" &&
                    s.StartTime.Value.Hour == 12
                )), Times.Once);
        }
        
        [Fact]
        public async Task UpdateConferenceSessionAsync_ShortDuration_ThrowsBadRequestException()
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var conf = new Conference { ConferenceId = "conf1", CreatedBy = "user1", IsInternalHosted = true, ConferenceStatusId = "draft", StartDate = date, EndDate = date };
            var session = new ConferenceSession { ConferenceSessionId = "sess1", ConferenceId = "conf1", RoomId = "room1", SessionDate = date, StartTime = date.ToDateTime(new TimeOnly(10,0)), EndTime = date.ToDateTime(new TimeOnly(11,0)) };
            SetupMocks("sess1", "conf1", "user1", session, conf);
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync("room1")).ReturnsAsync(new Room());

            var request = new UpdateConferenceSessionRequest 
            { 
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(10, 15) // 15 mins
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _conferenceStepService.UpdateConferenceSessionAsync("sess1", request, "user1"));
            Assert.Contains("ít nhất 30 phút", ex.Message);
        }
    }
}
