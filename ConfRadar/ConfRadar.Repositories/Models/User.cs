using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? BirthDay { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateTime? LastLogin { get; set; }

    public string? AvatarUrl { get; set; }

    public string? BioDescription { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsEmailConfirmed { get; set; }

    public string? LoginProvider { get; set; }

    public string? VerificationToken { get; set; }

    public DateTime? VerificationTokenExpiry { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<FavouriteConference> FavouriteConferences { get; set; } = new List<FavouriteConference>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserCheckIn> UserCheckIns { get; set; } = new List<UserCheckIn>();

    public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
