using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class AcademicProfile
{
    public string AcademicProfileId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public string? Scope { get; set; }

    public string? UserName { get; set; }

    public string? OrcidId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual ICollection<OrcidDataCache> OrcidDataCaches { get; set; } = new List<OrcidDataCache>();

    public virtual User? User { get; set; }
}
