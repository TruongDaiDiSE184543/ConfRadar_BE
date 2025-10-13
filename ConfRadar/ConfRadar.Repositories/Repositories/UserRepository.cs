using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateUserAsync(User user);
        Task<User?> GetUserByRegistrationConfirmationToken(string token);
        Task<int> UpdateUserAsync(User user);

    }
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateUserAsync(User user)
        {
            return await CreateAsync(user);
        }
        public async Task<User?> GetUserByRegistrationConfirmationToken(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Verificationtoken == token);
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            return await UpdateAsync(user);
        }
    }
}
