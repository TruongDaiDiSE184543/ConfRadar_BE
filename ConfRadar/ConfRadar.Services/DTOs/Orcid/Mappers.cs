using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Orcid
{
    public static class Mappers
    {
        public static AcademicProfile toModel(this OrcidAuthorizationResponse response,string userId,DateTime createdAt)
        {
            return new AcademicProfile
            {
                AcademicProfileId = new Guid().ToString(),
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
