using System.Text.Json.Serialization;

namespace AggregateVersions.Presentation.Models
{
    public class AccessNode
    {
        [JsonPropertyName("id")]
        public string? ID { get; set; }

        [JsonPropertyName("text")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("children")]
        public List<AccessNode>? Nodes { get; set; }

        [JsonIgnore()]
        public bool Children => Nodes != null && Nodes.Count != 0;

    }
}
