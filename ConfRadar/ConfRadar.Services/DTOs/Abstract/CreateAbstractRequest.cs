using ConfRadar.Services.Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Abstract
{
    public class CreateAbstractRequest
    {
        [Required(ErrorMessage = "Abstract file bắt buộc")]
        public IFormFile AbstractFile { get; set; }
        [Required(ErrorMessage = "Conference Price id bắt buộc")]
        public string ConferencePriceId { get; set; }
        [Required(ErrorMessage = "Payment method bắt buộc")]
        [EnumDataType(typeof(PaymentMethodEnum))]
        public PaymentMethodEnum PaymentMethod { get; set; }
    }

    public class CreateFullPaperRequest
    {
        [Required]
        public IFormFile FullPaperFile { get; set; }
        [Required]
        public string PaperId { get; set; }
    }

    public class FullPaperResponse
    {
        public string? ReviewStatus { get; set; }
        public string? FullPaperURL {  get; set; }
    }
}
