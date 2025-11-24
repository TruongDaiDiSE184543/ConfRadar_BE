using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.DTOs.Orcid
{
    public static class Mappers
    {
        public static AcademicProfile toModel(this OrcidAuthorizationResponse response, string userId, DateTime createdAt)
        {
            return new AcademicProfile
            {
                AcademicProfileId = Guid.NewGuid().ToString(),
                AccessToken = response.access_token,
                RefreshToken = response.refresh_token,
                OrcidId = response.orcid,
                Scope = response.scope,
                UserName = response.name,
                UserId = userId,
                CreatedAt = createdAt
            };
        }
    }
}
