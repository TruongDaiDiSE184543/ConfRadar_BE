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
        IPaymentMethodRepository PaymentMethodRepository { get; }
        ITransactionStatusRepository TransactionStatusRepository { get; }
        IPricePhaseRepository PricePhaseRepository { get; }
        IConferencePriceRepository ConferencePriceRepository { get; }
        ITransactionTypeRepository TransactionTypeRepository { get; }
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
        private IPaymentMethodRepository _PaymentMethodRepository;
        private ITransactionStatusRepository _TransactionStatusRepository;
        private IPricePhaseRepository _PricePhaseRepository;
        private IConferencePriceRepository _ConferencePriceRepository;
        private ITransactionTypeRepository _TransactionTypeRepository;
        public UnitOfWork(ConfRadarDbContext context)
        {
            _context = context;
        }
        public IUserRepository UserRepository => _UserRepository ??= new UserRepository(_context);
        public IRoleRepository RoleRepository => _RoleRepository ??= new RoleRepository(_context);
        public IUserRefreshTokenRepository UserRefreshTokenRepository => _UserRefreshTokenRepository ??= new UserRefreshTokenRepository(_context);


        public IUserRoleRepository UserRoleRepository => _UserRoleRepository ??= new UserRoleRepository(_context);

        public IGlobalStatusRepository GlobalStatusRepository => _GlobalStatusRepository ??= new GlobalStatusRepository(_context);

        public IPaymentMethodRepository PaymentMethodRepository => _PaymentMethodRepository ??= new PaymentMethodRepository(_context);

        public ITransactionStatusRepository TransactionStatusRepository => _TransactionStatusRepository ??= new TransactionStatusRepository(_context);

        public IPricePhaseRepository PricePhaseRepository => _PricePhaseRepository ??= new PricePhaseRepository(_context);

        public IConferencePriceRepository ConferencePriceRepository => _ConferencePriceRepository ??= new ConferencePriceRepository(_context);
        public ITransactionTypeRepository TransactionTypeRepository => _TransactionTypeRepository ??= new TransactionTypeRepository(_context);

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
