using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
        public IConferenceService ConferenceService { get; }
        public IRoomService RoomService { get; }
        public IDestinationService DestinationService { get; }
        public ISystemConfigurationService SystemConfigurationService { get; }
        public IConferencePriceTicketService ConferencePriceTicketService { get; }
        public IConferenceStepService ConferenceStepService { get; }
    }

    public class ServiceManager : IServiceManager
    {
        private readonly IAuthService _authService;
        private readonly IConferenceService _conferenceService;
        private readonly IRoomService _roomService;
        private readonly IDestinationService _destinationService;
        private readonly ISystemConfigurationService _systemConfigurationService;
        private readonly IConferencePriceTicketService _conferencePriceTicketService;
        private readonly IConferenceStepService _conferenceStepService;

        public ServiceManager(IAuthService authService, IConferenceService conferenceService, IRoomService roomService, IDestinationService destinationService, ISystemConfigurationService systemConfigurationService, IConferencePriceTicketService conferencePriceTicketService, IConferenceStepService conferenceStepService)
        {
            
            _authService = authService;
            _conferenceService = conferenceService;
            _roomService = roomService;
            _destinationService = destinationService;
            _systemConfigurationService = systemConfigurationService;
            _conferencePriceTicketService = conferencePriceTicketService;
            _conferenceStepService = conferenceStepService;
        }

        public IAuthService AuthService => _authService;
        public IConferenceService ConferenceService => _conferenceService;
        public IRoomService RoomService => _roomService;
        public IDestinationService DestinationService => _destinationService;
        public ISystemConfigurationService SystemConfigurationService => _systemConfigurationService;
        public IConferencePriceTicketService ConferencePriceTicketService => _conferencePriceTicketService;
        public IConferenceStepService ConferenceStepService => _conferenceStepService;
    }

}
