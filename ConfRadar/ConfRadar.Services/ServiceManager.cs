using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
        public IMomoService MomoService { get; }
        public ITicketService TicketService { get; }
        public IPaymentService PaymentService { get; }
        public IConferenceService ConferenceService { get; }
        public IRoomService RoomService { get; }
        public IDestinationService DestinationService { get; }
        public ISystemConfigurationService SystemConfigurationService { get; }
        public IConferencePriceTicketService ConferencePriceTicketService { get; }
        public IConferenceStepService ConferenceStepService { get; }
        public IConferenceCategoryService ConferenceCategoryService { get; }
        public IGlobalStatusService GlobalStatusService { get; }

    }

    public class ServiceManager : IServiceManager
    {
        private readonly IAuthService _authService;

        private readonly IMomoService _momoService;
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;
        private readonly IConferenceService _conferenceService;
        private readonly IRoomService _roomService;
        private readonly IDestinationService _destinationService;
        private readonly ISystemConfigurationService _systemConfigurationService;
        private readonly IConferencePriceTicketService _conferencePriceTicketService;
        private readonly IConferenceStepService _conferenceStepService;
        private readonly IConferenceCategoryService _conferenceCategoryService;
        private readonly IGlobalStatusService _globalStatusService;

        public ServiceManager(IAuthService authService,
            IMomoService momoService,
            ITicketService ticketService,
            IPaymentService paymentService,
            IConferenceService conferenceService,
            IRoomService roomService,
            IDestinationService destinationService,
            ISystemConfigurationService systemConfigurationService,
            IConferencePriceTicketService conferencePriceTicketService,
            IConferenceStepService conferenceStepService,
            IConferenceCategoryService conferenceCategoryService,
            IGlobalStatusService globalStatusService)
        {
            _authService = authService;
            _momoService = momoService;
            _ticketService = ticketService;
            _paymentService = paymentService;
            _conferenceService = conferenceService;
            _roomService = roomService;
            _destinationService = destinationService;
            _systemConfigurationService = systemConfigurationService;
            _conferencePriceTicketService = conferencePriceTicketService;
            _conferenceStepService = conferenceStepService;
            _conferenceCategoryService = conferenceCategoryService;
            _globalStatusService = globalStatusService;
        }

        public IAuthService AuthService => _authService;
        public IMomoService MomoService => _momoService;
        public ITicketService TicketService => _ticketService;
        public IPaymentService PaymentService => _paymentService;
        public IConferenceService ConferenceService => _conferenceService;
        public IRoomService RoomService => _roomService;
        public IDestinationService DestinationService => _destinationService;
        public ISystemConfigurationService SystemConfigurationService => _systemConfigurationService;
        public IConferencePriceTicketService ConferencePriceTicketService => _conferencePriceTicketService;
        public IConferenceStepService ConferenceStepService => _conferenceStepService;
        public IConferenceCategoryService ConferenceCategoryService => _conferenceCategoryService;

        public IGlobalStatusService GlobalStatusService => _globalStatusService;
    }

}
