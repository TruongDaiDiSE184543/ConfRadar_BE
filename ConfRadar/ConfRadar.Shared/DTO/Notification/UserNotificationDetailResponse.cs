namespace ConfRadar.Shared.DTO.Notification
{
    public class UserNotificationDetailResponse
    {
        public string NotificationId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? ReadStatus { get; set; }
    }
}
