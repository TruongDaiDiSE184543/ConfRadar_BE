using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Collaborator
{
    public class UpdateCollabContractRequest
    {
        public bool? IsClosed { get; set; }
        [Required(ErrorMessage = "Mã hợp đồng collaborator là bắt buộc")]
        public string CollaboratorContractId { get; set; }
    }
}
