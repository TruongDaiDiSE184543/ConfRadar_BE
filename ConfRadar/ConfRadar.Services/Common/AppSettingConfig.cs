using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
