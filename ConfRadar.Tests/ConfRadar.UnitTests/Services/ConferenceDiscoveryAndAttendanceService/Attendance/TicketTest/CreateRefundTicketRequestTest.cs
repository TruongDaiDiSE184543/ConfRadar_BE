using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.Ticket;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.TicketTest
{

    public class CreateRefundTicketRequestTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IGlobalStatusRepository> _mockGlobalStatusRepo;
        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo;
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IRefundRequestRepository> _mockRefundRequestRepo;
        private readonly Mock<IWalletRepository> _mockWalletRepo;
        private readonly Mock<IWalletTransactionRepository> _mockWalletTransactionRepo;
        private readonly Mock<IPricePhaseRepository> _mockPricePhaseRepo;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly Mock<IPaperRepository> _mockPaperRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;

        private readonly TicketService _service;


        public CreateRefundTicketRequestTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockGlobalStatusRepo = new Mock<IGlobalStatusRepository>();
            _mockPaymentMethodRepo = new Mock<IPaymentMethodRepository>();
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockRefundRequestRepo = new Mock<IRefundRequestRepository>();
            _mockWalletRepo = new Mock<IWalletRepository>();
            _mockWalletTransactionRepo = new Mock<IWalletTransactionRepository>();
            _mockPricePhaseRepo = new Mock<IPricePhaseRepository>();
            _mockPaperRepo = new Mock<IPaperRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();
            _mockTimeProvider = new Mock<ITimeProviderService>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();
            // Wire up UnitOfWork to return repo mocks
            _mockUnitOfWork.Setup(u => u.GlobalStatusRepository).Returns(_mockGlobalStatusRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaymentMethodRepository).Returns(_mockPaymentMethodRepo.Object);
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);
            _mockUnitOfWork.Setup(u => u.RefundRequestRepository).Returns(_mockRefundRequestRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletRepository).Returns(_mockWalletRepo.Object);
            _mockUnitOfWork.Setup(u => u.WalletTransactionRepository).Returns(_mockWalletTransactionRepo.Object);
            _mockUnitOfWork.Setup(u => u.PricePhaseRepository).Returns(_mockPricePhaseRepo.Object);
            _mockUnitOfWork.Setup(u => u.PaperRepository).Returns(_mockPaperRepo.Object);
            _mockUnitOfWork.Setup(u => u.TransactionRepository).Returns(_mockTransactionRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
            // The service under test
            _service = new TicketService(_mockUnitOfWork.Object, _mockTimeProvider.Object);
        }
        private RefundTicketRequest CreateRequest(string ticketId = "T1", string transactionId = "TX1")
        {
            return new RefundTicketRequest
            {
                TicketId = ticketId,
                TransactionId = transactionId,

            };
        }

        private GlobalStatus CreateGlobalStatus(string id = "G1", string name = "Pending")
        {
            return new GlobalStatus
            {
                GlobalStatusId = id,
                Name = name
            };
        }

        private PaymentMethod CreatePaymentMethod(string id = "PM_WALLET", string name = "Wallet")
        {
            return new PaymentMethod
            {
                PaymentMethodId = id,
                MethodName = name
            };
        }

        private Ticket CreateBasicTicket(string userId = "U1", string ticketId = "T1", decimal txAmount = 100m, bool isAuthor = false)
        {
            var pricePhase = new PricePhase
            {
                PricePhaseId = "PP1",
                RefundPolicies = new List<RefundPolicy>(),
                ConferencePrice = new ConferencePrice
                {
                    ConferencePriceId = "CP1",
                    IsAuthor = isAuthor,
                    Conference = new Conference
                    {
                        ConferenceId = "C1",
                        AvailableSlot = 10
                    },
                    AvailableSlot = 5
                }
            };

            var tx = new Transaction
            {
                TransactionId = "TX1",
                Amount = txAmount,
                IsRefunded = false,
                TicketId = ticketId,
                PaymentMethodId = "PM1",
                CreatedAt = DateTime.Now
            };

            return new Ticket
            {
                TicketId = ticketId,
                UserId = userId,
                IsRefunded = false,
                PricePhaseId = pricePhase.PricePhaseId,
                PricePhase = pricePhase,
                Transactions = new List<Transaction> { tx }
            };
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenStatusesOrPaymentMethodMissing()
        {
            // Arrange: make global statuses or payment method null
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>())).ReturnsAsync((GlobalStatus)null);
            _mockPaymentMethodRepo.Setup(p => p.GetPaymentMethodByName(It.IsAny<string>())).ReturnsAsync((PaymentMethod)null);

            var req = CreateRequest();
            // Act + Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateRefundTicketRequest(req, "U1"));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenTicketNotFound()
        {
            // Arrange
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(CreateGlobalStatus());
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(CreateGlobalStatus("A", "Accepted"));
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                .ReturnsAsync(CreateGlobalStatus("R", "Rejected"));

            _mockPaymentMethodRepo.Setup(p => p.GetPaymentMethodByName(It.IsAny<string>()))
                .ReturnsAsync(CreatePaymentMethod());

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Ticket)null);

            var req = CreateRequest();

            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateRefundTicketRequest(req, "U1"));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenTicketAlreadyRefunded()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            ticket.IsRefunded = true;

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            var req = CreateRequest(ticket.TicketId, "TX1");

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenMultipleTransactions()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            // add another transaction to make count > 1
            ticket.Transactions.Add(new Transaction { TransactionId = "TX2", Amount = 50m });

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            var req = CreateRequest(ticket.TicketId, "TX1");

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenTransactionNotFound()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            // transaction id differs
            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            var req = CreateRequest(ticket.TicketId, "NON_EXIST");

            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenPricePhaseNull()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            ticket.PricePhase = null; // missing pricePhase

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            var req = CreateRequest(ticket.TicketId, "TX1");

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenRefundRequestAlreadyExists()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            // simulate existing refund request
            _mockRefundRequestRepo.Setup(r => r.GetRefundRequestByTicketIdAsync(ticket.TicketId))
                .ReturnsAsync(new RefundRequest { RefundRequestId = "RR1", TicketId = ticket.TicketId, CreatedAt = DateTime.Now });

            var req = CreateRequest(ticket.TicketId, "TX1");

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenNoRefundPolicies()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            // ensure PricePhase exists but RefundPolicies empty
            ticket.PricePhase.RefundPolicies = new List<RefundPolicy>();

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            _mockRefundRequestRepo.Setup(r => r.GetRefundRequestByTicketIdAsync(ticket.TicketId))
                .ReturnsAsync((RefundRequest)null);

            var req = CreateRequest(ticket.TicketId, "TX1");

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenNoValidRefundPolicy()
        {
            // Arrange
            var ticket = CreateBasicTicket();

            ticket.PricePhase.RefundPolicies = new List<RefundPolicy>
    {
        new RefundPolicy
        {
            RefundPolicyId = "RP1",
            PercentRefund = 50,
            RefundDeadline = new DateOnly(2000,1,1) // expired
        }
    };

            SetupGlobalAndPaymentMethodMocks();

            _mockWalletRepo
                .Setup(x => x.GetWalletByUserIdAsync(ticket.UserId))
                .ReturnsAsync(new Wallet { WalletId = "wallet-1", UserId = ticket.UserId, Balance = 1000 });

            _mockTicketRepo
                .Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            _mockRefundRequestRepo
                .Setup(r => r.GetRefundRequestByTicketIdAsync(ticket.TicketId))
                .ReturnsAsync((RefundRequest)null);

            _mockPricePhaseRepo
                .Setup(p => p.GetPricePhaseByPricePhaseId(ticket.PricePhaseId))
                .ReturnsAsync(ticket.PricePhase);

            // mock TimeProviderService để dateNow là ngày hôm nay
            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));
            _mockTimeProvider.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.Now);

            // mock conference nếu cần
            if (ticket.PricePhase.ConferencePrice?.Conference != null)
            {
                _mockConferenceRepo
                    .Setup(c => c.GetConferenceByIdAsync(ticket.PricePhase.ConferencePrice.ConferenceId))
                    .ReturnsAsync(ticket.PricePhase.ConferencePrice.Conference);
            }

            var req = CreateRequest(ticket.TicketId, "TX1");

            // Act + Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateRefundTicketRequest(req, ticket.UserId)
            );

            Assert.Equal("Tất cả các vé hoàn tiền đã quá hạn", ex.Message);
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenAuthorTicketAndAbstractReviewed()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket(isAuthor: true);
            // add a valid policy
            ticket.PricePhase.RefundPolicies = new List<RefundPolicy>
            {
                new RefundPolicy { RefundPolicyId = "RP1", PercentRefund = 50, RefundDeadline = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) }
            };

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            // Simulate paper exists and its abstract GlobalStatus is not Pending
            var paper = new Paper
            {
                PaperId = "P1",
                Abstract = new Abstract { AbstractId = "A1", GlobalStatus = new GlobalStatus { GlobalStatusId = "GS_NOT_PENDING" } }
            };

            _mockPaperRepo.Setup(p => p.GetPaperByUserAndConference(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(paper);

            _mockRefundRequestRepo.Setup(r => r.GetRefundRequestByTicketIdAsync(ticket.TicketId)).ReturnsAsync((RefundRequest)null);

            var req = CreateRequest(ticket.TicketId, "TX1");

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldThrow_WhenWalletNotFound()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            ticket.PricePhase.RefundPolicies = new List<RefundPolicy>
            {
                new RefundPolicy { RefundPolicyId = "RP1", PercentRefund = 50, RefundDeadline = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) }
            };

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            _mockRefundRequestRepo.Setup(r => r.GetRefundRequestByTicketIdAsync(ticket.TicketId)).ReturnsAsync((RefundRequest)null);

            // wallet not found
            _mockWalletRepo.Setup(w => w.GetWalletByUserIdAsync(ticket.UserId)).ReturnsAsync((Wallet)null);

            var req = CreateRequest(ticket.TicketId, "TX1");

            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateRefundTicketRequest(req, ticket.UserId));
        }

        [Fact]
        public async Task CreateRefundTicketRequest_ShouldReturnResult_WhenAllValid()
        {
            // Arrange
            SetupGlobalAndPaymentMethodMocks();

            var ticket = CreateBasicTicket();
            ticket.PricePhase.RefundPolicies = new List<RefundPolicy>
            {
                new RefundPolicy { RefundPolicyId = "RP1", PercentRefund = 50, RefundDeadline = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) }
            };

            _mockTicketRepo.Setup(t => t.GetTicketByTicketIdAndUserId(ticket.TicketId, ticket.UserId))
                .ReturnsAsync(ticket);

            _mockRefundRequestRepo.Setup(r => r.GetRefundRequestByTicketIdAsync(ticket.TicketId)).ReturnsAsync((RefundRequest)null);

            var userWallet = new Wallet { WalletId = "W1", UserId = ticket.UserId!, Balance = 0m };
            _mockWalletRepo.Setup(w => w.GetWalletByUserIdAsync(ticket.UserId)).ReturnsAsync(userWallet);

            // pricePhase repo returns same pricePhase when queried by id
            _mockPricePhaseRepo.Setup(p => p.GetPricePhaseByPricePhaseId(ticket.PricePhaseId!)).ReturnsAsync(ticket.PricePhase);

            // time provider
            _mockTimeProvider.Setup(t => t.GetVietnamDate()).ReturnsAsync(DateOnly.FromDateTime(DateTime.Now));
            _mockTimeProvider.Setup(t => t.GetVietnamTime()).ReturnsAsync(DateTime.Now);

            // Begin/Commit
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Make repo update/create calls return 1 each
            _mockWalletRepo.Setup(w => w.UpdateWalletAsync(It.IsAny<Wallet>())).ReturnsAsync(1);
            _mockWalletTransactionRepo.Setup(w => w.CreateWalletTransactionAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(1);
            _mockPricePhaseRepo.Setup(p => p.UpdatePricePhaseAsync(It.IsAny<PricePhase>())).ReturnsAsync(1);
            _mockTransactionRepo.Setup(t => t.CreateTransactionAsync(It.IsAny<Transaction>())).ReturnsAsync(1);
            _mockTicketRepo.Setup(t => t.UpdateTicketAsync(It.IsAny<Ticket>())).ReturnsAsync(1);
            _mockRefundRequestRepo.Setup(r => r.CreateRefundRequestAsync(It.IsAny<RefundRequest>())).ReturnsAsync(1);

            var req = CreateRequest(ticket.TicketId, "TX1");

            // Act
            var result = await _service.CreateRefundTicketRequest(req, ticket.UserId!);

            // Assert
            // We set 6 repository calls that return 1: update wallet, create wallet transaction, update pricePhase,
            // create transaction, update ticket, create refund request => result should be 6
            Assert.Equal(6, result);

            // wallet balance must have increased by 50% of transaction amount (transaction amount 100)
            Assert.Equal(50m, userWallet.Balance);

            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        // ---------- Helper ----------
        private void SetupGlobalAndPaymentMethodMocks()
        {
            // Pending / Accepted / Rejected global statuses
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "GS_PENDING", Name = GlobalStatusEnum.Pending.GetDescription() });
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "GS_ACCEPTED", Name = GlobalStatusEnum.Accepted.GetDescription() });
            _mockGlobalStatusRepo.Setup(g => g.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "GS_REJECTED", Name = GlobalStatusEnum.Rejected.GetDescription() });

            // Wallet payment method
            _mockPaymentMethodRepo.Setup(p => p.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription()))
                .ReturnsAsync(new PaymentMethod { PaymentMethodId = "PM_WALLET", MethodName = PaymentMethodEnum.Wallet.GetDescription() });
        }
    }





}



