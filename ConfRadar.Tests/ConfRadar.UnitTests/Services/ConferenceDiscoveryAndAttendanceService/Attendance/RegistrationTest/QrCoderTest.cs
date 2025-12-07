namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{
    using ConfRadar.Repositories;
    using ConfRadar.Repositories.Models;
    using ConfRadar.Repositories.Repositories;
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
        // 1. INVALID JSON
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenJsonInvalid()
        {
            var request = new VerifyQrDataRequest { Content = "xx", ConferenceSessionId = "ss1" };

            _mockTokenService.Setup(t => t.DecryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("INVALID_JSON");

            await Assert.ThrowsAsync<BadRequestException>(() => _qrCoderService.ProceedQrCode(request));
        }

        // ============================================
        // 2. CHECKSUM INVALID
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenChecksumInvalid()
        {
            var payload = FakePayload();

            _mockTokenService.Setup(t => t.DecryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(payload));

            _mockTokenService.Setup(t => t.CreateSignature512(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("wrong");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));
        }

        // ============================================
        // 3. CHECKIN STATUS MISSING
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenCheckInStatusMissing()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupReadyStatus();

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((CheckinStatus?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));
        }

        // ============================================
        // 4. SESSION NOT FOUND
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenSessionNotFound()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupReadyStatus();

            _mockUnitOfWork.Setup(u =>
                u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(payload.conferenceSessionId))
                .ReturnsAsync((ConferenceSession?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));
        }

        // ============================================
        // 5. SESSION MISMATCH
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenSessionMismatch()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupReadyStatus();

            _mockUnitOfWork.Setup(u =>
                u.ConferenceSessionRepository.GetConferenceSessionByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceSession
                {
                    ConferenceSessionId = payload.conferenceSessionId,
                    Conference = new Conference
                    {
                        ConferenceStatus = new ConferenceStatus
                        {
                            ConferenceStatusId = "ready-id",
                            ConferenceStatusName = ConferenceStatusEnum.Ready.GetDescription()
                        }
                    },
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now.AddHours(2)
                });

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = "DIFF"
                }));
        }

        // ============================================
        // 6. USER CHECKIN NOT FOUND
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenUserCheckInNotFound()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupReadyStatus();
            SetupValidSession();

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync((UserCheckIn?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));
        }

        // ============================================
        // 7. TICKET REFUNDED
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenTicketRefunded()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupReadyStatus();
            SetupValidSession();

            var uc = FakeUserCheckIn(payload);
            uc.Ticket!.IsRefunded = true;

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(uc);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));
        }

        // ============================================
        // 8. PAYLOAD MISMATCH
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenPayloadMismatch()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupReadyStatus();
            SetupValidSession();

            var uc = FakeUserCheckIn(payload);
            uc.UserId = "WRONG";

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(uc);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));
        }

        // ============================================
        // 9. TOO EARLY
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenCheckInTooEarly()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();

            // ⭐ Lấy cùng readyStatus instance
            var readyStatus = SetupReadyStatus();

            var now = new DateTime(2025, 1, 1, 9, 0, 0);

            // Tạo session với readyStatus instance
            var session = new ConferenceSession
            {
                ConferenceSessionId = payload.conferenceSessionId,
                Conference = new Conference
                {
                    ConferenceStatus = readyStatus  // <-- Dùng cùng instance
                },
                StartTime = now.AddHours(1),
                EndTime = now.AddHours(3)
            };

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository
                .GetConferenceSessionByIdAsync(payload.conferenceSessionId))
                .ReturnsAsync(session);

            // Tạo UserCheckIn và gán lại session
            var uc = FakeUserCheckIn(payload);
            uc.ConferenceSession = session;

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(uc);

            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));

            Assert.Contains("chưa thể check in", ex.Message);
        }


        // ============================================
        // 10. TOO LATE
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenCheckInTooLate()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();

            // ⭐ Lấy cùng readyStatus instance
            var readyStatus = SetupReadyStatus();

            var now = new DateTime(2025, 1, 1, 12, 0, 0);

            var session = new ConferenceSession
            {
                ConferenceSessionId = payload.conferenceSessionId,
                Conference = new Conference
                {
                    ConferenceStatus = readyStatus  // <-- Dùng cùng instance
                },
                StartTime = now.AddHours(-3),
                EndTime = now.AddHours(-1)
            };

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository
                .GetConferenceSessionByIdAsync(payload.conferenceSessionId))
                .ReturnsAsync(session);

            // Gán lại session cho UserCheckIn
            var uc = FakeUserCheckIn(payload);
            uc.ConferenceSession = session;

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(uc);

            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));

            Assert.NotEmpty(ex.Message);
        }


        // ============================================
        // 11. ALREADY CHECKED IN
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldThrow_WhenAlreadyCheckedIn()
        {
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();
            SetupReadyStatus();

            var session = SetupValidSession();
            session.Conference = new Conference
            {
                ConferenceStatus = new ConferenceStatus
                {
                    ConferenceStatusId = "ready-id",
                    ConferenceStatusName = ConferenceStatusEnum.Ready.GetDescription()
                }
            };

            var checkedInStatus = new CheckinStatus
            {
                CheckinStatusId = "checked",
                CheckinStatusName = CheckInStatusEnum.CheckedIn.GetDescription()
            };

            var uc = FakeUserCheckIn(payload);
            uc.CheckinStatus = checkedInStatus;
            uc.CheckInTime = DateTime.Now;

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(uc);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _qrCoderService.ProceedQrCode(new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                }));
        }

        
        // ============================================
        // 12. SUCCESS - FIXED VERSION
        // ============================================
        [Fact]
        public async Task ProceedQrCode_ShouldReturnSuccess()
        {
            // 1. Setup payload và mocks cơ bản
            var payload = FakePayload();
            SetupDecrypt(payload);
            SetupChecksumValid(payload);
            SetupCheckinStatuses();

            // 2.  LẤY readyStatus instance từ SetupReadyStatus
            var readyStatus = SetupReadyStatus();

            // 3. Tạo Conference với CÙNG readyStatus instance
            var conference = new Conference
            {
                ConferenceId = "C1",
                ConferenceStatus = readyStatus  // <-- Key: Dùng cùng instance
            };

            // 4. Tạo Session liên kết với Conference
            var session = new ConferenceSession
            {
                ConferenceSessionId = payload.conferenceSessionId,
                ConferenceId = "C1",
                Conference = conference,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now.AddHours(1)
            };

            // 5. Mock session repository
            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository
                .GetConferenceSessionByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            // 6. Tạo UserCheckIn và gán lại session (để có đúng conference status)
            var uc = FakeUserCheckIn(payload);
            uc.ConferenceSession = session;
            uc.ConferenceSessionId = session.ConferenceSessionId;

            // 7. Mock UserCheckIn repository
            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .GetUserCheckInByIdAsync(payload.userCheckinId))
                .ReturnsAsync(uc);

            // 8. Mock Conference repository
            _mockUnitOfWork.Setup(u => u.ConferenceRepository
                .GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(conference);

            // 9. Mock time và update
            _mockTimeProviderService.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            _mockUnitOfWork.Setup(u => u.UserCheckInRepository
                .UpdateUserCheckInAsync(It.IsAny<UserCheckIn>()))
                .ReturnsAsync(1);

            // 10. Execute
            var result = await _qrCoderService.ProceedQrCode(
                new VerifyQrDataRequest
                {
                    Content = "xx",
                    ConferenceSessionId = payload.conferenceSessionId
                });

            // 11. Assert
            Assert.Contains("đã check in", result);
        }


        // ============================================
        // 13. Generate QR
        // ============================================
        [Fact]
        public async Task GenerateQrCode_ShouldReturnUrl()
        {
            var testData = new { id = "123" };
            string encrypted = "encrypted";
            string token = "abcxyz";

            _mockTokenService.Setup(s => s.EncryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(encrypted);

            _mockTokenService.Setup(s => s.GenerateSecureRandomToken())
                .Returns(token);

            _mockObjectStorageFileService.Setup(s =>
                s.UploadFileAsync("qrcodefile", token, It.IsAny<Stream>(), "image/png"))
                .ReturnsAsync($"/qrcodefile/{token}.png");

            var result = await _qrCoderService.GenerateQrCode(testData);

            result.Should().Be("https://mockstorage.com//qrcodefile/abcxyz.png");
        }

        // ============================================
        // helpers
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
        private ConferenceStatus SetupReadyStatus()
        {
            var readyStatus = new ConferenceStatus
            {
                ConferenceStatusId = "ready-id",
                ConferenceStatusName = ConferenceStatusEnum.Ready.GetDescription()
            };

            _mockUnitOfWork.Setup(u => u.ConferenceStatusRepository
                .GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                .ReturnsAsync(readyStatus);

            return readyStatus;
        }

        private void SetupDecrypt(QrDataPayload p)
        {
            _mockTokenService.Setup(t => t.DecryptString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(p));
        }

        private void SetupChecksumValid(QrDataPayload p)
        {
            _mockTokenService.Setup(t => t.CreateSignature512(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(p.signature);
        }

        private void SetupCheckinStatuses()
        {
            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync("Checked In"))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "checked", CheckinStatusName = "Checked In" });

            _mockUnitOfWork.Setup(u => u.CheckInStatusRepository.GetCheckInStatusByNameAsync("Expired"))
                .ReturnsAsync(new CheckinStatus { CheckinStatusId = "expired", CheckinStatusName = "Expired" });
        }

        private ConferenceSession SetupValidSession()
        {
            var conf = new Conference
            {
                ConferenceId = "C1",
                ConferenceStatus = new ConferenceStatus
                {
                    ConferenceStatusId = "ready-id",
                    ConferenceStatusName = ConferenceStatusEnum.Ready.GetDescription()
                }
            };

            var s = new ConferenceSession
            {
                ConferenceSessionId = "cs1",
                ConferenceId = "C1",
                Title = "Session A",
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now.AddHours(2),
                Conference = conf
            };

            _mockUnitOfWork.Setup(u => u.ConferenceSessionRepository
                .GetConferenceSessionByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(s);

            return s;
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

                ConferenceSession = new ConferenceSession
                {
                    ConferenceSessionId = p.conferenceSessionId,
                    Title = "Session A",
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now.AddHours(1),

                    Conference = new Conference
                    {
                        ConferenceStatus = new ConferenceStatus
                        {
                            ConferenceStatusId = "ready-id",
                            ConferenceStatusName = ConferenceStatusEnum.Ready.GetDescription()
                        }
                    }
                }
            };
        }

    }

}
