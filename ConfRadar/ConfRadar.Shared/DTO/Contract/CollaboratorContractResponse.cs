namespace ConfRadar.Shared.DTO.Contract
{
    public class CollaboratorContractResponse
    {
        public string CollaboratorContractId { get; set; } = null!;

        public string? CollaboratorContractUserId { get; set; }
        public string? OrganizationId { get; set; } = null!;
        public string? OrganizationDescription { get; set; }
        public string? OrganizationName { get; set; }


        public bool? IsSponsorStep { get; set; }
        public bool? IsMediaStep { get; set; }
        public bool? IsPolicyStep { get; set; }

        public bool? IsSessionStep { get; set; }

        public bool? IsPriceStep { get; set; }

        public bool? IsTicketSelling { get; set; }

        public bool? IsClosed { get; set; }

        public DateOnly? SignDay { get; set; }

        public DateOnly? FinalizePaymentDate { get; set; }

        public int? Commission { get; set; }
        public string? ContractUrl { get; set; }

        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceDescription { get; set; }
        public DateOnly? ConferenceStartDate { get; set; }
        public DateOnly? ConferenceEndDate { get; set; }
        public int? ConferenceTotalSlot { get; set; }
        public int? ConferenceAvailableSlot { get; set; }
        public string? ConferenceAddress { get; set; }
        public string? ConferenceBannerImageUrl { get; set; }
        public DateTime? ConferenceCreatedAt { get; set; }
        public DateOnly? ConferenceTicketSaleStart { get; set; }
        public DateOnly? ConferenceTicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCreatedBy { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceCategoryName { get; set; }
        public string? ConferenceStatusId { get; set; }
        public string? ConferenceStatusName { get; set; }

    }
    public class OwnCollaboratorContractDetailResponse
    {
        public string? CollaboratorContractId { get; set; } 
        public string? UserId { get; set; }
        public bool? IsSponsorStep { get; set; }
        public bool? IsMediaStep { get; set; }
        public bool? IsPolicyStep { get; set; }
        public bool? IsSessionStep { get; set; }
        public bool? IsPriceStep { get; set; }
        public bool? IsTicketSelling { get; set; }
        public bool? IsClosed { get; set; }
        public DateOnly? SignDay { get; set; }
        public DateOnly? FinalizePaymentDate { get; set; }
        public int? Commission { get; set; }
        public string? ContractUrl { get; set; }
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }

        public string? ConferenceDescription { get; set; }

        public DateOnly? ConferenceStartDate { get; set; }

        public DateOnly? ConferenceEndDate { get; set; }

        public int? TotalSlot { get; set; }

        public int? AvailableSlot { get; set; }

        public string? Address { get; set; }

        public string? BannerImageUrl { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateOnly? TicketSaleStart { get; set; }

        public DateOnly? TicketSaleEnd { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? CityName { get; set; }
        public string? ConferenceCreatedById { get; set; }
        public string? ConferenceCreatedByName { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceCategoryName { get; set; }

        public string? ConferenceStatusId { get; set; }
        public string? ConferenceStatusName { get; set; }

    }
}
