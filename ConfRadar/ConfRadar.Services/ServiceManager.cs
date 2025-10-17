using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
       
    }

    public class ServiceManager : IServiceManager
    {
        private readonly IAuthService _authService;

        public ServiceManager(IAuthService authService)
        {
            
            _authService = authService;
        }

        public IAuthService AuthService => _authService;
    }

}
