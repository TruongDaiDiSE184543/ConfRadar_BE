using System.ComponentModel;

namespace ConfRadar.Services.Common
{
    public enum LoginProviderEnum
    {
        Local,
        Firebase,
        Orcid,
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
        report

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
        [Description("Pending")]
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

        [Description("Refund")]
        Refund,

        [Description("Purchase")]
        Purchase,

        //[Description("Adjustment")]
        //Adjustment,        

        //[Description("Pending")]
        //Pending,          

        //[Description("Notified")]
        //Notified,           
    }


}
