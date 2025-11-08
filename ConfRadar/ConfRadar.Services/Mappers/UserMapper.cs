using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.User;

namespace ConfRadar.Services.Mappers
{
    public static class UserMapper
    {
        public static User FromCreateUserRequestToUser(CreateUserRequest request)
        {
            return new User()
            {
                UserId = Guid.NewGuid().ToString(),
                Email = request.Email,

                FullName = request.FullName,
                BirthDay = request.Birthday,
                PhoneNumber = request.PhoneNumber,
                Gender = request.Gender?.ToString(),
                BioDescription = request.BioDescription,
                IsActive = false,
                IsEmailConfirmed = false,
                CreatedAt = ExtensionHelper.GetVietnamTime(),
            };
        }
    }
}
