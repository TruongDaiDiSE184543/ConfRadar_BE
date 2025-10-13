using ConfRadar.Repositories;
using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
        public IUserService UserService { get; }
        public IEmailService EmailService { get; }
    }

    public class ServiceManager : IServiceManager
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;

        public ServiceManager(
            IUserService userService,
            IAuthService authService,
            IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
            _authService = authService;
        }

        public IAuthService AuthService => _authService;
        public IUserService UserService => _userService;
        public IEmailService EmailService => _emailService;
    }

}
