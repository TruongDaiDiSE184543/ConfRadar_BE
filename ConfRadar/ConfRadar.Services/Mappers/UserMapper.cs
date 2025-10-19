using ConfRadar.Repositories.Models;
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
                IsActive = true,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
        }
    }
}
