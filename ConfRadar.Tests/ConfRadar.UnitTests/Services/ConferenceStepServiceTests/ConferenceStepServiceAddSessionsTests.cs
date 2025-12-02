using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace ConfRadar.UnitTests.Services.ConferenceStepServiceTests
{
    // Placeholder for the user's actual helper class
    public static class ExtensionHelper
    {
        public static DateOnly GetVietnamDate()
        {
            return DateOnly.FromDateTime(DateTime.Now);
        }
    }

    public class ConferenceStepServiceAddSessionsTests
    {
        #region Fields and Constructor

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConferenceService> _mockConferenceService;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly ConferenceStepService _conferenceStepService;

        public ConferenceStepServiceAddSessionsTests()
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

        #endregion

        #region Helper Methods

        private Conference CreateTechnicalConference(string userId = "user-123", bool isInternal = true)
        {
            return new Conference
            {
                ConferenceId = "conf-tech-123",
                CreatedBy = userId,
                IsResearchConference = false,
                IsInternalHosted = isInternal,
                StartDate = ExtensionHelper.GetVietnamDate().AddDays(10),
                EndDate = ExtensionHelper.GetVietnamDate().AddDays(12),
                ConferenceStatusId = "status-preparing",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusName = "Preparing" }
            };
        }

        private AddConferenceSessionsRequest CreateValidAddSessionsRequest(string roomId = "room-1")
        {
            return new AddConferenceSessionsRequest
            {
                Sessions = new List<CreateConferenceSessionRequest>
                {
                    new CreateConferenceSessionRequest
                    {
                        Title = "Session 1",
                        Description = "Description 1",
                        Date = ExtensionHelper.GetVietnamDate().AddDays(10),
                        StartTime = new TimeOnly(9, 0, 0),
                        EndTime = new TimeOnly(10, 30, 0),
                        RoomId = roomId,
                        Speaker = new List<CreateSpeakerRequest> { new CreateSpeakerRequest { Name = "Speaker 1" } }
                    },
                    new CreateConferenceSessionRequest
                    {
                        Title = "Session 2",
                        Description = "Description 2",
                        Date = ExtensionHelper.GetVietnamDate().AddDays(11),
                        StartTime = new TimeOnly(10, 0, 0),
                        EndTime = new TimeOnly(11, 30, 0),
                        RoomId = roomId,
                        Speaker = new List<CreateSpeakerRequest> { new CreateSpeakerRequest { Name = "Speaker 2" } }
                    },
                    new CreateConferenceSessionRequest
                    {
                        Title = "Session 3",
                        Description = "Description 3",
                        Date = ExtensionHelper.GetVietnamDate().AddDays(12),
                        StartTime = new TimeOnly(11, 0, 0),
                        EndTime = new TimeOnly(12, 30, 0),
                        RoomId = roomId,
                        Speaker = new List<CreateSpeakerRequest> { new CreateSpeakerRequest { Name = "Speaker 3" } }
                    }
                }
            };
        }

        private void SetupValidMocks(Conference conference, bool isEditable = true)
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(conference.ConferenceId))
                           .ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conference.ConferenceId))
                           .ReturnsAsync(new List<ConferenceSession>());
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync(It.IsAny<string>())).ReturnsAsync(new Room());

            if (isEditable)
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft"))
                   .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" });
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusName = "Preparing" });
            }
            else
            {
                _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByIdAsync(It.IsAny<string>()))
                   .ReturnsAsync(new ConferenceStatus { ConferenceStatusName = "Published" });
            }

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
                           .ReturnsAsync(new List<ConferenceSession> { new ConferenceSession() });

            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.Now);
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionWithDetailsAsync(It.IsAny<string>()))
                          .ReturnsAsync(new ConferenceSession { Speakers = new List<Speaker>(), ConferenceSessionMedia = new List<ConferenceSessionMedium>() });
            _mockUnitOfWork.Setup(u => u.SpeakerRepository.CreateSpeakerAsync(It.IsAny<Speaker>())).Returns(Task.FromResult(1));
        }

        #endregion

        #region Exception Test Methods

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_ConferenceNotFound()
        {
            // Arrange
            var request = CreateValidAddSessionsRequest();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync("nonexistent-conf")).ReturnsAsync((Conference)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync("nonexistent-conf", request, "user-123"));

            exception.Message.Should().Contain("Không tìm thấy hội nghị với ID nonexistent-conf");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_UserIsNotCreator()
        {
            // Arrange
            var conference = CreateTechnicalConference(userId: "creator-id");
            var request = CreateValidAddSessionsRequest();
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "other-user-id"));

            exception.Message.Should().Contain("Bạn không có quyền thêm session cho hội nghị này.");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_RequestContainsNoSessions()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = new AddConferenceSessionsRequest { Sessions = new List<CreateConferenceSessionRequest>() };
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("Yêu cầu phải chứa ít nhất một phiên (session).");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_ConferenceIsNotEditable()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            conference.ConferenceStatusId = "status-published";
            var request = CreateValidAddSessionsRequest();
            SetupValidMocks(conference, isEditable: false);
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Preparing"))
                   .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-preparing", ConferenceStatusName = "Preparing" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync("Pending"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-pending", ConferenceStatusName = "Pending" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("Draft"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-draft", ConferenceStatusName = "Draft" });
            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByName("OnHold"))
                    .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "status-onhold", ConferenceStatusName = "OnHold" });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("Thao tác không được phép. Hội nghị đang ở trạng thái 'Published' và không thể chỉnh sửa.");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_SessionTitleIsEmpty()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            request.Sessions.First().Title = " ";
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("Tiêu đề của session không được để trống.");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_SessionTimeIsMissing()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            request.Sessions.First().StartTime = null;
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain($"Session '{request.Sessions.First().Title}' cần có đủ StartTime, EndTime, và Date.");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowException_When_InternalConferenceHasNoRoomId()
        {
            // Arrange
            var conference = CreateTechnicalConference(isInternal: true);
            var request = CreateValidAddSessionsRequest();
            request.Sessions.ForEach(s => s.RoomId = null);
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain($"bắt buộc phải có RoomId");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowNotFoundException_When_RoomDoesNotExist()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest(roomId: "non-existent-room");
            SetupValidMocks(conference);
            _mockUnitOfWork.Setup(u => u.RoomRepository.GetRoomByIdAsync("non-existent-room")).ReturnsAsync((Room)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("Phòng với ID non-existent-room không tồn tại.");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_SessionDateIsOutsideConferenceDates()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            DateOnly next12 = ExtensionHelper.GetVietnamDate().AddDays(12);
            DateOnly next10 = ExtensionHelper.GetVietnamDate().AddDays(10);
            request.Sessions.First(s => s.Date > next10 && s.Date < next12).Date = conference.EndDate.Value.AddDays(1);
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("nằm ngoài khoảng thời gian diễn ra hội nghị");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_SessionsConflict()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            SetupValidMocks(conference);
            var existingSession = new ConferenceSession
            {
                ConferenceSessionId = "existing-session",
                StartTime = request.Sessions.First().Date.Value.ToDateTime(request.Sessions.First().StartTime.Value),
                EndTime = request.Sessions.First().Date.Value.ToDateTime(request.Sessions.First().EndTime.Value),
                RoomId = request.Sessions.First().RoomId
            };
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<ConferenceSession> { existingSession });


            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("conflicts with an existing session");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_SpeakerImageIsInvalid()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            var invalidFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("this is a dummy file")), 0, 0, "Data", "dummy.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
            request.Sessions.First().Speaker.First().Image = invalidFile;
            SetupValidMocks(conference);
            _mockObjectStorageFileService.Setup(o => o.IsValidImageFile(invalidFile)).Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("không được hỗ trợ");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowException_When_SessionMediaIsInvalid()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            var invalidFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("this is a dummy file")), 0, 0, "Data", "dummy.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
            request.Sessions.First().SessionMedias = new List<CreateConferenceSessionMediaRequest> { new CreateConferenceSessionMediaRequest { MediaFile = invalidFile } };
            SetupValidMocks(conference);
            _mockObjectStorageFileService.Setup(o => o.IsValidVideoFile(invalidFile)).Returns(false);
            _mockObjectStorageFileService.Setup(o => o.IsValidImageFile(invalidFile)).Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("Khong ho tro dinh dang cho sessionMedia nay");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowException_When_SessionsInRequestOverlap()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            var today = ExtensionHelper.GetVietnamDate();
            request.Sessions = new List<CreateConferenceSessionRequest>
            {
                new CreateConferenceSessionRequest
                {
                    Title = "Session A",
                    Date = today.AddDays(10),
                    StartTime = new TimeOnly(9, 0, 0),
                    EndTime = new TimeOnly(10, 30, 0),
                    RoomId = "room-1"
                },
                new CreateConferenceSessionRequest
                {
                    Title = "Session B (Overlap)",
                    Date = today.AddDays(10),
                    StartTime = new TimeOnly(10, 0, 0), // Starts before Session A ends
                    EndTime = new TimeOnly(11, 0, 0),
                    RoomId = "room-1"
                },
                   new CreateConferenceSessionRequest
                {
                    Title = "Session B (Overlap)",
                    Date = today.AddDays(12),
                    StartTime = new TimeOnly(10, 0, 0), // Starts before Session A ends
                    EndTime = new TimeOnly(11, 0, 0),
                    RoomId = "room-1"
                }
            };
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("bị chồng chéo thời gian");
        }

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_ThrowBadRequestException_When_SpeakerNameIsEmpty()
        {
            // Arrange
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            request.Sessions.First().Speaker.First().Name = " "; // Invalid name
            SetupValidMocks(conference);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, "user-123"));

            exception.Message.Should().Contain("Tên của diễn giả trong phiên");
            exception.Message.Should().Contain("không được để trống");
        }

        #endregion

        #region Success Test

        [Fact]
        public async Task AddConferenceSessionsAsync_Should_Succeed_ForValidRequest()
        {
            // ARRANGE
            var conference = CreateTechnicalConference();
            var request = CreateValidAddSessionsRequest();
            var userId = "user-123";
            SetupValidMocks(conference);

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()))
                .Returns(Task.FromResult(1));

            // ACT
            var result = await _conferenceStepService.AddConferenceSessionsAsync(conference.ConferenceId, request, userId);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.ConferenceSessionRepository.CreateConferenceSessionAsync(It.IsAny<ConferenceSession>()), Times.Exactly(3));
            _mockUnitOfWork.Verify(u => u.SpeakerRepository.CreateSpeakerAsync(It.IsAny<Speaker>()), Times.Exactly(3));
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        #endregion
    }
}
