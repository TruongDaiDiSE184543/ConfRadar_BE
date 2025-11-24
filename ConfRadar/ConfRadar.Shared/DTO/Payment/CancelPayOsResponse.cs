using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ConfRadar.Shared.DTO.Payment
{
    public class CancelPayOsRequest
    {
        [Required(ErrorMessage = "code là bắt buộc")]
        [JsonPropertyName("code")]

        public string Code { get; set; }
        [Required(ErrorMessage = "id là bắt buộc")]
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("cancel")]

        public bool Cancel { get; set; }
        [JsonPropertyName("status")]

        public string Status { get; set; }
        [JsonPropertyName("orderCode")]

        public string OrderCode { get; set; }

    }
    public class PayOSCancelOrderResponse
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public PayOSCancelOrderData Data { get; set; }
        public string Signature { get; set; }
    }

    public class PayOSCancelOrderData
    {
        public string Id { get; set; }
        public long OrderCode { get; set; }
        public long Amount { get; set; }
        public long AmountPaid { get; set; }
        public long AmountRemaining { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public object Transactions { get; set; }  // không quan tâm
        public DateTime? CanceledAt { get; set; }
        public string CancellationReason { get; set; }
    }

}
