using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Transaction
{
    public class TransactionDetailResponse
    {
        public string TransactionId { get; set; } = null!;

        public string? UserId { get; set; }

        public string? Currency { get; set; }

        public decimal? Amount { get; set; }

        public string? TransactionCode { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? TransactionStatusId { get; set; }
        public string? TransactionTypeId { get; set; }
        public string? PaymentMethodId { get; set; }
        public string PaymentStatusName { get; set; }
        public string PaymentMethodName { get; set; }





    }
}
