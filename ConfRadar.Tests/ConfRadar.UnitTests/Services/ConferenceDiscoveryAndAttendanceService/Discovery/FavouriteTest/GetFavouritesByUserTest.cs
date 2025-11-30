using ConfRadar.Repositories;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Services;
using ConfRadar.Shared.DTO.FavouriteConference;
using Moq;
namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.FavouriteTest
{
    public class GetFavouritesByUserTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IFavouriteConferenceRepository> _mockFavouriteRepo;
        private readonly Mock<ITimeProviderService> _mockTimeProvider;

        private readonly FavouriteConferenceService _service;

        public GetFavouritesByUserTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockFavouriteRepo = new Mock<IFavouriteConferenceRepository>();
            _mockTimeProvider = new Mock<ITimeProviderService>();

            // Map repository
            _mockUnitOfWork.Setup(u => u.FavoriteConferenceRepository)
                           .Returns(_mockFavouriteRepo.Object);

            _service = new FavouriteConferenceService(_mockUnitOfWork.Object, _mockTimeProvider.Object);
        }

        [Fact]
        public async Task GetFavouritesByUserIdAsync_ShouldReturnListFromRepository()
        {
            // Arrange
            string userId = "user123";
            var fakeFavourites = new List<FavouriteConferenceDetailResponse>
        {
            new FavouriteConferenceDetailResponse
            {
                ConferenceId = "C1",
                ConferenceName = "Conf A",
                FavouriteCreatedAt = DateTime.UtcNow
            }
        };

            _mockFavouriteRepo
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(fakeFavourites);

            // Act
            var result = await _service.GetFavouritesByUserIdAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("C1", result[0].ConferenceId);
            Assert.Equal("Conf A", result[0].ConferenceName);

            _mockFavouriteRepo.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        }
    }
}