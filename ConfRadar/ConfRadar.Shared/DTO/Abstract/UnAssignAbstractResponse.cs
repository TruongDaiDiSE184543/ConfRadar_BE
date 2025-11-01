namespace ConfRadar.Shared.DTO.Abstract
{
    public class UnAssignAbstractResponse
    {
        public string AbstractId { get; set; } = null!;
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? AbstractUrl { get; set; }
        public string? PaperId { get; set; }
    }
}
