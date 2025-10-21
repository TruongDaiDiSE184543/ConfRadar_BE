using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Payment
{
    public class MomoCreatePaymentResponse
    {
        public string partnerCode { get; set; }
        public string requestId { get; set; }
        public string orderId { get; set; }
        public long amount { get; set; }
        public long responseTime { get; set; }  
        public string message { get; set; }
        public int resultCode { get; set; }
        public string payUrl { get; set; }
    }
}
