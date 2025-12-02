namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{
    using ConfRadar.Repositories;
    using ConfRadar.Repositories.Models;
    using ConfRadar.Services.Common;
    using ConfRadar.Services.Exceptions;
    using ConfRadar.Services.Services;
    using ConfRadar.Shared.DTO.QrCode;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;
    using static ConfRadar.Services.Common.AppSettingConfig;

    public class QrCoderTest
    {
        private readonly Mock<IObjectStorageFileService> _mockObjectStorageFileService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;

        private readonly QRCoderService _qrCoderService;
        private readonly IOptions<ObjectStorageSettings> _mockObjectStorageSettings;
        private readonly IOptions<QrSettings> _mockQrSettings;
        private readonly ObjectStorageSettings _objectStorageSettings = new()
        {
            EndPoint = "https://mockstorage.com/"
        };

        private readonly QrSettings _qrSettings = new()
        {
            HashKey = "HASH-123",
            CheckSumKey = "CHECKSUM-123"
        };

        public QrCoderTest()
        {
            _mockObjectStorageFileService = new Mock<IObjectStorageFileService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _mockObjectStorageSettings = Options.Create(new ObjectStorageSettings()
            {
                EndPoint = "https://storage.test/"
            });

            _mockQrSettings = Options.Create(new QrSettings()
            {
                HashKey = "test-key"
            });
            _qrCoderService = new QRCoderService(
                _mockObjectStorageFileService.Object,
                Options.Create(_objectStorageSettings),
                Options.Create(_qrSettings),
                _mockTokenService.Object,
                _mockUnitOfWork.Object,
                _mockTimeProviderService.Object
            );
        }

        // ============================================
        // 1. DECRYPT QR INVALID -> THROW BADREQUEST
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenJsonInvalid()
        {
            var request = new VerifyQrDataRequest
            {
                Content = "xx",
                ConferenceSessionId = "ss1"
            };

            _mockTokenService.Setup(t => t.DecryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("INVALID_JSON");

            await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(request));
        }


        // ============================================
        // 2. CHECKSUM SAI -> THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenChecksumInvalid()
        {
            var fakePayload = new QrDataPayload
            {
                userCheckinId = "u1",
                userId = "u2",
                ticketId = "t1",
                conferenceSessionId = "s1",
                createAt = DateTime.Now,
                signature = "WRONG"
            };

            var request = new VerifyQrDataRequest
            {
                Content = "abc",
                ConferenceSessionId = "s1"
            };

            _mockTokenService.Setup(t => t.DecryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(fakePayload));

            _mockTokenService.Setup(t => t.CreateSignature512(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("CORRECT");

            await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(request));
        }


        // ============================================
        // 3. CHECKIN STATUS MISSING → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenCheckInStatusMissing()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((CheckinStatus?)null);

            var req = new VerifyQrDataRequest { Content = "xx", ConferenceSessionId = payload.conferenceSessionId };

            await Assert.ThrowsAsync<NotFoundException>(() => _qrCoderService.ProceedQrCode(req));
        }


        // ============================================
        // 4. SESSION NOT FOUND → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenSessionNotFound()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);

            SetupCheckinStatuses();

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(payload.conferenceSessionId))
                .ReturnsAsync((ConferenceSession?)null);

            var req = new VerifyQrDataRequest { Content = "xx", ConferenceSessionId = payload.conferenceSessionId };

            await Assert.ThrowsAsync<NotFoundException>(() => _qrCoderService.ProceedQrCode(req));
        }

        // ============================================
        // 5. SESSION ID MISMATCH → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenSessionMismatch()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();

            var session = new ConferenceSession
            {
                ConferenceSessionId = payload.conferenceSessionId,
                Title = "Session A",
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now.AddHours(2)
            };

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            var req = new VerifyQrDataRequest
            {
                Content = "xx",
                ConferenceSessionId = "DIFF"
            };

            await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(req));
        }


        // ============================================
        // 6. USER CHECKIN NOT FOUND → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenUserCheckInNotFound()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupValidSession();

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync((UserCheckIn?)null);

            var req = new VerifyQrDataRequest { Content = "xx", ConferenceSessionId = payload.conferenceSessionId };

            await Assert.ThrowsAsync<NotFoundException>(() => _qrCoderService.ProceedQrCode(req));
        }


        // ============================================
        // 7. TICKET REFUNDED → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenTicketRefunded()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupValidSession();

            var userCheck = FakeUserCheckIn(payload);
            userCheck.Ticket!.IsRefunded = true;

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(userCheck);

            var req = new VerifyQrDataRequest { Content = "xx", ConferenceSessionId = payload.conferenceSessionId };

            await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(req));
        }


        // ============================================
        // 8. PAYLOAD MISMATCH → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenPayloadMismatch()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupValidSession();

            var userCheck = FakeUserCheckIn(payload);
            userCheck.UserId = "WRONG";

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(userCheck);

            var req = new VerifyQrDataRequest { Content = "xx", ConferenceSessionId = payload.conferenceSessionId };

            await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(req));
        }


        // ============================================
        // 9. CHECK-IN TOO EARLY → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenCheckInTooEarly()
        {
            // Arrange
            var payload = FakePayload();

            // setup decrypt & checksum
            SetupDecrypt(payload);
            SetupChecksumValid(payload);

            // setup checkin statuses (CheckedIn, Expired)
            SetupCheckinStatuses();

            // Giờ cố định để test
            var fixedNow = new DateTime(2025, 11, 27, 9, 0, 0);

            // Tạo session với startTime > timeNow để trigger "too early"
            var session = new ConferenceSession
            {
                ConferenceSessionId = payload.conferenceSessionId,
                Title = "Test Session",
                StartTime = fixedNow.AddHours(1),  // 10:00
                EndTime = fixedNow.AddHours(3)     // 12:00
            };

            // mock lấy session
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            // tạo UserCheckIn gắn session thật
            var checkin = FakeUserCheckIn(payload, session);

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(checkin);

            // mock thời gian hiện tại
            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(fixedNow);

            var request = new VerifyQrDataRequest
            {
                Content = "xx",
                ConferenceSessionId = payload.conferenceSessionId
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(request));

            Assert.Contains("chưa thể check in", ex.Message);
        }



        // ============================================
        // 10. CHECK-IN TOO LATE → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenCheckInTooLate()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();

            // Giờ cố định để test
            var fixedNow = new DateTime(2025, 11, 27, 12, 0, 0);

            var session = new ConferenceSession
            {
                ConferenceSessionId = payload.conferenceSessionId,
                Title = "Test Late",
                StartTime = fixedNow.AddHours(-3),  // 09:00
                EndTime = fixedNow.AddHours(-1)     // 11:00
            };

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            // Gán session mock cho userCheckIn
            var checkin = FakeUserCheckIn(payload, session);

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(checkin);

            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(fixedNow);

            var req = new VerifyQrDataRequest
            {
                Content = "xx",
                ConferenceSessionId = payload.conferenceSessionId
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(req));
            Assert.Contains("dã hết hạn check in", ex.Message);
        }


        private UserCheckIn FakeUserCheckIn(QrDataPayload p, ConferenceSession? session = null)
        {
            return new UserCheckIn
            {
                UserCheckinId = p.userCheckinId,
                UserId = p.userId,
                TicketId = p.ticketId,
                ConferenceSessionId = p.conferenceSessionId,
                Ticket = new Ticket { TicketId = p.ticketId, IsRefunded = false },
                User = new User { FullName = "John Doe" },
                ConferenceSession = session ?? new ConferenceSession(),
                CheckinStatus = null, // sẽ mock qua SetupCheckinStatuses()
                CheckInTime = null
            };
        }


        // ============================================
        // 11. ALREADY CHECKED-IN → THROW
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenUserAlreadyCheckedIn()
        {
            // Arrange
            var payload = new QrDataPayload
            {
                userCheckinId = "checkin-id",
                userId = "user-id",
                ticketId = "ticket-id",
                conferenceSessionId = "session-id",
                createAt = DateTime.Now
            };

            var encryptedContent = "encrypted"; // just a placeholder
            _mockTokenService
                .Setup(ts => ts.DecryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(payload));

            // Mock CheckInStatusRepository
            var checkedInStatus = new CheckinStatus { CheckinStatusId = "checkedin-id", CheckinStatusName = "Checked In" };
            var expiredStatus = new CheckinStatus { CheckinStatusId = "expired-id", CheckinStatusName = "Expired" };

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync("Checked In"))
                .ReturnsAsync(checkedInStatus);

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync("Expired"))
                .ReturnsAsync(expiredStatus);

            // Mock ConferenceSession
            var session = new ConferenceSession
            {
                ConferenceSessionId = payload.conferenceSessionId,
                StartTime = DateTime.Now.AddMinutes(-30),
                EndTime = DateTime.Now.AddMinutes(30)
            };
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(payload.conferenceSessionId))
                .ReturnsAsync(session);

            // Mock UserCheckIn
            var checkin = new UserCheckIn
            {
                UserCheckinId = payload.userCheckinId,
                UserId = payload.userId,
                TicketId = payload.ticketId,
                ConferenceSessionId = payload.conferenceSessionId,
                CheckinStatus = checkedInStatus, // already checked in
                CheckInTime = DateTime.Now.AddMinutes(-10),
                User = new User { FullName = "Nguyen Van A" },
                Ticket = new Ticket { IsRefunded = false },
                ConferenceSession = session
            };
            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(checkin);

            var request = new VerifyQrDataRequest { Content = "dummy-content", ConferenceSessionId = payload.conferenceSessionId };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(request));
            Assert.Contains("Nguời dùng với tên Nguyen Van A đã checkin vào lúc", ex.Message);
        }


        // ============================================
        // 12. SUCCESS
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldReturnSuccessMessage()
        {
            var payload = FakePayload();

            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();

            var session = SetupValidSession();

            var checkin = FakeUserCheckIn(payload);

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(checkin);

            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.Now);

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository.UpdateUserCheckInAsync(It.IsAny<UserCheckIn>()))
                .ReturnsAsync(1);

            var req = new VerifyQrDataRequest { Content = "xx", ConferenceSessionId = payload.conferenceSessionId };

            var result = await _qrCoderService.ProceedQrCode(req);

            Assert.Contains("đã check in", result);
        }


        // ============================================
        // Helper Methods
        // ============================================

        private QrDataPayload FakePayload()
        {
            return new QrDataPayload
            {
                userCheckinId = "uc1",
                userId = "u1",
                ticketId = "t1",
                conferenceSessionId = "cs1",
                createAt = DateTime.Now,
                signature = "xx"
            };
        }

        private void SetupDecrypt(QrDataPayload payload)
        {
            _mockTokenService.Setup(t => t.DecryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(payload));
        }

        private void SetupChecksumValid(QrDataPayload payload)
        {
            _mockTokenService.Setup(t => t.CreateSignature512(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(payload.signature);
        }

        private void SetupCheckinStatuses()
        {
            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.CheckedIn.GetDescription()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "1", CheckinStatusName = CheckInStatusEnum.CheckedIn.GetDescription() });

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Expired.GetDescription()))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "2", CheckinStatusName = CheckInStatusEnum.Expired.GetDescription() });
        }


        private ConferenceSession SetupValidSession()
        {
            var session = new ConferenceSession
            {
                ConferenceSessionId = "cs1",
                Title = "Session A",
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now.AddHours(2)
            };

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            return session;
        }

        private UserCheckIn FakeUserCheckIn(QrDataPayload p)
        {
            return new UserCheckIn
            {
                UserCheckinId = p.userCheckinId,
                UserId = p.userId,
                TicketId = p.ticketId,
                ConferenceSessionId = p.conferenceSessionId,
                Ticket = new Ticket { TicketId = p.ticketId, IsRefunded = false },
                User = new User { FullName = "John Doe" },
                ConferenceSession = new ConferenceSession()
            };
        }



        // generate qr code
        [Fact]
        public async Task GenerateQrCode_ShouldReturn_UploadedFileUrl()
        {
            // Arrange
            var testData = new { Id = "123", Name = "Test User" };
            string encrypted = "encrypted-content";
            string randomToken = "abc123xyz";

            _mockTokenService
                .Setup(t => t.EncryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(encrypted);

            _mockTokenService
                .Setup(t => t.GenerateSecureRandomToken())
                .Returns(randomToken);

            _mockObjectStorageFileService
                .Setup(o => o.UploadFileAsync(
                    ObjectStorageBucketEnum.qrcodefile.ToString(),
                    randomToken,
                    It.IsAny<Stream>(),
                    "image/png"))
                .ReturnsAsync("/qrcodefile/" + randomToken + ".png");

            // Act
            string resultUrl = await _qrCoderService.GenerateQrCode(testData);

            // Assert
            resultUrl.Should().Be("https://mockstorage.com//qrcodefile/abc123xyz.png");


            _mockTokenService.Verify(t =>
    t.EncryptString(It.IsAny<string>(), It.IsAny<string>()),
    Times.Once);
            _mockTokenService.Verify(t => t.GenerateSecureRandomToken(), Times.Once);
            _mockObjectStorageFileService.Verify(o =>
                o.UploadFileAsync(ObjectStorageBucketEnum.qrcodefile.ToString(),
                randomToken,
                It.IsAny<Stream>(),
                "image/png"),
                Times.Once);
        }
    }

}
