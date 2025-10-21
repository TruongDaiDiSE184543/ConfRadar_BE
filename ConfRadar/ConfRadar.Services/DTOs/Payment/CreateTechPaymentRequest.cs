using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Payment
{
    public class CreateTechPaymentRequest
    {
        [Required(ErrorMessage ="Price id is required!")]
        public string ConferencePriceId { get; set; } 

    }
}
