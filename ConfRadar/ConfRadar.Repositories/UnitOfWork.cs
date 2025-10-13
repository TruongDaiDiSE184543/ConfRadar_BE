using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Repositories;

namespace ConfRadar.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        Task<int> SaveChangesAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private ConfRadarDbContext _context;
        private IUserRepository _UserRepository;
        private IRoleRepository _RoleRepository;
        public UnitOfWork(ConfRadarDbContext context)
        {
            _context = context;
        }
        public IUserRepository UserRepository => _UserRepository ??= new UserRepository(_context);

        public IRoleRepository RoleRepository => _RoleRepository ??= new RoleRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


    }
}
