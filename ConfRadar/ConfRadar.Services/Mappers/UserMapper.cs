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
                Userid = Guid.NewGuid().ToString(),
                Email = request.Email,

                Fullname = request.FullName,
                Birthday = request.Birthday,
                Phonenumber = request.PhoneNumber,
                Gender = request.Gender,
                Biodescription = request.BioDescription,
                Isactive = true,
                Isemailconfirmed = false,
                Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
        }
    }
}
