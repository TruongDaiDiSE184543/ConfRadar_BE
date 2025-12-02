using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Destination
{
    public string DestinationId { get; set; } = null!;

    public string? Name { get; set; }

    public string? CityId { get; set; }

    public string? District { get; set; }

    public string? Street { get; set; }

    public virtual City? City { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
