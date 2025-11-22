using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Orcid;
using System.Net.Http.Json;

namespace ConfRadar.Services.Services
{
    public interface IOrcidService
    {
        string GenerateAuthorizationLink(string scope, string userId);
        Task<OrcidAuthorizationResponse> ExchangeCodeForTokenAsync(string authorizationCode, string userId);
    }

    public class OrcidService : IOrcidService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        private readonly HttpClient _httpClient;
        private readonly string fullAccess = "read-limited%20/activities/update%20/person/update";
        public OrcidService(IUnitOfWork unitOfWork, HttpClient httpClient, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;

            // Always use sandbox for ORCID API
            _httpClient.BaseAddress = new Uri("https://sandbox.orcid.org/");

            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _timeProviderService = timeProviderService;
        }

        private readonly List<string> validScopes = new List<string>()
        {
            "authenticate ","read-limited","activities/update","person/update","webhook","read-public",
        };

        public string GenerateAuthorizationLink(string scope, string userId)
        {
            // Hardcoded ORCID credentials
            //string clientId = "APP-VD0FICYKL76Y895Y";
            //string redirectUri = "https://confradar.io.vn/api/Orcid/callback";

            if (!validScopes.Contains(scope) && scope != fullAccess)
                throw new Exception("Scope không hợp lệ");

            //localhost version orcid redirect
            string clientId = "APP-CYDYAGET07D4CRW0";
            string redirectUri = "https://localhost:7001/signin-orcid";

            string orcidBaseUrl = "https://sandbox.orcid.org";
            // Include the userId in the state parameter so we can retrieve it in the callback
            string state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userId));
            string authorizationUrl = $"{orcidBaseUrl}/oauth/authorize?client_id={clientId}&response_type=code&scope=/{scope}&redirect_uri={redirectUri}&state={state}";

            return authorizationUrl;
        }

        public async Task<OrcidAuthorizationResponse> ExchangeCodeForTokenAsync(string authorizationCode, string userId)
        {
            // Hardcoded ORCID credentials
            //string clientId = "APP-VD0FICYKL76Y895Y";
            //string clientSecret = "f8f0046f-b390-474e-a1d3-5786da93067c";
            //string redirectUri = "https://confradar.io.vn/api/Orcid/callback";

            //localhost version orcid redirect

            string redirectUri = "https://localhost:7001/signin-orcid";
            string clientId = "APP-CYDYAGET07D4CRW0";
            string clientSecret = "29854145-dffe-48d9-9a30-698be511c149";
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

            // Check if there's already an academic profile with this ORCID and scope for this user
            DateTime createAt =  ExtensionHelper.GetVietnamTime();
            var existingProfileByUserAndScope = await _unitOfWork.AcademicProfileRepository.GetAcademicProfileByUserIdAndScopeAsync(userId, tokenResponse.scope);
            var academicProfile = tokenResponse.toModel(userId, createAt);

            if (existingProfileByUserAndScope != null)
            {
                // Check if this ORCID ID with this scope already exists for another user (potential conflict)
                existingProfileByUserAndScope.UserId = userId; // Update to the current user
                existingProfileByUserAndScope.AccessToken = academicProfile.AccessToken;
                existingProfileByUserAndScope.RefreshToken = academicProfile.RefreshToken;
                existingProfileByUserAndScope.UserName = academicProfile.UserName;
                existingProfileByUserAndScope.CreatedAt = createAt;


                await _unitOfWork.AcademicProfileRepository.UpdateAcademicProfileAsync(existingProfileByUserAndScope);

            }
            else
            {
                await _unitOfWork.AcademicProfileRepository.CreateAcademicProfileAsync(academicProfile);

            }


            return tokenResponse;
        }


    }
}
