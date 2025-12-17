using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Payment;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.RegistrationTest
{
    public class CreatePaymentForAbstractTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRedisService> _redisServiceMock;
        private readonly Mock<IMomoService> _momoServiceMock;
        private readonly Mock<IPayOsService> _payOsServiceMock;
        private readonly Mock<IVnPayService> _vnPayServiceMock;
        private readonly Mock<ITimeProviderService> _timeProviderMock;

        private readonly PaymentService _paymentService;

        public CreatePaymentForAbstractTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _redisServiceMock = new Mock<IRedisService>();
            _momoServiceMock = new Mock<IMomoService>();
            _payOsServiceMock = new Mock<IPayOsService>();
            _vnPayServiceMock = new Mock<IVnPayService>();
            _timeProviderMock = new Mock<ITimeProviderService>();

            _paymentService = new PaymentService(
                _unitOfWorkMock.Object,
                Options.Create(new MomoSettings()),
                _redisServiceMock.Object,
                Mock.Of<ITokenService>(),
                _momoServiceMock.Object,
                _payOsServiceMock.Object,
                Options.Create(new PayOsSettings()),
                _vnPayServiceMock.Object,
                Mock.Of<IQRCoderService>(),
                _timeProviderMock.Object
            );
        }
        private void SetupBasicMocks()
        {
            var readyConfStatus = new ConferenceStatus { ConferenceStatusId = "ready" };
            var reviewStatusAccepted = new ReviewStatus { ReviewStatusId = "accepted" };
            var globalStatusAccepted = new GlobalStatus { GlobalStatusId = "accepted" };

            _unitOfWorkMock.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                           .ReturnsAsync(readyConfStatus);
            _unitOfWorkMock.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                           .ReturnsAsync(reviewStatusAccepted);
            _unitOfWorkMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                           .ReturnsAsync(globalStatusAccepted);

            _unitOfWorkMock.Setup(u => u.PaymentMethodRepository.GetPaymentMethodById(It.IsAny<string>()))
                           .ReturnsAsync(new PaymentMethod { MethodName = PaymentMethodEnum.Wallet.GetDescription() });

            _timeProviderMock.Setup(t => t.GetVietnamDate())
     .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        }

        private Paper CreateBasicPaper(string userId = "user1")
        {
            return new Paper
            {
                PaperId = "paper1",
                PaperAuthors = new List<PaperAuthor> { new() { UserId = userId, IsRootAuthor = true } },
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    IsResearchConference = true,
                    IsInternalHosted = true,
                    ConferenceSessions = new List<ConferenceSession> { new() { ConferenceSessionId = "s1" } },
                    ResearchConferenceDetail = new ResearchConferenceDetail { NumberPaperAccept = 10 },
                    ResearchConferencePhases = new List<ResearchConferencePhase>
                {
                    new() { ResearchConferencePhaseId = "phase1", IsActive = true, AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1) }
                }
                },
                ConferenceId = "conf1"
            };
        }

        private ConferencePrice CreateConferencePrice()
        {
            return new ConferencePrice
            {
                ConferencePriceId = "cp1",
                ConferenceId = "conf1",
                TicketPrice = 100000,
                IsAuthor = true,
                Conference = CreateBasicPaper().Conference,
                PricePhases = new List<PricePhase>
            {
                new() { PricePhaseId = "p1", AvailableSlot = 1, ResearchConferencePhase = new ResearchConferencePhase { IsActive = true }, EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1) }
            }
            };
        }

        [Fact]
        public async Task ShouldThrow_WhenPaperNotExist()
        {
            SetupBasicMocks();
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync((Paper)null);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Bài báo không tồn tại", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenConferenceStatusNull()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper();
            paper.Conference = null;
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(paper);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Không tìm thấy trạng thái của hội nghị", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenUserNotRootAuthor()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper("otherUser");
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(paper);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Bài báo không thuộc quyền sỡ hữu của bạn", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenPaperAlreadyPaid()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper();
            paper.TicketId = "ticket1";
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(paper);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Bài báo đã được thanh toán rồi", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenFullPaperAndRevisionNotAccepted()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper();
            paper.FullPaperId = "fp1";
            paper.RevisionPaperId = "rp1";
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "notAccepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new RevisionPaper { GlobalStatusId = "notAccepted" });

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Bạn phải có fullpaper được chấp nhận hoặc revision được chấp nhận", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenConferencePriceNotFound()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper();
            paper.FullPaperId = "fp1";
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp1"))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" }); // Quan trọng
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync((ConferencePrice)null);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Giá hội nghị với id", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenConferencePriceNoSlot()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper();
            paper.FullPaperId = "fp1";
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp1"))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" }); // Quan trọng
            var confPrice = CreateConferencePrice();
            confPrice.Conference.AvailableSlot = 0;
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("đã bán hết vé", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenNotResearchConference()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper();
            paper.FullPaperId = "fp1";
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp1"))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" }); // Quan trọng
            var confPrice = CreateConferencePrice();
            confPrice.Conference.IsResearchConference = false;

            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Bạn chỉ có thể nộp abstract cho research conference", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenConferenceNotInternalHosted()
        {
            SetupBasicMocks();

            var paper = CreateBasicPaper();
            paper.FullPaperId = "fp1";
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp1"))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" }); // Quan trọng
            var confPrice = CreateConferencePrice();
            confPrice.Conference.IsInternalHosted = false;

            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("Bạn chỉ có thể nộp abstract cho research conference tổ chức bởi confradar", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenPaperCountExceedConferenceLimit()
        {
            SetupBasicMocks();
            var paper = CreateBasicPaper();
            paper.FullPaperId = "fp1";
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync("fp1"))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" }); // Quan trọng
            var confPrice = CreateConferencePrice();
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperCountByConference(It.IsAny<string>()))
                           .ReturnsAsync(10); // max number

            var request = new CreatePaperPaymentRequest { PaperId = "p1", PaymentMethodId = "pm1", ConferencePriceId = "cp1" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );
            Assert.Contains("trên tổng số bài báo quy định", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenNoPhaseActive()
        {
            // 1. Setup base mocks
            SetupBaseMocks();

            // 2. Tạo ResearchConferencePhase inactive
            var inactivePhase = new ResearchConferencePhase
            {
                ResearchConferencePhaseId = "rcp1",
                IsActive = false, // inactive để test "no active phase"
                AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };

            // 3. Tạo paper với Conference đầy đủ
            var paper = new Paper
            {
                PaperId = "paper1",
                PaperAuthors = new List<PaperAuthor> { new() { UserId = "user1", IsRootAuthor = true } },
                FullPaperId = "fp1",
                RevisionPaperId = "rp1",
                ConferenceId = "conf1",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    IsResearchConference = true,
                    IsInternalHosted = true,
                    ConferenceSessions = new List<ConferenceSession> { new() { ConferenceSessionId = "s1" } },
                    ResearchConferenceDetail = new ResearchConferenceDetail { NumberPaperAccept = 10, ConferenceId = "conf1" },
                    ResearchConferencePhases = new List<ResearchConferencePhase> { inactivePhase }
                }
            };

            // 4. Tạo PricePhase liên kết với phase inactive
            var pricePhase = new PricePhase
            {
                PricePhaseId = "pp1",
                AvailableSlot = 10,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                ApplyPercent = 100,
                ResearchConferencePhase = inactivePhase,
                ResearchConferencePhaseId = inactivePhase.ResearchConferencePhaseId
            };

            // 5. Tạo ConferencePrice link tới Conference và PricePhase
            var confPrice = new ConferencePrice
            {
                ConferencePriceId = "cp1",
                ConferenceId = "conf1",
                TicketPrice = 100000,
                Conference = paper.Conference,
                PricePhases = new List<PricePhase> { pricePhase },
                IsAuthor = true
            };

            // 6. Setup repository mocks
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new RevisionPaper { GlobalStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync((PaperWaitList)null);

            // 7. Setup Redis mocks
            _redisServiceMock.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisServiceMock.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            // 8. Setup time provider
            _timeProviderMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            // 9. Tạo request
            var request = new CreatePaperPaymentRequest
            {
                PaperId = "paper1",
                PaymentMethodId = "pm1",
                ConferencePriceId = "cp1"
            };

            // 10. Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );

            Assert.Contains("Giai đoạn hội nghị nghiên cứu không khả dụng", ex.Message);
        }

      

        private void SetupBaseMocks()
        {
            var readyConfStatus = new ConferenceStatus { ConferenceStatusId = "ready" };
            var reviewStatusAccepted = new ReviewStatus { ReviewStatusId = "accepted" };
            var globalStatusAccepted = new GlobalStatus { GlobalStatusId = "accepted" };

            _unitOfWorkMock.Setup(u => u.ConferenceStatusRepository.GetConferenceStatusByNameAsync(It.IsAny<string>()))
                           .ReturnsAsync(readyConfStatus);
            _unitOfWorkMock.Setup(u => u.ReviewStatusRepository.GetReviewStatusByNameAsync(It.IsAny<string>()))
                           .ReturnsAsync(reviewStatusAccepted);
            _unitOfWorkMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                           .ReturnsAsync(globalStatusAccepted);
            _unitOfWorkMock.Setup(u => u.PaymentMethodRepository.GetPaymentMethodById(It.IsAny<string>()))
                           .ReturnsAsync(new PaymentMethod { MethodName = PaymentMethodEnum.Wallet.GetDescription() });

            _timeProviderMock.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));
        }

        private Paper CreatePaper(string userId = "user1")
        {
            return new Paper
            {
                PaperId = "paper1",
                PaperAuthors = new List<PaperAuthor> { new() { UserId = userId, IsRootAuthor = true } },
                FullPaperId = "fp1",
                RevisionPaperId = "rp1",
                ConferenceId = "conf1",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    IsResearchConference = true,
                    IsInternalHosted = true,
                    ConferenceSessions = new List<ConferenceSession> { new() { ConferenceSessionId = "s1" } },
                    ResearchConferenceDetail = new ResearchConferenceDetail { NumberPaperAccept = 10 },
                    ResearchConferencePhases = new List<ResearchConferencePhase>
                {
                    new() { ResearchConferencePhaseId = "phase1", IsActive = true, AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
                }
                }
            };
        }

       

       

      
       

       

        [Fact]
        public async Task ShouldThrow_WhenPhaseNotActive()
        {
            // 1. Setup base mocks
            SetupBaseMocks();

            // 2. Tạo paper
            var paper = CreatePaper();

            // 3. Tạo ConferencePrice và Conference
            var confPrice = CreateConferencePrice();
            confPrice.TicketPrice = 1000000;

            confPrice.Conference = new Conference
            {
                ConferenceId = "conf1",
                ConferenceName = "Test Conference",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                IsResearchConference = true,
                IsInternalHosted = true,
                ConferenceSessions = new List<ConferenceSession>
        {
            new() { ConferenceSessionId = "s1" }
        },
                ResearchConferenceDetail = new ResearchConferenceDetail
                {
                    ConferenceId = "conf1",
                    NumberPaperAccept = 10
                },
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase
            {
                ResearchConferencePhaseId = "phase1",
                IsActive = false, // inactive
                AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            }
        }
            };

            // 4. Tạo PricePhase hợp lệ (cần có để service không crash)
            confPrice.PricePhases = new List<PricePhase>
    {
        new PricePhase
        {
            PricePhaseId = "pp1",
            AvailableSlot = 10,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ApplyPercent = 100,
            ResearchConferencePhase = confPrice.Conference.ResearchConferencePhases.First(),
            ResearchConferencePhaseId = confPrice.Conference.ResearchConferencePhases.First().ResearchConferencePhaseId
        }
    };

            // 5. Gán Conference cho paper
            paper.Conference = confPrice.Conference;

            // 6. Setup repository mocks
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new RevisionPaper { GlobalStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync((PaperWaitList)null);

            // 7. Setup Redis mocks
            _redisServiceMock.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisServiceMock.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            // 8. Setup time provider
            _timeProviderMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            // 9. Tạo request
            var request = new CreatePaperPaymentRequest
            {
                PaperId = paper.PaperId,
                PaymentMethodId = "pm1",
                ConferencePriceId = "cp1"
            };

            // 10. Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );

            Assert.Contains("Giai đoạn hội nghị nghiên cứu không khả dụng", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenPaymentDeadlinePassed()
        {
            // 1. Setup base mocks
            SetupBaseMocks();

            // 2. Tạo paper
            var paper = CreatePaper();

            // 3. Tạo ConferencePrice và Conference
            var confPrice = CreateConferencePrice();
            confPrice.TicketPrice = 1000000;

            confPrice.Conference = new Conference
            {
                ConferenceId = "conf1",
                ConferenceName = "Test Conference",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                IsResearchConference = true,
                IsInternalHosted = true,
                ConferenceSessions = new List<ConferenceSession>
        {
            new() { ConferenceSessionId = "s1" }
        },
                ResearchConferenceDetail = new ResearchConferenceDetail
                {
                    ConferenceId = "conf1",
                    NumberPaperAccept = 10
                },
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase
            {
                ResearchConferencePhaseId = "phase1",
                IsActive = true,
                AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) // đã hết hạn
            }
        }
            };

            // 4. Tạo PricePhase hợp lệ
            confPrice.PricePhases = new List<PricePhase>
    {
        new PricePhase
        {
            PricePhaseId = "pp1",
            AvailableSlot = 10,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ApplyPercent = 100,
            ResearchConferencePhase = confPrice.Conference.ResearchConferencePhases.First(),
            ResearchConferencePhaseId = confPrice.Conference.ResearchConferencePhases.First().ResearchConferencePhaseId
        }
    };

            // 5. Gán Conference cho paper
            paper.Conference = confPrice.Conference;

            // 6. Setup repository mocks
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new RevisionPaper { GlobalStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync((PaperWaitList)null);

            // 7. Setup Redis mocks
            _redisServiceMock.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisServiceMock.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            // 8. Setup time provider
            _timeProviderMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            // 9. Tạo request
            var request = new CreatePaperPaymentRequest
            {
                PaperId = paper.PaperId,
                PaymentMethodId = "pm1",
                ConferencePriceId = "cp1"
            };

            // 10. Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );

            Assert.Contains("Ðã hết thời hạn mua vé", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenUserHasAttendeeTicket()
        {
            SetupBaseMocks();

            // 1. Tạo paper đầy đủ
            var paper = CreatePaper();
            paper.FullPaperId = "fp1";
            paper.RevisionPaperId = "rp1";
            paper.Conference = new Conference
            {
                ConferenceId = "conf1",
                ConferenceName = "Test Conference",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                IsResearchConference = true,
                IsInternalHosted = true,
                ConferenceSessions = new List<ConferenceSession>
        {
            new() { ConferenceSessionId = "s1" }
        },
                ResearchConferenceDetail = new ResearchConferenceDetail
                {
                    ConferenceId = "conf1",
                    NumberPaperAccept = 10
                },
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new()
            {
                ResearchConferencePhaseId = "phase1",
                IsActive = true,
                AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            }
        }
            };

            // 2. Tạo ConferencePrice với PricePhase hợp lệ
            var confPrice = CreateConferencePrice();
            confPrice.Conference = paper.Conference;
            confPrice.PricePhases = new List<PricePhase>
    {
        new()
        {
            PricePhaseId = "pp1",
            AvailableSlot = 10,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ApplyPercent = 100,
            ResearchConferencePhase = paper.Conference.ResearchConferencePhases.First(),
            ResearchConferencePhaseId = paper.Conference.ResearchConferencePhases.First().ResearchConferencePhaseId
        }
    };

            // 3. Setup repository mocks
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>())).ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new RevisionPaper { GlobalStatusId = "accepted" });

            // 4. Simulate user đã có attendee ticket
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket> { new Ticket() });

            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());

            _unitOfWorkMock.Setup(u => u.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync((PaperWaitList)null);

            // 5. Setup Redis mocks
            _redisServiceMock.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisServiceMock.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            _timeProviderMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            // 6. Tạo request
            var request = new CreatePaperPaymentRequest
            {
                PaperId = paper.PaperId,
                PaymentMethodId = "pm1",
                ConferencePriceId = "cp1"
            };

            // 7. Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );

            Assert.Contains("đang có 1 vé là người tham dự hội nghị", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenUserAlreadyHasAuthorTicket()
        {
            SetupBaseMocks();

            // 1. Tạo paper và gán Conference đầy đủ
            var paper = CreatePaper();
            paper.FullPaperId = "fp1";
            paper.RevisionPaperId = "rp1";
            paper.Conference = new Conference
            {
                ConferenceId = "conf1",
                ConferenceName = "Test Conference",
                ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                IsResearchConference = true,
                IsInternalHosted = true,
                ConferenceSessions = new List<ConferenceSession>
        {
            new() { ConferenceSessionId = "s1" }
        },
                ResearchConferenceDetail = new ResearchConferenceDetail
                {
                    ConferenceId = "conf1",
                    NumberPaperAccept = 10
                },
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new()
            {
                ResearchConferencePhaseId = "phase1",
                IsActive = true,
                AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            }
        }
            };

            // 2. ConferencePrice với PricePhase hợp lệ
            var confPrice = CreateConferencePrice();
            confPrice.Conference = paper.Conference;
            confPrice.PricePhases = new List<PricePhase>
    {
        new()
        {
            PricePhaseId = "pp1",
            AvailableSlot = 10,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ApplyPercent = 100,
            ResearchConferencePhase = paper.Conference.ResearchConferencePhases.First(),
            ResearchConferencePhaseId = paper.Conference.ResearchConferencePhases.First().ResearchConferencePhaseId
        }
    };

            // 3. Setup repository mocks
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>())).ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new RevisionPaper { GlobalStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket> { new Ticket() }); // simulate user already has author ticket
            _unitOfWorkMock.Setup(u => u.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync((PaperWaitList)null);

            // 4. Setup Redis mocks
            _redisServiceMock.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisServiceMock.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            _timeProviderMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            // 5. Tạo request
            var request = new CreatePaperPaymentRequest
            {
                PaperId = paper.PaperId,
                PaymentMethodId = "pm1",
                ConferencePriceId = "cp1"
            };

            // 6. Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );

            Assert.Contains("mua vé 1 lần cho research paper", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenNoValidPhase()
        {
            SetupBaseMocks();

            // 1. Paper
            var paper = CreatePaper();

            // 2. ConferencePrice + Conference
            var confPrice = CreateConferencePrice();
            confPrice.TicketPrice = 100000;
            confPrice.PricePhases.Clear(); // Không có phase hợp lệ
            confPrice.Conference ??= new Conference
            {
                ConferenceId = "conf1",
                ConferenceSessions = new List<ConferenceSession> { new() { ConferenceSessionId = "s1" } },
                ResearchConferenceDetail = new ResearchConferenceDetail
                {
                    ConferenceId = "conf1",
                    NumberPaperAccept = 10
                },
                ResearchConferencePhases = new List<ResearchConferencePhase>
        {
            new ResearchConferencePhase { IsActive = false, AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
        }
            };

            paper.Conference = confPrice.Conference;

            // 4. Setup repository mocks
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new RevisionPaper { GlobalStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync((PaperWaitList)null);

            // 5. Setup Redis mocks
            _redisServiceMock.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisServiceMock.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            // 6. Setup time provider
            _timeProviderMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            // 7. Request
            var request = new CreatePaperPaymentRequest { PaperId = paper.PaperId, PaymentMethodId = "pm1", ConferencePriceId = "cp1" };

            // 8. Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );

            Assert.Contains("Hiện tại không có phase hợp lệ", ex.Message);
        }

        [Fact]
        public async Task ShouldThrow_WhenCurrentPhaseSlotZero()
        {
            // 1. Setup base mocks
            SetupBaseMocks();

            // 2. Tạo paper với Conference đầy đủ
            var activePhase = new ResearchConferencePhase
            {
                ResearchConferencePhaseId = "rcp1",
                IsActive = true,
                AuthorPaymentEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };

            var paper = new Paper
            {
                PaperId = "paper1",
                PaperAuthors = new List<PaperAuthor> { new() { UserId = "user1", IsRootAuthor = true } },
                FullPaperId = "fp1",
                RevisionPaperId = "rp1",
                ConferenceId = "conf1",
                Conference = new Conference
                {
                    ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "ready" },
                    IsResearchConference = true,
                    IsInternalHosted = true,
                    ConferenceSessions = new List<ConferenceSession> { new() { ConferenceSessionId = "s1" } },
                    ResearchConferenceDetail = new ResearchConferenceDetail { NumberPaperAccept = 10, ConferenceId = "conf1" },
                    ResearchConferencePhases = new List<ResearchConferencePhase> { activePhase }
                }
            };

            // 3. Tạo PricePhase slot = 0
            var pricePhase = new PricePhase
            {
                PricePhaseId = "pp1",
                AvailableSlot = 0,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                ApplyPercent = 100,
                ResearchConferencePhase = activePhase,
                ResearchConferencePhaseId = activePhase.ResearchConferencePhaseId
            };

            // 4. ConferencePrice link tới Conference và PricePhase
            var confPrice = new ConferencePrice
            {
                ConferencePriceId = "cp1",
                ConferenceId = "conf1",
                TicketPrice = 100000,
                Conference = paper.Conference,
                PricePhases = new List<PricePhase> { pricePhase },
                IsAuthor = true
            };

            // 5. Setup repository mocks
            _unitOfWorkMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(paper);
            _unitOfWorkMock.Setup(u => u.ConferencePriceRepository.GetConferencePriceByIdAsync(It.IsAny<string>())).ReturnsAsync(confPrice);
            _unitOfWorkMock.Setup(u => u.FullPaperRepository.GetFullPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(new FullPaper { ReviewStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.RevisionPaperRepository.GetRevisionPaperByIdAsync(It.IsAny<string>())).ReturnsAsync(new RevisionPaper { GlobalStatusId = "accepted" });
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<Ticket>());
            _unitOfWorkMock.Setup(u => u.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((PaperWaitList)null);

            // 6. Setup Redis mocks
            _redisServiceMock.Setup(r => r.KeyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisServiceMock.Setup(r => r.GetKeysByPatternAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            // 7. Setup time provider
            _timeProviderMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            // 8. Tạo request
            var request = new CreatePaperPaymentRequest
            {
                PaperId = "paper1",
                PaymentMethodId = "pm1",
                ConferencePriceId = "cp1"
            };

            // 9. Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _paymentService.CreatePaymentForAbstract(request, "user1")
            );

            Assert.Contains("Giai đoạn hiện tại đã hết slot", ex.Message);
        }

    }
}
       





