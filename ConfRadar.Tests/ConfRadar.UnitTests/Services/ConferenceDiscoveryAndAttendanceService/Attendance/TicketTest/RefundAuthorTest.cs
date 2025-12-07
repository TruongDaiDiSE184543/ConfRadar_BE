//using ConfRadar.Repositories;
//using ConfRadar.Repositories.Models;
//using ConfRadar.Repositories.Repositories;
//using ConfRadar.Services.Exceptions;
//using ConfRadar.Services.Services;
//using Moq;

//namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.TicketTest
//{
//    public class RefundAuthorTest
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<ITimeProviderService> _mockTimeProvider;
//        private readonly TicketService _service;

//        public RefundAuthorTest()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockTimeProvider = new Mock<ITimeProviderService>();

//            // mock repositories
//            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(new Mock<ITicketRepository>().Object);
//            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(new Mock<IWalletRepository>().Object);
//            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository).Returns(new Mock<IWalletTransactionRepository>().Object);
//            _mockUnitOfWork.Setup(u => u.TransactionRepository).Returns(new Mock<ITransactionRepository>().Object);
//            _mockUnitOfWork.Setup(u => u.PricePhaseRepository).Returns(new Mock<IPricePhaseRepository>().Object);
//            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(new Mock<IPaymentMethodRepository>().Object);
//            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository).Returns(new Mock<IPaperPhaseRepository>().Object);

//            _service = new TicketService(_mockUnitOfWork.Object, _mockTimeProvider.Object);
//        }

//        private Ticket CreateBasicTicket(string userId = "U1", string ticketId = "T1", decimal txAmount = 100m)
//        {
//            var conference = new Conference
//            {
//                ConferenceId = "C1",
//                AvailableSlot = 10,
//                ResearchConferenceDetail = new ResearchConferenceDetail
//                {
//                    ReviewFee = 20m
//                }
//            };

//            var conferencePrice = new ConferencePrice
//            {
//                ConferencePriceId = "CP1",
//                Conference = conference
//            };

//            var pricePhase = new PricePhase
//            {
//                PricePhaseId = "PP1",
//                ConferencePrice = conferencePrice,
//                AvailableSlot = 5
//            };

//            var transaction = new Transaction
//            {
//                TransactionId = "TX1",
//                Amount = txAmount,
//                IsRefunded = false,
//                TicketId = ticketId
//            };

//            return new Ticket
//            {
//                TicketId = ticketId,
//                UserId = userId,
//                IsRefunded = false,
//                PricePhase = pricePhase,
//                Transactions = new List<Transaction> { transaction }
//            };
//        }

//        [Fact]
//        public async Task RefundAuthorCloneFunction_ShouldThrow_WhenTicketNotFound()
//        {
//            // Arrange
//            _mockUnitOfWork.Setup(u => u.TicketRepository.GetTicketByTicketIdAndUserId(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync((Ticket)null);

//            // Act + Assert
//            //await Assert.ThrowsAsync<NotFoundException>(() => _service.RefundAuthorCloneFunction("U1", "T1", "refund"));
//        }

//        [Fact]
//        public async Task RefundAuthorCloneFunction_ShouldThrow_WhenMultipleTransactions()
//        {
//            // Arrange
//            var ticket = CreateBasicTicket();
//            ticket.Transactions.Add(new Transaction { TransactionId = "TX2", Amount = 50m }); // tạo multiple transactions

//            // Mock ticket repository
//            _mockUnitOfWork.Setup(u => u.TicketRepository.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
//                .ReturnsAsync(ticket);

//            // Mock PaymentMethodRepository để tránh NotFoundException
//            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository.GetPaymentMethodByName(It.IsAny<string>()))
//                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM1" });

//            // Mock PaperPhaseRepository để tránh NotFoundException
//            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
//                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PP1" });

//            // Act + Assert
//            //await Assert.ThrowsAsync<BadRequestException>(() =>
//            //    //_service.RefundAuthorCloneFunction(ticket.UserId, ticket.TicketId, "refund"));
//        }

//        [Fact]
//        public async Task RefundAuthorCloneFunction_ShouldThrow_WhenTransactionNotFound()
//        {
//            // Arrange
//            var ticket = CreateBasicTicket();
//            ticket.Transactions.First().IsRefunded = true; // tất cả transaction đã refunded

//            _mockUnitOfWork.Setup(u => u.TicketRepository.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
//                .ReturnsAsync(ticket);

//            // Mock PaymentMethodRepository để tránh NotFoundException
//            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository.GetPaymentMethodByName(It.IsAny<string>()))
//                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM1" });

//            // Mock PaperPhaseRepository để tránh NotFoundException
//            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
//                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PP1" });

//            // Act + Assert
//            //await Assert.ThrowsAsync<BadRequestException>(() =>
//            //    _service.RefundAuthorCloneFunction(ticket.UserId, ticket.TicketId, "refund"));
//        }

//        [Fact]
//        public async Task RefundAuthorCloneFunction_ShouldThrow_WhenWalletNotFound()
//        {
//            // Arrange
//            var ticket = CreateBasicTicket();

//            _mockUnitOfWork.Setup(u => u.TicketRepository.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
//                .ReturnsAsync(ticket);

//            _mockUnitOfWork.Setup(u => u.WalletRepository.GetWalletByUserIdAsync(ticket.UserId))
//                .ReturnsAsync((Wallet)null);

//            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository.GetPaymentMethodByName(It.IsAny<string>()))
//                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM1" });

//            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
//                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PP1" });

//            // Act + Assert
//            //await Assert.ThrowsAsync<NotFoundException>(() => _service.RefundAuthorCloneFunction(ticket.UserId, ticket.TicketId, "refund"));
//        }

//        [Fact]
//        public async Task RefundAuthorCloneFunction_ShouldReturnResult_WhenAllValid()
//        {
//            // Arrange
//            var ticket = CreateBasicTicket();

//            _mockUnitOfWork.Setup(u => u.TicketRepository.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
//                .ReturnsAsync(ticket);

//            var wallet = new Wallet { WalletId = "W1", UserId = ticket.UserId, Balance = 0m };
//            _mockUnitOfWork.Setup(u => u.WalletRepository.GetWalletByUserIdAsync(ticket.UserId))
//                .ReturnsAsync(wallet);

//            _mockUnitOfWork.Setup(u => u.WalletRepository.UpdateWalletAsync(It.IsAny<Wallet>()))
//                .ReturnsAsync(1);
//            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository.CreateWalletTransactionAsync(It.IsAny<WalletTransaction>()))
//                .ReturnsAsync(1);
//            _mockUnitOfWork.Setup(u => u.PricePhaseRepository.UpdatePricePhaseAsync(It.IsAny<PricePhase>()))
//                .ReturnsAsync(1);
//            _mockUnitOfWork.Setup(u => u.TransactionRepository.CreateTransactionAsync(It.IsAny<Transaction>()))
//                .ReturnsAsync(1);
//            _mockUnitOfWork.Setup(u => u.TicketRepository.UpdateTicketAsync(It.IsAny<Ticket>()))
//                .ReturnsAsync(1);

//            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository.GetPaymentMethodByName(It.IsAny<string>()))
//                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM1" });

//            _mockUnitOfWork.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByNameAsync(It.IsAny<string>()))
//                .ReturnsAsync(new PaperPhase { PaperPhaseId = "PP1" });

//            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));
//            _mockTimeProvider.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.Now);

//            // Act
//            //var result = await _service.RefundAuthorCloneFunction(ticket.UserId, ticket.TicketId, "Refund for author");

//            // Assert
//            //Assert.Equal(5, result); // 5 repo actions: update wallet, create wallet tx, update pricePhase, create transaction, update ticket
//            Assert.Equal(ticket.Transactions.ToList()[0].Amount - ticket.PricePhase.ConferencePrice.Conference.ResearchConferenceDetail.ReviewFee,
//                         wallet.Balance);
//        }
//    }
//}
