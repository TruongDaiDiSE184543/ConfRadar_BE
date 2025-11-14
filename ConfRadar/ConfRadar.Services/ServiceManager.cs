using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
        public IReviewStatusService ReviewStatusService { get; }
        public IAssigningPresenterSessionService AssigningPresenterSessionService { get; }
        public IRankingCategoryService RankingCategoryService { get; }
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
        public IPaperService PaperService { get; }
        public IPaperAssignmentService PaperAssignmentService { get; }
        public IFullPaperService FullPaperService { get; }
        public IRevisionPaperService RevisionPaperService { get; }
        public ICameraReadyService CameraReadyService { get; }
        public ICityService CityService { get; }
        public IConferenceStatusService ConferenceStatusService { get; }
        public IConferenceTimelineService ConferenceTimelineService { get; }
        public IFavouriteConferenceService FavoriteConferenceService { get; }
        public IReportService ReportService { get; }
        public IContractService ContractService { get; }
        public IWalletService WalletService { get; }
        public IQRCoderService QRCoderService { get; }
        //public ICityService CityService { get; }

    }

    public class ServiceManager : IServiceManager
    {
        private readonly IAuthService _authService;
        private readonly IRankingCategoryService _rankingCategoryService;

        private readonly IMomoService _momoService;
        private readonly IReviewStatusService _reviewStatusService;
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
        private readonly IPaperService _paperService;
        private readonly IPaperAssignmentService _paperAssignmentService;
        private readonly IFullPaperService _fullPaperService;
        private readonly IRevisionPaperService _revisionPaperService;
        private readonly ICameraReadyService _cameraReadyService;
        private readonly ICityService _cityService;
        private readonly IConferenceStatusService _conferenceStatusService;
        private readonly IConferenceTimelineService _conferenceTimelineService;
        private readonly IFavouriteConferenceService _favouriteConferenceService;
        private readonly IReportService _reportService;
        private readonly IAssigningPresenterSessionService _assigningPresenterSessionService;
        private readonly IContractService _contractService;
        private readonly IWalletService _walletService;
        private readonly IQRCoderService _qrRCoderService;

        public ServiceManager(IAuthService authService,
            IMomoService momoService,
            ITicketService ticketService,
            IPaymentService paymentService,
            IConferenceService conferenceService,
            IRoomService roomService,
            IReviewStatusService reviewStatusService,
            IDestinationService destinationService,
            ISystemConfigurationService systemConfigurationService,
            IConferencePriceTicketService conferencePriceTicketService,
            IConferenceStepService conferenceStepService,
            IConferenceCategoryService conferenceCategoryService,
            IGlobalStatusService globalStatusService,
            IPaperService paperService,
            IPaperAssignmentService paperAssignmentService,
            IFullPaperService fullPaperService,
            IRevisionPaperService revisionPaperService,
            ICameraReadyService cameraReadyService,
            IRankingCategoryService rankingCategoryService,
            ICityService cityService,
            IConferenceStatusService conferenceStatusService,
            IConferenceTimelineService conferenceTimelineService,

            IFavouriteConferenceService favouriteConferenceService,
            IReportService reportService,

            IContractService contractService,
            IAssigningPresenterSessionService assigningPresenterSessionService,
            IWalletService walletService,
            IQRCoderService qRCoderService

           )
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
            _rankingCategoryService = rankingCategoryService;
            _paperService = paperService;
            _paperAssignmentService = paperAssignmentService;
            _fullPaperService = fullPaperService;
            _revisionPaperService = revisionPaperService;
            _cameraReadyService = cameraReadyService;
            _cityService = cityService;
            _conferenceStatusService = conferenceStatusService;
            _conferenceTimelineService = conferenceTimelineService;
            _favouriteConferenceService = favouriteConferenceService;
            _contractService = contractService;

            _reportService = reportService;
            _assigningPresenterSessionService = assigningPresenterSessionService;
            _walletService = walletService;
            _qrRCoderService = qRCoderService;
        }

        public IAuthService AuthService => _authService;
        public IMomoService MomoService => _momoService;
        public ITicketService TicketService => _ticketService;
        public IReviewStatusService ReviewStatusService => _reviewStatusService;
        public IPaymentService PaymentService => _paymentService;
        public IConferenceService ConferenceService => _conferenceService;
        public IRankingCategoryService RankingCategoryService => _rankingCategoryService;
        public IRoomService RoomService => _roomService;
        public IDestinationService DestinationService => _destinationService;
        public ISystemConfigurationService SystemConfigurationService => _systemConfigurationService;
        public IConferencePriceTicketService ConferencePriceTicketService => _conferencePriceTicketService;
        public IConferenceStepService ConferenceStepService => _conferenceStepService;
        public IConferenceCategoryService ConferenceCategoryService => _conferenceCategoryService;

        public IGlobalStatusService GlobalStatusService => _globalStatusService;

        public IPaperService PaperService => _paperService;
        public IPaperAssignmentService PaperAssignmentService => _paperAssignmentService;
        public IFullPaperService FullPaperService => _fullPaperService;
        public IRevisionPaperService RevisionPaperService => _revisionPaperService;
        public ICameraReadyService CameraReadyService => _cameraReadyService;

        public ICityService CityService => _cityService;

        public IConferenceStatusService ConferenceStatusService => _conferenceStatusService;

        public IConferenceTimelineService ConferenceTimelineService => _conferenceTimelineService;

        public IFavouriteConferenceService FavoriteConferenceService => _favouriteConferenceService;
        public IReportService ReportService => _reportService;

        public IContractService ContractService => _contractService;
        public IAssigningPresenterSessionService AssigningPresenterSessionService => _assigningPresenterSessionService;

        public IWalletService WalletService => _walletService;

        public IQRCoderService QRCoderService => _qrRCoderService;
    }

}
