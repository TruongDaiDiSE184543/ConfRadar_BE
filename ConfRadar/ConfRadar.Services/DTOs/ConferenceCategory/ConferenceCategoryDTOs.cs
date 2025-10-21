using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.ConferenceCategory
{
    public class CreateConferenceCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string ConferenceCategoryName { get; set; }
    }

    public class UpdateConferenceCategoryRequest
    {
        [MaxLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string? ConferenceCategoryName { get; set; }
    }

    public class ConferenceCategoryResponse
    {
        public string ConferenceCategoryId { get; set; }
        public string ConferenceCategoryName { get; set; }
    }

    public class ConferenceCategoryListResponse
    {
        public string ConferenceCategoryId { get; set; }
        public string ConferenceCategoryName { get; set; }
        public int ConferenceCount { get; set; }
    }
}