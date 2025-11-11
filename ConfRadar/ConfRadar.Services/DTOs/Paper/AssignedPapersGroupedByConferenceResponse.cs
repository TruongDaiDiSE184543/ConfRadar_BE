namespace ConfRadar.Services.DTOs.Paper
{
    public class ConferenceWithAssignedPapersResponse
    {
        public string ConferenceId { get; set; }
        public string ConferenceName { get; set; }
        public List<BasicAssignedPaperResponse> AssignedPapers { get; set; } = new List<BasicAssignedPaperResponse>();
    }

    public class BasicAssignedPaperResponse
    {
        public string PaperId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? PaperPhaseId { get; set; }
        public string? PaperPhaseName { get; set; }
        
        // Basic IDs only - no full objects
        public string? AbstractId { get; set; }
        public string? FullPaperId { get; set; }
        public string? CameraReadyId { get; set; }
        public string? RevisionPaperId { get; set; }
        
      
    }
}