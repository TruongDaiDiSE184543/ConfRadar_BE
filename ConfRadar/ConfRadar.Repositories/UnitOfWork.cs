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
        private IResearchConferenceDetailRepository _ResearchConferenceDetailRepository;
        private IResearchConferencePhaseRepository _ResearchConferencePhaseRepository;
        private IMaterialDownloadRepository _MaterialDownloadRepository;
        private IRankingFileUrlRepository _RankingFileUrlRepository;
        private IRankingReferenceUrlRepository _RankingReferenceUrlRepository;
        private IRevisionRoundDeadlineRepository _RevisionRoundDeadlineRepository;
        private IPaperAuthorRepository _PaperAuthorRepository;
        private IPaperReviewerRepository _PaperReviewerRepository;

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

        public IResearchConferenceDetailRepository ResearchConferenceDetailRepository => _ResearchConferenceDetailRepository ??= new ResearchConferenceDetailRepository(_context);

        public IResearchConferencePhaseRepository ResearchConferencePhaseRepository => _ResearchConferencePhaseRepository ??= new ResearchConferencePhaseRepository(_context);

        public IMaterialDownloadRepository MaterialDownloadRepository => _MaterialDownloadRepository ??= new MaterialDownloadRepository(_context);

        public IRankingFileUrlRepository RankingFileUrlRepository => _RankingFileUrlRepository ??= new RankingFileUrlRepository(_context);

        public IRankingReferenceUrlRepository RankingReferenceUrlRepository => _RankingReferenceUrlRepository ??= new RankingReferenceUrlRepository(_context);

        public IRevisionRoundDeadlineRepository RevisionRoundDeadlineRepository => _RevisionRoundDeadlineRepository ??= new RevisionRoundDeadlineRepository(_context);

        public IPaperAuthorRepository PaperAuthorRepository => _PaperAuthorRepository ??= new PaperAuthorRepository(_context);

        public IPaperReviewerRepository PaperReviewerRepository => _PaperReviewerRepository ??= new PaperReviewerRepository(_context);

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
