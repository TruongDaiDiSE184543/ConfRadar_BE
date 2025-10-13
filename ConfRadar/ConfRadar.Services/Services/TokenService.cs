using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface ITokenService
    {
        string GenerateVerificationToken();
    }
    public class TokenService : ITokenService
    {
        public string GenerateVerificationToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
