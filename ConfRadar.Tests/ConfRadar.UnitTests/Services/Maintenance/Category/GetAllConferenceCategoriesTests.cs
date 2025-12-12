using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Services;
using FluentAssertions;
using Moq;

namespace ConfRadar.UnitTests.Services.Maintenance.Category
{
    public class GetAllConferenceCategoriesTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceCategoryRepository> _mockCategoryRepo;
        private readonly Mock<IConferenceRepository> _mockConferenceRepo;
        private readonly ConferenceCategoryService _categoryService;

        public GetAllConferenceCategoriesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepo = new Mock<IConferenceCategoryRepository>();
            _mockConferenceRepo = new Mock<IConferenceRepository>();
            _mockUnitOfWork.Setup(u => u.ConferenceCategoryRepository).Returns(_mockCategoryRepo.Object);
            _mockUnitOfWork.Setup(u => u.ConferenceRepository).Returns(_mockConferenceRepo.Object);
            _categoryService = new ConferenceCategoryService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllConferenceCategories_Should_ReturnListWithCorrectConferenceCounts()
        {
            // ARRANGE
            var categories = new List<ConferenceCategory>
            {
                new ConferenceCategory { ConferenceCategoryId = "cat1", ConferenceCategoryName = "Tech" },
                new ConferenceCategory { ConferenceCategoryId = "cat2", ConferenceCategoryName = "Science" },
                new ConferenceCategory { ConferenceCategoryId = "cat3", ConferenceCategoryName = "Art" } // No conferences
            };
            var conferences = new List<Conference>
            {
                new Conference { ConferenceCategoryId = "cat1" },
                new Conference { ConferenceCategoryId = "cat1" }, // Tech has 2
                new Conference { ConferenceCategoryId = "cat2" }, // Science has 1
                new Conference { ConferenceCategoryId = "cat-other" } // Belongs to another category
            };

            _mockCategoryRepo.Setup(r => r.GetAllConferenceCategoriesAsync()).ReturnsAsync(categories);
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync()).ReturnsAsync(conferences);

            // ACT
            var result = await _categoryService.GetAllConferenceCategoriesAsync();

            // ASSERT
            result.Should().HaveCount(3);
            result.First(c => c.ConferenceCategoryId == "cat1").ConferenceCount.Should().Be(2);
            result.First(c => c.ConferenceCategoryId == "cat2").ConferenceCount.Should().Be(1);
            result.First(c => c.ConferenceCategoryId == "cat3").ConferenceCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllConferenceCategories_Should_ReturnZeroCounts_When_NoConferencesExist()
        {
            var categories = new List<ConferenceCategory> { new ConferenceCategory { ConferenceCategoryId = "cat1" } };
            _mockCategoryRepo.Setup(r => r.GetAllConferenceCategoriesAsync()).ReturnsAsync(categories);
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync()).ReturnsAsync(new List<Conference>()); // Empty conference list

            var result = await _categoryService.GetAllConferenceCategoriesAsync();

            result.Should().HaveCount(1);
            result[0].ConferenceCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllConferenceCategories_Should_ReturnEmptyList_When_NoCategoriesExist()
        {
            _mockCategoryRepo.Setup(r => r.GetAllConferenceCategoriesAsync()).ReturnsAsync(new List<ConferenceCategory>());
            _mockConferenceRepo.Setup(r => r.GetAllConferencesAsync()).ReturnsAsync(new List<Conference>());

            var result = await _categoryService.GetAllConferenceCategoriesAsync();

            result.Should().BeEmpty();
        }
    }
}
