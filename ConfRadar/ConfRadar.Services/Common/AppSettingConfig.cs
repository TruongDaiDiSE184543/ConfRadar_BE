namespace ConfRadar.Services.Common
{
    public static class AppSettingConfig
    {
        public class EmailSettings
        {
            public string FromName { get; set; }
            public string FromEmail { get; set; }
            public string SmtpServer { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool UseSSL { get; set; }
        }
        public class JwtSettings
        {
            public string Issuer { get; set; }
            public string Audience { get; set; }
            public string SecretKey { get; set; }
            public int ExpiresAccessToken { get; set; }
            public int ExpiresRefreshToken { get; set; }
        }
    }
}
