using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.Orcid;
using static System.Net.WebRequestMethods;
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
        private readonly HttpClient _httpClient;

        public OrcidService(IUnitOfWork unitOfWork, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;

            // Always use sandbox for ORCID API
            _httpClient.BaseAddress = new Uri("https://sandbox.orcid.org/");

            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GenerateAuthorizationLink()
        {
            // Hardcoded ORCID credentials
            string clientId = "APP-VD0FICYKL76Y895Y";
            string redirectUri = "https://confradar.io.vn/api/Orcid/callback";

            string orcidBaseUrl = "https://sandbox.orcid.org";
            string authorizationUrl = $"{orcidBaseUrl}/oauth/authorize?client_id={clientId}&response_type=code&scope=/authenticate&redirect_uri={redirectUri}";

            return authorizationUrl;
        }

        public async Task<OrcidAuthorizationResponse> ExchangeCodeForTokenAsync(string authorizationCode)
        {
            // Hardcoded ORCID credentials
            string clientId = "APP-VD0FICYKL76Y895Y";
            string clientSecret = "f8f0046f-b390-474e-a1d3-5786da93067c";
            string redirectUri = "https://confradar.io.vn/api/Orcid/callback";

            var formData = new Dictionary<string, string>()
            {
                 { "client_id", clientId },
                 { "client_secret", clientSecret },
                 { "grant_type", "authorization_code" },
                 { "code", authorizationCode },
                 { "redirect_uri", redirectUri }
            };

            var response = await _httpClient.PostAsync("oauth/token", new FormUrlEncodedContent(formData));
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
