using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class User
{
    public string Userid { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public DateOnly? Birthday { get; set; }

    public string? Phonenumber { get; set; }

    public string? Gender { get; set; }

    public DateTime? Lastlogin { get; set; }

    public string? Avatarurl { get; set; }

    public string? Biodescription { get; set; }

    public bool? Isactive { get; set; }

    public bool? Isemailconfirmed { get; set; }

    public DateTime? Createdat { get; set; }

    public string? Verificationtoken { get; set; }

    public DateTime? Verificationtokenexpiry { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
