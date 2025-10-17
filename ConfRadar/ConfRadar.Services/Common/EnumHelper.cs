using System.ComponentModel;

namespace ConfRadar.Services.Common
{
    public enum LoginProvider
    {
        Local,
        Firebase,
        Orcid,
    }
    public enum SystemRole
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
    public enum GenderType
    {
        Male,
        Female,
        Other
    }
    public enum ObjectStorageBucket
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
}
