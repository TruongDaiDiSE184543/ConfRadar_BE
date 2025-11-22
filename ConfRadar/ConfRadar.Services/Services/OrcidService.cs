using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Orcid;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace ConfRadar.Services.Services
{
    public interface IOrcidService
    {
        string GenerateAuthorizationLink(string scope, string userId);
        Task<OrcidAuthorizationResponse> ExchangeCodeForTokenAsync(string authorizationCode, string userId);
        Task<String> SyncWorksAsync(string userId);
        Task<string> SyncBiographyAsync(string userId);
        Task<string> SyncEducationAsync(string userId);

    }

    public class OrcidService : IOrcidService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        private readonly HttpClient _httpClient;
        private readonly string fullAccess = "read-limited%20/activities/update%20/person/update";
        private readonly string baseVersion3 = "https://api.sandbox.orcid.org/v3.0/";
        private readonly string localRedirect = "https://localhost:7001/signin-orcid";
        private readonly string deployedRedirect = "https://confradar.io.vn/api/Orcid/callback";
        public OrcidService(IUnitOfWork unitOfWork, HttpClient httpClient, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;

            // Always use sandbox for ORCID API
            _httpClient.BaseAddress = new Uri("https://api.sandbox.orcid.org/");

            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _timeProviderService = timeProviderService;
        }

        #region 
        private readonly List<string> validScopes = new List<string>()
        {
            "authenticate ","read-limited","activities/update","person/update","webhook","read-public",
        };
        private async Task RefreshTokenAsync(AcademicProfile profile)
        {
            // Lấy Client ID và Secret từ cấu hình thay vì hardcode
            string clientId = "APP-CYDYAGET07D4CRW0";
            string clientSecret = "29854145-dffe-48d9-9a30-698be511c149";

            var formData = new Dictionary<string, string>()
             {
                 { "client_id", clientId },
                 { "client_secret", clientSecret },
                 { "grant_type", "refresh_token" },
                 { "refresh_token", profile.RefreshToken } // Dùng refresh token đang có
             };

            // Endpoint để refresh token
            var response = await _httpClient.PostAsync("https://sandbox.orcid.org/oauth/token", new FormUrlEncodedContent(formData));

            if (!response.IsSuccessStatusCode)
            {
                // Nếu refresh token thất bại (ví dụ: người dùng đã thu hồi quyền),
                // bạn cần xử lý, có thể là xóa profile hoặc đánh dấu là "invalid"
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Không thể refresh token cho ORCID {profile.OrcidId}. Lỗi: {error}. Người dùng cần phải xác thực lại.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<OrcidAuthorizationResponse>();

            // CẬP NHẬT PROFILE VỚI TOKEN MỚI
            profile.AccessToken = tokenResponse.access_token;
            profile.RefreshToken = tokenResponse.refresh_token; // QUAN TRỌNG: ORCID có thể trả về refresh token mới
            //profile.ExpiresAt = ExtensionHelper.GetVietnamTime().AddSeconds(tokenResponse.expires_in);
            profile.ExpiresAt = ExtensionHelper.GetVietnamTime().AddSeconds(120);

            // Lưu ngay lập tức thay đổi vào DB
            await _unitOfWork.AcademicProfileRepository.UpdateAcademicProfileAsync(profile);
        }

        private async Task<AcademicProfile> GetValidAccessTokenAsync(string userId, string requiredScope)
        {
            // 1. Lấy profile từ DB
            var userProfile = await _unitOfWork.AcademicProfileRepository.GetAcademicProfileByUserIdAndScopeAsync(userId, requiredScope);
            if (userProfile == null)
            {
                throw new Exception($"Không tìm thấy scope {requiredScope} cho user với ID {userId}");
            }

            // 2. Kiểm tra token có hết hạn không (trừ đi 5 phút để an toàn)
            if (userProfile.ExpiresAt == null || userProfile.ExpiresAt <= ExtensionHelper.GetVietnamTime().AddMinutes(5))
            {
                // Token đã hết hạn hoặc sắp hết hạn -> Gọi refresh
                await RefreshTokenAsync(userProfile);
            }

            // 3. Trả về access token (bây giờ đã chắc chắn là hợp lệ)
            return userProfile;
        }
        #endregion


        public string GenerateAuthorizationLink(string scope, string userId)
        {
            // Hardcoded ORCID credentials
            //string clientId = "APP-VD0FICYKL76Y895Y";
            //string redirectUri = "https://confradar.io.vn/api/Orcid/callback";

            if (!validScopes.Contains(scope) && scope != fullAccess)
                throw new Exception("Scope không hợp lệ");

            //localhost version orcid redirect
            string clientId = "APP-CYDYAGET07D4CRW0";
            //string redirectUri = "https://localhost:7001/signin-orcid";

            string orcidBaseUrl = "https://sandbox.orcid.org";
            // Include the userId in the state parameter so we can retrieve it in the callback
            string state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userId));
            string authorizationUrl = $"{orcidBaseUrl}/oauth/authorize?client_id={clientId}&response_type=code&scope=/{scope}&redirect_uri={deployedRedirect}&state={state}";

            return authorizationUrl;
        }

        public async Task<OrcidAuthorizationResponse> ExchangeCodeForTokenAsync(string authorizationCode, string userId)
        {
            // Hardcoded ORCID credentials
            //string clientId = "APP-VD0FICYKL76Y895Y";
            //string clientSecret = "f8f0046f-b390-474e-a1d3-5786da93067c";
            //string redirectUri = "https://confradar.io.vn/api/Orcid/callback";

            //localhost version orcid redirect

            //string redirectUri = "https://localhost:7001/signin-orcid";
            string clientId = "APP-CYDYAGET07D4CRW0";
            string clientSecret = "29854145-dffe-48d9-9a30-698be511c149";
            var formData = new Dictionary<string, string>()
            {
                 { "client_id", clientId },
                 { "client_secret", clientSecret },
                 { "grant_type", "authorization_code" },
                 { "code", authorizationCode },
                 { "redirect_uri", deployedRedirect }
            };

            var response = await _httpClient.PostAsync("oauth/token", new FormUrlEncodedContent(formData));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi từ Orcid khi đổi auth code sang access token {error}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<OrcidAuthorizationResponse>();

            if (!string.IsNullOrEmpty(tokenResponse.scope) && tokenResponse.scope.StartsWith("/"))
            {
                tokenResponse.scope = tokenResponse.scope.TrimStart('/');
            }

            // Check if there's already an academic profile with this ORCID and scope for this user
            DateTime createAt = ExtensionHelper.GetVietnamTime();
            var existingProfileByUserAndScope = await _unitOfWork.AcademicProfileRepository.GetAcademicProfileByUserIdAndScopeAsync(userId, tokenResponse.scope);
            var academicProfile = tokenResponse.toModel(userId, createAt);
            //academicProfile.ExpiresAt = ExtensionHelper.GetVietnamTime().AddSeconds(tokenResponse.expires_in);
            academicProfile.ExpiresAt = ExtensionHelper.GetVietnamTime().AddMinutes(2);

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

       
        public async Task<String> SyncWorksAsync(string userId)
        {
            // === PHẦN 1: GỌI API ĐỂ LẤY DỮ LIỆU GỐC ===
            var userToken = await GetValidAccessTokenAsync(userId, "read-limited");
            if (userToken == null)
            {
                throw new Exception($"Không tìm thấy scope read-limited cho user với ID {userId}");
            }

            var requestUrl = $"https://api.sandbox.orcid.org/v3.0/{userToken.OrcidId}/works";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken.AccessToken);

            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi khi lấy dữ liệu works từ ORCID: {error}");
            }

            //var orcidJsonString = await response.Content.ReadAsStringAsync();

            // === PHẦN 2: PARSE VÀ TRÍCH XUẤT THÔNG TIN CHÍNH ===
            //var orcidWorksData = JsonSerializer.Deserialize<OrcidWorksResponse>(orcidJsonString);

            var orcidWorksData = await response.Content.ReadFromJsonAsync<OrcidWorksResponse>();

            var simpleWorksList = new List<WorkConfRadarResponse>();

            if (orcidWorksData != null && orcidWorksData.Group != null)
            {
                foreach (var summary in orcidWorksData.Group.SelectMany(g => g.WorkSummary))
                {
                    // ===== SỬA ĐỔI LOGIC LẤY ĐỊNH DANH (IDENTIFIER) =====
                    var identifier = summary.ExternalIds?.ExternalId
                        .FirstOrDefault(id =>
                            "doi".Equals(id.Type, StringComparison.OrdinalIgnoreCase) ||
                            "urn".Equals(id.Type, StringComparison.OrdinalIgnoreCase) ||
                            "ark".Equals(id.Type, StringComparison.OrdinalIgnoreCase) // Thêm cả "ark" cho chắc
                        );

                    var simpleWork = new WorkConfRadarResponse
                    {
                        OrcidPutCode = summary.PutCode,
                        Title = summary.Title?.Title?.Value,
                        WorkType = summary.Type,
                        PublicationYear = summary.PublicationDate?.Year?.Value,
                        Doi = identifier?.Value,
                        Link = summary.Url?.Value
                    };
                    simpleWorksList.Add(simpleWork);
                }
            }

            // === PHẦN 3: CHUYỂN DANH SÁCH ĐƠN GIẢN THÀNH JSON VÀ LƯU VÀO DB ===

            // Chuyển danh sách DTO thành một chuỗi JSON sạch sẽ
            string simpleJsonToStore = JsonSerializer.Serialize(simpleWorksList);
            DateTime now = ExtensionHelper.GetVietnamTime();

            // Lấy profile từ DB để cập nhật
            var orcidDataCache = await _unitOfWork.OrcidDataCacheRepository.GetOrcidDataCacheByAcademicProfileIdAndDataTypeAsync(userToken.AcademicProfileId, OrcidDataTypeEnum.Works.ToString()); // Cần có phương thức này
            if (orcidDataCache == null)
            {
                OrcidDataCache newCache = new OrcidDataCache()
                {
                    AcademicProfileId = userToken.AcademicProfileId,
                    DataType = OrcidDataTypeEnum.Works.ToString(),
                    JsonContent = simpleJsonToStore,
                    OrcidDataCacheId = Guid.NewGuid().ToString(),
                    LastSyncedAt = now,
                };

                await _unitOfWork.OrcidDataCacheRepository.CreateOrcidDataCacheAsync(newCache);


            }
            else
            {
                orcidDataCache.JsonContent = simpleJsonToStore;
                orcidDataCache.LastSyncedAt = now;


                // Cập nhật và lưu thay đổi
                await _unitOfWork.OrcidDataCacheRepository.UpdateOrcidDataCacheAsync(orcidDataCache);
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SyncBiographyAsync(string userId)
        {
            // === PHẦN 1: GỌI API ĐỂ LẤY DỮ LIỆU GỐC (Giữ nguyên) ===
            var userToken = await GetValidAccessTokenAsync(userId, "read-limited");
            if (userToken == null)
            {
                throw new Exception($"Không tìm thấy scope read-limited cho user với ID {userId}");
            }

            var requestUrl = $"https://api.sandbox.orcid.org/v3.0/{userToken.OrcidId}/biography";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken.AccessToken);

            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi khi lấy dữ liệu biography từ ORCID: {error}");
            }

            // === PHẦN 2: PARSE VÀ TRÍCH XUẤT THÔNG TIN CHÍNH ===
            var orcidBioData = await response.Content.ReadFromJsonAsync<OrcidBiographyResponse>();

            if (orcidBioData == null)
            {
                throw new Exception("Không thể parse dữ liệu biography từ ORCID.");
            }

            // Xử lý dữ liệu để tạo DTO đơn giản
            var simpleBioDto = new BiographyConfRadarResponse
            {
                Content = orcidBioData.Content,
                // Chuyển đổi Unix timestamp (milliseconds) sang DateTime
                LastModified = DateTimeOffset.FromUnixTimeMilliseconds(orcidBioData.LastModifiedDate.Value).UtcDateTime
            };

            // === PHẦN 3: CHUYỂN DTO THÀNH JSON VÀ LƯU VÀO DB ===
            string simpleJsonToStore = JsonSerializer.Serialize(simpleBioDto);
            DateTime now = ExtensionHelper.GetVietnamTime();

            var orcidDataCache = await _unitOfWork.OrcidDataCacheRepository.GetOrcidDataCacheByAcademicProfileIdAndDataTypeAsync(userToken.AcademicProfileId, OrcidDataTypeEnum.Biography.ToString());

            if (orcidDataCache == null)
            {
                // Tạo mới nếu chưa có
                OrcidDataCache newCache = new OrcidDataCache()
                {
                    AcademicProfileId = userToken.AcademicProfileId,
                    DataType = OrcidDataTypeEnum.Biography.ToString(),
                    JsonContent = simpleJsonToStore,
                    OrcidDataCacheId = Guid.NewGuid().ToString(),
                    LastSyncedAt = now
                };
                await _unitOfWork.OrcidDataCacheRepository.CreateOrcidDataCacheAsync(newCache);
            }
            else
            {
                // Cập nhật nếu đã có
                orcidDataCache.JsonContent = simpleJsonToStore;
                orcidDataCache.LastSyncedAt = now;
                await _unitOfWork.OrcidDataCacheRepository.UpdateOrcidDataCacheAsync(orcidDataCache);
            }

            // Lưu thay đổi vào DB
            // Trả về chuỗi JSON đã được xử lý cho frontend
            return simpleJsonToStore;
        }

        public async Task<string> SyncEducationAsync(string userId)
        {
            // === PHẦN 1: GỌI API (Giữ nguyên) ===
            var userToken = await GetValidAccessTokenAsync(userId, "read-limited");
            if (userToken == null)
            {
                throw new Exception($"Không tìm thấy scope read-limited cho user với ID {userId}");
            }

            var requestUrl = $"https://api.sandbox.orcid.org/v3.0/{userToken.OrcidId}/educations";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken.AccessToken);

            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi khi lấy dữ liệu educations từ ORCID: {error}");
            }

            // === PHẦN 2: PARSE VÀ XỬ LÝ DỮ LIỆU ===
            var orcidEduData = await response.Content.ReadFromJsonAsync<OrcidEducationsResponse>();
            var simpleEduList = new List<EducationConfRadarResponse>();

            if (orcidEduData != null && orcidEduData.AffiliationGroup != null)
            {
                // Dùng SelectMany để làm phẳng cấu trúc lồng nhau
                foreach (var summaryContainer in orcidEduData.AffiliationGroup.SelectMany(g => g.Summaries))
                {
                    var edu = summaryContainer.EducationSummary;

                    // Xử lý để tạo chuỗi thời gian (Period)
                    string startYear = edu.StartDate?.Year?.Value;
                    string endYear = edu.EndDate?.Year?.Value;
                    string period = (startYear, endYear) switch
                    {
                        (not null, not null) => $"{startYear} - {endYear}",
                        (not null, null) => $"{startYear} - Present",
                        (null, not null) => $"Until {endYear}",
                        _ => null
                    };

                    var simpleEdu = new EducationConfRadarResponse
                    {
                        OrcidPutCode = edu.PutCode,
                        Degree = edu.RoleTitle,
                        Institution = edu.Organization?.Name,
                        Period = period,
                        Location = $"{edu.Organization?.Address?.City}, {edu.Organization?.Address?.Country}"
                    };
                    simpleEduList.Add(simpleEdu);
                }
            }

            // === PHẦN 3: LƯU VÀO DB ===
            string simpleJsonToStore = JsonSerializer.Serialize(simpleEduList);
            DateTime now = ExtensionHelper.GetVietnamTime();

            var orcidDataCache = await _unitOfWork.OrcidDataCacheRepository.GetOrcidDataCacheByAcademicProfileIdAndDataTypeAsync(userToken.AcademicProfileId, OrcidDataTypeEnum.Education.ToString());

            if (orcidDataCache == null)
            {
                OrcidDataCache newCache = new OrcidDataCache
                {
                    AcademicProfileId = userToken.AcademicProfileId,
                    DataType = OrcidDataTypeEnum.Education.ToString(),
                    JsonContent = simpleJsonToStore,
                    OrcidDataCacheId = Guid.NewGuid().ToString(),
                    LastSyncedAt = now
                };
                await _unitOfWork.OrcidDataCacheRepository.CreateOrcidDataCacheAsync(newCache);
            }
            else
            {
                orcidDataCache.JsonContent = simpleJsonToStore;
                orcidDataCache.LastSyncedAt = now;
                await _unitOfWork.OrcidDataCacheRepository.UpdateOrcidDataCacheAsync(orcidDataCache);
            }

            return simpleJsonToStore;
        }
    }
}
