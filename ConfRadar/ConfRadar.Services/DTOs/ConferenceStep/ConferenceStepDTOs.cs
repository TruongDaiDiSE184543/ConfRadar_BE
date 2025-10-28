using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.ConferenceStep
{
    // Step 1: Basic Conference Information
    public class CreateTechnicalConferenceBasicRequest
    {
        [Required(ErrorMessage = "Tên hội nghị là bắt buộc")]
        [MaxLength(255)]
        public string ConferenceName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateOnly EndDate { get; set; }
        [Required(ErrorMessage = "Tổng số slot là bắt buộc")]
        public int TotalSlot { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public IFormFile BannerImageFile { get; set; }
        public string? bannerImageFileUrl { get; set; }

        [Required(ErrorMessage = "Bạn cần xác định hội nghị này do nội bộ tổ chức hay không")]
        public bool? IsInternalHosted { get; set; }
        [Required(ErrorMessage = "Đây có phải là hội nghị nghiên cứu không?")]

        public bool? IsResearchConference { get; set; }

        [Required(ErrorMessage = "ID danh mục là bắt buộc")]
        [MaxLength(50)]
        public string ConferenceCategoryId { get; set; }

        [Required(ErrorMessage = "ID thành phố là bắt buộc")]
        public string CityId { get; set; }
        [Required(ErrorMessage = "Ngày bắt đầu bán vé là bắt buộc")]
        public DateOnly TicketSaleStart { get; set; }
        [Required(ErrorMessage = "Ngày kết thúc bán vé là bắt buộc")]
        public DateOnly TicketSaleEnd { get; set; }
        public string? createdby {  get; set; }
        [Required(ErrorMessage = "Đối tượng mục tiêu là bắt buộc")]
        public string? targetAudienceTechnicalConference { get; set; }
    }

    // Step 2: Price Phase and Conference Prices
    public class CreatePricePhaseRequest
    {
        [Required(ErrorMessage ="Tên giai đoạn là bắt buộc")]
        public string PhaseName { get; set; }

        [Required(ErrorMessage ="Phần trăm áp dụng là bắt buộc, công thức là actualprice = ticketprice * applypercent của giai đoạn")]
        [Range(0, 100, ErrorMessage = "Phần trăm áp dụng phải là kiểu thập phân")]
        public decimal ApplyPercent { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateOnly StartDate { get; set; }
        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage ="Số lượng vé cho giai đoạn này là bắt buộc")]
        public int Totalslot { get; set; }
    }

    public class CreateConferencePriceRequest
    {
        [Required(ErrorMessage = "Giá vé là bắt buộc")]
        public decimal TicketPrice { get; set; }

        [MaxLength(255)]
        [Required(ErrorMessage = "Tên vé là bắt buộc")]
        public string TicketName { get; set; }

        [MaxLength(500)]
        [Required(ErrorMessage = "Mô tả vé là bắt buộc")]
        public string TicketDescription { get; set; }
        [Required(ErrorMessage ="Đây có phải là vé cho vai trò tác giả?")]

        public Boolean isAuthor { get; set; }
        [Required(ErrorMessage = "Tổng số lượng là bắt buộc")]
        public int TotalSlot { get; set; }
    }

    public class AddConferencePricesRequest
    {
        public CreateConferencePriceRequest TypeOfTicket { get; set; }
        public List<CreatePricePhaseRequest> Phases { get; set; }
    }

    // Step 3: Conference Sessions
    public class CreateConferenceSessionRequest
    {
        [Required(ErrorMessage = "Tiêu đề phiên là bắt buộc")]
        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public TimeOnly? StartTime { get; set; }
        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public TimeOnly? EndTime { get; set; }
        [Required(ErrorMessage = "Ngày diễn ra phiên là bắt buộc")]
        public DateOnly? Date { get; set; }

        [Required(ErrorMessage = "ID phòng là bắt buộc cho phiên")]

        public string? RoomId { get; set; }
        [Required(ErrorMessage = "Cần có ít nhất một diễn giả")]
        public List<CreateSpeakerRequest> Speaker { get; set; }
        public List<CreateConferenceSessionMediaRequest> SessionMedias { get; set; }
    }

    public class CreateConferenceSessionMediaRequest
    {
        public IFormFile MediaFile { get; set; }
        public string? MediaUrl { get; set; }
    }

    public class CreateSpeakerRequest
    {
        [Required(ErrorMessage = "Tên diễn giả là bắt buộc")]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile Image {  get; set; }
    }

    public class AddConferenceSessionsRequest
    {
        public List<CreateConferenceSessionRequest>? Sessions { get; set; }
    }

    // Step 4: Conference Policies
    public class CreateConferencePolicyRequest
    {
        [MaxLength(255)]
        [Required(ErrorMessage = "Tên chính sách là bắt buộc")]
        public string? PolicyName { get; set; }
        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string? Description { get; set; }
    }

    public class AddConferencePoliciesRequest
    {
        public List<CreateConferencePolicyRequest>? Policies { get; set; }
    }

    // Step 5: Conference Media
    public class CreateConferenceMediaRequest
    {
        public IFormFile MediaFile { get; set; }
        public string? MediaUrl { get; set; }
    }

    public class AddConferenceMediaRequest
    {
        public List<CreateConferenceMediaRequest>? Media { get; set; }
    }

    // Step 6: Conference Sponsors
    public class CreateSponsorRequest
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "Tên nhà tài trợ là bắt buộc")]
        public string? Name { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class AddConferenceSponsorsRequest
    {
        public List<CreateSponsorRequest>? Sponsors { get; set; }
    }

    // Update Requests for individual components
    public class UpdateConferenceBasicRequest
    {
        [Required(ErrorMessage = "Tên hội nghị là bắt buộc")]
        [MaxLength(255)]
        public string ConferenceName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateOnly EndDate { get; set; }
        [Required(ErrorMessage = "Tổng số slot là bắt buộc")]
        public int? TotalSlot { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public IFormFile BannerImageFile { get; set; }

        [Required(ErrorMessage = "Bạn cần xác định hội nghị này do nội bộ tổ chức hay không")]
        public bool? IsInternalHosted { get; set; }
        [Required(ErrorMessage = "Đây có phải là hội nghị nghiên cứu không?")]

        public bool? IsResearchConference { get; set; }

        [Required(ErrorMessage = "ID danh mục là bắt buộc")]
        [MaxLength(50)]
        public string ConferenceCategoryId { get; set; }

        [Required(ErrorMessage = "ID thành phố là bắt buộc")]
        public string CityId { get; set; }
        [Required(ErrorMessage = "Ngày bắt đầu bán vé là bắt buộc")]
        public DateOnly? TicketSaleStart { get; set; }
        [Required(ErrorMessage = "Ngày kết thúc bán vé là bắt buộc")]
        public DateOnly? TicketSaleEnd { get; set; }
    }

    public class UpdateConferencePriceRequest
    {
        public decimal? TicketPrice { get; set; }

        [MaxLength(255)]
        public string? TicketName { get; set; }

        [MaxLength(500)]
        public string? TicketDescription { get; set; }

        public int? TotalSlot {get; set; }
    }

    public class UpdateConferenceSessionRequest
    {
        [MaxLength(50)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }
        public DateOnly? Date {  get; set; }
        

        public string? RoomId { get; set; }
    }

    public class UpdateSpeakerRequest
    {
        [Required(ErrorMessage = "Tên diễn giả là bắt buộc")]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }

    }
    public class ConferencePolicyResponse
    {
        public string? PolicyId { get; set; }
        public string? PolicyName { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateConferencePolicyRequest
    {
        [MaxLength(255)]
        public string? PolicyName { get; set; }

        public string? Description { get; set; }
    }

    public class UpdateConferenceMediaRequest
    {
        public IFormFile? MediaFile { get; set; }
        public string? MediaUrl { get; set; }

    }

    public class UpdateSponsorRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }

    }

    // Response DTOs
    public class TechnicalConferenceBasicStepResponse
    {
       public string? conferenceId { get; set; }
        public string? ConferenceName { get; set; }

       
        public string? Description { get; set; }

       
        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public string? Address { get; set; }

        public string? bannerImageFileUrl { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }
        public string? ConferenceCategoryId { get; set; }

        public string? CityId { get; set; }
        public DateOnly? createdAt {  get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public string? createdby { get; set; }
        public string? TargetAudience { get; set; }
    }

    public class ConferencePriceStepResponse
    {
        public string PriceId { get; set; }
        public decimal? TicketPrice { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public decimal? ActualPrice { get; set; }
        public string? CurrentPhase { get; set; }
        public string? PricePhaseId { get; set; }
    }

    public class ConferenceSessionStepResponse
    {
        public string SessionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public DateOnly? Date { get; set; }
        public string? RoomId { get; set; }
        public RoomInfoResponse? Room { get; set; }
        public List<SpeakerResponse>? Speakers { get; set; }
    }

    public class RoomInfoResponse
    {
        public string RoomId { get; set; }
        public string? Number { get; set; }
        public string? DisplayName { get; set; }
        public string? DestinationId { get; set; }
    }

    public class SpeakerResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl {  get; set; } 
    }

    // Add missing ConferenceMediaResponse
    public class ConferenceMediaResponse
    {
        public string MediaId { get; set; }
        public string? MediaUrl { get; set; }
    }

    // Add missing SponsorResponse
    public class SponsorResponse
    {
        public string SponsorId { get; set; }
        public string Name { get; set; }
        public string ImageUrl {get; set; }
    }

    // Additional Response DTOs for the remaining operations
    public class ConferenceSessionMediaResponse
    {
        public string MediaId { get; set; }
        public string? MediaUrl { get; set; }
    }

    public class PricePhaseResponse
    {
        public string PricePhaseId { get; set; }
        public string? PhaseName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal? ApplyPercent { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
    }

    public class ConferencePriceWithPhasesResponse
    {
        public string ConferencePriceId { get; set; }
        public decimal? TicketPrice { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public List<PricePhaseResponse>? PricePhases { get; set; }
    }

    public class ConferenceSessionWithMediaResponse
    {
        public string ConferenceSessionId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public DateOnly? Date { get; set; }
        public string? ConferenceId { get; set; }
        public string? RoomId { get; set; }
        public List<SpeakerResponse>? Speakers { get; set; }
        public List<ConferenceSessionMediaResponse>? SessionMedia { get; set; }
    }
    
    // Step 7: Refund Policies
    public class CreateRefundPolicyRequest
    {
        [Required(ErrorMessage = "Phần trăm hoàn trả là bắt buộc")]
        [Range(0, 100, ErrorMessage = "Phần trăm hoàn trả phải từ 0 đến 100")]
        public int? PercentRefund { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn hoàn trả là bắt buộc")]
        public DateOnly? RefundDeadline { get; set; }

        [Required(ErrorMessage = "Thứ tự hoàn trả là bắt buộc")]
        public int? RefundOrder { get; set; }
    }

    public class AddRefundPoliciesRequest
    {
        public List<CreateRefundPolicyRequest>? RefundPolicies { get; set; }
    }

    public class UpdateRefundPolicyRequest
    {
        [Range(0, 100, ErrorMessage = "Phần trăm hoàn trả phải từ 0 đến 100")]
        public int? PercentRefund { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn hoàn trả là bắt buộc")]
        public DateOnly? RefundDeadline { get; set; }

        [Required(ErrorMessage = "Thứ tự hoàn trả là bắt buộc")]
        public int? RefundOrder { get; set; }
    }

    public class RefundPolicyResponse
    {
        public string? RefundPolicyId { get; set; }
        public int? PercentRefund { get; set; }
        public DateOnly? RefundDeadline { get; set; }
        public int? RefundOrder { get; set; }
    }
}