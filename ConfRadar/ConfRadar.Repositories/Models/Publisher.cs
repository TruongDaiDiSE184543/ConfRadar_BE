using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Publisher
{
    public string PublisherId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public virtual ICollection<ConferencePrice> ConferencePrices { get; set; } = new List<ConferencePrice>();
}
