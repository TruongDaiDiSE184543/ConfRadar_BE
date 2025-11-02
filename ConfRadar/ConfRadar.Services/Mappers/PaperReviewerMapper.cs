using ConfRadar.Repositories.Models;

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
