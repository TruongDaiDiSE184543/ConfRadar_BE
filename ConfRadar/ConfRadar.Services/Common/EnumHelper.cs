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

    public enum PaperPhase
    {
        Abstract,
        FullPaper,
        Revise,
        CameraReady
    }
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
        speakerimage
    }

    public enum ConferenceStatus
    {
        Pending,
        Rejected,
        Rreparing,
        Ready,
        OnHold,
        Canceled,
        Completed
    }
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }
    }

    public enum RankingCategories
    {
        Core,
        IF,
        H5,
        CiteScore
    }

    public enum ReviewStatus
    {
        Accepted,
        Rejected,
        Revise,
        Pending
    }
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
    }
   

}
