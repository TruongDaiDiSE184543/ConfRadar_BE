namespace ConfRadar.Services.Common
{
    public static class ExtensionHelper
    {
        public static DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
        }
        public static DateOnly GetVietnamDate()
        {
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")
            );
            return DateOnly.FromDateTime(vnTime);
        }
    }
}
