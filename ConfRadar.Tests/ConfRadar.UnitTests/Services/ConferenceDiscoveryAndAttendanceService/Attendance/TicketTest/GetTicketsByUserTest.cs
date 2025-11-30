using ConfRadar.Repositories;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.Ticket;
using Moq;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Attendance.TicketTest
{
    public class GetTicketsByUserTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;

        private readonly TicketService _service;

        public GetTicketsByUserTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockTimeProvider = new Mock<ITimeProviderService>();

            // Map TicketRepository
            _mockUnitOfWork.Setup(u => u.TicketRepository).Returns(_mockTicketRepo.Object);

            _service = new TicketService(_mockUnitOfWork.Object, _mockTimeProvider.Object);
        }

        [Fact]
        public async Task GetTicketsByUserId_ShouldReturnRepositoryResult()
        {
            // Arrange
            string userId = "user123";
            string keyword = null;
            int page = 1, size = 10;

            var fakeResult = new PagedResultResponseDto<CustomerPaidTicketResponse>
            {
                Page = 1,
                PageSize = 10,
                TotalCount = 1,
                Items = new List<CustomerPaidTicketResponse>
            {
                new CustomerPaidTicketResponse { TicketId = "T1" }
            }
            };

            _mockTicketRepo
                .Setup(r => r.GetTicketsByUserId(userId, keyword, page, size, null, null))
                .ReturnsAsync(fakeResult);

            // Act
            var result = await _service.GetTicketsByUserId(userId, keyword, page, size);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal("T1", result.Items[0].TicketId);

            _mockTicketRepo.Verify(r =>
                r.GetTicketsByUserId(userId, keyword, page, size, null, null),
                Times.Once);
        }
    }
}