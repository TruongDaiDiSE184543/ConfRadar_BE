using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IUserRefreshTokenRepository
    {
        Task<int> CreateUserRefreshToken(UserRefreshToken userRefreshToken);
        Task<int> UpdateUserRefreshToken(UserRefreshToken userRefreshToken);
        Task<UserRefreshToken?> GetUserRefreshTokenByRefreshToken(string userId, string refreshToken);
    }
    public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(ConfRadarDbContext context) : base(context)
        {
        }
        public async Task<int> CreateUserRefreshToken(UserRefreshToken userRefreshToken)
        {
            return await CreateAsync(userRefreshToken);
        }
        public async Task<int> UpdateUserRefreshToken(UserRefreshToken userRefreshToken)
        {
            return await UpdateAsync(userRefreshToken);
        }
        public async Task<UserRefreshToken?> GetUserRefreshTokenByRefreshToken(string userId, string refreshToken)
        {
            return await _context.UserRefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Token == refreshToken);
        }
    }
}
