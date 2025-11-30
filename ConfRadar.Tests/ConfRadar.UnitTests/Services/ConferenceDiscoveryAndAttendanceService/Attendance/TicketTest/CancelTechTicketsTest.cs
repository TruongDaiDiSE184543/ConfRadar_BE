using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Ticket;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.TicketTest
{
    public class CancelTechTicketsTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly Mock<IWalletRepository> _mockWalletRepo;
        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo;
        private readonly Mock<IWalletTransactionRepository> _mockWalletTransactionRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;

        private readonly TicketService _service;

        public CancelTechTicketsTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProvider = new Mock<ITimeProviderService>();
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();
            _mockWalletRepo = new Mock<IWalletRepository>();
            _mockPaymentMethodRepo = new Mock<IPaymentMethodRepository>();
            _mockWalletTransactionRepo = new Mock<IWalletTransactionRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();

            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(_mockWalletRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository).Returns(_mockWalletTransactionRepo.Object);
            _mockUnitOfWork.Setup(u => u.TransactionRepository).Returns(_mockTransactionRepo.Object);

            _service = new TicketService(_mockUnitOfWork.Object, _mockTimeProvider.Object);
        }

        private CancelTechnicalTickets CreateCancelRequest(params string[] ticketIds)
        {
            return new CancelTechnicalTickets
            {
                TicketIds = ticketIds.ToList()
            };
        }

        private Ticket CreateTechTicket(string ticketId, string userId, string conferenceId, decimal amount = 100m)
        {
            var user = new User
            {
                UserId = userId,
                FullName = "Test User",
                Wallet = new Wallet { WalletId = $"W_{userId}", UserId = userId, Balance = 0m }
            };

            var conference = new Conference
            {
                ConferenceId = conferenceId,
                ConferenceName = "Test Tech Conf",
                AvailableSlot = 10
            };

            var confPrice = new ConferencePrice
            {
                ConferencePriceId = $"CP_{conferenceId}",
                ConferenceId = conferenceId,  // ✅ THÊM DÒNG NÀY
                IsAuthor = false,
                Conference = conference,
                AvailableSlot = 5
            };

            var pricePhase = new PricePhase
            {
                PricePhaseId = $"PP_{ticketId}",
                ConferencePrice = confPrice,
                AvailableSlot = 3
            };

            var transaction = new Transaction
            {
                TransactionId = $"TX_{ticketId}",
                Amount = amount,
                IsRefunded = false,
                CreatedAt = DateTime.Now
            };

            return new Ticket
            {
                TicketId = ticketId,
                UserId = userId,
                IsRefunded = false,
                User = user,
                PricePhase = pricePhase,
                Transactions = new List<Transaction> { transaction }
            };
        }

        private void SetupCommonMocks()
        {
            var now = new DateTime(2025, 11, 29, 10, 0, 0);
            _mockTimeProvider.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);
            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(now));

            _mockPaymentMethodRepo
                .Setup(p => p.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription()))
                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM_WALLET", MethodName = "Wallet" });
        }

        [Fact]
        public async Task CancelTechTickets_ShouldReturnZero_WhenNoTicketsFound()
        {
            // Arrange
            SetupCommonMocks();
            var request = CreateCancelRequest("T1", "T2");
            _mockTicketRepo
                .Setup(r => r.GetNotRefundTechnicalTicketListByTicketIdsForCancel(request.TicketIds))
                .ReturnsAsync(new List<Ticket>());

            // Act
            var result = await _service.CancelTechTickets(request, "U1");

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task CancelTechTickets_ShouldThrow_WhenTicketNotBelongToUserConference()
        {
            // Arrange
            SetupCommonMocks();
            var ticket = CreateTechTicket("T1", "U1", "C1");
            _mockTicketRepo
                .Setup(r => r.GetNotRefundTechnicalTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Ticket> { ticket });

            // User only owns C2, not C1
            _mockConferenceRepo
                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", false))
                .ReturnsAsync(new List<string> { "C2" });

            var request = CreateCancelRequest("T1");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CancelTechTickets(request, "U1"));

            Assert.Contains("không thuộc về hội nghị của bạn", ex.Message);
        }

        [Fact]
        public async Task CancelTechTickets_ShouldThrow_WhenUserWalletIsNull()
        {
            // Arrange
            SetupCommonMocks();
            var ticket = CreateTechTicket("T1", "U1", "C1");
            ticket.User.Wallet = null; // simulate missing wallet

            _mockTicketRepo
                .Setup(r => r.GetNotRefundTechnicalTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Ticket> { ticket });

            _mockConferenceRepo
                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", false))
                .ReturnsAsync(new List<string> { "C1" });

            var request = CreateCancelRequest("T1");

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CancelTechTickets(request, "U1"));
        }

        [Fact]
        public async Task CancelTechTickets_ShouldProcessSuccessfully_WhenAllValid()
        {
            // Arrange
            SetupCommonMocks();
            var ticket1 = CreateTechTicket("T1", "U1", "C1", 100m);
            var ticket2 = CreateTechTicket("T2", "U1", "C1", 200m);

            _mockTicketRepo
                .Setup(r => r.GetNotRefundTechnicalTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Ticket> { ticket1, ticket2 });

            _mockConferenceRepo
                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", false))
                .ReturnsAsync(new List<string> { "C1" });

            // Mock DB responses
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            _mockTicketRepo.Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>()))
                .ReturnsAsync(2); // 2 tickets updated

            _mockWalletTransactionRepo.Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>()))
                .ReturnsAsync(2); // 2 wallet transactions

            _mockTransactionRepo.Setup(r => r.CreateTransactionListAsync(It.IsAny<List<Transaction>>()))
                .ReturnsAsync(2); // 2 refund transactions

            var request = CreateCancelRequest("T1", "T2");

            // Act
            var result = await _service.CancelTechTickets(request, "U1");

            // Assert
            Assert.Equal(6, result); // 2 + 2 + 2

            // Verify wallet balances
            Assert.Equal(100m, ticket1.User.Wallet.Balance);
            Assert.Equal(200m, ticket2.User.Wallet.Balance);

            // Verify tickets marked as refunded
            Assert.True(ticket1.IsRefunded);
            Assert.True(ticket2.IsRefunded);

            // Verify slots increased
            Assert.Equal(4, ticket1.PricePhase.AvailableSlot);        // 3 + 1
            Assert.Equal(6, ticket1.PricePhase.ConferencePrice!.AvailableSlot); // 5 + 1
            Assert.Equal(11, ticket1.PricePhase.ConferencePrice!.Conference!.AvailableSlot); // 10 + 1

            // Same for ticket2
            Assert.Equal(4, ticket2.PricePhase.AvailableSlot);
            Assert.Equal(6, ticket2.PricePhase.ConferencePrice!.AvailableSlot);
            Assert.Equal(11, ticket2.PricePhase.ConferencePrice!.Conference!.AvailableSlot);

            // Verify transaction & wallet transaction created
            _mockWalletTransactionRepo
    .Setup(r => r.CreateWalletTransactionListAsync(It.IsAny<List<WalletTransaction>>()))
    .ReturnsAsync(2);

            _mockTransactionRepo.Verify(
                r => r.CreateTransactionListAsync(It.Is<List<Transaction>>(list =>
                    list.Count == 2 &&
                    list.All(t => t.IsRefunded == true && t.PaymentMethod.MethodName == "Wallet"))),
                Times.Once);

            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelTechTickets_ShouldRollback_OnException()
        {
            // Arrange
            SetupCommonMocks();
            var ticket = CreateTechTicket("T1", "U1", "C1"); // ticket thuộc conference C1

            _mockTicketRepo
                .Setup(r => r.GetNotRefundTechnicalTicketListByTicketIdsForCancel(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Ticket> { ticket });

            // ✅ CRUCIAL: User PHẢI sở hữu conference C1 → để validation PASS
            _mockConferenceRepo
                .Setup(r => r.GetTechnicalConferenceOrResearchConferenceIdsByUserId("U1", false))
                .ReturnsAsync(new List<string> { "C1" }); // <-- "C1" match với ticket.ConferenceId

            // ✅ Gây lỗi ở DB operation (trong try block)
            _mockTicketRepo
                .Setup(r => r.UpdateTicketListAsync(It.IsAny<List<Ticket>>()))
                .ThrowsAsync(new InvalidOperationException("Simulated DB failure"));

            var request = CreateCancelRequest("T1");

            // Act & Assert
            // Giờ đây, lỗi là InvalidOperationException (hoặc bất kỳ Exception nào), KHÔNG phải BadRequestException
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CancelTechTickets(request, "U1"));

            // Verify rollback được gọi
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}