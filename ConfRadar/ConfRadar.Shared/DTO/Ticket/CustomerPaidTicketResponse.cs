namespace ConfRadar.Shared.DTO.Ticket
{
    public class CustomerPaidTicketResponse
    {
        public string TicketId { get; set; } = null!;

        public DateOnly? RegisteredDate { get; set; }

        public bool? IsRefunded { get; set; }

        public decimal? ActualPrice { get; set; }

        public List<CustomerTransactionDetailRespone> Transactions { get; set; }

        public List<CustomerCheckInDetailResponse> UserCheckIns { get; set; }



    }

    public class CustomerTransactionDetailRespone
    {
        public string TransactionId { get; set; } = null!;
        public string? Currency { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? TransactionCode { get; set; }
        public bool? IsRefunded { get; set; }
        public string? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }

    }
    public class CustomerCheckInDetailResponse
    {
        public string UserCheckinId { get; set; } = null!;
        public bool? IsPresenter { get; set; }
        public string? CheckinStatusId { get; set; }
        public string? CheckinStatusName { get; set; }
        public DateTime? CheckInTime { get; set; }
        public string? TicketId { get; set; }
        public string? ConferenceSessionId { get; set; }

        public CustomerSessionDetailResponse ConferenceSessionDetail { get; set; }


    }
    public class CustomerSessionDetailResponse
    {
        public string ConferenceSessionId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateOnly? SessionDate { get; set; }
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomDisplayName { get; set; }
        public string? DestinationId { get; set; }
        public string? DestinationName { get; set; }
        public string? CityId { get; set; }
        public string? CityName { get; set; }
        public string? District { get; set; }
        public string? Street { get; set; }
    }
}
