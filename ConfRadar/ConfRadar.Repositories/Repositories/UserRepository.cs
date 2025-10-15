using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateUserAsync(User user);


        Task<int> UpdateUserAsync(User user);



        Task<User?> GetUserByRegistrationConfirmationToken(string token);
        Task<User?> GetUserByForgetPasswordToken(string token);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByName(string name);
        Task<User?> GetUserByUserId(string userId);



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

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserByForgetPasswordToken(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Passwordresettoken == token);
        }

        public async Task<User?> GetUserByName(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Fullname == name);
        }

        public async Task<User?> GetUserByRegistrationConfirmationToken(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Verificationtoken == token);
        }

        public async Task<User?> GetUserByUserId(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Userid == userId);
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            return await UpdateAsync(user);
        }
    }
}
