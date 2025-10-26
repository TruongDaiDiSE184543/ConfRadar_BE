using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class GeneralFaq
{
    public string GeneralFaqid { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }
}
