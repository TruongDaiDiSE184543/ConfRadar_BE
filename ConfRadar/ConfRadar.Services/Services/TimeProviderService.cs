using Firebase.Database;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface ITimeProviderService
    {
        Task<DateOnly> GetVietnamDate();
        Task<DateTime> GetVietnamTime();
    }
    public class TimeProviderService : ITimeProviderService
    {
        private readonly IOptions<FirebaseRealtimeDbSettings> _fireaseRealTimeSettings;
        public TimeProviderService(IOptions<FirebaseRealtimeDbSettings> fireaseRealTimeSettings)
        {
            _fireaseRealTimeSettings = fireaseRealTimeSettings;
        }
        public async Task<TimeConfig> GetFireBaseKeyAsync(string key)
        {
            string firebaseDatabaseUrl = _fireaseRealTimeSettings.Value.Url.TrimEnd('/');
            var firebaseClient = new FirebaseClient(firebaseDatabaseUrl);

            var getConfig = await firebaseClient
                .Child(key)
                .OnceSingleAsync<TimeConfig>();

            if (getConfig == null)
            {
                throw new Exception("config not found in real time db");
            }

            return getConfig;
        }
        public async Task<DateTime> GetVietnamTime()
        {
            var finalTime = await GetVietnamDateTimeAsync();
            return finalTime;
        }
        public async Task<DateOnly> GetVietnamDate()
        {
            var finalTime = await GetVietnamDateTimeAsync();
            return DateOnly.FromDateTime(finalTime);
        }

        private async Task<DateTime> GetVietnamDateTimeAsync()
        {
            var cfg = await GetFireBaseKeyAsync("fakeTime");
            DateTime finalTime;
            if (cfg.UseFakeTime)
            {
                var parsed = DateTime.Parse(cfg.CustomVnTime!);
                finalTime = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
            }
            else
            {
                var utcNow = DateTime.UtcNow;
                finalTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
            }
            return finalTime;
        }


        public class TimeConfig
        {
            public bool UseFakeTime { get; set; }
            public string? CustomVnTime { get; set; }
        }


    }
}
