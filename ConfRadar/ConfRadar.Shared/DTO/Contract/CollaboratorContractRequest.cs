using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Contract
{
    public class CollaboratorContractRequest
    {
        [Required(ErrorMessage = "User id là bắt buộc")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "IsSponsorStep bắt buộc")]

        public bool? IsSponsorStep { get; set; }
        [Required(ErrorMessage = "IsMediaStep bắt buộc")]

        public bool? IsMediaStep { get; set; }
        [Required(ErrorMessage = "IsPolicyStep bắt buộc")]

        public bool? IsPolicyStep { get; set; }
        [Required(ErrorMessage = "IsSessionStep bắt buộc")]

        public bool? IsSessionStep { get; set; }
        [Required(ErrorMessage = "IsPriceStep bắt buộc")]

        public bool? IsPriceStep { get; set; }
        [Required(ErrorMessage = "Ticket selling là bắt buộc")]
        public bool? IsTicketSelling { get; set; }
        [Required(ErrorMessage = "Ngày kí hợp đồng là bắt buộc")]

        public DateOnly? SignDay { get; set; }
        [Required(ErrorMessage = "Ngày giải ngân là bắt buộc")]

        public DateOnly? FinalizePaymentDate { get; set; }

        public int? Commission { get; set; }


        [Required(ErrorMessage = "File hợp đồng là bắt buộc")]
        public IFormFile ContractFile { get; set; }
        [Required(ErrorMessage = "Mã hội nghị là bắt buộc")]
        public string ConferenceId { get; set; }
    }

    public class CollaboratorContractSearchParam
    {
        public string? ConferenceName { get; set; }
        public string? OrganizationId { get; set; }
      
        public string? UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
