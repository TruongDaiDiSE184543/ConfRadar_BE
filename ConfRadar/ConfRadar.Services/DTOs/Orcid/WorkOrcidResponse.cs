using System.Text.Json.Serialization;

namespace ConfRadar.Services.DTOs.Orcid
{
    internal class WorkOrcidResponse
    {
    }
    public class OrcidWorksResponse { [JsonPropertyName("group")] public List<WorkGroup> Group { get; set; } }
    public class WorkGroup { [JsonPropertyName("work-summary")] public List<WorkSummary> WorkSummary { get; set; } }
    public class WorkSummary
    {
        [JsonPropertyName("put-code")] public long PutCode { get; set; }
        [JsonPropertyName("title")] public TitleContainer Title { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("publication-date")] public PublicationDate PublicationDate { get; set; }
        [JsonPropertyName("external-ids")] public ExternalIdsContainer ExternalIds { get; set; }
        [JsonPropertyName("url")] public ValueObject Url { get; set; }
    }
    public class TitleContainer { [JsonPropertyName("title")] public Title Title { get; set; } }
    public class Title { [JsonPropertyName("value")] public string Value { get; set; } }
    public class PublicationDate { [JsonPropertyName("year")] public ValueObject Year { get; set; } }
    public class ValueObject { [JsonPropertyName("value")] public string Value { get; set; } }
    public class ExternalIdsContainer { [JsonPropertyName("external-id")] public List<ExternalId> ExternalId { get; set; } }
    public class ExternalId
    {
        [JsonPropertyName("external-id-type")] public string Type { get; set; }
        [JsonPropertyName("external-id-value")] public string Value { get; set; }
    }
}
