using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Moq;

namespace ConfRadar.UnitTests.Services.Maintenance.Category
{
    public class DeleteConferenceCategoryTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceCategoryRepository> _mockCategoryRepo;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly ConferenceCategoryService _categoryService;

        public DeleteConferenceCategoryTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepo = new Mock<IConferenceCategoryRepository>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();
            _mockUnitOfWork.Setup(u => u.ConferenceCategoryRepository).Returns(_mockCategoryRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
            _categoryService = new ConferenceCategoryService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task DeleteConferenceCategory_Should_Succeed_When_CategoryIsNotUsed()
        {
            // ARRANGE
            var category = new ConferenceCategory { ConferenceCategoryId = "cat1" };
            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("cat1")).ReturnsAsync(category);
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync()).ReturnsAsync(new List<Conference>());
            _mockCategoryRepo.Setup(r => r.DeleteConferenceCategoryAsync(category)).ReturnsAsync(1);

            // ACT
            var result = await _categoryService.DeleteConferenceCategoryAsync("cat1");

            // ASSERT
            result.Should().BeTrue();
            _mockCategoryRepo.Verify(r => r.DeleteConferenceCategoryAsync(category), Times.Once);
        }

        [Fact]
        public async Task DeleteConferenceCategory_Should_ThrowBadRequest_When_CategoryIsUsed()
        {
            // ARRANGE
            var category = new ConferenceCategory { ConferenceCategoryId = "cat1", ConferenceCategoryName = "Tech" };
            var conferences = new List<Conference> { new Conference { ConferenceCategoryId = "cat1" } };
            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("cat1")).ReturnsAsync(category);
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync()).ReturnsAsync(conferences);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _categoryService.DeleteConferenceCategoryAsync("cat1"));
            ex.Message.Should().Contain("Cannot delete conference category 'Tech'");
        }

        [Fact]
        public async Task DeleteConferenceCategory_Should_ThrowNotFound_When_CategoryDoesNotExist()
        {
            // ARRANGE
            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("not-found")).ReturnsAsync((ConferenceCategory)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() => _categoryService.DeleteConferenceCategoryAsync("not-found"));
        }
    }
}
