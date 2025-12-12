using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.DTOs.ConferenceCategory;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Moq;

namespace ConfRadar.UnitTests.Services.Maintenance.Category
{
    public class ConferenceCategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceCategoryRepository> _mockCategoryRepo;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly ConferenceCategoryService _categoryService;

        public ConferenceCategoryServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepo = new Mock<IConferenceCategoryRepository>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();

            _mockUnitOfWork.Setup(u => u.ConferenceCategoryRepository).Returns(_mockCategoryRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);

            _categoryService = new ConferenceCategoryService(_mockUnitOfWork.Object);
        }

        #region Create Tests

        [Fact]
        public async Task CreateConferenceCategory_Should_Succeed_When_NameIsUnique()
        {
            // ARRANGE
            var request = new CreateConferenceCategoryRequest { ConferenceCategoryName = "New Tech" };

            // Mock tên chưa tồn tại
            _mockCategoryRepo.Setup(r => r.GetCategoryByCategoryName("New Tech")).ReturnsAsync((ConferenceCategory)null);
            _mockCategoryRepo.Setup(r => r.CreateConferenceCategoryAsync(It.IsAny<ConferenceCategory>())).ReturnsAsync(1);

            // ACT
            var result = await _categoryService.CreateConferenceCategoryAsync(request);

            // ASSERT
            result.Should().NotBeNull();
            result.ConferenceCategoryName.Should().Be("New Tech");
            _mockCategoryRepo.Verify(r => r.CreateConferenceCategoryAsync(It.IsAny<ConferenceCategory>()), Times.Once);
        }

        [Fact]
        public async Task CreateConferenceCategory_Should_Throw_When_NameExists()
        {
            var request = new CreateConferenceCategoryRequest { ConferenceCategoryName = "Existing Tech" };

            _mockCategoryRepo.Setup(r => r.GetCategoryByCategoryName("Existing Tech"))
                .ReturnsAsync(new ConferenceCategory());

            await Assert.ThrowsAsync<BadRequestException>(() => _categoryService.CreateConferenceCategoryAsync(request));
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateConferenceCategory_Should_Succeed_When_NameIsUnique()
        {
            var request = new UpdateConferenceCategoryRequest { ConferenceCategoryName = "Updated Name" };
            var category = new ConferenceCategory { ConferenceCategoryId = "cat1", ConferenceCategoryName = "Old Name" };

            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("cat1")).ReturnsAsync(category);
            _mockCategoryRepo.Setup(r => r.GetCategoryByCategoryName("Updated Name")).ReturnsAsync((ConferenceCategory)null);
            _mockCategoryRepo.Setup(r => r.UpdateConferenceCategoryAsync(It.IsAny<ConferenceCategory>())).ReturnsAsync(1);

            var result = await _categoryService.UpdateConferenceCategoryAsync("cat1", request);

            result.ConferenceCategoryName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateConferenceCategory_Should_Throw_When_NameExistsInAnotherCategory()
        {
            var request = new UpdateConferenceCategoryRequest { ConferenceCategoryName = "Existing Name" };
            var categoryToUpdate = new ConferenceCategory { ConferenceCategoryId = "cat1", ConferenceCategoryName = "Old Name" };
            var otherCategory = new ConferenceCategory { ConferenceCategoryId = "cat2", ConferenceCategoryName = "Existing Name" };

            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("cat1")).ReturnsAsync(categoryToUpdate);
            _mockCategoryRepo.Setup(r => r.GetCategoryByCategoryName("Existing Name")).ReturnsAsync(otherCategory);

            await Assert.ThrowsAsync<BadRequestException>(() => _categoryService.UpdateConferenceCategoryAsync("cat1", request));
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteConferenceCategory_Should_Succeed_When_NotUsedByConferences()
        {
            var category = new ConferenceCategory { ConferenceCategoryId = "cat1" };

            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("cat1")).ReturnsAsync(category);
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync()).ReturnsAsync(new List<Conference>()); // Không có conference nào
            _mockCategoryRepo.Setup(r => r.DeleteConferenceCategoryAsync(category)).ReturnsAsync(1);

            var result = await _categoryService.DeleteConferenceCategoryAsync("cat1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteConferenceCategory_Should_Throw_When_UsedByConferences()
        {
            var category = new ConferenceCategory { ConferenceCategoryId = "cat1", ConferenceCategoryName = "Tech" };

            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("cat1")).ReturnsAsync(category);
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync())
                .ReturnsAsync(new List<Conference> { new Conference { ConferenceCategoryId = "cat1" } }); // Có 1 conference dùng

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _categoryService.DeleteConferenceCategoryAsync("cat1"));
            ex.Message.Should().Contain("Cannot delete conference category 'Tech'");
        }

        #endregion

        #region Get All Tests

        [Fact]
        public async Task GetAllConferenceCategories_Should_ReturnCorrectConferenceCounts()
        {
            // ARRANGE
            var categories = new List<ConferenceCategory>
            {
                new ConferenceCategory { ConferenceCategoryId = "cat1", ConferenceCategoryName = "Tech" },
                new ConferenceCategory { ConferenceCategoryId = "cat2", ConferenceCategoryName = "Science" },
                new ConferenceCategory { ConferenceCategoryId = "cat3", ConferenceCategoryName = "Art" }
            };

            var conferences = new List<Conference>
            {
                new Conference { ConferenceCategoryId = "cat1" },
                new Conference { ConferenceCategoryId = "cat1" }, // Tech có 2
                new Conference { ConferenceCategoryId = "cat2" }, // Science có 1
                new Conference { ConferenceCategoryId = null } // Không thuộc category nào
            };

            _mockCategoryRepo.Setup(r => r.GetAllConferenceCategoriesAsync()).ReturnsAsync(categories);
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync()).ReturnsAsync(conferences);

            // ACT
            var result = await _categoryService.GetAllConferenceCategoriesAsync();

            // ASSERT
            result.Should().HaveCount(3);
            result.First(c => c.ConferenceCategoryId == "cat1").ConferenceCount.Should().Be(2);
            result.First(c => c.ConferenceCategoryId == "cat2").ConferenceCount.Should().Be(1);
            result.First(c => c.ConferenceCategoryId == "cat3").ConferenceCount.Should().Be(0); // Art không có
        }

        #endregion
    }
}

