using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.ConferenceStep
{
    // Step 1: Basic Conference Information
    // DÁN PHIÊN BẢN NÀY ĐỂ THAY THẾ DTO CŨ CỦA BẠN

    public class CreateTechnicalConferenceBasicRequest
    {
        [Required(ErrorMessage = "Tên hội nghị là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "Tên hội nghị không được vượt quá 255 ký tự.")]
        public string ConferenceName { get; set; }

        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Tổng số vé là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Tổng số vé phải là một số dương.")]
        public int TotalSlot { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Ảnh bìa là bắt buộc.")]
        public IFormFile BannerImageFile { get; set; }

        // Thuộc tính này chỉ dùng nội bộ, không cần validation
        public string? bannerImageFileUrl { get; set; }

        [Required(ErrorMessage = "Bạn cần xác định hội nghị có do nội bộ tổ chức hay không.")]
        public bool? IsInternalHosted { get; set; }

        [Required(ErrorMessage = "Bạn cần xác định đây có phải là hội nghị nghiên cứu không.")]
        public bool? IsResearchConference { get; set; }

        [Required(ErrorMessage = "ID danh mục là bắt buộc.")]
        [MaxLength(50)]
        public string ConferenceCategoryId { get; set; }

        [Required(ErrorMessage = "ID thành phố là bắt buộc.")]
        public string CityId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu bán vé là bắt buộc.")]
        public DateOnly TicketSaleStart { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc bán vé là bắt buộc.")]
        public DateOnly TicketSaleEnd { get; set; }

        [Required(ErrorMessage = "Đối tượng mục tiêu là bắt buộc.")]
        public string targetAudienceTechnicalConference { get; set; }
    }

    // Step 2: Price Phase and Conference Prices
    public class CreatePricePhaseRequest
    {
        [Required(ErrorMessage = "Tên giai đoạn là bắt buộc")]
        public string PhaseName { get; set; }

        [Required(ErrorMessage = "Phần trăm áp dụng là bắt buộc, công thức là actualprice = ticketprice * applypercent của giai đoạn")]
        [Range(0, 1000, ErrorMessage = "Phần trăm áp dụng phải là kiểu thập phân, nằm trong khoảng 0-1000")]
        public decimal ApplyPercent { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateOnly StartDate { get; set; }
        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Số lượng vé cho giai đoạn này là bắt buộc")]
        public int Totalslot { get; set; }
        public List<CreateRefundPolicyRequest>? refundInPhase { get; set; }
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
        [Required(ErrorMessage = "Đây có phải là vé cho vai trò tác giả?")]

        public Boolean isAuthor { get; set; }
        [Required(ErrorMessage = "Tổng số lượng là bắt buộc")]
        public int TotalSlot { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "Mỗi loại vé phải có ít nhất một giai đoạn.")]
        public List<CreatePricePhaseRequest> Phases { get; set; }
    }

    public class PhaseForWaitList
    {
        public List<CreatePricePhaseRequest> Phases { get; set; }
    }

    public class AddConferencePricesRequest
    {
        public List<CreateConferencePriceRequest> TypeOfTicket { get; set; }

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

        public string? RoomId { get; set; }

        public List<CreateSpeakerRequest>? Speaker { get; set; }
        public List<CreateConferenceSessionMediaRequest>? SessionMedias { get; set; }
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
        //public string? ImageUrl { get; set; }
        public IFormFile Image { get; set; }
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
        [MaxLength(255)]
        public string? ConferenceName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }


        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Tổng số vé phải là một số dương.")]
        public int? TotalSlot { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public IFormFile? BannerImageFile { get; set; }


        [MaxLength(50)]
        public string? ConferenceCategoryId { get; set; }

        public string? CityId { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }

        public string? targetaudience { get; set; }
    }



    public class UpdateResearchConferenceBasicRequest
    {
        [MaxLength(255)]
        public string? ConferenceName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }


        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Tổng số vé phải là một số dương.")]
        public int? TotalSlot { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public IFormFile? BannerImageFile { get; set; }


        [MaxLength(50)]
        public string? ConferenceCategoryId { get; set; }

        public string? CityId { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
    }

    public class UpdateConferencePriceRequest
    {
        public decimal? TicketPrice { get; set; }

        [MaxLength(255)]
        public string? TicketName { get; set; }

        [MaxLength(500)]
        public string? TicketDescription { get; set; }

        public int? TotalSlot { get; set; }
    }

    public class UpdateConferenceSessionRequest
    {
        [MaxLength(250)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }
        public DateOnly? Date { get; set; }


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
        public DateTime? createdAt { get; set; }
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
        public string SpeakerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string? ConferenceSessionId { get; set; }
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
        public string ImageUrl { get; set; }
    }

    // Additional Response DTOs for the remaining operations
    public class ConferenceSessionMediaResponse
    {
        public string MediaId { get; set; }
        public string? MediaUrl { get; set; }
    }



    public class ConferencePriceListWithPhasesResponse
    {
        public List<ConferencePriceWithPhasesResponse>? conferencePriceWithPhasesResponses { get; set; }
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


    }

    public class RefundPolicyResponse
    {
        public string? RefundPolicyId { get; set; }
        public int? PercentRefund { get; set; }
        public DateOnly? RefundDeadline { get; set; }
        public int? RefundOrder { get; set; }
        public string? pricePhaseId { get; set; }
    }

    // Research Conference DTOs
    // Step 1: Research Conference Basic Information (without target audience)
    public class CreateResearchConferenceBasicRequest
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
        public string? createdby { get; set; }
        // Note: No target audience for research conference
    }

    // Step 3: Research Conference Sessions (without speakers)
    public class CreateResearchSessionRequest
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


        public string? RoomId { get; set; }
        // Note: No speakers for research conference sessions
        public List<CreateConferenceSessionMediaRequest> SessionMedias { get; set; }
    }

    public class AddResearchSessionsRequest
    {
        public List<CreateResearchSessionRequest>? Sessions { get; set; }
    }

    // Response DTOs for Research Conference
    public class ResearchConferenceBasicStepResponse
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
        public DateTime? createdAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public string? createdby { get; set; }
        // Note: No target audience for research conference
    }

    public class ResearchSessionWithMediaResponse
    {
        public string ConferenceSessionId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public DateOnly? Date { get; set; }
        public string? ConferenceId { get; set; }
        public string? RoomId { get; set; }
        // Note: No speakers for research conference
        public List<ConferenceSessionMediaResponse>? SessionMedia { get; set; }
    }

    // Research Conference Step 2: Research Conference Detail

    public class CreateResearchConferenceDetailRequest
    {

        [Required(ErrorMessage = "Định dạng bài báo là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "Định dạng bài báo không được vượt quá 255 ký tự.")]
        public string PaperFormat { get; set; }

        // `int` không phải nullable, nên [Required] chỉ để làm rõ. Quan trọng là Range.
        [Required(ErrorMessage = "Số lượng bài báo dự kiến chấp nhận là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng bài báo chấp nhận phải là một số dương.")]
        public int NumberPaperAccept { get; set; }

        // `int?` là nullable, nên [Required] là cần thiết.
        [Required(ErrorMessage = "Số lần cho phép sửa đổi là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lần cho phép sửa đổi phải là một số dương.")]
        public int? RevisionAttemptAllowed { get; set; }

        [MaxLength(1000, ErrorMessage = "Mô tả xếp hạng không được vượt quá 1000 ký tự.")]
        public string? RankingDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng cho biết hội nghị có cho phép người nghe tham dự hay không.")]
        public bool? AllowListener { get; set; }


        public string? RankValue { get; set; }

        // Có thể không bắt buộc, nhưng nếu có thì phải hợp lệ.
        [Range(2000, 2050, ErrorMessage = "Năm xếp hạng không hợp lệ.")]
        public int? RankYear { get; set; }

        // Có thể không bắt buộc (nếu miễn phí), nhưng nếu có thì không được âm.
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Phí review không được là số âm.")]
        public decimal? ReviewFee { get; set; }

        [Required(ErrorMessage = "Loại xếp hạng (Ranking Category) là bắt buộc.")]
        [MaxLength(50)]
        public string RankingCategoryId { get; set; }
    }

    public class UpdateResearchConferenceDetailRequest
    {

        [MaxLength(255, ErrorMessage = "Định dạng bài báo không được vượt quá 255 ký tự.")]
        public string? PaperFormat { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng bài báo chấp nhận phải là một số dương.")]
        public int? NumberPaperAccept { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lần cho phép sửa đổi phải là một số dương.")]
        public int? RevisionAttemptAllowed { get; set; }

        [MaxLength(1000, ErrorMessage = "Mô tả xếp hạng không được vượt quá 1000 ký tự.")]
        public string? RankingDescription { get; set; }

        public bool? AllowListener { get; set; }


        public string? RankValue { get; set; }

        [Range(2000, 2025, ErrorMessage = "Năm xếp hạng không hợp lệ (2000-2025).")]
        public int? RankYear { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Phí review không được là số âm.")]
        public decimal? ReviewFee { get; set; }

        [MaxLength(50)]
        public string? RankingCategoryId { get; set; }
    }


    public class ResearchConferenceDetailResponse
    {
        public string? ConferenceId { get; set; }
        public string? Name { get; set; }
        public string? PaperFormat { get; set; }
        public int? NumberPaperAccept { get; set; }
        public int? RevisionAttemptAllowed { get; set; }
        public string? RankingDescription { get; set; }
        public bool? AllowListener { get; set; }
        public string? RankValue { get; set; }
        public int? RankYear { get; set; }
        public decimal? ReviewFee { get; set; }
        public string? RankingCategoryId { get; set; }
        public string? RankingCategoryName { get; set; }
    }

    // Research Conference Step 4: Research Conference Phases and Revision Round Deadlines
    public class CreateResearchConferencePhasesRequest
    {
        [Required]
        [MinLength(2, ErrorMessage = "Phải có ít nhất 2 phase: một phase chính và một phase waitlist.")]
        public List<CreateResearchConferencePhaseItemRequest> Phases { get; set; }
    }

    public class CreateResearchConferencePhaseItemRequest
    {
        [Required]
        public DateOnly? RegistrationStartDate { get; set; }
        [Required]
        public DateOnly? RegistrationEndDate { get; set; }
        public DateOnly? AbstractDecideStatusStart { get; set; }
        public DateOnly? AbstractDecideStatusEnd { get; set; }
        [Required]
        public DateOnly? FullPaperStartDate { get; set; }
        [Required]
        public DateOnly? FullPaperEndDate { get; set; }
        [Required]
        public DateOnly? ReviewStartDate { get; set; }
        [Required]
        public DateOnly? ReviewEndDate { get; set; }
        [Required]
        public DateOnly? FullPaperDecideStatusStart { get; set; }
        [Required]
        public DateOnly? FullPaperDecideStatusEnd { get; set; }
        [Required]
        public DateOnly? ReviseStartDate { get; set; }
        [Required]
        public DateOnly? ReviseEndDate { get; set; }
        [Required]
        public DateOnly? RevisionPaperDecideStatusStart { get; set; }
        [Required]
        public DateOnly? RevisionPaperDecideStatusEnd { get; set; }
        [Required]
        public DateOnly? CameraReadyStartDate { get; set; }
        [Required]
        public DateOnly? CameraReadyEndDate { get; set; }
        [Required]
        public DateOnly? CameraReadyDecideStatusStart { get; set; }
        [Required]
        public DateOnly? CameraReadyDecideStatusEnd { get; set; }

        [Required(ErrorMessage = "Phải xác định đây có phải là phase waitlist hay không.")]
        public bool? IsWaitlist { get; set; }
        public List<CreateRevisionRoundDeadlineRequest>? RevisionRoundDeadlines { get; set; }
    }

    public class addRevisionRequest
    {
        public List<CreateRevisionRoundDeadlineRequest> revision { get; set; }
    }

    public class CreateRevisionRoundDeadlineRequest
    {
        [Required(ErrorMessage = "Ngày bắt đầu nộp bài là bắt buộc.")]
        public DateOnly? StartSubmissionDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc nộp bài là bắt buộc.")]
        public DateOnly? EndSubmissionDate { get; set; }

    }

    public class UpdateResearchConferencePhaseRequest
    {
        public DateOnly? RegistrationStartDate { get; set; }
        public DateOnly? RegistrationEndDate { get; set; }
        public DateOnly? AbstractDecideStatusStart { get; set; }
        public DateOnly? AbstractDecideStatusEnd { get; set; }
        public DateOnly? FullPaperStartDate { get; set; }
        public DateOnly? FullPaperEndDate { get; set; }
        public DateOnly? ReviewStartDate { get; set; }
        public DateOnly? ReviewEndDate { get; set; }
        public DateOnly? FullPaperDecideStatusStart { get; set; }
        public DateOnly? FullPaperDecideStatusEnd { get; set; }
        public DateOnly? ReviseStartDate { get; set; }
        public DateOnly? ReviseEndDate { get; set; }
        public DateOnly? RevisionPaperDecideStatusStart { get; set; }
        public DateOnly? RevisionPaperDecideStatusEnd { get; set; }
        public DateOnly? CameraReadyStartDate { get; set; }
        public DateOnly? CameraReadyEndDate { get; set; }
        public DateOnly? CameraReadyDecideStatusStart { get; set; }
        public DateOnly? CameraReadyDecideStatusEnd { get; set; }
    }

    public class UpdateRevisionRoundDeadlineRequest
    {
        public DateOnly? StartSubmissionDate { get; set; }
        public DateOnly? EndSubmissionDate { get; set; }
    }


    public class CreatePhasesResponse
    {
        public string Message { get; set; }
        public List<string> CreatedPhaseIds { get; set; }
    }

    public class ResearchConferencePhaseResponse
    {
        public string? ResearchConferencePhaseId { get; set; }
        public string? ConferenceId { get; set; }
        public DateOnly? RegistrationStartDate { get; set; }
        public DateOnly? RegistrationEndDate { get; set; }
        // Abstract phase decide status dates (conference organizer only)
        public DateOnly? AbstractDecideStatusStart { get; set; }
        public DateOnly? AbstractDecideStatusEnd { get; set; }
        public DateOnly? FullPaperStartDate { get; set; }
        public DateOnly? FullPaperEndDate { get; set; }
        // Full paper review dates (normal reviewers)
        public DateOnly? ReviewStartDate { get; set; }
        public DateOnly? ReviewEndDate { get; set; }
        // Full paper decide status dates (head reviewer)
        public DateOnly? FullPaperDecideStatusStart { get; set; }
        public DateOnly? FullPaperDecideStatusEnd { get; set; }
        public DateOnly? ReviseStartDate { get; set; }
        public DateOnly? ReviseEndDate { get; set; }
        // Revision paper review dates (normal reviewers)
        public DateOnly? RevisionPaperReviewStart { get; set; }
        public DateOnly? RevisionPaperReviewEnd { get; set; }
        // Revision paper decide status dates (head reviewer)
        public DateOnly? RevisionPaperDecideStatusStart { get; set; }
        public DateOnly? RevisionPaperDecideStatusEnd { get; set; }
        public DateOnly? CameraReadyStartDate { get; set; }
        public DateOnly? CameraReadyEndDate { get; set; }
        // Camera ready decide status dates (head reviewer only)
        public DateOnly? CameraReadyDecideStatusStart { get; set; }
        public DateOnly? CameraReadyDecideStatusEnd { get; set; }
        public bool? IsWaitlist { get; set; }
        public bool? IsActive { get; set; }
        public List<RevisionRoundDeadlineResponse>? RevisionRoundDeadlines { get; set; }
    }

    public class RevisionRoundDeadlineResponse
    {
        public string? RevisionRoundDeadlineId { get; set; }
        public DateOnly? StartSubmissionDate { get; set; }
        public DateOnly? EndSubmissionDate { get; set; }
        public int? RoundNumber { get; set; }
        public string? ResearchConferencePhaseId { get; set; }
    }

    // Research Conference Step 5: Material Downloads
    public class CreateMaterialDownloadRequest
    {

        [MaxLength(1000)]
        public string? FileDescription { get; set; }
        //public string? FileName{ get; set; }

        public IFormFile? File { get; set; }
    }

    public class UpdateMaterialDownloadRequest
    {

        [MaxLength(1000)]
        public string? FileDescription { get; set; }

        public IFormFile? File { get; set; }
    }

    public class MaterialDownloadResponse
    {
        public string? MaterialDownloadId { get; set; }
        public string? FileName { get; set; }
        public string? FileDescription { get; set; }
        public string? FileUrl { get; set; }
    }

    // Research Conference Step 6: Ranking File URLs
    public class CreateRankingFileUrlRequest
    {
        [MaxLength(1000)]

        public string? FileUrl { get; set; }
        [Required(ErrorMessage = "Cần phải có File là bắt buộc")]
        public IFormFile File { get; set; }
    }

    public class UpdateRankingFileUrlRequest
    {
        [MaxLength(1000)]
        public string? FileUrl { get; set; }
        [Required]
        public IFormFile? File { get; set; }
    }

    public class RankingFileUrlResponse
    {
        public string? RankingFileUrlId { get; set; }
        public string? FileUrl { get; set; }
    }

    // Research Conference Step 7: Ranking Reference URLs
    public class CreateRankingReferenceUrlRequest
    {
        [MaxLength(1000)]
        [Required(ErrorMessage = "URL tham khảo là bắt buộc")]
        public string? ReferenceUrl { get; set; }
    }

    public class UpdateRankingReferenceUrlRequest
    {
        [Required]
        [MaxLength(1000)]
        public string ReferenceUrlId { get; set; }
    }

    public class RankingReferenceUrlResponse
    {
        public string? ReferenceUrlId { get; set; }
        public string? ReferenceUrl { get; set; }
    }

    // Price Phase DTOs - For CRUD operations on PricePhase
    public class CreatePricePhaseRequestForConferencePrice
    {
        [Required(ErrorMessage = "Tên giai đoạn là bắt buộc")]
        public string PhaseName { get; set; }

        [Required(ErrorMessage = "Phần trăm áp dụng là bắt buộc, công thức là actualprice = ticketprice * applypercent của giai đoạn")]
        [Range(0, 1000, ErrorMessage = "Phần trăm áp dụng phải là kiểu thập phân, nằm trong khoảng 0-1000")]
        public decimal ApplyPercent { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateOnly StartDate { get; set; }
        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Số lượng vé cho giai đoạn này là bắt buộc")]
        public int TotalSlot { get; set; }
        public bool ForWaitlist { get; set; } = false;
    }

    public class UpdatePricePhaseRequest
    {
        public string? PhaseName { get; set; }

        [Range(0, 1000, ErrorMessage = "Phần trăm áp dụng phải là kiểu thập phân, nằm trong khoảng 0-1000")]
        public decimal? ApplyPercent { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public int? TotalSlot { get; set; }
    }

    public class AddPricePhasesRequest
    {
        public List<CreatePricePhaseRequestForConferencePrice>? PricePhases { get; set; }
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
        public string? ConferencePriceId { get; set; }
        public string? ResearchConferencePhaseId { get; set; }
        public List<RefundPolicyResponse> RefundPolicy { get; set; }
    }

    // Speaker DTOs - For CRUD operations on Speaker
    public class CreateSpeakerRequestForConferenceSession
    {
        [Required(ErrorMessage = "Tên diễn giả là bắt buộc")]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
        public IFormFile Image { get; set; }
    }

    public class UpdateSpeakerRequestForConferenceSession
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class AddSpeakersRequest
    {
        public List<CreateSpeakerRequestForConferenceSession>? Speakers { get; set; }
    }


}