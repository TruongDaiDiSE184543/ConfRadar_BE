using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.DTO.User;
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
        Task<List<User>> GetListUser();

        Task<List<AvailableCustomerResponse>> GetAvailalbeCustomer();



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

        public async Task<List<AvailableCustomerResponse>> GetAvailalbeCustomer()
        {
            var listCustomer = await (from u in _context.Users
                                      where !_context.PaperReviewers.Any(pr => pr.UserId == u.UserId) && u.IsActive ==true && u.IsEmailConfirmed == true
                                      select new AvailableCustomerResponse()
                                      {
                                          UserId = u.UserId,
                                          FullName = u.FullName,
                                          Email = u.Email,
                                          AvatarUrl = u.AvatarUrl,
                                      }).ToListAsync();
            return listCustomer;
        }

        public async Task<List<User>> GetListUser()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserByForgetPasswordToken(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == token);
        }

        public async Task<User?> GetUserByName(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.FullName == name);
        }

        public async Task<User?> GetUserByRegistrationConfirmationToken(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.VerificationToken == token);
        }

        public async Task<User?> GetUserByUserId(string userId)
        {
            return await _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            return await UpdateAsync(user);
        }
    }
}
