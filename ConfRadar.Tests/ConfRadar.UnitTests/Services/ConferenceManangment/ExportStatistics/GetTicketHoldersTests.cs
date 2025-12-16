//using ConfRadar.Repositories.Models;
//using ConfRadar.Repositories.Repositories;
//using ConfRadar.Repositories;
//using ConfRadar.Services.Common;
//using ConfRadar.Services.DTOs.Statistics;
//using ConfRadar.Services.Services;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ConfRadar.UnitTests.Services.ConferenceManangment.ExportStatistics
//{
//    public class GetTicketHoldersTests
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<ITicketRepository> _mockTicketRepo;
//        private readonly TicketService _ticketService; // Giả sử hàm này nằm trong TicketService

//        public GetTicketHoldersTests()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockTicketRepo = new Mock<ITicketRepository>();

//            // Gắn mock TicketRepo vào UnitOfWork
//            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);



//            var mockTime = new Mock<ITimeProviderService>();
//            var mockMomo = new Mock<IMomoService>();


//            // Khởi tạo TicketService
//            _ticketService = new TicketService(
//                _mockUnitOfWork.Object,
//                mockTime.Object, 
//                mockMomo.Object  

//            );
//        }

//        #region Helper: Generate Mock Data
//        private List<Ticket> GenerateMockData()
//        {
//            var checkedInStatus = new CheckinStatus { CheckinStatusName = CheckInStatusEnum.CheckedIn.GetDescription() };
//            var pendingStatus = new CheckinStatus { CheckinStatusName = CheckInStatusEnum.Pending.GetDescription() };

//            return new List<Ticket>
//            {
//                // Ticket 1: Alice, Standard, đã check-in
//                new Ticket
//                {
//                    TicketId = "TICKET-001",
//                    IsRefunded = false,
//                    RegisteredDate = new DateOnly(2023, 10, 1),
//                    User = new User { FullName = "Alice", Email = "alice@example.com" },
//                    PricePhase = new PricePhase { ConferencePrice = new PricePhase { TicketName = "Standard" } },
//                    UserCheckIns = new List<UserCheckIn>
//                    {
//                        new UserCheckIn { CheckinStatus = checkedInStatus }
//                    }
//                },
//                // Ticket 2: Bob, VIP, chưa check-in
//                new Ticket
//                {
//                    TicketId = "TICKET-002",
//                    IsRefunded = false,
//                    RegisteredDate = new DateOnly(2023, 10, 5),
//                    User = new User { FullName = "Bob", Email = "bob@example.com" },
//                    PricePhase = new PricePhase { ConferencePrice = new PricePhase { TicketName = "VIP" } },
//                    UserCheckIns = new List<UserCheckIn>
//                    {
//                        new UserCheckIn { CheckinStatus = pendingStatus }
//                    }
//                },
//                // Ticket 3: Charlie, Standard, đã refund
//                new Ticket
//                {
//                    TicketId = "TICKET-003",
//                    IsRefunded = true, // Đã refund
//                    RegisteredDate = new DateOnly(2023, 10, 10),
//                    User = new User { FullName = "Charlie", Email = "charlie@example.com" },
//                    PricePhase = new PricePhase { ConferencePrice = new PricePhase { TicketName = "Standard" } },
//                    UserCheckIns = new List<UserCheckIn>()
//                }
//            };
//        }
//        #endregion

//        [Fact]
//        public async Task GetTicketHolders_Should_ReturnPagedResult_WithoutFilters()
//        {
//            // ARRANGE
//            var mockData = GenerateMockData();
//            var request = new TicketHolderSearchParam { ConferenceId = "conf-1", Page = 1, PageSize = 2 };

//            // Setup repo trả về IQueryable từ List
//            _mockTicketRepo.Setup(r => r.GetTicketHolderInfo(request.ConferenceId))
//                .Returns(mockData.AsQueryable());

//            // ACT
//            var result = await _ticketService.GetTicketHoldersByConferenceIdAsync(request);

//            // ASSERT
//            result.Should().NotBeNull();
//            result.TotalCount.Should().Be(3);
//            result.Items.Should().HaveCount(2);
//            // Mặc định sắp xếp theo ngày mới nhất -> Charlie, Bob
//            result.Items[0].CustomerName.Should().Be("Charlie");
//            result.Items[1].CustomerName.Should().Be("Bob");
//        }

//        [Fact]
//        public async Task GetTicketHolders_Should_FilterBySearchKeyword_CaseInsensitive()
//        {
//            // ARRANGE
//            var mockData = GenerateMockData();
//            // Tìm kiếm "bob" (chữ thường)
//            var request = new TicketHolderSearchParam { ConferenceId = "conf-1", SearchKeyword = "bob" };

//            _mockTicketRepo.Setup(r => r.GetTicketHolderInfo(request.ConferenceId))
//                .Returns(mockData.AsQueryable());

//            // ACT
//            var result = await _ticketService.GetTicketHoldersByConferenceIdAsync(request);

//            // ASSERT
//            result.TotalCount.Should().Be(1);
//            result.Items.Should().HaveCount(1);
//            result.Items[0].CustomerName.Should().Be("Bob");
//        }

//        [Fact]
//        public async Task GetTicketHolders_Should_FilterByDateRange()
//        {
//            // ARRANGE
//            var mockData = GenerateMockData();
//            // Tìm từ 04/10 đến 09/10 -> Chỉ có Bob
//            var request = new TicketHolderSearchParam
//            {
//                ConferenceId = "conf-1",
//                FromDate = new DateOnly(2023, 10, 4),
//                ToDate = new DateOnly(2023, 10, 9)
//            };

//            _mockTicketRepo.Setup(r => r.GetTicketHolderInfo(request.ConferenceId))
//                .Returns(mockData.AsQueryable());

//            // ACT
//            var result = await _ticketService.GetTicketHoldersByConferenceIdAsync(request);

//            // ASSERT
//            result.TotalCount.Should().Be(1);
//            result.Items[0].CustomerName.Should().Be("Bob");
//        }

//        [Fact]
//        public async Task GetTicketHolders_Should_FilterByIsRefunded()
//        {
//            // ARRANGE
//            var mockData = GenerateMockData();
//            var request = new TicketHolderSearchParam { ConferenceId = "conf-1", IsRefunded = true };

//            _mockTicketRepo.Setup(r => r.GetTicketHolderInfo(request.ConferenceId))
//                .Returns(mockData.AsQueryable());

//            // ACT
//            var result = await _ticketService.GetTicketHoldersByConferenceIdAsync(request);

//            // ASSERT
//            result.TotalCount.Should().Be(1);
//            result.Items[0].CustomerName.Should().Be("Charlie");
//        }

//        [Fact]
//        public async Task GetTicketHolders_Should_FilterByTicketType()
//        {
//            // ARRANGE
//            var mockData = GenerateMockData();
//            var request = new TicketHolderSearchParam { ConferenceId = "conf-1", TicketType = "Standard" };

//            _mockTicketRepo.Setup(r => r.GetTicketHolderInfo(request.ConferenceId))
//                .Returns(mockData.AsQueryable());

//            // ACT
//            var result = await _ticketService.GetTicketHoldersByConferenceIdAsync(request);

//            // ASSERT
//            result.TotalCount.Should().Be(2); // Alice và Charlie
//            result.Items.Select(i => i.CustomerName).Should().Contain("Alice");
//            result.Items.Select(i => i.CustomerName).Should().Contain("Charlie");
//        }

//        [Fact]
//        public async Task GetTicketHolders_Should_FilterByCheckInStatus_CheckedIn()
//        {
//            // ARRANGE
//            var mockData = GenerateMockData();
//            var request = new TicketHolderSearchParam { ConferenceId = "conf-1", CheckInStatus = CheckInStatusEnum.CheckedIn.GetDescription() };

//            _mockTicketRepo.Setup(r => r.GetTicketHolderInfo(request.ConferenceId))
//                .Returns(mockData.AsQueryable());

//            // ACT
//            var result = await _ticketService.GetTicketHoldersByConferenceIdAsync(request);

//            // ASSERT
//            result.TotalCount.Should().Be(1);
//            result.Items[0].CustomerName.Should().Be("Alice");
//        }
//    }
//}
