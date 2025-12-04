using System.ComponentModel;

namespace ConfRadar.Services.Common
{
    public enum LoginProviderEnum
    {
        Local,
        Firebase,
        Orcid,
    }

    public enum OrcidDataTypeEnum
    {
        Works,
        Biography,
        Education
    }

    public enum SystemRoleEnum
    {


        [Description("Conference Organizer")]
        ConferenceOrganizer,

        [Description("Collaborator")]
        Collaborator,

        [Description("Local Reviewer")]
        LocalReviewer,

        [Description("Admin")]
        Admin,

        [Description("External Reviewer")]
        ExternalReviewer,

        [Description("Customer")]
        Customer
    }

    //public enum PaperPhase
    //{
    //    Abstract,
    //    FullPaper,
    //    Revise,
    //    CameraReady
    //}
    public enum GenderTypeEnum
    {
        Male,
        Female,
        Other
    }
    public enum ObjectStorageBucketEnum
    {
        avatar,
        conferencebanner,
        conferencemedia,

        conferencesessionmedia,
        sponsorimage,
        speakerimage,

        abstractfile,
        rankingreference,
        rankingfile,
        fullpaperfile,
        revisionpaperfile,
        camerareadyfile,
        materialdownload,
        feedbackmaterial,
        contract,
        sessionmedia,
        speakermedia,
        //abstractfile,
        //fullpaperfile,
        //revisionpaperfile,
        revisionpaperreviewfile,
        reviewercontractfile,
        qrcodefile,
        report,
        collaboratorcontract
    }

    //public enum ConferenceStatus
    //{
    //    Pending,
    //    Rejected,
    //    Preparing,
    //    Ready,
    //    OnHold,
    //    Cancelled,
    //    Completed
    //}

    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }
    }

    public enum RankingCategoriesEnum
    {
        Core,
        IF,
        H5,
        CiteScore
    }

    //public enum ReviewStatus
    //{
    //    Accepted,
    //    Rejected,
    //    Revise,
    //    Pending
    //}
    public enum GlobalStatusEnum
    {

        [Description("Pending")]
        Pending,
        [Description("Accepted")]
        Accepted,
        [Description("Rejected")]
        Rejected
    }


    public enum PaymentMethodEnum
    {
        [Description("ZaloPay")]
        ZaloPay,
        [Description("MoMo")]
        MoMo,
        [Description("PayOs")]
        PayOs,
        [Description("VnPay")]
        VnPay,
        [Description("Wallet")]
        Wallet,

    }
    public enum ConferenceStatusEnum
    {

        [Description("Preparing")]
        Preparing,

        [Description("Pending")]
        Pending,

        [Description("Completed")]
        Completed,

        [Description("Cancelled")]
        Cancelled,

        [Description("OnHold")]
        OnHold,

        [Description("Ready")]
        Ready,

        [Description("Draft")]
        Draft,

        [Description("Deleted")]
        Deleted,
        Rejected,
        [Description("Disabled")]
        Disabled
    }
    public enum PaperPhaseEnum
    {
        [Description("Abstract")]
        Abstract,

        [Description("FullPaper")]
        FullPaper,

        [Description("Revise")]
        Revise,

        [Description("CameraReady")]
        CameraReady
    }
    public enum CheckInStatusEnum
    {
        [Description("Not Checked In")]
        Pending,

        [Description("Checked In")]
        CheckedIn,

        [Description("Expired")]
        Expired
    }
    public enum ReviewStatusEnum
    {
        [Description("Pending")]
        Pending,

        [Description("Revise")]
        Revise,

        [Description("Rejected")]
        Rejected,

        [Description("Accepted")]
        Accepted
    }
    public enum WaitListStatusEnum
    {
        [Description("Pending")]
        Pending,

        [Description("Notified")]
        Notified,


    }

    public enum WalletTransactionTypeEnum
    {
        //[Description("Deposit")]
        //Deposit,            

        //[Description("Withdraw")]
        //Withdraw,          

        [Description("Hoàn tiền")]
        Refund,

        [Description("Mua hàng")]
        Purchase,

        //[Description("Adjustment")]
        //Adjustment,        

        //[Description("Pending")]
        //Pending,          

        //[Description("Notified")]
        //Notified,           
    }







    public enum AuditLogActionNameEnum
    {
        [Description("Người dùng")]
        User,

        [Description("Hội nghị")]
        Conference,

        [Description("Bài báo")]
        Paper,

        [Description("Hợp đồng")]
        Contract,

        [Description("Vé")]
        Ticket,

        [Description("Báo cáo")]
        Report,

        [Description("Phòng")]
        Room,

        [Description("Danh mục")]
        Category,

        [Description("Xác thực")]
        Authentication,

        [Description("Giao dịch")]
        Transaction
    }



    public static class AuditLogDescriptionData
    {
        // QUẢN LÝ NGƯỜI DÙNG
        public const string CREATE_USER = "Tạo người dùng mới";
        public const string UPDATE_PROFILE = "Cập nhật thông tin cá nhân";
        public const string CHANGE_PASSWORD = "Đổi mật khẩu";
        public const string SUSPEND_ACCOUNT = "Tạm ngừng tài khoản";
        public const string ACTIVATE_ACCOUNT = "Kích hoạt tài khoản";
        public const string CREATE_COLLABORATOR = "Tạo tài khoản cộng tác viên";
        public const string CREATE_LOCAL_REVIEWER = "Tạo tài khoản reviewer nội bộ";
        public const string DELETE_USER = "Xóa người dùng";

        // QUẢN LÝ HỘI NGHỊ
        public const string CREATE_CONFERENCE = "Tạo hội nghị mới";
        public const string UPDATE_CONFERENCE = "Cập nhật hội nghị";
        public const string DELETE_CONFERENCE = "Xóa hội nghị";
        public const string APPROVE_CONFERENCE = "Phê duyệt / từ chối hội nghị";
        public const string ACTIVATE_WAITLIST = "Kích hoạt danh sách chờ";
        public const string ADD_TO_FAVOURITE = "Thêm vào yêu thích";
        public const string REMOVE_FROM_FAVOURITE = "Gỡ khỏi yêu thích";

        // QUẢN LÝ BÀI BÁO
        public const string SUBMIT_ABSTRACT = "Nộp tóm tắt";
        public const string SUBMIT_FULL_PAPER = "Nộp bài báo đầy đủ";
        public const string SUBMIT_CAMERA_READY = "Nộp bài camera-ready";
        public const string SUBMIT_REVISION = "Nộp bài sửa đổi";
        public const string UPDATE_ABSTRACT = "Cập nhật tóm tắt";
        public const string UPDATE_FULL_PAPER = "Cập nhật bài báo đầy đủ";
        public const string UPDATE_CAMERA_READY = "Cập nhật bài camera-ready";
        public const string DECIDE_ABSTRACT_STATUS = "Phê duyệt/từ chối tóm tắt";
        public const string DECIDE_FULL_PAPER_STATUS = "Phê duyệt/từ chối bài báo đầy đủ";
        public const string DECIDE_CAMERA_READY = "Phê duyệt/từ chối bài camera-ready";
        public const string ASSIGN_PAPER_TO_REVIEWER = "Phân công bài báo cho reviewer";
        public const string SUBMIT_PAPER_REVIEW = "Nộp đánh giá bài báo";
        public const string ASSIGN_PRESENTER = "Phân công người trình bày";

        // QUẢN LÝ HỢP ĐỒNG
        public const string CREATE_REVIEWER_CONTRACT = "Tạo hợp đồng reviewer";
        public const string CREATE_COLLABORATOR_CONTRACT = "Tạo hợp đồng cộng tác viên";
        public const string CREATE_REVIEW_CONTRACT_NEW_USER = "Tạo hợp đồng reviewer cho người dùng mới";

        // QUẢN LÝ VÉ
        public const string REFUND_TICKET = "Hoàn tiền vé";
        public const string CANCEL_RESEARCH_TICKET = "Hủy vé nghiên cứu";
        public const string CANCEL_TECHNICAL_TICKET = "Hủy vé kỹ thuật";

        // QUẢN LÝ BÁO CÁO
        public const string SUBMIT_REPORT = "Gửi báo cáo";
        public const string RESPOND_TO_REPORT = "Phản hồi báo cáo";

        // QUẢN LÝ PHÒNG
        public const string CREATE_ROOM = "Tạo phòng";
        public const string UPDATE_ROOM = "Cập nhật phòng";
        public const string DELETE_ROOM = "Xóa phòng";

        // QUẢN LÝ DANH MỤC
        public const string CREATE_CATEGORY = "Tạo danh mục";
        public const string UPDATE_CATEGORY = "Cập nhật danh mục";
        public const string DELETE_CATEGORY = "Xóa danh mục";

        // XÁC THỰC (AUTHENTICATION)
        public const string LOGIN = "Đăng nhập";
        public const string LOGOUT = "Đăng xuất";
        public const string REGISTER = "Đăng ký";
        public const string FORGOT_PASSWORD = "Yêu cầu đặt lại mật khẩu";
        public const string RESET_PASSWORD = "Hoàn tất đặt lại mật khẩu";

        // GIAO DỊCH / THANH TOÁN
        public const string PROCESS_PAYMENT = "Xử lý thanh toán";
        public const string REFUND_PAYMENT = "Hoàn tiền thanh toán";
    }


    public enum SuspendTypeEnum
    {
        User,
        UserRole
    }



}
