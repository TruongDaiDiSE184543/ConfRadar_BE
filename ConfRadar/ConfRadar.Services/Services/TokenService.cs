using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
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
        Task<string> GenerateAccessToken(string userId, string email,bool active);
        string GenerateSecureRandomToken();
        string CreateSignature(string rawData, string secretKey);
        string CreateSignature512(string rawData, string secretKey);
        string EncryptString(string plainText, string key);
        string DecryptString(string cipherText, string key);

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

        public async Task<string> GenerateAccessToken(string userId, string email,bool active)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var userRoles = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userId);
            var activeUserRole = userRoles.Where(ur => ur.IsActive == true).ToList();
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email,email),
                new Claim(JwtRegisteredClaimNames.Sub,userId),
                new Claim("active", active ? "true" : "false")

            };
            foreach (var role in activeUserRole)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.RoleName));
            }
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.ExpiresAccessToken),
                signingCredentials: creds);
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);
            return accessToken;
        }
        public string CreateSignature(string rawData, string secretKey)
        {
            string signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            return signature;

        }
        public string CreateSignature512(string rawData, string secretKey)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
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

        public string EncryptString(string plainText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.Substring(0, 32)); // AES-256
            aes.IV = RandomNumberGenerator.GetBytes(16); // random IV

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            // Ghi IV trước vào stream
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        public string DecryptString(string cipherText, string key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.Substring(0, 32)); // AES-256

            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }



    }
}
