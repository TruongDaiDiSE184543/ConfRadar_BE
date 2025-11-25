namespace ConfRadar.Services.DTOs.Orcid
{
    public class OrcidStatusResponse
    {
        public bool IsLinked { get; set; }
        public string OrcidId { get; set; }
        public string UserName { get; set; }
        public List<string> GrantedScopes { get; set; }
    }
}
