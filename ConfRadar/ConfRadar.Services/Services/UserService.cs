using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface IUserService
    {
        Task<int> CreateUserAsync(User user);
        Task<User?> GetUserByRegistrationConfirmationToken(string token);
        Task<int> UpdateUserAsync(User user);
    }
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateUserAsync(User user)
        {
            return await _unitOfWork.UserRepository.CreateUserAsync(user);
        }
        public async Task<int> UpdateUserAsync(User user)
        {
            return await _unitOfWork.UserRepository.UpdateUserAsync(user);
        }
        public async Task<User?> GetUserByRegistrationConfirmationToken(string token)
        {
            return await _unitOfWork.UserRepository.GetUserByRegistrationConfirmationToken(token);
        }

    }
}
