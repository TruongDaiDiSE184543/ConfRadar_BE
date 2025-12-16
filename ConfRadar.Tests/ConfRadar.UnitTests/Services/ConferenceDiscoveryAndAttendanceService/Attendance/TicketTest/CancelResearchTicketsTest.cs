////using ConfRadar.Repositories;
////using ConfRadar.Repositories.Models;
////using ConfRadar.Repositories.Repositories;
////using ConfRadar.Services.Common;
////using ConfRadar.Services.Exceptions;
////using ConfRadar.Services.Services;
////using ConfRadar.Shared.DTO.Ticket;
////using Moq;

////namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.TicketTest
////{
////    public class CancelResearchTicketsTest
////    {
////        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
////        private readonly Mock<ITimeProviderService> _mockTimeProvider;
////        private readonly Mock<ITicketRepository> _mockTicketRepo;
////        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
////        private readonly Mock<IWalletRepository> _mockWalletRepo;
////        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo;
////        private readonly Mock<IWalletTransactionRepository> _mockWalletTransactionRepo;
////        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
////        private readonly Mock<IGlobalStatusRepository> _mockGlobalStatusRepo;
////        private readonly Mock<IReviewStatusRepository> _mockReviewStatusRepo;

////        private readonly TicketService _service;

////        public CancelResearchTicketsTest()
////        {
////            _mockUnitOfWork = new Mock<IUnitOfWork>();
////            _mockTimeProvider = new Mock<ITimeProviderService>();
////            _mockTicketRepo = new Mock<ITicketRepository>();
////            _mockConferenceRepo = new Mock<IConferenceRepository>();
////            _mockWalletRepo = new Mock<IWalletRepository>();
////            _mockPaymentMethodRepo = new Mock<IPaymentMethodRepository>();
////            _mockWalletTransactionRepo = new Mock<IWalletTransactionRepository>();
////            _mockTransactionRepo = new Mock<ITransactionRepository>();
////            _mockGlobalStatusRepo = new Mock<IGlobalStatusRepository>();
////            _mockReviewStatusRepo = new Mock<IReviewStatusRepository>();

////            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
////            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
////            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(_mockWalletRepo.Object);
////            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
////            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository).Returns(_mockWalletTransactionRepo.Object);
////            _mockUnitOfWork.Setup(u => u.TransactionRepository).Returns(_mockTransactionRepo.Object);
////            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository).Returns(_mockGlobalStatusRepo.Object);
////            _mockUnitOfWork.Setup(u => u.ReviewStatusRepository).Returns(_mockReviewStatusRepo.Object);

////            _service = new TicketService(_mockUnitOfWork.Object, _mockTimeProvider.Object);
////        }

////        private CancelResearchTickets CreateCancelRequest(params string[] ticketIds)
////        {
////            return new CancelResearchTickets
////            {
////                TicketIds = ticketIds.ToList()
////            };
////        }

////        private Ticket CreateResearchTicket(string ticketId, string userId, string conferenceId, decimal amount = 150m, bool withPaper = false)
////        {
////            var user = new User
////            {
////                UserId = userId,
////                FullName = "Research User",
////                Wallet = new Wallet { WalletId = $"W_{userId}", UserId = userId, Balance = 0m }
////            };

////            var conference = new Conference
////            {
////                ConferenceId = conferenceId,
////                ConferenceName = "Research Conference 2025",
////                AvailableSlot = 20
////            };

////            var confPrice = new ConferencePrice
////            {
////                ConferencePriceId = $"CP_{conferenceId}",
////                ConferenceId = conferenceId,
////                IsAuthor = true,
////                Conference = conference,
////                AvailableSlot = 10
////            };

////            var pricePhase = new PricePhase
////            {
////                PricePhaseId = $"PP_{ticketId}",
////                ConferencePrice = confPrice,
////                AvailableSlot = 5
////            };

////            var transaction = new Transaction
////            {
////                TransactionId = $"TX_{ticketId}",
////                Amount = amount,
////                IsRefunded = false,
////                CreatedAt = DateTime.Now
////            };

////            var ticket = new Ticket
////            {
////                TicketId = ticketId,
////                UserId = userId,
////                IsRefunded = false,
////                User = user,
////                PricePhase = pricePhase,
////                Transactions = new List<Transaction> { transaction }
////            };

////            if (withPaper)
////            {
////                ticket.Paper = CreatePaper(ticketId);
////            }

////            return ticket;
////        }

//        private Paper CreatePaper(string ticketId)
//        {
//            return new Paper
//            {
//                PaperId = $"P_{ticketId}",
//                Abstract = new Abstract
//                {
//                    AbstractId = $"ABS_{ticketId}",
//                    GlobalStatus = new GlobalStatus { GlobalStatusId = "GS_PENDING", Name = "Pending" }
//                },
//                FullPaper = new FullPaper
//                {
//                    FullPaperId = $"FP_{ticketId}",
//                    ReviewStatus = new ReviewStatus { ReviewStatusId = "RS_PENDING", Name = "Pending" }
//                },
//                RevisionPaper = new RevisionPaper
//                {
//                    RevisionPaperId = $"REV_{ticketId}",
//                    GlobalStatus = new GlobalStatus { GlobalStatusId = "GS_PENDING", Name = "Pending" }
//                },
//                CameraReady = new CameraReady
//                {
//                    CameraReadyId = $"CR_{ticketId}",
//                }
//            };
//        }

////        private void SetupCommonMocks()
////        {
////            var now = new DateTime(2025, 11, 29, 10, 0, 0);
////            _mockTimeProvider.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
////            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

////            _mockPaymentMethodRepo
////                .Setup(p => p.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription()))
////                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM_WALLET", MethodName = "Wallet" });

////            _mockGlobalStatusRepo
////                .Setup(g => g.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
////                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "GS_REJECTED", Name = "Rejected" });

////            _mockReviewStatusRepo
////                .Setup(r => r.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription()))
////                .ReturnsAsync(new ReviewStatus { ReviewStatusId = "RS_REJECTED", Name = "Rejected" });
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldReturnZero_WhenNoTicketsFound()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var request = CreateCancelRequest("T1", "T2");
////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(request.TicketIds))
////                .ReturnsAsync(new List<Ticket>());

////            // Act
////            var result = await _service.CancelResearchTickets(request, "U1");

////            // Assert
////            Assert.Equal(0, result);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldThrow_WhenTicketNotBelongToUserConference()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket = CreateResearchTicket("T1", "U1", "C1");
////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket });

////            // User owns C2, not C1
////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C2" });

////            var request = CreateCancelRequest("T1");

////            // Act & Assert
////            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
////                _service.CancelResearchTickets(request, "U1"));

////            Assert.Contains("không thuộc về bất cứ hội nghị nào của bạn", ex.Message);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldThrow_WhenUserWalletIsNull()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket = CreateResearchTicket("T1", "U1", "C1");
////            ticket.User.Wallet = null;

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            var request = CreateCancelRequest("T1");

////            // Act & Assert
////            await Assert.ThrowsAsync<NotFoundException>(() =>
////                _service.CancelResearchTickets(request, "U1"));
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldProcessSuccessfully_WithoutPaper()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket1 = CreateResearchTicket("T1", "U1", "C1", 150m, withPaper: false);
////            var ticket2 = CreateResearchTicket("T2", "U1", "C1", 200m, withPaper: false);

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket1, ticket2 });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
////            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
////            _mockTicketRepo.Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>())).ReturnsAsync(2);
////            _mockWalletTransactionRepo.Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>())).ReturnsAsync(2);
////            _mockTransactionRepo.Setup(r => r.CreateTransactionListAsync(It.IsAny<List<Transaction>>())).ReturnsAsync(2);

////            var request = CreateCancelRequest("T1", "T2");

////            // Act
////            var result = await _service.CancelResearchTickets(request, "U1");

////            // Assert
////            Assert.Equal(6, result); // 2 + 2 + 2

////            Assert.Equal(150m, ticket1.User.Wallet.Balance);
////            Assert.Equal(200m, ticket2.User.Wallet.Balance);
////            Assert.True(ticket1.IsRefunded);
////            Assert.True(ticket2.IsRefunded);

////            // Verify slots increased
////            Assert.Equal(6, ticket1.PricePhase.AvailableSlot); // 5 + 1
////            Assert.Equal(11, ticket1.PricePhase.ConferencePrice!.AvailableSlot); // 10 + 1
////            Assert.Equal(21, ticket1.PricePhase.ConferencePrice!.Conference!.AvailableSlot); // 20 + 1

////            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldRejectAllPaperPhases_WhenPaperExists()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket = CreateResearchTicket("T1", "U1", "C1", 150m, withPaper: true);

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
////            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
////            _mockTicketRepo.Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>())).ReturnsAsync(1);
////            _mockWalletTransactionRepo.Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>())).ReturnsAsync(1);
////            _mockTransactionRepo.Setup(r => r.CreateTransactionListAsync(It.IsAny<List<Transaction>>())).ReturnsAsync(1);

////            var request = CreateCancelRequest("T1");

////            // Act
////            var result = await _service.CancelResearchTickets(request, "U1");

////            // Assert
////            Assert.Equal(3, result);

//            // Verify all paper phases are rejected
//            Assert.Equal("GS_REJECTED", ticket.Paper!.Abstract!.GlobalStatus.GlobalStatusId);
//            Assert.Equal("RS_REJECTED", ticket.Paper!.FullPaper!.ReviewStatus.ReviewStatusId);
//            Assert.Equal("GS_REJECTED", ticket.Paper!.RevisionPaper!.GlobalStatus.GlobalStatusId);
//            //Assert.Equal("GS_REJECTED", ticket.Paper!.CameraReady!.GlobalStatus.GlobalStatusId);

////            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldHandlePartialPaperPhases()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket = CreateResearchTicket("T1", "U1", "C1", 150m, withPaper: true);

////            // Only Abstract and FullPaper exist, no RevisionPaper or CameraReady
////            ticket.Paper!.RevisionPaper = null;
////            ticket.Paper!.CameraReady = null;

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
////            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
////            _mockTicketRepo.Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>())).ReturnsAsync(1);
////            _mockWalletTransactionRepo.Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>())).ReturnsAsync(1);
////            _mockTransactionRepo.Setup(r => r.CreateTransactionListAsync(It.IsAny<List<Transaction>>())).ReturnsAsync(1);

////            var request = CreateCancelRequest("T1");

////            // Act
////            var result = await _service.CancelResearchTickets(request, "U1");

////            // Assert
////            Assert.Equal(3, result);

////            // Only Abstract and FullPaper should be rejected
////            Assert.Equal("GS_REJECTED", ticket.Paper!.Abstract!.GlobalStatus.GlobalStatusId);
////            Assert.Equal("RS_REJECTED", ticket.Paper!.FullPaper!.ReviewStatus.ReviewStatusId);

////            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldCreateCorrectWalletTransactions()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket = CreateResearchTicket("T1", "U1", "C1", 150m);

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
////            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
////            _mockTicketRepo.Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>())).ReturnsAsync(1);
////            _mockWalletTransactionRepo.Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>())).ReturnsAsync(1);
////            _mockTransactionRepo.Setup(r => r.CreateTransactionListAsync(It.IsAny<List<Transaction>>())).ReturnsAsync(1);

////            var request = CreateCancelRequest("T1");

////            // Act
////            await _service.CancelResearchTickets(request, "U1");

////            // Assert
////            _mockWalletTransactionRepo.Verify(
////                r => r.CreateWalletTransactionListAsync(It.Is<List<WalletTransaction>>(list =>
////                    list.Count == 1 &&
////                    list[0].Amount == 150m &&
////                    list[0].TransactionType == WalletTransactionTypeEnum.Refund.GetDescription() &&
////                    list[0].Description.Contains("Research Conference 2025"))),
////                Times.Once);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldCreateCorrectRefundTransactions()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket = CreateResearchTicket("T1", "U1", "C1", 150m);

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
////            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
////            _mockTicketRepo.Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>())).ReturnsAsync(1);
////            _mockWalletTransactionRepo.Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>())).ReturnsAsync(1);
////            _mockTransactionRepo.Setup(r => r.CreateTransactionListAsync(It.IsAny<List<Transaction>>())).ReturnsAsync(1);

////            var request = CreateCancelRequest("T1");

////            // Act
////            await _service.CancelResearchTickets(request, "U1");

////            // Assert
////            _mockTransactionRepo.Verify(
////                r => r.CreateTransactionListAsync(It.Is<List<Transaction>>(list =>
////                    list.Count == 1 &&
////                    list[0].Amount == 150m &&
////                    list[0].IsRefunded == true &&
////                    list[0].Currency == "VND" &&
////                    list[0].PaymentMethod.MethodName == "Wallet" &&
////                    list[0].TicketId == "T1")),
////                Times.Once);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldRollback_OnException()
////        {
////            // Arrange
////            SetupCommonMocks();
////            var ticket = CreateResearchTicket("T1", "U1", "C1");

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);

////            // Simulate DB failure
////            _mockTicketRepo
////                .Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>()))
////                .ThrowsAsync(new InvalidOperationException("DB error"));

////            var request = CreateCancelRequest("T1");

////            // Act & Assert
////            await Assert.ThrowsAsync<InvalidOperationException>(() =>
////                _service.CancelResearchTickets(request, "U1"));

////            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
////            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
////        }

////        [Fact]
////        public async Task CancelResearchTickets_ShouldProcessMultipleTickets_WithMixedPaperStates()
////        {
////            // Arrange
////            SetupCommonMocks();

////            // Create shared user and wallet
////            var sharedWallet = new Wallet { WalletId = "W_U1", UserId = "U1", Balance = 0m };
////            var sharedUser = new User
////            {
////                UserId = "U1",
////                FullName = "Research User",
////                Wallet = sharedWallet
////            };

////            var ticket1 = CreateResearchTicket("T1", "U1", "C1", 150m, withPaper: true);
////            var ticket2 = CreateResearchTicket("T2", "U1", "C1", 200m, withPaper: false);
////            var ticket3 = CreateResearchTicket("T3", "U1", "C1", 180m, withPaper: true);

////            // Share the same user and wallet across all tickets
////            ticket1.User = sharedUser;
////            ticket2.User = sharedUser;
////            ticket3.User = sharedUser;

////            _mockTicketRepo
////                .Setup(r => r.GetNotRefundResearchTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
////                .ReturnsAsync(new List<Ticket> { ticket1, ticket2, ticket3 });

////            _mockConferenceRepo
////                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", true))
////                .ReturnsAsync(new List<string> { "C1" });

////            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
////            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
////            _mockTicketRepo.Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>())).ReturnsAsync(3);
////            _mockWalletTransactionRepo.Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>())).ReturnsAsync(3);
////            _mockTransactionRepo.Setup(r => r.CreateTransactionListAsync(It.IsAny<List<Transaction>>())).ReturnsAsync(3);

////            var request = CreateCancelRequest("T1", "T2", "T3");

////            // Act
////            var result = await _service.CancelResearchTickets(request, "U1");

////            // Assert
////            Assert.Equal(9, result); // 3 + 3 + 3

////            // Verify wallet balances accumulated
////            Assert.Equal(530m, sharedWallet.Balance); // 150 + 200 + 180

////            // Verify all tickets are refunded
////            Assert.True(ticket1.IsRefunded);
////            Assert.True(ticket2.IsRefunded);
////            Assert.True(ticket3.IsRefunded);

////            // Verify papers are rejected for ticket1 and ticket3
////            Assert.Equal("GS_REJECTED", ticket1.Paper!.Abstract!.GlobalStatus.GlobalStatusId);
////            Assert.Null(ticket2.Paper); // No paper
////            Assert.Equal("GS_REJECTED", ticket3.Paper!.Abstract!.GlobalStatus.GlobalStatusId);

////            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
////        }
////    }
////}