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

        IConferenceRepository ConferenceRepository { get; }
        IConferencePolicyRepository ConferencePolicyRepository { get; }
        IConferenceMediumRepository ConferenceMediumRepository { get; }
        ITechnicalConferenceDetailRepository TechnicalConferenceDetailRepository { get; }
        ISponsorRepository SponsorRepository { get; }
        IConferencePriceRepository ConferencePriceRepository { get; }
        IConferenceSessionRepository ConferenceSessionRepository { get; }
        ISpeakerRepository SpeakerRepository { get; }
        IRoomRepository RoomRepository { get; }
        IDestinationRepository DestinationRepository { get; }
        IPricePhaseRepository PricePhaseRepository { get; }
        //IMediaTypeRepository MediaTypeRepository { get; }
        IConferenceCategoryRepository ConferenceCategoryRepository { get; }
        IConferenceSessionMediumRepository ConferenceSessionMediumRepository { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private ConfRadarDbContext _context;
        private IDbContextTransaction? _transaction;
        private IUserRepository _UserRepository;
        private IRoleRepository _RoleRepository;
        private IUserRoleRepository _UserRoleRepository;
        private IUserRefreshTokenRepository _UserRefreshTokenRepository;
        private IGlobalStatusRepository _GlobalStatusRepository;
        private IConferenceStatusRepository _ConferenceStatusRepository;
        private IRankingCategoryRepository _RankingCategoryRepository;
        private IReviewStatusRepository _ReviewStatusRepository;
        private IPaperPhaseRepository _PaperPhaseRepository;
        private IPaymentMethodRepository _PaymentMethodRepository;
        //private ITransactionStatusRepository _TransactionStatusRepository;

        private IPricePhaseRepository _PricePhaseRepository;
        private IConferencePriceRepository _ConferencePriceRepository;
        //private ITransactionTypeRepository _TransactionTypeRepository;
        private ITransactionRepository _TransactionRepository;
        private ITicketRepository _TicketRepository;

        private IConferenceRepository _ConferenceRepository;
        private IConferencePolicyRepository _ConferencePolicyRepository;
        private IConferenceMediumRepository _ConferenceMediumRepository;
        private ITechnicalConferenceDetailRepository _TechnicalConferenceDetailRepository;
        private ISponsorRepository _SponsorRepository;

        private IConferenceSessionRepository _ConferenceSessionRepository;
        private ISpeakerRepository _SpeakerRepository;
        private IRoomRepository _RoomRepository;
        private IDestinationRepository _DestinationRepository;

        //private IMediaTypeRepository _MediaTypeRepository;
        private IConferenceCategoryRepository _ConferenceCategoryRepository;
        private IConferenceSessionMediumRepository _ConferenceSessionMediumRepository;

        public UnitOfWork(ConfRadarDbContext context)
        {
            _context = context;
        }
        public IUserRepository UserRepository => _UserRepository ??= new UserRepository(_context);
        public IRoleRepository RoleRepository => _RoleRepository ??= new RoleRepository(_context);
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

        public IConferenceMediumRepository ConferenceMediumRepository => _ConferenceMediumRepository ??= new ConferenceMediumRepository(_context);

        public ITechnicalConferenceDetailRepository TechnicalConferenceDetailRepository => _TechnicalConferenceDetailRepository ??= new TechnicalConferenceDetailRepository(_context);

        public ISponsorRepository SponsorRepository => _SponsorRepository ??= new SponsorRepository(_context);



        public IConferenceSessionRepository ConferenceSessionRepository => _ConferenceSessionRepository ??= new ConferenceSessionRepository(_context);

        public ISpeakerRepository SpeakerRepository => _SpeakerRepository ??= new SpeakerRepository(_context);

        public IRoomRepository RoomRepository => _RoomRepository ??= new RoomRepository(_context);

        public IDestinationRepository DestinationRepository => _DestinationRepository ??= new DestinationRepository(_context);




        public IConferenceCategoryRepository ConferenceCategoryRepository => _ConferenceCategoryRepository ??= new ConferenceCategoryRepository(_context);

        public IConferenceSessionMediumRepository ConferenceSessionMediumRepository => _ConferenceSessionMediumRepository ??= new ConferenceSessionMediumRepository(_context);

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
