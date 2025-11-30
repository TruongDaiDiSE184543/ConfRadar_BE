using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.UnitTests.Services.ConferenceDiscoveryAndAttendanceService.Discovery.FavouriteTest
{
    public class FavouriteConferenceServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITimeProviderService> _mockTimeProviderService;
        private readonly FavouriteConferenceService _service;

        public FavouriteConferenceServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTimeProviderService = new Mock<ITimeProviderService>();
            _service = new FavouriteConferenceService(_mockUnitOfWork.Object, _mockTimeProviderService.Object);
        }

        // ========================
        // AddFavouriteAsync
        // ========================

        [Fact]
        public async Task AddFavourite_ShouldThrow_WhenConferenceNotFound()
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Conference?)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.AddFavouriteAsync("user1", "conf1"));
        }

        [Fact]
        public async Task AddFavourite_ShouldThrow_WhenAlreadyFavourite()
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference());

            _mockUnitOfWork.Setup(u => u.FavoriteConferenceRepository.GetByUserAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new FavouriteConference());

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.AddFavouriteAsync("user1", "conf1"));
        }

        [Fact]
        public async Task AddFavourite_ShouldAddSuccessfully()
        {
            var now = DateTime.UtcNow;
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference());

            _mockUnitOfWork.Setup(u => u.FavoriteConferenceRepository.GetByUserAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((FavouriteConference?)null);

            _mockTimeProviderService.Setup(t => t.GetVietnamTime()).ReturnsAsync(now);

            _mockUnitOfWork.Setup(u => u.FavoriteConferenceRepository.AddFavouriteAsync(It.IsAny<FavouriteConference>()))
               .ReturnsAsync(1);

            var result = await _service.AddFavouriteAsync("user1", "conf1");

            Assert.True(result.IsAdded);
            Assert.Equal("conf1", result.ConferenceId);
            _mockUnitOfWork.Verify(u => u.FavoriteConferenceRepository.AddFavouriteAsync(
                It.Is<FavouriteConference>(f => f.UserId == "user1" && f.ConferenceId == "conf1" && f.CreatedAt == now)
            ), Times.Once);
        }

        // ========================
        // DeleteFavouriteAsync
        // ========================

        [Fact]
        public async Task DeleteFavourite_ShouldThrow_WhenConferenceNotFound()
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Conference?)null);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.DeleteFavouriteAsync("user1", "conf1"));
        }

        [Fact]
        public async Task DeleteFavourite_ShouldThrow_WhenFavouriteNotFound()
        {
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference());

            _mockUnitOfWork.Setup(u => u.FavoriteConferenceRepository.GetByUserAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((FavouriteConference?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteFavouriteAsync("user1", "conf1"));
        }

        [Fact]
        public async Task DeleteFavourite_ShouldDeleteSuccessfully()
        {
            var favourite = new FavouriteConference();
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Conference());

            _mockUnitOfWork.Setup(u => u.FavoriteConferenceRepository.GetByUserAndConferenceIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(favourite);

            _mockUnitOfWork.Setup(u => u.FavoriteConferenceRepository.DeleteFavouriteAsync(favourite))
                .ReturnsAsync(true);

            var result = await _service.DeleteFavouriteAsync("user1", "conf1");

            Assert.True(result.IsDeleted);
            Assert.Equal("conf1", result.ConferenceId);
            _mockUnitOfWork.Verify(u => u.FavoriteConferenceRepository.DeleteFavouriteAsync(favourite), Times.Once);
        }
    }

}
