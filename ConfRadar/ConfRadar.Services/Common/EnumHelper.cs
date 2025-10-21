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
    public enum GenderTypeEnum
    {
        Male,
        Female,
        Other
    }
    public enum ObjectStorageBucketEnum
    {
        avatar
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
    public enum GlobalStatusEnum
    {

        [Description("Pending")]
        Pending,
        [Description("Accepted")]
        Accepted,
        [Description("Rejected")]
        Rejected
    }
    public enum TransactionStatusEnum
    {

        [Description("Pending")]
        Pending,
        [Description("Success")]
        Success,
        [Description("Fail")]
        Fail
    }

    public enum PaymentMethodEnum
    {
        [Description("ZaloPay")]
        ZaloPay,
        [Description("MoMo")]
        MoMo,
    }
    public enum TransactionTypeEnum
    {
        [Description("Refund")]
        Refund,
        [Description("Payment")]
        Payment,
    }
   
}
