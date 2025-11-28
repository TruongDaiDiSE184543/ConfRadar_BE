using ConfRadar.Shared.DTO.Contract;

namespace ConfRadar.Shared.DTO.Collaborator
{
    public class CollaboratorDetailResponse
    {
        public string? OrganizationId { get; set; }
        public string? OrganizationDescription { get; set; }
        public string? OrganizationName { get; set; }

        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }
        public string? BioDescription { get; set; }


        //contract
        public List<CollaboratorContractResponse> ContractDetail { get; set; } = new List<CollaboratorContractResponse>();



    }
}
