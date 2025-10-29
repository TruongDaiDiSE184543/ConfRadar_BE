using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Mappers
{
    public static class PaperReviewerMapper
    {
        
    }
    public class PapersAssignedToReviewerResponse()
    {
        public Paper Paper { get; set; }
        public string phaseName { get; set; }
    }
}
