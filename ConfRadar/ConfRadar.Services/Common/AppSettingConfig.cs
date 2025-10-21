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
        public class ObjectStorageSettings
        {
            public string EndPointAccess { get; set; }
            public string EndPoint { get; set; }
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public bool Secure { get; set; }
        }
        public class FirebaseSettings
        {
            public string ServiceAccountPath { get; set; }
        }
        public class MomoSettings
        {
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public string PartnerCode { get; set; }
            public string RedirectUrl { get; set; }
            public string IpnUrl { get; set; }
            public string RequestType { get; set; }
            public string PaymentCode { get; set; }
            public bool AutoCapture { get; set; }
            public string ExtraData { get; set; }
            public string Lang { get; set; }
        }
        public class RedisSettings
        {
            public string Host { get; set; }
            public string Password { get; set; }
            public int Port { get; set; }
        }
    }
}
