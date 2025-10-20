using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
        public IConferenceService ConferenceService { get; }
    }

    public class ServiceManager : IServiceManager
    {
        private readonly IAuthService _authService;
        private readonly IConferenceService _conferenceService;

        public ServiceManager(IAuthService authService, IConferenceService conferenceService)
        {
            
            _authService = authService;
            _conferenceService = conferenceService;
        }

        public IAuthService AuthService => _authService;
        public IConferenceService ConferenceService => _conferenceService;
    }

}
