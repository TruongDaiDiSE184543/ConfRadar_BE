using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.ConferenceCategory;
using ConfRadar.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.UnitTests.Services.Maintenance.Category
{
    public class UpdateConferenceCategoryTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConferenceCategoryRepository> _mockCategoryRepo;
        private readonly ConferenceCategoryService _categoryService;

        public UpdateConferenceCategoryTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepo = new Mock<IConferenceCategoryRepository>();
            _mockUnitOfWork.Setup(u => u.ConferenceCategoryRepository).Returns(_mockCategoryRepo.Object);
            _categoryService = new ConferenceCategoryService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task UpdateConferenceCategory_Should_Succeed_When_NameIsUnique()
        {
            // ARRANGE
            var categoryId = "cat1";
            var request = new UpdateConferenceCategoryRequest { ConferenceCategoryName = "Updated Name" };
            var category = new ConferenceCategory { ConferenceCategoryId = categoryId, ConferenceCategoryName = "Old Name" };

            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync(categoryId)).ReturnsAsync(category);
            _mockCategoryRepo.Setup(r => r.GetCategoryByCategoryName("Updated Name")).ReturnsAsync((ConferenceCategory)null);
            _mockCategoryRepo.Setup(r => r.UpdateConferenceCategoryAsync(It.IsAny<ConferenceCategory>())).ReturnsAsync(1);

            // ACT
            var result = await _categoryService.UpdateConferenceCategoryAsync(categoryId, request);

            // ASSERT
            result.ConferenceCategoryName.Should().Be("Updated Name");
            _mockCategoryRepo.Verify(r => r.UpdateConferenceCategoryAsync(It.Is<ConferenceCategory>(c => c.ConferenceCategoryName == "Updated Name")), Times.Once);
        }

        [Fact]
        public async Task UpdateConferenceCategory_Should_ThrowBadRequest_When_NewNameExistsInAnotherCategory()
        {
            var categoryId = "cat1";
            var request = new UpdateConferenceCategoryRequest { ConferenceCategoryName = "Existing Name" };
            var categoryToUpdate = new ConferenceCategory { ConferenceCategoryId = categoryId, ConferenceCategoryName = "Old Name" };
            var otherCategoryWithSameName = new ConferenceCategory { ConferenceCategoryId = "cat2", ConferenceCategoryName = "Existing Name" };

            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync(categoryId)).ReturnsAsync(categoryToUpdate);
            _mockCategoryRepo.Setup(r => r.GetCategoryByCategoryName("Existing Name")).ReturnsAsync(otherCategoryWithSameName);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _categoryService.UpdateConferenceCategoryAsync(categoryId, request));
            ex.Message.Should().Contain("'Existing Name' already exists");
        }

        [Fact]
        public async Task UpdateConferenceCategory_Should_Succeed_When_NameIsNotChanged()
        {
            var categoryId = "cat1";
            var request = new UpdateConferenceCategoryRequest { ConferenceCategoryName = "Same Name" };
            var category = new ConferenceCategory { ConferenceCategoryId = categoryId, ConferenceCategoryName = "Same Name" };

            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync(categoryId)).ReturnsAsync(category);
            _mockCategoryRepo.Setup(r => r.UpdateConferenceCategoryAsync(It.IsAny<ConferenceCategory>())).ReturnsAsync(1);

            await _categoryService.UpdateConferenceCategoryAsync(categoryId, request);

            // Verify that the check for existing name was NOT called because the name didn't change.
            _mockCategoryRepo.Verify(r => r.GetCategoryByCategoryName(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateConferenceCategory_Should_ThrowNotFound_When_CategoryDoesNotExist()
        {
            var request = new UpdateConferenceCategoryRequest();
            _mockCategoryRepo.Setup(r => r.GetConferenceCategoryByIdAsync("not-found")).ReturnsAsync((ConferenceCategory)null);
            await Assert.ThrowsAsync<NotFoundException>(() => _categoryService.UpdateConferenceCategoryAsync("not-found", request));
        }
    }
}
