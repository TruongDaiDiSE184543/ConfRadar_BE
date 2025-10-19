using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Conference
{
    public string ConferenceId { get; set; } = null!;

    public string ConferenceName { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? Capacity { get; set; }

    public string? Address { get; set; }

    public string? BannerImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsInternalHosted { get; set; }

    public bool? IsResearchConference { get; set; }

    public bool? IsActive { get; set; }

    public string? ConferenceRankingId { get; set; }

    public string? UserId { get; set; }

    public string? LocationId { get; set; }

    public string? ConferenceCategoryId { get; set; }

    public string? GlobalStatusId { get; set; }

    public virtual ConferenceCategory? ConferenceCategory { get; set; }

    public virtual ICollection<ConferenceMedium> ConferenceMedia { get; set; } = new List<ConferenceMedium>();

    public virtual ICollection<ConferencePolicy> ConferencePolicies { get; set; } = new List<ConferencePolicy>();

    public virtual ICollection<ConferencePrice> ConferencePrices { get; set; } = new List<ConferencePrice>();

    public virtual ICollection<ConferenceSession> ConferenceSessions { get; set; } = new List<ConferenceSession>();

    public virtual ICollection<FavouriteConference> FavouriteConferences { get; set; } = new List<FavouriteConference>();

    public virtual ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();

    public virtual TechnicalConferenceDetail? TechnicalConferenceDetail { get; set; }
}
