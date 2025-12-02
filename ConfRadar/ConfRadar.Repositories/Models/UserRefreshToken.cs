using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class UserRefreshToken
{
    public string TokenId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime? Expiry { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsRevoked { get; set; }

    public virtual User User { get; set; } = null!;
}
