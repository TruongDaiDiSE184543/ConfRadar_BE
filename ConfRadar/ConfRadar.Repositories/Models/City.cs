using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class City
{
    public string CityId { get; set; } = null!;

    public string? CityName { get; set; }

    public virtual ICollection<Conference> Conferences { get; set; } = new List<Conference>();

    public virtual ICollection<Destination> Destinations { get; set; } = new List<Destination>();
}
