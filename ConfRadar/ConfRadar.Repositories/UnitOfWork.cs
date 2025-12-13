using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ConfRadar.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        IUserRoleRepository UserRoleRepository { get; }
        IUserRefreshTokenRepository UserRefreshTokenRepository { get; }
        IGlobalStatusRepository GlobalStatusRepository { get; }
        IConferenceStatusRepository ConferenceStatusRepository { get; }
        IRankingCategoryRepository RankingCategoryRepository { get; }
        IReviewStatusRepository ReviewStatusRepository { get; }
        IPaperPhaseRepository PaperPhaseRepository { get; }
        IPaymentMethodRepository PaymentMethodRepository { get; }
        //ITransactionStatusRepository TransactionStatusRepository { get; }
        IConferenceTimelineRepository ConferenceTimelineRepository { get; }


        //ITransactionTypeRepository TransactionTypeRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        ITicketRepository TicketRepository { get; }
        ITechnicalConferenceDetailRepository TechnicalConferenceDetailRepository { get; }
        IConferenceSessionRepository ConferenceSessionRepository { get; }

        IConferenceRepository ConferenceRepository { get; }
        IConferencePolicyRepository ConferencePolicyRepository { get; }
        IConferenceMediaRepository ConferenceMediaRepository { get; }
        ISponsorRepository SponsorRepository { get; }
        IConferencePriceRepository ConferencePriceRepository { get; }
        IConferenceSessionMediumRepository ConferenceSessionMediumRepository { get; }
        ISpeakerRepository SpeakerRepository { get; }
        IRoomRepository RoomRepository { get; }
        IDestinationRepository DestinationRepository { get; }
        IPricePhaseRepository PricePhaseRepository { get; }
        //IMediaTypeRepository MediaTypeRepository { get; }
        IConferenceCategoryRepository ConferenceCategoryRepository { get; }
        ICityRepository CityRepository { get; }
        IConferenceRefundPolicyRepository ConferenceRefundPolicyRepository { get; }
        //IConferenceStatusRepository ConferenceStatusRepository { get; }
        //IConferenceRefundPolicyRepository ConferenceRefundPolicyRepository { get; }
        //IPaperPhaseRepository PaperPhaseRepository { get; }
        ICheckInStatusRepository CheckInStatusRepository { get; }
        IAbstractRepository AbstractRepository { get; }
        IPaperRepository PaperRepository { get; }
        IResearchConferenceDetailRepository ResearchConferenceDetailRepository { get; }
        IResearchConferencePhaseRepository ResearchConferencePhaseRepository { get; }
        IMaterialDownloadRepository MaterialDownloadRepository { get; }
        IRankingFileUrlRepository RankingFileUrlRepository { get; }
        IRankingReferenceUrlRepository RankingReferenceUrlRepository { get; }
        IRevisionRoundDeadlineRepository RevisionRoundDeadlineRepository { get; }
        IPaperAuthorRepository PaperAuthorRepository { get; }
        IPaperReviewerRepository PaperReviewerRepository { get; }
        IFullPaperRepository FullPaperRepository { get; }
        IRevisionPaperRepository RevisionPaperRepository { get; }
        ICameraReadyRepository CameraReadyRepository { get; }
        IRevisionPaperSubmissionRepository RevisionPaperSubmissionRepository { get; }
        IRevisionSubmissionFeedbackRepository RevisionSubmissionFeedbackRepository { get; }
        //IRevisionPaperReviewRepository RevisionPaperReviewRepository { get; }
        IFullPaperReviewRepository FullPaperReviewRepository { get; }
        IReviewerContractRepository ReviewerContractRepository { get; }
        IWaitListStatusRepository WaitListStatusRepository { get; }
        IPaperWaitListRepository PaperWaitListRepository { get; }
        IFavouriteConferenceRepository FavoriteConferenceRepository { get; }
        IReportRepository ReportRepository { get; }
        IReportFeedbackRepository ReportFeedbackRepository { get; }
        INotificationRepository NotificationRepository { get; }

        IPresentAuthorRepository PresentAuthorRepository { get; }
        IPresenterChangeRequestRepository PresenterChangeRequestRepository { get; }
        ISessionChangeRequestRepository SessionChangeRequestRepository { get; }

        IUserCheckInRepository UserCheckInRepository { get; }
        IConferenceFeedbackRepository ConferenceFeedbackRepository { get; }
        IRefundRequestRepository RefundRequestRepository { get; }
        IWalletRepository WalletRepository { get; }
        IWalletTransactionRepository WalletTransactionRepository { get; }
        IAuditLogRepository AuditLogRepository { get; }

        IAcademicProfileRepository AcademicProfileRepository { get; }
        IOrcidDataCacheRepository OrcidDataCacheRepository { get; }
        IOrganizationRepository OrganizationRepository { get; }
        ICollaboratorContractRepository CollaboratorContractRepository { get; }
        IUserSuspendHistoryRepository UserSuspendHistoryRepository { get; }
        IAuditLogCategoryRepository AuditLogCategoryRepository { get; }
        //IPublisherRepository PublisherRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private ConfRadarDbContext _context;
        private IDbContextTransaction? _transaction;
        private ITechnicalConferenceDetailRepository _TechnicalConferenceDetailRepository;
        private IUserRepository _UserRepository;
        private IRoleRepository _RoleRepository;
        private IConferenceSessionRepository _ConferenceSessionRepository;
        private IUserRoleRepository _UserRoleRepository;
        private IUserRefreshTokenRepository _UserRefreshTokenRepository;
        private IGlobalStatusRepository _GlobalStatusRepository;
        private IConferenceStatusRepository _ConferenceStatusRepository;
        private IRankingCategoryRepository _RankingCategoryRepository;
        private IReviewStatusRepository _ReviewStatusRepository;
        private IPaperPhaseRepository _PaperPhaseRepository;
        private IPaymentMethodRepository _PaymentMethodRepository;
        private IConferenceSessionMediumRepository _ConferenceSessionMediumRepository;
        private IConferenceTimelineRepository _ConferenceTimelineRepository;
        //private ITransactionStatusRepository _TransactionStatusRepository;

        private IPricePhaseRepository _PricePhaseRepository;
        private IConferencePriceRepository _ConferencePriceRepository;
        //private ITransactionTypeRepository _TransactionTypeRepository;
        private ITransactionRepository _TransactionRepository;
        private ITicketRepository _TicketRepository;

        private IConferenceRepository _ConferenceRepository;
        private IConferencePolicyRepository _ConferencePolicyRepository;
        private IConferenceMediaRepository _ConferenceMediaRepository;
        private ISponsorRepository _SponsorRepository;


        private ISpeakerRepository _SpeakerRepository;
        private IRoomRepository _RoomRepository;
        private IDestinationRepository _DestinationRepository;

        //private IMediaTypeRepository _MediaTypeRepository;
        private IConferenceCategoryRepository _ConferenceCategoryRepository;

        private ICityRepository _CityRepository;
        private IConferenceRefundPolicyRepository _ConferenceRefundPolicyRepository;
        private ICheckInStatusRepository _CheckInStatusRepository;
        private IAbstractRepository _AbstractRepository;
        private IPaperRepository _PaperRepository;
        //private IReviewStatusRepository _ReviewStatusRepository;
        private IFullPaperRepository _FullPaperRepository;
        private IPaperReviewerRepository _PaperReviewerRepository;
        private ICameraReadyRepository _CameraReadyRepository;
        private IRevisionPaperRepository _RevisionPaperRepository;
        private IRevisionPaperSubmissionRepository _RevisionPaperSubmissionRepository;
        private IRevisionSubmissionFeedbackRepository _RevisionSubmissionFeedbackRepository;
        private IResearchConferenceDetailRepository _ResearchConferenceDetailRepository;
        private IResearchConferencePhaseRepository _ResearchConferencePhaseRepository;
        private IMaterialDownloadRepository _MaterialDownloadRepository;
        private IRankingFileUrlRepository _RankingFileUrlRepository;
        private IRankingReferenceUrlRepository _RankingReferenceUrlRepository;
        private IRevisionRoundDeadlineRepository _RevisionRoundDeadlineRepository;
        private IPaperAuthorRepository _PaperAuthorRepository;

        //private IRevisionPaperReviewRepository _RevisionPaperReviewRepository;
        private IFullPaperReviewRepository _FullPaperReviewRepository;
        private IReviewerContractRepository _ReviewerContractRepository;
        private IWaitListStatusRepository _WaitListStatusRepository;
        private IPaperWaitListRepository _PaperWaitListRepository;
        private IFavouriteConferenceRepository _FavouriteConferenceRepository;
        private INotificationRepository _NotificationRepository;
        private IReportRepository _ReportRepository;
        private IReportFeedbackRepository _ReportFeedbackRepository;

        private IPresentAuthorRepository _PresentAuthorRepository;

        private IPresenterChangeRequestRepository _PresenterChangeRequestRepository;
        private ISessionChangeRequestRepository _SessionChangeRequestRepository;

        private IUserCheckInRepository _UserCheckInRepository;
        private IConferenceFeedbackRepository _ConferenceFeedbackRepository;
        private IRefundRequestRepository _RefundRequestRepository;
        private IWalletRepository _WalletRepository;
        private IWalletTransactionRepository _WalletTransactionRepository;
        private IAcademicProfileRepository _AcademicProfileRepository;
        private IOrcidDataCacheRepository _OrcidDataCacheRepository;
        private IAuditLogRepository _AuditLogRepository;
        private IOrganizationRepository _OrganizationRepository;
        private ICollaboratorContractRepository _CollaboratorContractRepository;
        private IUserSuspendHistoryRepository _UserSuspendHistoryRepository;
        private IAuditLogCategoryRepository _AuditLogCategoryRepository;
        //private IPublisherRepository _PublisherRepository;
        public UnitOfWork(ConfRadarDbContext context)
        {
            _context = context;
        }
        public IUserRepository UserRepository => _UserRepository ??= new UserRepository(_context);
        public ITechnicalConferenceDetailRepository TechnicalConferenceDetailRepository => _TechnicalConferenceDetailRepository ??= new TechnicalConferenceDetailRepository(_context);
        public IRoleRepository RoleRepository => _RoleRepository ??= new RoleRepository(_context);
        public IConferenceSessionRepository ConferenceSessionRepository => _ConferenceSessionRepository ??= new ConferenceSessionRepository(_context);
        public IConferenceSessionMediumRepository ConferenceSessionMediumRepository => _ConferenceSessionMediumRepository ??= new ConferenceSessionMediumRepository(_context);
        public IUserRefreshTokenRepository UserRefreshTokenRepository => _UserRefreshTokenRepository ??= new UserRefreshTokenRepository(_context);

        public IConferenceTimelineRepository ConferenceTimelineRepository => _ConferenceTimelineRepository ??= new ConferenceTimelineRepository(_context);
        public IUserRoleRepository UserRoleRepository => _UserRoleRepository ??= new UserRoleRepository(_context);

        public IGlobalStatusRepository GlobalStatusRepository => _GlobalStatusRepository ??= new GlobalStatusRepository(_context);

        public IConferenceStatusRepository ConferenceStatusRepository => _ConferenceStatusRepository ??= new ConferenceStatusRepository(_context);

        public IRankingCategoryRepository RankingCategoryRepository => _RankingCategoryRepository ??= new RankingCategoryRepository(_context);

        public IReviewStatusRepository ReviewStatusRepository => _ReviewStatusRepository ??= new ReviewStatusRepository(_context);

        public IPaperPhaseRepository PaperPhaseRepository => _PaperPhaseRepository ??= new PaperPhaseRepository(_context);

        public IPaymentMethodRepository PaymentMethodRepository => _PaymentMethodRepository ??= new PaymentMethodRepository(_context);


        public IPricePhaseRepository PricePhaseRepository => _PricePhaseRepository ??= new PricePhaseRepository(_context);

        public IConferencePriceRepository ConferencePriceRepository => _ConferencePriceRepository ??= new ConferencePriceRepository(_context);

        public ITransactionRepository TransactionRepository => _TransactionRepository ??= new TransactionRepository(_context);

        public ITicketRepository TicketRepository => _TicketRepository ??= new TicketRepository(_context);

        public IConferenceRepository ConferenceRepository => _ConferenceRepository ??= new ConferenceRepository(_context);

        public IConferencePolicyRepository ConferencePolicyRepository => _ConferencePolicyRepository ??= new ConferencePolicyRepository(_context);

        public IConferenceMediaRepository ConferenceMediaRepository => _ConferenceMediaRepository ??= new ConferenceMediaRepository(_context);

        public ISponsorRepository SponsorRepository => _SponsorRepository ??= new SponsorRepository(_context);





        public ISpeakerRepository SpeakerRepository => _SpeakerRepository ??= new SpeakerRepository(_context);

        public IRoomRepository RoomRepository => _RoomRepository ??= new RoomRepository(_context);

        public IDestinationRepository DestinationRepository => _DestinationRepository ??= new DestinationRepository(_context);




        public IConferenceCategoryRepository ConferenceCategoryRepository => _ConferenceCategoryRepository ??= new ConferenceCategoryRepository(_context);

        public ICityRepository CityRepository => _CityRepository ??= new CityRepository(_context);


        public IConferenceRefundPolicyRepository ConferenceRefundPolicyRepository => _ConferenceRefundPolicyRepository ??= new ConferenceRefundPolicyRepository(_context);


        public ICheckInStatusRepository CheckInStatusRepository => _CheckInStatusRepository ??= new CheckInStatusRepository(_context);

        public IAbstractRepository AbstractRepository => _AbstractRepository ??= new AbstractRepository(_context);

        public IPaperRepository PaperRepository => _PaperRepository ??= new PaperRepository(_context);

        //public IReviewStatusRepository ReviewStatusRepository => _ReviewStatusRepository ??= new ReviewStatusRepository(_context);

        public IFullPaperRepository FullPaperRepository => _FullPaperRepository ??= new FullPaperRepository(_context);

        public IPaperReviewerRepository PaperReviewerRepository => _PaperReviewerRepository ??= new PaperReviewerRepository(_context);

        public ICameraReadyRepository CameraReadyRepository => _CameraReadyRepository ??= new CameraReadyRepository(_context);
        public IRevisionPaperRepository RevisionPaperRepository => _RevisionPaperRepository ??= new RevisionPaperRepository(_context);

        public IRevisionPaperSubmissionRepository RevisionPaperSubmissionRepository => _RevisionPaperSubmissionRepository ??= new RevisionPaperSubmissionRepository(_context);

        public IRevisionSubmissionFeedbackRepository RevisionSubmissionFeedbackRepository => _RevisionSubmissionFeedbackRepository ??= new RevisionSubmissionFeedbackRepository(_context);

        public IResearchConferenceDetailRepository ResearchConferenceDetailRepository => _ResearchConferenceDetailRepository ??= new ResearchConferenceDetailRepository(_context);

        public IResearchConferencePhaseRepository ResearchConferencePhaseRepository => _ResearchConferencePhaseRepository ??= new ResearchConferencePhaseRepository(_context);

        public IMaterialDownloadRepository MaterialDownloadRepository => _MaterialDownloadRepository ??= new MaterialDownloadRepository(_context);

        public IRankingFileUrlRepository RankingFileUrlRepository => _RankingFileUrlRepository ??= new RankingFileUrlRepository(_context);

        public IRankingReferenceUrlRepository RankingReferenceUrlRepository => _RankingReferenceUrlRepository ??= new RankingReferenceUrlRepository(_context);

        public IRevisionRoundDeadlineRepository RevisionRoundDeadlineRepository => _RevisionRoundDeadlineRepository ??= new RevisionRoundDeadlineRepository(_context);

        public IPaperAuthorRepository PaperAuthorRepository => _PaperAuthorRepository ??= new PaperAuthorRepository(_context);
        //public IRevisionPaperReviewRepository RevisionPaperReviewRepository => _RevisionPaperReviewRepository ??= new RevisionPaperReviewRepository(_context);

        public IFullPaperReviewRepository FullPaperReviewRepository => _FullPaperReviewRepository ??= new FullPaperReviewRepository(_context);

        public IReviewerContractRepository ReviewerContractRepository => _ReviewerContractRepository ??= new ReviewerContractRepository(_context);

        public IWaitListStatusRepository WaitListStatusRepository => _WaitListStatusRepository ??= new WaitListStatusRepository(_context);

        public IPaperWaitListRepository PaperWaitListRepository => _PaperWaitListRepository ??= new PaperWaitListRepository(_context);

        public IFavouriteConferenceRepository FavoriteConferenceRepository => _FavouriteConferenceRepository ??= new FavouriteConferenceRepository(_context);

        public INotificationRepository NotificationRepository => _NotificationRepository ??= new NotificationRepository(_context);

        public IReportRepository ReportRepository => _ReportRepository ??= new ReportRepository(_context);
        public IReportFeedbackRepository ReportFeedbackRepository => _ReportFeedbackRepository ??= new ReportFeedbackRepository(_context);
        public IPresentAuthorRepository PresentAuthorRepository => _PresentAuthorRepository ??= new PresentAuthorRepository(_context);
        public IUserCheckInRepository UserCheckInRepository => _UserCheckInRepository ??= new UserCheckInRepository(_context);
        public IPresenterChangeRequestRepository PresenterChangeRequestRepository => _PresenterChangeRequestRepository ??= new PresenterChangeRequestRepository(_context);
        public ISessionChangeRequestRepository SessionChangeRequestRepository => _SessionChangeRequestRepository ??= new SessionChangeRequestRepository(_context);


        public IConferenceFeedbackRepository ConferenceFeedbackRepository => _ConferenceFeedbackRepository ??= new ConferenceFeedbackRepository(_context);

        public IRefundRequestRepository RefundRequestRepository => _RefundRequestRepository ??= new RefundRequestRepository(_context);

        public IWalletRepository WalletRepository => _WalletRepository ??= new WalletRepository(_context);

        public IWalletTransactionRepository WalletTransactionRepository => _WalletTransactionRepository ??= new WalletTransactionRepository(_context);

        public IAcademicProfileRepository AcademicProfileRepository => _AcademicProfileRepository ??= new AcademicProfileRepository(_context);

        public IOrcidDataCacheRepository OrcidDataCacheRepository => _OrcidDataCacheRepository ??= new OrcidDataCacheRepository(_context);

        public IAuditLogRepository AuditLogRepository => _AuditLogRepository ??= new AuditLogRepository(_context);

        public IOrganizationRepository OrganizationRepository => _OrganizationRepository ??= new OrganizationRepository(_context);

        public ICollaboratorContractRepository CollaboratorContractRepository => _CollaboratorContractRepository ??= new CollaboratorContractRepository(_context);

        public IUserSuspendHistoryRepository UserSuspendHistoryRepository => _UserSuspendHistoryRepository ??= new UserSuspendHistoryRepository(_context);

        public IAuditLogCategoryRepository AuditLogCategoryRepository => _AuditLogCategoryRepository ??= new AuditLogCategoryRepository(_context);

        //public IPublisherRepository PublisherRepository => _PublisherRepository ??= new PublisherRepository(_context);

        public async Task BeginTransactionAsync()
        {
            _transaction ??= await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            //await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


    }
}
