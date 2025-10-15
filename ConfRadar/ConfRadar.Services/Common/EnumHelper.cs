namespace ConfRadar.Services.Common
{
    public enum LoginProvider
    {
        Local,
        Google,
        Orcid,
    }
    public enum SystemRole
    {
        ConferenceOrganizer,
        Collaborator,
        LocalReviewer,
        Admin
    }
    public enum ObjectStorageBucket
    {
        ConfRadar
    }
}
