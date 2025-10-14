using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IUserRefreshTokenRepository
    {
        Task<int> CreateUserRefreshToken(UserRefreshToken userRefreshToken);
        Task<int> UpdateUserRefreshToken(UserRefreshToken userRefreshToken);
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
    }
}
