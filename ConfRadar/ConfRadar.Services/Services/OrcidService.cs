using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using static ConfRadar.Services.Common.AppSettingConfig;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Options;

namespace ConfRadar.Services.Services
{
    public interface IOrcidService
    {
        string GenerateAuthorizationLink();
    }

    public class OrcidService : IOrcidService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<OrcidSettings> _orcidSettings;

        public OrcidService(IUnitOfWork unitOfWork, IOptions<OrcidSettings> orcidSettings)
        {
            _unitOfWork = unitOfWork;
            _orcidSettings = orcidSettings;
        }

        public string GenerateAuthorizationLink()
        {
            string clientId = _orcidSettings.Value.ClientId;
            string clientSecret = _orcidSettings.Value.ClientSecret;

            // You can now use clientId and clientSecret from the configuration
            // Here's an example of how ORCID authorization link might be constructed:
            string authorizationUrl = $"https://orcid.org/oauth/authorize?client_id={clientId}&response_type=code&scope=/authenticate&redirect_uri=";

            return authorizationUrl;
        }
    }
}
