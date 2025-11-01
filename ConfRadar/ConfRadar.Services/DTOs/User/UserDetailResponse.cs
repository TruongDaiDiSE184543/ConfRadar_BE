namespace ConfRadar.Services.DTOs.User
{
    public class UserDetailResponse
    {
        public string UserId { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateOnly? BirthDay { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BioDescription { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class ListUserDetailForAdminAndOrganizerResponse
    {
        public List<UserDetailForAdminAndOrganizerResponse> Users { get; set; }
    }
    public class UserDetailForAdminAndOrganizerResponse
    {
        public string UserId { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<string> Roles { get; set; }
    }
    public class ReviewerDetailResponse
    {
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
