using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.ConferenceCategory;
using ConfRadar.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface IConferenceCategoryService
    {
        Task<ConferenceCategoryResponse> CreateConferenceCategoryAsync(CreateConferenceCategoryRequest request);
        Task<ConferenceCategoryResponse> GetConferenceCategoryByIdAsync(string categoryId);
        Task<List<ConferenceCategoryListResponse>> GetAllConferenceCategoriesAsync();
        Task<ConferenceCategoryResponse> UpdateConferenceCategoryAsync(string categoryId, UpdateConferenceCategoryRequest request);
        Task<bool> DeleteConferenceCategoryAsync(string categoryId);
    }

    public class ConferenceCategoryService : IConferenceCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConferenceCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ConferenceCategoryResponse> CreateConferenceCategoryAsync(CreateConferenceCategoryRequest request)
        {
            // Check if category with the same name already exists
            var existingCategory = await _unitOfWork.ConferenceCategoryRepository.GetCategoryByCategoryName(request.ConferenceCategoryName);
            if (existingCategory != null)
            {
                throw new BadRequestException($"Conference category with name '{request.ConferenceCategoryName}' already exists");
            }

            var category = new ConferenceCategory
            {
                ConferenceCategoryId = Guid.NewGuid().ToString(),
                ConferenceCategoryName = request.ConferenceCategoryName
            };

            var result = await _unitOfWork.ConferenceCategoryRepository.CreateConferenceCategoryAsync(category);
            if (result <= 0)
            {
                throw new BadRequestException("Failed to create conference category");
            }

            return new ConferenceCategoryResponse
            {
                ConferenceCategoryId = category.ConferenceCategoryId,
                ConferenceCategoryName = category.ConferenceCategoryName
            };
        }

        public async Task<ConferenceCategoryResponse> GetConferenceCategoryByIdAsync(string categoryId)
        {
            var category = await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Conference category with ID {categoryId} not found");
            }

            return new ConferenceCategoryResponse
            {
                ConferenceCategoryId = category.ConferenceCategoryId,
                ConferenceCategoryName = category.ConferenceCategoryName
            };
        }

        public async Task<List<ConferenceCategoryListResponse>> GetAllConferenceCategoriesAsync()
        {
            // Get all categories with conference counts
            var categories = await _unitOfWork.ConferenceCategoryRepository.GetAllConferenceCategoriesAsync();
            
            // Get conference counts for each category
            var conferences = await _unitOfWork.ConferenceRepository.GetAllConferencesAsync();
            var categoryCounts = conferences
                .GroupBy(c => c.ConferenceCategoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            var responses = categories.Select(category => new ConferenceCategoryListResponse
            {
                ConferenceCategoryId = category.ConferenceCategoryId,
                ConferenceCategoryName = category.ConferenceCategoryName,
                ConferenceCount = categoryCounts.ContainsKey(category.ConferenceCategoryId) ? categoryCounts[category.ConferenceCategoryId] : 0
            }).ToList();

            return responses;
        }

        public async Task<ConferenceCategoryResponse> UpdateConferenceCategoryAsync(string categoryId, UpdateConferenceCategoryRequest request)
        {
            var category = await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Conference category with ID {categoryId} not found");
            }

            // If name is being updated, check if another category with the same name exists
            if (!string.IsNullOrEmpty(request.ConferenceCategoryName) && 
                request.ConferenceCategoryName != category.ConferenceCategoryName)
            {
                var existingCategory = await _unitOfWork.ConferenceCategoryRepository.GetCategoryByCategoryName(request.ConferenceCategoryName);
                if (existingCategory != null && existingCategory.ConferenceCategoryId != categoryId)
                {
                    throw new BadRequestException($"Conference category with name '{request.ConferenceCategoryName}' already exists");
                }
                
                category.ConferenceCategoryName = request.ConferenceCategoryName;
            }

            var result = await _unitOfWork.ConferenceCategoryRepository.UpdateConferenceCategoryAsync(category);
            if (result <= 0)
            {
                throw new BadRequestException("Failed to update conference category");
            }

            return new ConferenceCategoryResponse
            {
                ConferenceCategoryId = category.ConferenceCategoryId,
                ConferenceCategoryName = category.ConferenceCategoryName
            };
        }

        public async Task<bool> DeleteConferenceCategoryAsync(string categoryId)
        {
            var category = await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Conference category with ID {categoryId} not found");
            }

            // Check if category is being used by any conferences
            var conferences = await _unitOfWork.ConferenceRepository.GetAllConferencesAsync();
            var conferenceCount = conferences.Count(c => c.ConferenceCategoryId == categoryId);
            
            if (conferenceCount > 0)
            {
                throw new BadRequestException($"Cannot delete conference category '{category.ConferenceCategoryName}' because it is being used by {conferenceCount} conference(s)");
            }

            var result = await _unitOfWork.ConferenceCategoryRepository.DeleteConferenceCategoryAsync(category);
            return result > 0;
        }
    }
}