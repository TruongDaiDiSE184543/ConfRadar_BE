using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
        public IEmailService EmailService { get; }
    }

    public class ServiceManager : IServiceManager
    {
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;

        public ServiceManager(
            IAuthService authService,
            IEmailService emailService)
        {
            _emailService = emailService;
            _authService = authService;
        }

        public IAuthService AuthService => _authService;
        public IEmailService EmailService => _emailService;
    }

}
