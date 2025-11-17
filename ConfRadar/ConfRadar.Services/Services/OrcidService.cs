using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using static ConfRadar.Services.Common.AppSettingConfig;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Options;
using ConfRadar.Services.DTOs.Orcid;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Net.Http.Json;

namespace ConfRadar.Services.Services
{
    public interface IOrcidService
    {
        string GenerateAuthorizationLink();
        Task<OrcidAuthorizationResponse> ExchangeCodeForTokenAsync(string authorizationCode);
    }

    public class OrcidService : IOrcidService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<OrcidSettings> _orcidSettings;
        private readonly HttpClient _httpClient;

        public OrcidService(IUnitOfWork unitOfWork, IOptions<OrcidSettings> orcidSettings, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _orcidSettings = orcidSettings;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://sandbox.orcid.org/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GenerateAuthorizationLink()
        {
            string clientId = _orcidSettings.Value.ClientId;
            string clientSecret = _orcidSettings.Value.ClientSecret;
            string redirectURl = "https://confradar.io.vn/api/Orcid/authorization";
            // You can now use clientId and clientSecret from the configuration
            // Here's an example of how ORCID authorization link might be constructed:
            string authorizationUrl = $"https://sandbox.orcid.org/oauth/authorize?client_id={clientId}&response_type=code&scope=/authenticate&redirect_uri={redirectURl}";

            return authorizationUrl;
        }

        public async Task<OrcidAuthorizationResponse> ExchangeCodeForTokenAsync(string authorizationCode)
        {
            var settings = _orcidSettings.Value;
            var redirectUri = "https://confradar.io.vn/api/Orcid/callback";
            var formData = new Dictionary<string, string>()
            {
                 { "client_id", settings.ClientId },
                 { "client_secret", settings.ClientSecret },
                 { "grant_type", "authorization_code" },
                 { "code", authorizationCode },
                 { "redirect_uri", redirectUri }
            };
            var response =await  _httpClient.PostAsync("oauth/token", new FormUrlEncodedContent(formData));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi từ Orcid khi đổi auth code sang access token {error}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<OrcidAuthorizationResponse>();
            return tokenResponse;
        }

    }
}
