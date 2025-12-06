using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Repositories;
using ConfRadar.Services.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.UnitTests.Services.Maintenance.Category
{
    public class GetConferenceCategoryByIdTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceCategoryRepository> _mockCategoryRepo;
        private readonly ConferenceCategoryService _categoryService;

        public GetConferenceCategoryByIdTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepo = new Mock<IConferenceCategoryRepository>();
            _mockUnitOfWork.Setup(u => u.ConferenceCategoryRepository).Returns(_mockCategoryRepo.Object);
            _categoryService = new ConferenceCategoryService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetConferenceCategoryById_Should_ReturnCategory_When_ItExists()
        {
            // ARRANGE
            var categoryId = "cat-tech";
            var category = new ConferenceCategory
            {
                ConferenceCategoryId = categoryId,
                ConferenceCategoryName = "Technology"
            };
            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync(categoryId)).ReturnsAsync(category);

            // ACT
            var result = await _categoryService.GetConferenceCategoryByIdAsync(categoryId);

            // ASSERT
            result.Should().NotBeNull();
            result.ConferenceCategoryId.Should().Be(categoryId);
            result.ConferenceCategoryName.Should().Be("Technology");
        }

        [Fact]
        public async Task GetConferenceCategoryById_Should_ThrowNotFoundException_When_ItDoesNotExist()
        {
            // ARRANGE
            var categoryId = "cat-not-found";
            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync(categoryId)).ReturnsAsync((ConferenceCategory)null);

            // ACT & ASSERT
            var action = async () => await _categoryService.GetConferenceCategoryByIdAsync(categoryId);
            await action.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Conference category with ID {categoryId} not found");
        }
    }
}
