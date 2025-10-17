using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class UserRefreshToken
{
    public string Tokenid { get; set; } = null!;

    public string Userid { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime? Expiry { get; set; }

    public DateTime? Createdat { get; set; }

    public bool? Isrevoked { get; set; }

    public virtual User User { get; set; } = null!;
}
