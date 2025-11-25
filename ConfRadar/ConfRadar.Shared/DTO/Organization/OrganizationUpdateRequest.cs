using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Organization
{
    public class OrganizationUpdateRequest
    {
        [Required(ErrorMessage = "Mã organization là bắt buộc")]
        public string? OrganizationId { get; set; }
        public string? OrganizationDescription { get; set; }
        public string? OrganizationName { get; set; }
    }
}
