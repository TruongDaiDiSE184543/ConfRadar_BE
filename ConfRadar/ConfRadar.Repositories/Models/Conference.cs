using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Conference
{
    public string ConferenceId { get; set; } = null!;

    public string? ConferenceName { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? TotalSlot { get; set; }

    public int? AvailableSlot { get; set; }

    public string? Address { get; set; }

    public string? BannerImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateOnly? TicketSaleStart { get; set; }

    public DateOnly? TicketSaleEnd { get; set; }

    public bool? IsInternalHosted { get; set; }

    public bool? IsResearchConference { get; set; }

    public string? CityId { get; set; }

    public string? CreatedBy { get; set; }

    public string? ConferenceCategoryId { get; set; }

    public string? ConferenceStatusId { get; set; }

    public virtual City? City { get; set; }

    public virtual CollaboratorContract? CollaboratorContract { get; set; }

    public virtual ConferenceCategory? ConferenceCategory { get; set; }

    public virtual ICollection<ConferenceMedium> ConferenceMedia { get; set; } = new List<ConferenceMedium>();

    public virtual ICollection<ConferencePrice> ConferencePrices { get; set; } = new List<ConferencePrice>();

    public virtual ICollection<ConferenceSession> ConferenceSessions { get; set; } = new List<ConferenceSession>();

    public virtual ConferenceStatus? ConferenceStatus { get; set; }

    public virtual ICollection<ConferenceTimeline> ConferenceTimelines { get; set; } = new List<ConferenceTimeline>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<FavouriteConference> FavouriteConferences { get; set; } = new List<FavouriteConference>();

    public virtual ICollection<MaterialDownload> MaterialDownloads { get; set; } = new List<MaterialDownload>();

    public virtual ICollection<PaperWaitList> PaperWaitLists { get; set; } = new List<PaperWaitList>();

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public virtual ICollection<RankingFileUrl> RankingFileUrls { get; set; } = new List<RankingFileUrl>();

    public virtual ICollection<RankingReferenceUrl> RankingReferenceUrls { get; set; } = new List<RankingReferenceUrl>();

    public virtual ICollection<RefundPolicy> RefundPolicies { get; set; } = new List<RefundPolicy>();

    public virtual ResearchConferenceDetail? ResearchConferenceDetail { get; set; }

    public virtual ICollection<ResearchConferencePhase> ResearchConferencePhases { get; set; } = new List<ResearchConferencePhase>();

    public virtual ICollection<ReviewerContract> ReviewerContracts { get; set; } = new List<ReviewerContract>();

    public virtual ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();

    public virtual TechnicalConferenceDetail? TechnicalConferenceDetail { get; set; }
}
