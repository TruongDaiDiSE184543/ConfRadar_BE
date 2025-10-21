using ConfRadar.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static ConfRadar.Services.Common.AppSettingConfig;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ConfRadar.Services.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(string userId, string email);
        string GenerateSecureRandomToken();

    }
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        public TokenService(IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork)
        {
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateAccessToken(string userId, string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var userRoles = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userId);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email,email),
                new Claim(JwtRegisteredClaimNames.Sub,userId),

            };
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.RoleName));
            }
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresAccessToken),
                signingCredentials: creds);
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);
            return accessToken;
        }

        public string GenerateSecureRandomToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            var token = Convert.ToBase64String(randomNumber)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ""); ;
            return token;
        }
    }
}
