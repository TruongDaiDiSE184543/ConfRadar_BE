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
        public string RoleId { get; set; }
        public string RoleName { get; set; }
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
        public bool? IsActive { get; set; }
        public bool? IsEmailConfirmed { get; set; }
        public UserSuspendDetailForAdminAndOrganizerResponse? CurrentUserSuspend { get; set; }
        public UserSuspendDetailForAdminAndOrganizerResponse? CurrentUserRoleSuspend { get; set; }


        public List<UserSuspendDetailForAdminAndOrganizerResponse> SuspendHistories { get; set; } = new List<UserSuspendDetailForAdminAndOrganizerResponse>();
    }


    public class UserSuspendDetailForAdminAndOrganizerResponse
    {
        public string SuspendId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string? Reason { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public DateTime? ResumedAt { get; set; }
        public string? SuspendType { get; set; }

        public bool? IsActiveSuspend { get; set; }

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
