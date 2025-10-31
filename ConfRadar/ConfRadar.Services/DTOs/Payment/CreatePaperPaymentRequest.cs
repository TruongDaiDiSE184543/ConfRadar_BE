using ConfRadar.Services.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Payment
{
    public class CreatePaperPaymentRequest
    {
        [Required(ErrorMessage = "Conference Price id bắt buộc")]
        public string ConferencePriceId { get; set; }
        
        //[Required(ErrorMessage = "Payment method bắt buộc")]
        //[EnumDataType(typeof(PaymentMethodEnum))]
        //public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
