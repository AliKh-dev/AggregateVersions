using System.Text.Json.Serialization;

namespace AggregateVersions.Domain.DTO
{
    public class AccessRequest
    {
        [JsonPropertyName("id")]
        public long ID { get; set; }

        [JsonPropertyName("text")]
        public string? Title { get; set; }
    }
}
