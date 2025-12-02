using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.User;
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
        Task<List<User>> GetReviewerList(string localReviewerRoleId);

        Task<List<AvailableCustomerResponse>> GetAvailableCustomer(List<string> systemRoleIds, List<string> conferenceStatus);
        Task<List<User>> GetUserByRole(Role role);




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

        public async Task<List<AvailableCustomerResponse>> GetAvailableCustomer(List<string> systemRoleIds, List<string> conferenceStatusIds)
        {

            var listCustomer = await (from u in _context.Users
                                      where !_context.PaperReviewers.Any(pr => pr.UserId == u.UserId
                                      && pr.Paper != null && pr.Paper.Conference != null && pr.Paper.Conference.ConferenceStatus != null &&
                                      conferenceStatusIds.Contains(pr.Paper.Conference.ConferenceStatusId))
                                      && u.IsActive == true && u.IsEmailConfirmed == true
                                      && u.UserRoles.All(ur => ur.IsActive == true)
                                      && !u.UserRoles.Any(ur => systemRoleIds.Contains(ur.RoleId))
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

        public async Task<List<User>> GetReviewerList(string localReviewerRoleId)
        {
            return await _context.Users
                .Include(u => u.ReviewerContracts)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == localReviewerRoleId) || u.ReviewerContracts.Any(rc => rc.IsActive == true))
                .ToListAsync();
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
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
            return await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }



        public async Task<int> UpdateUserAsync(User user)
        {
            return await UpdateAsync(user);
        }

        public async Task<List<User>> GetUserByRole(Role role)
        {
            return await _context.Users



                 .Include(u => u.CollaboratorContracts)
                    .ThenInclude(cc => cc.Conference)
                        .ThenInclude(c => c.ConferenceCategory)

                 .Include(u => u.CollaboratorContracts)
                    .ThenInclude(cc => cc.Conference)
                        .ThenInclude(c => c.ConferenceStatus)

                .Include(u => u.CollaboratorContracts)
                    .ThenInclude(cc => cc.Conference)
                        .ThenInclude(c => c.City)

                 .Include(u => u.CollaboratorContracts)
                    .ThenInclude(cc => cc.Conference)
                        .ThenInclude(c => c.CreatedByNavigation)


                 .Include(u => u.Organization)
                 .Where(u => u.UserRoles.Any(ur => ur.RoleId == role.RoleId))
                 .AsSplitQuery()
                 .ToListAsync();
        }
    }
}
