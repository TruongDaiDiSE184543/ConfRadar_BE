using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.User;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;

namespace ConfRadar.Services.Services
{
    public interface IAuthService
    {

        Task<int> RegisterAccount(CreateUserRequest request);
        Task VerifyRegistration(string token);

    }
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        public AuthService(IUserService userService, IPasswordHasher passwordHasher, IEmailService emailService, ITokenService tokenService)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _tokenService = tokenService;
        }
        public async Task<int> RegisterAccount(CreateUserRequest request)
        {
            var hashedPassword = _passwordHasher.Hash(request.Password);
            var verificationToken = _tokenService.GenerateVerificationToken();
            string confirmationLink = ConfRadarDomain.Url + $"api/Auth/token={verificationToken}";
            var user = UserMapper.FromCreateUserRequestToUser(request);
            user.Passwordhash = hashedPassword;
            user.Verificationtoken = verificationToken;
            user.Verificationtokenexpiry = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(24), DateTimeKind.Unspecified);
            await _emailService.SendRegistrationConfirmationEmailAsync(request.Email,request.Fullname, confirmationLink);
            return await _userService.CreateUserAsync(user);
        }
        public async Task VerifyRegistration(string token)
        {
            var user = await _userService.GetUserByRegistrationConfirmationToken(token);
            if (user == null)
            {
                throw new ConfRadarAuthenticationException("User not found");
            }
            if (user.Isemailconfirmed==true)
            {
                throw new ConfRadarAuthenticationException("User is already confirmed");
            }
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (user.Verificationtokenexpiry <= timeNow)
            {
                throw new ConfRadarAuthenticationException("Token is expired");
            }
            user.Isemailconfirmed = true;
            user.Verificationtoken = null;
            await _userService.UpdateUserAsync(user);
        }
    }
}
